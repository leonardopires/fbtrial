using System;
using System.Web.Mvc;

namespace Journals.Web.Controllers
{
    /// <summary>
    /// Implements a controller that displays error messages.
    /// </summary>
    public class ErrorController : Controller
    {
        /// <summary>
        /// Display an error message when the upload of a large file is not supported.
        /// </summary>
        /// <returns>
        ///   <see cref="System.Web.Mvc.ActionResult" />
        /// </returns>
        public ActionResult RequestLengthExceeded()
        {
            return View(new HandleErrorInfo(new Exception("Uploading file this large is not supported. Please try again."), "Error", "RequestLengthExceeded"));
        }
    }
}