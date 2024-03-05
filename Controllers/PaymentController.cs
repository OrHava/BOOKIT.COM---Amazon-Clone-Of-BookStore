

using FirebaseLoginAuth.Helpers;
using Microsoft.AspNetCore.Mvc;
using FirebaseLoginAuth.Helpers;

namespace FirebaseLoginAuth.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index(decimal totalPrice)
        {
            ViewBag.TotalPrice = totalPrice;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(decimal totalPrice)
        {
            try
            {
                // Perform payment processing here, assuming it's successful for demonstration purposes
                // You may integrate with a payment gateway or service

                // Once payment is successful, get the user's cart items and add them to the list of bought books
                var userAuthId = HttpContext.Session.GetString("_UserId");
                if (userAuthId != null)
                {
                    var cartItems = await FirebaseHelper.GetItemsFromCart(userAuthId);

                    // Add the bought books to a new list
                    await FirebaseHelper.AddBoughtBooks(userAuthId, cartItems);

                    // Clear the user's cart after successful purchase
                    await FirebaseHelper.ClearUserCart(userAuthId);

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
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "An error occurred while processing your payment. Please try again later." });
            }
        }

    }
}
