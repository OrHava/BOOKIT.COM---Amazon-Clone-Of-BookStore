
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using FirebaseLoginAuth.Models;

namespace FirebaseLoginAuth.Helpers // Adjusted the namespace
{
    public class FirebaseHelper
    {
        private static readonly IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        private static readonly FirebaseClient firebase = new(configuration["Firebase:Url"],
                                                                             new FirebaseOptions
                                                                             {
                            
                                                                                 AuthTokenAsyncFactory = () => Task.FromResult(configuration["Firebase:Secret"])
                                                                             });


        public static async Task<bool> CreateUserData( string uid, string userType,string UserName)
        {
            try
            {
                await firebase.Child("users").Child(uid).PutAsync(new {  userType, UserName });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return false;
            }
        }

        public static async Task<string?> GetCustomerNameById(string customerId)
        {
            try
            {
                var userSnapshot = await firebase.Child("users").Child(customerId).OnceSingleAsync<Dictionary<string, object>>();

                if (userSnapshot != null && userSnapshot.TryGetValue("UserName", out object? userName))
                {
                    return userName?.ToString();
                }
                else
                {
                    return null; 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving customer name by ID: {ex.Message}");
                return null;
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
                return null; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user type: {ex.Message}");
                return null;
            }
        }

        public static async Task<List<BookProduct>> GetBoughtItems(string userId)
        {
            try
            {
                // Path to the user's bought items
                string boughtItemsPath = $"users/{userId}/boughtBooks";

                // Retrieve the user's bought items from Firebase
                var boughtItems = await firebase.Child(boughtItemsPath).OnceSingleAsync<List<BookProduct>>();

                if (boughtItems == null)
                {
                    // If no bought items found, return an empty list
                    return new List<BookProduct>();
                }

                return boughtItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting bought items: {ex.Message}");
                throw; // Rethrow the exception for handling in the calling method
            }
        }

        public static async Task<bool> AddBoughtBook(string userId, BookProduct boughtBook)
        {
            try
            {
                // Path to the user's bought books list
                string boughtBooksPath = $"users/{userId}/boughtBooks";

                // Retrieve the user's existing bought books list
                var existingBoughtBooks = await firebase.Child(boughtBooksPath).OnceSingleAsync<List<BookProduct>>();

                existingBoughtBooks ??= new List<BookProduct>();

                // Add the new book to the existing bought books list
                existingBoughtBooks.Add(boughtBook);

                // Update the user's bought books list in the database
                await firebase.Child(boughtBooksPath).PutAsync(existingBoughtBooks);

                return true; // Update successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding bought book: {ex.Message}");
                return false; // Update failed
            }
        }

        public static async Task<bool> AddBoughtBooks(string userId, List<BookProduct> boughtBooks)
        {
            try
            {
                // Path to the user's bought books list
                string boughtBooksPath = $"users/{userId}/boughtBooks";

                // Retrieve the user's existing bought books list
                var existingBoughtBooks = await firebase.Child(boughtBooksPath).OnceSingleAsync<List<BookProduct>>();

                existingBoughtBooks ??= new List<BookProduct>();

                // Add the new books to the existing bought books list
                existingBoughtBooks.AddRange(boughtBooks);

                // Update the user's bought books list in the database
                await firebase.Child(boughtBooksPath).PutAsync(existingBoughtBooks);

                return true; // Update successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding bought books: {ex.Message}");
                return false; // Update failed
            }
        }


        public static async Task<bool> ClearUserCart(string userId)
        {
            try
            {
                // Path to the user's cart
                string cartPath = $"users/{userId}/cart";

                // Clear the user's cart by setting it to an empty list
                await firebase.Child(cartPath).PutAsync(new List<BookProduct>());

                return true; // Cart cleared successfully
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing user's cart: {ex.Message}");
                return false; // Clearing failed
            }
        }

        public static async Task<bool> RemoveFromCart(string userId, int index)
        {
            try
            {
                // Construct the path to the user's cart
                string cartPath = $"users/{userId}/cart";

                // Retrieve the user's current cart
                var userCart = await firebase.Child(cartPath).OnceSingleAsync<List<BookProduct>>();

                // If the user's cart is null or empty, return false
                if (userCart == null || userCart.Count == 0)
                {
                    Console.WriteLine("Cart is empty.");
                    return false;
                }

                // Check if the index is within the range of the cart
                if (index < 0 || index >= userCart.Count)
                {
                    Console.WriteLine("Invalid index.");
                    return false;
                }

                // Remove the item at the specified index
                userCart.RemoveAt(index);

                // Update the user's cart in the database
                await firebase.Child(cartPath).PutAsync(userCart);

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
                userCart ??= new List<BookProduct>();

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

        public static async Task<bool> AddItemToNotify(string userId, BookProduct book)
        {
            try
            {
                // First, retrieve the user's current notification list
                var userNotifications = await firebase.Child("users").Child(userId).Child("notify_books").OnceSingleAsync<List<BookProduct>>();

                // If the user's notifications list is null, create a new list
                userNotifications ??= new List<BookProduct>();

                // Check if the book already exists in the notification list
                if (userNotifications.Any(b => b.BookId == book.BookId))
                {
                    return false; // Book already exists, no need to add again
                }

                // Add the new item to the notification list
                userNotifications.Add(book);

                // Update the user's notification list in the database
                await firebase.Child("users").Child(userId).Child("notify_books").PutAsync(userNotifications);

                // Check if the book is available again (NumberOfAvailability > 0)
                if (book.NumberOfAvailability > 0)
                {
                    // Notify the user that the book is available again
                    Console.WriteLine($"Book with ID {book.BookId} is available again. You can purchase it now.");

                    // Remove the book from the notified list
                    userNotifications.Remove(book);

                    // Update the user's notification list in the database
                    await firebase.Child("users").Child(userId).Child("notify_books").PutAsync(userNotifications);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding item to notifications: {ex.Message}");
                return false;
            }
        }
     

        //create a list of all notified books in database
        public static async Task<List<BookProduct>> GetNotifiedBooks(string userId)
        {
            try
            {
                // Retrieve the user's notified books list from the database
                var userNotifications = await firebase.Child("users").Child(userId).Child("notify_books").OnceSingleAsync<List<BookProduct>>();

                // If the user's notifications list is null, return an empty list
                if (userNotifications == null)
                {
                    return new List<BookProduct>();
                }

                List<BookProduct> availableBooks = new();

                foreach (var notification in userNotifications)
                {
                    var bookId = notification.BookId;

                    if (bookId != null)
                    {
                        // Get the book product by its ID
                        var bookProduct = await GetBookProductById(bookId);

                        // If the book product is available, add it to the available books list
                        if (bookProduct != null && bookProduct.NumberOfAvailability > 0)
                        {
                            availableBooks.Add(bookProduct);
                        }
                    }
                }

                return availableBooks;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting notified books: {ex.Message}");
                return new List<BookProduct>();
            }
        }


        public static async Task<List<BookProduct>> GetAllNotifiedBooks(string userId)
        {
            try
            {
                var userNotifications = await firebase.Child("users").Child(userId).Child("notify_books").OnceSingleAsync<List<BookProduct>>();

                if (userNotifications == null)
                {
                    return new List<BookProduct>();
                }

                return userNotifications;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting available again books: {ex.Message}");
                return new List<BookProduct>();
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


        public static async Task<int> GetBookCartSizeByUserId(string userId)
        {
            try
            {
                var cartSnapshot = await firebase
                    .Child("users")
                    .Child(userId)
                    .Child("cart")
                    .OnceSingleAsync<List<BookProduct>>();

                // Check if cartSnapshot is not null and is a list
                if (cartSnapshot != null && cartSnapshot is List<BookProduct> cart)
                {
                    int cartSize = cart.Count;
                    Console.WriteLine($"Retrieved cart size for user {userId}: {cartSize}");
                    return cartSize;
                }
                else
                {
                    // Cart is empty or not found
                    Console.WriteLine($"No cart found for user {userId}");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving book cart size for user {userId}: {ex.Message}");
                return 0;
            }
        }


        public static async Task<bool> UpdateBookProductAvailabilityToBeLessOne(BookProduct book, String customerId)
        {
           
                try
                {
                    if (book != null && !string.IsNullOrEmpty(customerId) && book.BookId != null)
                    {
                        // Construct the paths to update the book product availability
                        string productsRootPath = $"Products/{book.BookId}";
                        string adminRootPath = $"users/{book.AdminId}/Products/{book.BookId}";
                        string adminBoughtBooksPath = $"users/{book.AdminId}/ProductsBought";

                        // Retrieve the existing book product from the database
                        var existingBookProduct = await GetBookProductById(book.BookId);
                        if (existingBookProduct != null)
                        {
                            // Update the existing book product's NumberOfAvailability property
                            existingBookProduct.NumberOfAvailability -= 1; // Decrease availability by 1
                                                                           // Increment sold count or set to 1 if not exist
                        existingBookProduct.SoldBooks = existingBookProduct.SoldBooks ?? 0;
                        existingBookProduct.SoldBooks += 1;
                        // Save the updated book product back to the database in the products root
                        await firebase.Child(productsRootPath).PutAsync(existingBookProduct);

                            // Save the updated book product back to the database in the admin's root
                            await firebase.Child(adminRootPath).PutAsync(existingBookProduct);

                            // Create a new association between the book and the customer
                            var association = new BookCustomerAssociation
                            {
                                BookId = book.BookId,
                                CustomerId = customerId
                            };

                            // Save the association to the ProductsBought node under the admin's root
                            await firebase.Child(adminBoughtBooksPath).PostAsync(association);

                            return true; // Update successful
                        }
                        else
                        {
                            return false; // Book product not found
                        }
                    }
                    else
                    {
                        return false; // Book object or customer ID is null or empty
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating book product availability: {ex.Message}");
                    return false; // Update failed
                }
            }




        public static async Task<List<BookCustomerAssociation>> GetAdminBoughtBooks(string adminId)
        {
            try
            {
                var adminBoughtBooksSnapshot = await firebase
                    .Child("users")
                    .Child(adminId)
                    .Child("ProductsBought")
                    .OnceAsync<BookCustomerAssociation>();

                var adminBoughtBooksTasks = adminBoughtBooksSnapshot
                    .Select(async item =>
                    {
                        var association = item.Object;

                        if (association.BookId != null && association.CustomerId != null)
                        {
                            // Fetch book and customer information concurrently
                            var bookTask = GetBookProductById(association.BookId);
                            var customerNameTask = GetCustomerNameById(association.CustomerId);

                            // Await both tasks simultaneously
                            await Task.WhenAll(bookTask, customerNameTask);

                            // Populate association with book and customer information
                            if (bookTask.Result != null)
                                association.BookName = bookTask.Result.Name;

                            association.CustomerName = customerNameTask.Result;

                            return association;
                        }

                        return null;
                    });

                // Await all tasks and filter out null results
                var adminBoughtBooksArray = await Task.WhenAll(adminBoughtBooksTasks);

                // Filter out null elements and convert to a list of non-nullable type
                var adminBoughtBooks = adminBoughtBooksArray
                    .Where(x => x != null) // Filter out null elements
                    .Select(x => x!) // Convert nullable elements to non-nullable using the ! operator
                    .ToList();

                return adminBoughtBooks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving bought books for admin {adminId}: {ex.Message}");
                return new List<BookCustomerAssociation>(); // Return an empty list on error
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
                        existingBookProduct.OldPrice = updatedBookProduct.OldPrice;
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
        public static async Task<List<BookProduct>> GetAllProducts()
        {
            try
            {
                var productsSnapshot = await firebase.Child("Products").OnceAsync<BookProduct>();
                List<BookProduct> allProducts = new();

                foreach (var productSnapshot in productsSnapshot)
                {
                    allProducts.Add(productSnapshot.Object);
                }

                return allProducts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all products: {ex.Message}");
                return new List<BookProduct>();
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

        public static async Task<List<BookProduct>> GetBestSellingBooksByGenre(string genre)
        {
            try
            {
                var productsSnapshot = await firebase.Child("Products").OnceAsync<BookProduct>();
                List<BookProduct> bestSellingBooksByGenre = new();

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
        public static async Task<List<BookProduct>?> SearchBookProducts(string searchTerm)
        {
            try
            {
                var productsSnapshot = await firebase.Child("Products").OnceAsync<BookProduct>();
                List<BookProduct> searchResults = new();

                if (string.IsNullOrEmpty(searchTerm)){
                    foreach (var productSnapshot in productsSnapshot)
                    {
                        BookProduct product = productSnapshot.Object;
                  
                        
                            searchResults.Add(product);

                        
                    }
                    return searchResults;

                }
                // Perform case-insensitive partial matching client-side
                foreach (var productSnapshot in productsSnapshot)
                {
                    BookProduct product = productSnapshot.Object;
                    if (product.Name != null) {
                        if (product.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase ) || product.BookId == searchTerm)
                        {
                            searchResults.Add(product);

                        }
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

     
        public static async Task<List<BookProduct>> ApplyFilters(string category, string sortBy, string releaseDate, int ageLimit, int minpriceRange,int maxpriceRange, string format,string searchQuery, bool onSale)
        {
            try
            {
                // Get all products initially
                var allProducts = await GetAllProducts();

                // Apply filters based on the criteria
                var filteredProducts = allProducts;

                Console.WriteLine($"filteredProducts: ");
                foreach (var book in filteredProducts)
                {
                    Console.WriteLine($"Book Title: {book.Name}");
                    Console.WriteLine($"Author: {book.Author}");
                    Console.WriteLine($"ISBN: {book.ISBN}");
                    // Add other properties as needed
                    Console.WriteLine(); // Add a blank line for separation
                }

                // Apply filters

                // Filter by category

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    filteredProducts = filteredProducts.Where(p => p.Name == searchQuery).ToList();


                }

  

                Console.WriteLine($"After filteredProducts: ");
                foreach (var book in filteredProducts)
                {
                    Console.WriteLine($"Book Title: {book.Name}");
                    Console.WriteLine($"Author: {book.Author}");
                    Console.WriteLine($"ISBN: {book.ISBN}");
                    // Add other properties as needed
                    Console.WriteLine(); // Add a blank line for separation
                }

                if (!string.IsNullOrEmpty(category) && category != "All")
                {
                    filteredProducts = filteredProducts.Where(p => p.Genre == category).ToList();
                }

                if((minpriceRange <maxpriceRange) || (minpriceRange!=0 && maxpriceRange!=0))
                {
                    filteredProducts = filteredProducts.Where(p => p.Price>=minpriceRange && p.Price<=maxpriceRange).ToList();

                }

                if (sortBy == "price-increase")
                {
                    filteredProducts = filteredProducts.OrderBy(p => p.Price).ToList();
                }
                else if (sortBy == "price-decrease")
                {
                    filteredProducts = filteredProducts.OrderByDescending(p => p.Price).ToList();
                }

             
                else if (sortBy == "most-popular")
                {
                    filteredProducts = filteredProducts.OrderByDescending(p => p.SoldBooks).ToList();
                }
                if (sortBy == "category")
                {
                    filteredProducts = filteredProducts.OrderBy(p => p.Genre).ToList();
                }


                if (!string.IsNullOrEmpty(releaseDate))
                {
                    var parsedReleaseDate = DateTime.Parse(releaseDate);
                    filteredProducts = filteredProducts.Where(p => p.ReleaseDate == parsedReleaseDate.Date).ToList();
                }
              
                if (ageLimit != 0) {

                    filteredProducts = filteredProducts.Where(p => p.AgeLimitation <= ageLimit).ToList();

                }
                if (format != null)
                {
                    filteredProducts = filteredProducts.Where(p => p.Format == format).ToList();

                }

                if (onSale)
                {
                    filteredProducts = filteredProducts.Where(p => p.IsOnSell == true).ToList();
                }
                else if (!onSale)
                {
                    filteredProducts = filteredProducts.Where(p => p.IsOnSell == false).ToList();

                }


                // Return the filtered products
                return filteredProducts;
            }
            catch (Exception)
            {
                // Handle exceptions appropriately
                return new List<BookProduct>();
            }
        }



        public static async Task<string?> UploadImage(IFormFile image,String atoken)
        {
            try
            {




                // Convert IFormFile to Stream
                using var stream = image.OpenReadStream();
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