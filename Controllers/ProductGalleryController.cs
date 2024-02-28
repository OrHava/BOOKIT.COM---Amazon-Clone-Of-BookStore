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
     

           
                var bookProduct = await FirebaseHelper.GetBookProductById( bookId);
                if (bookProduct != null)
                {
                    return View(bookProduct);
                }
         

            // If userAuthId is null or empty, or if bookProduct is null,
            // or if there's any other condition where you don't return a View,
            // you should return an appropriate action result, such as a redirect.

            return RedirectToAction("Index", "Home"); // Example redirect to Home/Index
        }

        public async Task<IActionResult> SearchPage(string bookId)
        {



            var bookProduct = await FirebaseHelper.GetBookProductById(bookId);
            if (bookProduct != null)
            {
                return View(bookProduct);
            }


            // If userAuthId is null or empty, or if bookProduct is null,
            // or if there's any other condition where you don't return a View,
            // you should return an appropriate action result, such as a redirect.

            return RedirectToAction("Index", "Home"); // Example redirect to Home/Index
        }
        [HttpPost]
        public async Task<IActionResult> SearchBooks(string searchInput)
        {

            // Perform the search based on the searchInput
            var searchResults = await FirebaseHelper.SearchBookProducts(searchInput);

            // Pass the search results to the SearchPage view
            var viewModel = new HomeViewModel
            {
                SearchResults = searchResults
            };

            return View("SearchPage", viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> SearchBooksByGenre(string searchInput)
        {

            Console.WriteLine($"Search input: {searchInput}");
            if (searchInput== "Fantasy") {
                var bestSellersFantasy = await FirebaseHelper.GetBestSellingBooksByGenre("Fantasy");
                // Pass the search results to the SearchPage view
                var viewModel = new HomeViewModel
                {
                    SearchResults = bestSellersFantasy
                };
                Console.WriteLine($"Search input: Fantasy");

                return View("SearchPage", viewModel);
            }
            if (searchInput == "ScienceFiction")
            {
                var bestSellersScienceFiction = await FirebaseHelper.GetBestSellingBooksByGenre("Science Fiction");
                // Pass the search results to the SearchPage view
                var viewModel = new HomeViewModel
                {
                    SearchResults = bestSellersScienceFiction
                };
                Console.WriteLine($"Search input: Science Fiction");
                return View("SearchPage", viewModel);
            }
            else {
                // Perform the search based on the searchInput
                var searchResults = await FirebaseHelper.SearchBookProducts(searchInput);
                // Pass the search results to the SearchPage view
                var viewModel = new HomeViewModel
                {
                    SearchResults = searchResults
                };
                Console.WriteLine($"Search input: nothing");

                return View("SearchPage", viewModel);
            }


          

        
        }
        public async Task<IActionResult> ApplyFilters(string category, string sortBy, string releaseDate, string ageLimit, string priceRange, string format, bool onSale)
        {
            try
            {
                
                var filteredProducts = await FirebaseHelper.ApplyFilters(category, sortBy, releaseDate, ageLimit, priceRange, format, onSale);
          
                var viewModel = new HomeViewModel
                {
                    SearchResults = filteredProducts
                };

                return View("SearchPage", viewModel);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }




    }


}
