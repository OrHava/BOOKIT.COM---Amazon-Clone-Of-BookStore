using FirebaseLoginAuth.Models;
using Microsoft.AspNetCore.Mvc;
using FirebaseLoginAuth.Helpers;

namespace FirebaseLoginAuth.Controllers
{
    public class ProductGalleryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ItemSalePage(string bookId)
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");

            if (!string.IsNullOrEmpty(userAuthId))
            {
                var bookProduct = await FirebaseHelper.GetBookProductById(userAuthId, bookId);
                if (bookProduct != null)
                {
                    return View(bookProduct);
                }
            }

            // If userAuthId is null or empty, or if bookProduct is null,
            // or if there's any other condition where you don't return a View,
            // you should return an appropriate action result, such as a redirect.

            return RedirectToAction("Index", "Home"); // Example redirect to Home/Index
        }

    }
}
