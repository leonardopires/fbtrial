using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Journals.Web.Tests.Controllers
{
    public static class ControllerTestExtensions
    {

        public static void ValidateViewModel<TViewModel, TController>(this TController controller, TViewModel viewModelToValidate)
            where TController : Controller
        {
            var validationContext = new ValidationContext(viewModelToValidate, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(viewModelToValidate, validationContext, validationResults, true);
            foreach (var validationResult in validationResults)
            {
                controller.ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault() ?? string.Empty, validationResult.ErrorMessage);
            }
        }

        public static string Dump(this ModelStateDictionary modelState)
        {
            return string.Join("\n\n", modelState.Values.SelectMany(m => m.Errors).Select(e => $"Error: {e.ErrorMessage} - Exception: {e.Exception}"));
        }


    }
}