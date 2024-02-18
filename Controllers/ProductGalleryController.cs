using Microsoft.AspNetCore.Mvc;

namespace FirebaseLoginAuth.Controllers
{
    public class ProductGalleryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
