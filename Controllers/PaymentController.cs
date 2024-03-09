

using FirebaseLoginAuth.Helpers;
using FirebaseLoginAuth.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirebaseLoginAuth.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index(decimal totalPrice, string bookId)
        {
            ViewBag.TotalPrice = totalPrice;
            ViewBag.BookId = bookId;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ProcessPayment(decimal totalPrice, string bookId)
        {
            try
            {
                // Perform payment processing here, assuming it's successful for demonstration purposes
                // You may integrate with a payment gateway or service
             
                // Once payment is successful, add the bought book(s) to the user's list of bought books
                var userAuthId = HttpContext.Session.GetString("_UserId");
                if (userAuthId != null)
                {
                    if (bookId!="Cart") {
                        var book = await FirebaseHelper.GetBookProductById(bookId); // Get book by ID

                        if (book != null)
                        {
                            // Add the bought book to the user's bought books
                            await FirebaseHelper.AddBoughtBook(userAuthId, book);

                            // Update availability for the bought book in the products root
                            await FirebaseHelper.UpdateBookProductAvailabilityToBeLessOne(book, userAuthId); // Decrease availability by 1
                        }

                    }


                    else
                    {
                        var cartItems = await FirebaseHelper.GetItemsFromCart(userAuthId);

                        // Add the bought books to the user's list of bought books
                        await FirebaseHelper.AddBoughtBooks(userAuthId, cartItems);

                        // Update availability for each bought book in the products root
                        foreach (var item in cartItems)
                        {
                            await FirebaseHelper.UpdateBookProductAvailabilityToBeLessOne(item, userAuthId); // Decrease availability by 1
                        }

                        // Clear the user's cart after successful purchase
                        await FirebaseHelper.ClearUserCart(userAuthId);
                    }

                    // Return a success response to the client
                    return Ok(new { success = true, message = "Payment processed successfully." });
                }
                else
                {
                    // Handle the case where the user is not authenticated
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                // Handle payment failure or other errors
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while processing your payment. Please try again later." + ex });
            }
        }

    }
}
