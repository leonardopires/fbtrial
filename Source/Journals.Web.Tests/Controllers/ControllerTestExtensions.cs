using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;

namespace Journals.Web.Tests.Controllers
{
    public static class ControllerTestExtensions
    {

        /// <summary>
        /// Forces the validation of the specified model object by the controller.
        /// </summary>
        /// <typeparam name="TModel">The type of the model object.</typeparam>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="controller">The instance of the controller.</param>
        /// <param name="modelToValidate">The model to be validated.</param>
        /// <remarks>
        /// This method will validate any model object using validation attributes based on <see cref="System.ComponentModel.DataAnnotations"/>.
        /// </remarks>
        public static void ValidateModel<TModel, TController>(this TController controller, TModel modelToValidate)
            where TController : Controller
        {
            var validationContext = new ValidationContext(modelToValidate, null, null);
            var validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(modelToValidate, validationContext, validationResults, true);

            foreach (var validationResult in validationResults)
            {
                controller.ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault() ?? string.Empty, validationResult.ErrorMessage);
            }
        }

        /// <summary>
        /// Dumps the errors and exceptions from the specified model state into a string.
        /// </summary>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/> that is going to be dumped.</param>
        /// <returns>A <see cref="string"/> with the errors and exceptions from the  Model State</returns>
        public static string Dump(this ModelStateDictionary modelState)
        {
            return string.Join("\n\n", modelState.Values.SelectMany(m => m.Errors).Select(e => $"Error: {e.ErrorMessage} - Exception: {e.Exception}"));
        }

        /// <summary>
        /// Invokes the specified action.
        /// </summary>
        /// <typeparam name="TResult">The type of the action result.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        ///   <see cref="TResult" />
        /// </returns>
        public static TResult InvokeAction<TResult>(this ControllerBase controller, string actionName, params object[] arguments)
                    where TResult : ActionResult
        {
            return controller.GetType().GetMethod(actionName)?.Invoke(controller, null)?.As<TResult>();
        }
    }
}