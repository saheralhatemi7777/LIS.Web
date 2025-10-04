using Microsoft.AspNetCore.Mvc;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class MainScreensController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
