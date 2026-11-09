using Microsoft.AspNetCore.Authentication.Cookies;
using RaceDay_MVC.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options => {
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<ApiExceptionFilter>();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
    options.Cookie.Name = "RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.LoginPath = "/Home/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.SlidingExpiration = false;
});
builder.Services.AddAuthorization();
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddHttpClient("RaceDayApi", client => {
    var address = builder.Configuration["RaceDayApi:BaseUrl"] ?? "https://localhost:7152/";
    client.BaseAddress = new Uri(address.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
}).AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddScoped<RaceDayApi>();
builder.Services.AddSingleton<LocalImageStore>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
