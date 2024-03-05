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

            // Get the cart count
            var cartCount = await GetCartCount();

            // Pass the cart count to the view
            ViewData["CartCount"] = cartCount;
            return RedirectToAction("Cart");
        }

        public async Task<int> GetCartCount()
        {
            try
            {
                if (HttpContext != null && !string.IsNullOrEmpty(HttpContext.Session.GetString("_UserId")))
                {
                    var userId = HttpContext.Session.GetString("_UserId");
                    if (userId != null) // Add null check here
                    {
                        var cartSize = await FirebaseHelper.GetBookCartSizeByUserId(userId);
                        return cartSize;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cart count: {ex.Message}");
                return 0;
            }
        }

        public async Task<IActionResult> Cart()
        {

            var userAuthId = HttpContext.Session.GetString("_UserId");
            if (userAuthId != null)
            {
                // Retrieve the user's cart items from Firebase
                var cartItems = await FirebaseHelper.GetItemsFromCart(userAuthId);
                // Get the cart count
                var cartCount = await GetCartCount();

                // Pass the cart count to the view
                ViewData["CartCount"] = cartCount;
                return View(cartItems);

            }
            else {
                return View(null);
            }

         

        }
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int index)
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");
            // Get the cart count
            var cartCount = await GetCartCount();

            // Pass the cart count to the view
            ViewData["CartCount"] = cartCount;
            if (userAuthId != null)
            {
                // Remove the book from the user's cart in Firebase
                await FirebaseHelper.RemoveFromCart(userAuthId, index);
                var cartItems = await FirebaseHelper.GetItemsFromCart(userAuthId);
                return RedirectToAction("Cart", cartItems);
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> NotifyWhenAvailable(string bookId)
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");

            if (string.IsNullOrEmpty(userAuthId))
            {
                // Redirect to the sign-in page if the user is not authenticated
                return RedirectToAction("SignIn", "Home");
            }

            var book = await FirebaseHelper.GetBookProductById(bookId);

            if (book != null)
            {
                // Attempt to add the book to the user's notification list in Firebase
                bool addedToNotifications = await FirebaseHelper.AddItemToNotify(userAuthId, book);

                if (addedToNotifications)
                {
                    TempData["NotificationMessage"] = "You will be notified when the product is available.";
                }
                else
                {
                    TempData["NotificationMessage"] = "Book is already added to notifications.";
                }
            }
            else
            {
                TempData["NotificationMessage"] = "Failed to add to notifications.";
            }

            // Redirect back to the ItemSalePage action of the ProductGalleryController
            return RedirectToAction("ItemSalePage", "ProductGallery", new { bookId = bookId });
        }




    }
}
