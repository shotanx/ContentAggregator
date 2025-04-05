using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ContentAggregator.API.Services
{
    public class OperationCanceledExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<OperationCanceledExceptionFilter> _logger;

        public OperationCanceledExceptionFilter(ILogger<OperationCanceledExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is OperationCanceledException)
            {
                _logger.LogInformation(context.Exception, context.Exception.Message);
                var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                var controllerName = context.ActionDescriptor.RouteValues["controller"];
                var httpMethod = context.HttpContext.Request.Method;

                var response = new
                {
                    Message = "The request was cancelled by the client.",
                    Status = 499,
                    Controller = controllerName,
                    HttpMethod = httpMethod,
                    TraceId = traceId
                };

                context.Result = new JsonResult(response)
                {
                    StatusCode = 499
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
