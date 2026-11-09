using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Text.Json;
namespace RaceDay_MVC.Services;
public class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is ApiException api && api.Status == HttpStatusCode.Unauthorized) {
            await context.HttpContext.SignOutAsync();
            context.Result = new RedirectToActionResult("Login", "Home", null);
            context.ExceptionHandled = true;
            return;
        }
        if (context.Exception is not (ApiException or HttpRequestException or TaskCanceledException or JsonException)) return;
        logger.LogWarning(context.Exception, "RaceDay API request failed");
        string message = context.Exception is ApiException error ? error.Message : "Could not reach the RaceDay API. Start the API, check its database connection and confirm RaceDayApi:BaseUrl in appsettings.json.";
        var data = new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState) { ["Message"] = message, ["Title"] = "Request could not be completed" };
        context.Result = new ViewResult { ViewName = "~/Views/Shared/ApiError.cshtml", ViewData = data, StatusCode = context.Exception is ApiException e ? (int)e.Status : 503 };
        context.ExceptionHandled = true;
    }
}
