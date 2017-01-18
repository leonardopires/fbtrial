using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Journals.Web.Filters
{
    public class ValidateOnlyIncomingValuesAttribute : ActionFilterAttribute
    {
        public override async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = (ControllerBase)filterContext.Controller;
            var modelState = filterContext.ModelState;

            var valueProvider = await CompositeValueProvider.CreateAsync(controller.ControllerContext);

            var keysWithNoIncomingValue = modelState.Keys.Where(x => !valueProvider.ContainsPrefix(x));

            foreach (var key in keysWithNoIncomingValue)
            {
                modelState[key].Errors.Clear();
            }
        }
    }
}