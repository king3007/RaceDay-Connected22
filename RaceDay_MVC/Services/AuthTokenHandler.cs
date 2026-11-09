using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
namespace RaceDay_MVC.Services;
public class AuthTokenHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = accessor.HttpContext;
        var token = context == null ? null : await context.GetTokenAsync("access_token");
        if (!string.IsNullOrWhiteSpace(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
