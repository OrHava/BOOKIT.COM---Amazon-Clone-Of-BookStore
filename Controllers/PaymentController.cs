

using Microsoft.AspNetCore.Mvc;

namespace FirebaseLoginAuth.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index(decimal totalPrice)
        {
            ViewBag.TotalPrice = totalPrice;
            return View();
        }
    }
}
