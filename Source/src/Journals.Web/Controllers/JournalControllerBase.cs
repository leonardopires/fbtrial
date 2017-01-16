using Journals.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Journals.Web.Controllers
{
    public class JournalControllerBase : Controller
    {
        protected IActionResult Result(string format, object model = null, int statusCode = 200)
        {
            if (string.IsNullOrEmpty(format))
            {
                format = "html";
            }

            switch (format)
            {
                case "html":
                    var viewResult = model as ViewResult ?? View(model);
                    return viewResult.WithStatusCode(statusCode);
                case "json":
                    return Json(model).WithStatusCode(statusCode);
                case "text":
                    return Content(model?.ToString()).WithStatusCode(statusCode);
                default:
                    return StatusCode(statusCode, model);

            }
        }
    }
}