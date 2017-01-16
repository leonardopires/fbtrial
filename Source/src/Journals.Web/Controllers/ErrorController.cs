using System;
using Microsoft.AspNetCore.Mvc;

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
        ///   <see cref="ActionResult" />
        /// </returns>
        public ActionResult RequestLengthExceeded()
        {
            return BadRequest("Uploading file this large is not supported. Please try again.");
        }
    }
}