using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace el7erafe.Web.Filters
{
    public class CustomValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var firstErrorMessage = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value?.Errors.Select(x => x.ErrorMessage) ?? Array.Empty<string>())
                    .FirstOrDefault() ?? "Validation error occurred";

                var response = new
                {
                    statusCode = 400,
                    errorMessage = firstErrorMessage
                };

                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}