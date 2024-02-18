using Microsoft.AspNetCore.Mvc;

namespace FirebaseLoginAuth.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
