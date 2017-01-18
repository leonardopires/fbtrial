using System.Net;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Journals.Web.Helpers
{
    public static class HelperExtensions
    {
        public static TResult WithStatusCode<TResult>(this TResult result, HttpStatusCode statusCode)
            where TResult : IActionResult
        {
            return WithStatusCode(result, (int)statusCode);
        }

        public static TResult WithStatusCode<TResult>(this TResult result, int statusCode)
            where TResult : IActionResult
        {
            var jsonResult = result as JsonResult;

            if (jsonResult != null)
            {
                jsonResult.StatusCode = statusCode;
            }

            var viewResult = result as ViewResult;

            if (viewResult != null)
            {
                viewResult.StatusCode = statusCode;
            }

            var contentResult = result as ContentResult;

            if (contentResult != null)
            {
                contentResult.StatusCode = statusCode;
            }

            var objectResult = result as ObjectResult;

            if (objectResult != null)
            {
                objectResult.StatusCode = statusCode;
            }
            return result;
        }

    }
}