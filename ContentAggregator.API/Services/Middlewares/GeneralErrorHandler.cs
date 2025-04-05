using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContentAggregator.API.Services.Middlewares
{
    public class GeneralErrorHandler(IHostEnvironment _env, ILogger<GeneralErrorHandler> _logger)
    : IExceptionHandler
    {
        private const string UnhandledExceptionMsg = "An unhandled exception occurred while executing the request.";

        private static readonly JsonSerializerOptions SerializerOptions = new (JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, exception.Message);

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/problem+json";

            var problemDetails = CreateProblemDetails(httpContext, exception);
            var problemDetailsJson = ToJson(problemDetails);
            await httpContext.Response.WriteAsync(problemDetailsJson);

            return true;
        }

        private ProblemDetails CreateProblemDetails(in HttpContext httpContext, in Exception exception)
        {
            var statusCode = httpContext.Response.StatusCode;
            var reasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode);
            if (string.IsNullOrEmpty(reasonPhrase))
            {
                reasonPhrase = UnhandledExceptionMsg;
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = reasonPhrase,
                Instance = httpContext.Request.Path,
                Extensions = {
                    ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier
                }
            };

            if (!_env.IsDevOrQA())
            {
                return problemDetails;
            }

            problemDetails.Detail = exception.ToString();
            problemDetails.Extensions["traceId"] = Activity.Current?.Id;
            problemDetails.Extensions["traceId2"] = httpContext.TraceIdentifier;

            return problemDetails;
        }

        private string ToJson(in ProblemDetails problemDetails)
        {
            try
            {
                return JsonSerializer.Serialize(problemDetails, SerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while serializing error to JSON");
            }

            return string.Empty;
        }
    }
}
