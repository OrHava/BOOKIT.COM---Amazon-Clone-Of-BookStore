using Microsoft.AspNetCore.Mvc;
using FirebaseLoginAuth.Models;
using System.Collections.Generic;
using FirebaseLoginAuth.Helpers;
using Microsoft.AspNetCore.Http;

namespace FirebaseLoginAuth.Controllers
{
    public class UserController : Controller
    {
        public async Task<IActionResult> ViewCart(string bookId)
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");

            // Check if the user is authenticated
            if (string.IsNullOrEmpty(userAuthId))
            {
                // Redirect to the sign-in page if the user is not authenticated
                return RedirectToAction("SignIn", "Home");
            }

            var book = await FirebaseHelper.GetBookProductById(bookId);

            if (book != null)
            {
                // Add the book to the user's cart in Firebase
                await FirebaseHelper.AddItemToCart(userAuthId, book);
            }

            return RedirectToAction("Cart");
        }


        public async Task<IActionResult> Cart()
        {

            var userAuthId = HttpContext.Session.GetString("_UserId");
            if (userAuthId != null)
            {
                // Retrieve the user's cart items from Firebase
                var cartItems = await FirebaseHelper.GetItemsFromCart(userAuthId);
                return View(cartItems);

            }
            else {
                return View(null);
            }

         

        }
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(string bookId)
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");
            if (userAuthId != null)
            {
                // Remove the book from the user's cart in Firebase
                await FirebaseHelper.RemoveFromCart(userAuthId, bookId);
            }

            return RedirectToAction("Cart");
        }
    }
}
