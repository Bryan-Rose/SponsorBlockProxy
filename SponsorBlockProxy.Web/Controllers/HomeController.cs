using Microsoft.AspNetCore.Mvc;

namespace SponsorBlockProxy.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
