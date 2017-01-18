using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Journals.Web.Controllers
{
    [FormatFilter]
    public class HomeController : NegotiableContentController
    {

        private readonly IHostingEnvironment environment;

        public HomeController(IHostingEnvironment environment)
        {
            this.environment = environment;
        }

        [Authorize]
        public IActionResult Index()
        {            
            return RedirectToAction("Index", "Subscriber");
        }

        public IActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public IActionResult Echo(string id, string format = "json")
        {
            if (environment.IsDevelopment())
            {
                return Result(format, id);
            }
            return NotFound();
        }

        
    }

}