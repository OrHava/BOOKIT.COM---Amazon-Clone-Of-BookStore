using Microsoft.AspNetCore.Mvc;
using FirebaseLoginAuth.Models;
using System.Collections.Generic;
using FirebaseLoginAuth.Helpers;
using Microsoft.AspNetCore.Http;
using Firebase.Auth;

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
                // Generate a random ID
                userAuthId = Guid.NewGuid().ToString(); // Use Guid to generate a unique ID

                // Store the random ID in the session
                HttpContext.Session.SetString("_UserId", userAuthId);
            }

            var book = await FirebaseHelper.GetBookProductById(bookId);
            

            if (book != null)
            {
                book.OrderBooks = 1;
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
        [HttpPost]
        public async Task<IActionResult> UpdateCart(Dictionary<int, int> quantities)
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");

            // Get the cart count
            var cartCount = await GetCartCount();

            // Pass the cart count to the view
            ViewData["CartCount"] = cartCount;

            if (userAuthId != null)
            {
                foreach (var entry in quantities)
                {
                    int index = entry.Key;
                    int quantity = entry.Value;

                    // Update the quantity of the book in the user's cart
                    await FirebaseHelper.UpdateCartQuantity(userAuthId, index, quantity);
                }

                return RedirectToAction("Cart");
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
            Console.WriteLine($"cart index: {index}");
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
            return RedirectToAction("ItemSalePage", "ProductGallery", new { bookId });
        }

        public async Task<IActionResult> BoughtItems()
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");

            // Check if the user is authenticated
            if (string.IsNullOrEmpty(userAuthId))
            {
                // Redirect to the sign-in page if the user is not authenticated
                return RedirectToAction("SignIn", "Home");
            }

            try
            {
                // Retrieve the user's bought items from Firebase
                var boughtItems = await FirebaseHelper.GetBoughtItems(userAuthId);

                return View(boughtItems);
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error retrieving bought items: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }
        public async Task<IActionResult> DisplayNotifiedBooks()
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");
            // Check if the user is authenticated
            if (string.IsNullOrEmpty(userAuthId))
            {
                // Redirect to the sign-in page if the user is not authenticated
                return RedirectToAction("SignIn", "Home");
            }

            try
            {
                // Retrieve the user's bought items from Firebase
                // Get all notified books
                var allNotifiedBooks = await FirebaseHelper.GetAllNotifiedBooks(userAuthId);

                // Get notified books with availability greater than 0
                var availableNotifiedBooks = await FirebaseHelper.GetNotifiedBooks(userAuthId);

                // Pass both lists to the view
                ViewData["AllNotifiedBooks"] = allNotifiedBooks;
                ViewData["AvailableNotifiedBooks"] = availableNotifiedBooks;

                return View();

            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error retrieving bought items: {ex.Message}");
                return RedirectToAction("SignIn", "Home");
            }
           
        }



    }
}


