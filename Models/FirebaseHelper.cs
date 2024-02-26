using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using FirebaseLoginAuth.Models;
using Google.Cloud.Storage.V1;
using Humanizer.Localisation;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration; // Added this using statement
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FirebaseLoginAuth.Helpers // Adjusted the namespace
{
    public class FirebaseHelper
    {
        private static readonly IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        private static readonly FirebaseClient firebase = new FirebaseClient(configuration["Firebase:Url"],
                                                                             new FirebaseOptions
                                                                             {
                            
                                                                                 AuthTokenAsyncFactory = () => Task.FromResult(configuration["Firebase:Secret"])
                                                                             });


        public static async Task<bool> CreateUserData( string uid, string Usertype,string UserName)
        {
            try
            {
                await firebase.Child("users").Child(uid).PutAsync(new {  Usertype, UserName });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return false;
            }
        }

        public static async Task<string?> GetUserType(string uid)
        {
            try
            {
                var userSnapshot = await firebase.Child("users").Child(uid).OnceSingleAsync<Dictionary<string, object>>();
                if (userSnapshot != null)
                {
                    if (userSnapshot.TryGetValue("Usertype", out object? userType))
                    {
                        return userType?.ToString();
                    }
                }
                return null; // User type not found or user snapshot is null
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user type: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> RemoveFromCart(string userId, string bookId)
        {
            try
            {
                // Construct the path to the book in the user's cart
                string path = $"users/{userId}/Cart/{bookId}";

                // Remove the book from the user's cart
                await firebase.Child(path).DeleteAsync();

                return true; // Removal successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing item from cart: {ex.Message}");
                return false; // Removal failed
            }
        }
    
    public static async Task<bool> AddItemToCart(string userId, BookProduct book)
        {
            try
            {
                // First, retrieve the user's current cart
                var userCart = await firebase.Child("users").Child(userId).Child("cart").OnceSingleAsync<List<BookProduct>>();

                // If the user's cart is null, create a new list
                if (userCart == null)
                {
                    userCart = new List<BookProduct>();
                }

                // Add the new item to the cart
                userCart.Add(book);

                // Update the user's cart in the database
                await firebase.Child("users").Child(userId).Child("cart").PutAsync(userCart);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding item to cart: {ex.Message}");
                return false;
            }
        }
        public static async Task<List<BookProduct>> GetItemsFromCart(string userId)
        {
            try
            {
                // Retrieve the user's cart from the database
                var userCart = await firebase.Child("users").Child(userId).Child("cart").OnceSingleAsync<List<BookProduct>>();

                // If the user's cart is null, return an empty list
                if (userCart == null)
                {
                    return new List<BookProduct>();
                }

                return userCart;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving items from cart: {ex.Message}");
                return new List<BookProduct>(); // Return an empty list on error
            }
        }


        public static async Task<bool> CreateBookProductData(BookProduct bookProduct,string uid )
        {
            try
            {
                bookProduct.AdminId = uid;
                await firebase.Child("users").Child(uid).Child("Products").Child(bookProduct.BookId).PutAsync(bookProduct);
                await firebase.Child("Products").Child(bookProduct.BookId).PutAsync(bookProduct);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return false;
            }
        }
        public static async Task<BookProduct?> GetBookProductById( string productId)
        {
            try
            {
                var products = await firebase.Child("Products").OnceAsync<BookProduct>();

                var bookProduct = products.FirstOrDefault(p => p.Object.BookId == productId.ToString())?.Object;


                return bookProduct;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving book product by ID: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> UpdateBookProductAvailability(string adminId, string bookId, int newAvailability)
        {
            try
            {
                // Construct the path to the book product node in your database
                string path = $"users/{adminId}/Products/{bookId}";
                string path2 = $"Products/{bookId}";

                // Retrieve the existing book product from the database
                var existingBookProduct = await GetBookProductById( bookId);
                if (existingBookProduct != null)
                {
                    // Update the existing book product's NumberOfAvailability property
                    existingBookProduct.NumberOfAvailability += newAvailability;

                    // Save the updated book product back to the database
                    await firebase.Child(path).PutAsync(existingBookProduct);
                    await firebase.Child(path2).PutAsync(existingBookProduct);
                    return true; // Update successful
                }
                else
                {
                    return false; // Book product not found
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating book product availability: {ex.Message}");
                return false; // Update failed
            }
        }

        public static async Task<bool> UpdateBookProduct(BookProduct updatedBookProduct, IFormFile image, string atoken)
        {
            try
            {
                // Construct the path to the book product node in your database
                string path = $"users/{updatedBookProduct.AdminId}/Products/{updatedBookProduct.BookId}";

                string path2 = $"Products/{updatedBookProduct.BookId}";

                // Retrieve the existing book product from the database
                if (updatedBookProduct.AdminId != null && updatedBookProduct.BookId != null)
                {
                    var existingBookProduct = await GetBookProductById( updatedBookProduct.BookId);
                    if (existingBookProduct != null)
                    {
                        // Update the existing book product with the new data
                        existingBookProduct.Name = updatedBookProduct.Name;
                        existingBookProduct.Author = updatedBookProduct.Author;
                        existingBookProduct.Publisher = updatedBookProduct.Publisher;
                        existingBookProduct.ReleaseDate = updatedBookProduct.ReleaseDate;
                        existingBookProduct.NumberOfAvailability = updatedBookProduct.NumberOfAvailability;
                        existingBookProduct.Price = updatedBookProduct.Price;
                        existingBookProduct.ISBN = updatedBookProduct.ISBN;
                        existingBookProduct.Description = updatedBookProduct.Description;
                        existingBookProduct.Pages = updatedBookProduct.Pages;
                        existingBookProduct.Language = updatedBookProduct.Language;
                        existingBookProduct.Genre = updatedBookProduct.Genre;
                        existingBookProduct.IsBestseller = updatedBookProduct.IsBestseller;
                        existingBookProduct.Format = updatedBookProduct.Format;
                        existingBookProduct.Country = updatedBookProduct.Country;
                        existingBookProduct.Dimensions = updatedBookProduct.Dimensions;
                        existingBookProduct.Weight = updatedBookProduct.Weight;
                        existingBookProduct.Edition = updatedBookProduct.Edition;
                        existingBookProduct.IsOnSell = updatedBookProduct.IsOnSell;
                        existingBookProduct.AgeLimitation = updatedBookProduct.AgeLimitation;

                        if (image != null && !string.IsNullOrEmpty(atoken))
                        {
                            // Upload the new image
                            string? imageUrl = await UploadImage(image, atoken);
                            if (imageUrl != null) // Ensure imageUrl is not null before assignment
                            {
                                existingBookProduct.ImageUrl = imageUrl;
                            }
                        }

                        // Save the updated book product back to the database
                        await firebase.Child(path).PutAsync(existingBookProduct);
                        await firebase.Child(path2).PutAsync(existingBookProduct);
                        return true; // Update successful
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false; // Update failed
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating book product: {ex.Message}");
                return false; // Update failed
            }
        }

     


        public static async Task<bool> DeleteBookProduct(string adminId, string bookId)
        {
            try
            {
                // Construct the path to the book product node in your database
                string path = $"users/{adminId}/Products/{bookId}";

                string path2 = $"Products/{bookId}";
                // Remove the book product from the database
                await firebase.Child(path).DeleteAsync();
                await firebase.Child(path2).DeleteAsync();
                return true; // Deletion successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting book product: {ex.Message}");
                return false; // Deletion failed
            }
        }

        public static async Task<List<BookProduct>> GetAllBookedAdminBookProducts(string uid)
        {
            try
            {
                var flights = await firebase.Child("users").Child(uid).Child("Products").OnceAsync<BookProduct>();

                return flights.Select(f => f.Object).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving booked flights: {ex.Message}");
                return new List<BookProduct>();
            }
        }
        public static async Task<List<BookProduct>?> SearchBookProducts(string searchTerm)
        {
            try
            {
                var productsSnapshot = await firebase.Child("Products").OnceAsync<BookProduct>();
                List<BookProduct> searchResults = new List<BookProduct>();

                // Perform case-insensitive partial matching client-side
                foreach (var productSnapshot in productsSnapshot)
                {
                    BookProduct product = productSnapshot.Object;
                    if (product.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        searchResults.Add(product);
                    
                    }
                }

                return searchResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching book products: {ex.Message}");
                return null;
            }
        }

        public static async Task<List<BookProduct>> GetBestSellingBooksByGenre(string genre)
        {
            try
            {
                var productsSnapshot = await firebase.Child("Products").OnceAsync<BookProduct>();
                List<BookProduct> bestSellingBooksByGenre = new List<BookProduct>();

                foreach (var productSnapshot in productsSnapshot)
                {
                    BookProduct product = productSnapshot.Object;
                    if (product.Genre == genre && product.IsBestseller == true)
                    {
                        bestSellingBooksByGenre.Add(product);
                    }
                }

                return bestSellingBooksByGenre;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving best selling books by genre: {ex.Message}");
                return new List<BookProduct>();
            }
        }

        public static async Task<string?> UploadImage(IFormFile image,String atoken)
        {
            try
            {


              

                // Convert IFormFile to Stream
                using (var stream = image.OpenReadStream())
                {
                    // Construct FirebaseStorage, specify path and Put the file there
                    var task = new FirebaseStorage(
                       "studyproject-b90ae.appspot.com",
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(atoken),
                            ThrowOnCancel = true
                        })
                        .Child("data")
                        .Child("images")
                        .Child(image.FileName) // use file name as object in storage
                        .PutAsync(stream);

                    // Track progress of the upload
                    task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                    // await the task to wait until upload completes and get the download url
                    var downloadUrl = await task;

                    return downloadUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image: {ex.Message}");
                return null;
            }
        }
      
   



    }

    public class FirebaseUser
    {
        public string? Key { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    

}



public class SignInException : Exception
{
    public SignInException() { }

    public SignInException(string message) : base(message) { }

    public SignInException(string message, Exception innerException) : base(message, innerException) { }
}