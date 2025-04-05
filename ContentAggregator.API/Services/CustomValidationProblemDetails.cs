using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ContentAggregator.API.Services
{
    public class CustomValidationProblemDetails : ProblemDetails
    {
        public CustomValidationProblemDetails(ActionContext context, IHostEnvironment env)
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1";
            Title = "Validation of model properties has Failed. Make sure that you have provided all the required fields.";
            Status = StatusCodes.Status400BadRequest;
            Instance = context.HttpContext.Request.Path;

            if (env.IsDevOrQA())
            {
                var errors = context.ModelState
                    .Where(e => e.Value!.Errors.Count > 0)
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray()
                    );

                Extensions.Add("errors", errors);
            }

            Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        }
    }
}