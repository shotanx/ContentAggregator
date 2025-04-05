using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ContentAggregator.API.Services
{
    public class ValidateModelFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Message = "Invalid input data",
                    Details = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                };
                context.Result = new BadRequestObjectResult(errorResponse);
            }
        }
    }

    public class ErrorResponse
    {
        public string? Message { get; set; }
        public List<string> Details { get; set; } = new List<string>();
    }
}
