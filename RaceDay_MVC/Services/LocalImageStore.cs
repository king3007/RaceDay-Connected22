using System.Text.Json;
using System.Text.RegularExpressions;
namespace RaceDay_MVC.Services;

public enum ImageKind { Banners, Profiles }
public sealed record ValidatedImage(byte[] Bytes, string Extension);
public sealed class ImageUploadException(string message) : Exception(message);

// Image bytes live in wwwroot. Private per-record references live in App_Data.
// This keeps uploads persistent across application restarts without changing the API database.
public sealed class LocalImageStore(IWebHostEnvironment environment, ILogger<LocalImageStore> logger)
{
    public const int MaxBytes = 5 * 1024 * 1024;
    private static readonly SemaphoreSlim WriteLock = new(1, 1);
    private string WebRoot => environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
    private static string Folder(ImageKind kind) => kind switch {
        ImageKind.Banners => "banners", ImageKind.Profiles => "profiles",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
    private string ReferenceFile(ImageKind kind, int id)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        return Path.Combine(environment.ContentRootPath, "App_Data", "image-references", Folder(kind), $"{id}.json");
    }
    private static bool SafeFileName(string name) => Regex.IsMatch(name, @"\A[a-f0-9]{32}\.(jpg|png|webp)\z");
    private string? ReadReference(ImageKind kind, int id)
    {
        var path = ReferenceFile(kind, id);
        if (!File.Exists(path)) return null;
        var file = JsonSerializer.Deserialize<string>(File.ReadAllText(path));
        return file != null && SafeFileName(file) ? file : null;
    }
    public string? GetPath(ImageKind kind, int id)
    {
        if (id <= 0) return null;
        try {
            var file = ReadReference(kind, id);
            return file != null && File.Exists(Path.Combine(WebRoot, "uploads", Folder(kind), file))
                ? $"/uploads/{Folder(kind)}/{file}" : null;
        } catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException) {
            logger.LogWarning(e, "Could not read local image reference for {Kind} {Id}", kind, id);
            return null;
        }
    }
    // Never rely on the browser MIME type or client filename alone.
    public async Task<ValidatedImage?> ValidateAsync(IFormFile? file)
    {
        if (file == null) return null;
        if (file.Length <= 0) throw new ImageUploadException("Choose an image that is not empty.");
        if (file.Length > MaxBytes) throw new ImageUploadException("Choose an image smaller than or equal to 5 MB.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension == ".jpeg") extension = ".jpg";
        if (extension is not (".jpg" or ".png" or ".webp"))
            throw new ImageUploadException("Choose a JPG, PNG or WebP image.");
        await using var stream = file.OpenReadStream();
        using var output = new MemoryStream();
        var buffer = new byte[81920];
        int count;
        while ((count = await stream.ReadAsync(buffer.AsMemory())) > 0) {
            if (output.Length + count > MaxBytes) throw new ImageUploadException("Choose an image smaller than or equal to 5 MB.");
            await output.WriteAsync(buffer.AsMemory(0, count));
        }
        var bytes = output.ToArray();
        bool matches = extension switch {
            ".jpg" => bytes.Length >= 4 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff,
            ".png" => bytes.Length >= 24 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".webp" => bytes.Length >= 12 && System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP",
            _ => false
        };
        if (!matches) throw new ImageUploadException("This file does not match the selected image type. Choose a valid JPG, PNG or WebP image.");
        return new ValidatedImage(bytes, extension);
    }
    public async Task SaveAsync(ImageKind kind, int id, ValidatedImage image)
    {
        var reference = ReferenceFile(kind, id);
        var folder = Path.Combine(WebRoot, "uploads", Folder(kind));
        var name = Guid.NewGuid().ToString("N") + image.Extension;
        if (!SafeFileName(name)) throw new ImageUploadException("Unsupported image type.");
        var destination = Path.Combine(folder, name);
        var tempReference = reference + "." + Guid.NewGuid().ToString("N") + ".tmp";
        await WriteLock.WaitAsync();
        try {
            Directory.CreateDirectory(folder);
            Directory.CreateDirectory(Path.GetDirectoryName(reference)!);
            var oldFile = ReadReference(kind, id);
            try {
                await File.WriteAllBytesAsync(destination, image.Bytes);
                await File.WriteAllTextAsync(tempReference, JsonSerializer.Serialize(name));
                File.Move(tempReference, reference, overwrite: true);
            } catch {
                try { File.Delete(destination); File.Delete(tempReference); } catch (IOException) { }
                throw;
            }
            if (oldFile != null) {
                try { File.Delete(Path.Combine(folder, oldFile)); }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException) { logger.LogWarning(e, "Old image cleanup failed"); }
            }
        } finally { WriteLock.Release(); }
    }
}
