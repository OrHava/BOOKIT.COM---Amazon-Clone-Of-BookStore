﻿using Firebase.Auth;
using FirebaseLoginAuth.Helpers;
using FirebaseLoginAuth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;

namespace FirebaseLoginAuth.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        readonly FirebaseAuthProvider auth;
        public HomeController(ILogger<HomeController> logger)
        {
            auth = new FirebaseAuthProvider(
                            new FirebaseConfig("AIzaSyBCGbr6Ia4YcSY7Mf931F3OK1qRRY8z0nc"));
            _logger = logger;
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



        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new HomeViewModel();

                // Concurrently fetch best-selling books for different genres
                var tasks = new List<Task<List<BookProduct>>>
        {
            FirebaseHelper.GetBestSellingBooksByGenre("Fantasy"),
            FirebaseHelper.GetBestSellingBooksByGenre("Science Fiction"),
            FirebaseHelper.GetBestSellingBooksByGenre("Mystery"),
            FirebaseHelper.GetBestSellingBooksByGenre("Thriller"),
            FirebaseHelper.GetBestSellingBooksByGenre("Romance"),
            FirebaseHelper.GetBestSellingBooksByGenre("Historical Fiction")
        };

                // Await all tasks simultaneously
                await Task.WhenAll(tasks);

                // Assign results to the view model
                viewModel.BestSellersFantasy = tasks[0].Result;
                viewModel.BestSellersScienceFiction = tasks[1].Result;
                viewModel.BestSellersMystery = tasks[2].Result;
                viewModel.BestSellersThriller = tasks[3].Result;
                viewModel.BestSellersRomance = tasks[4].Result;
                viewModel.BestSellersHistoricalFiction = tasks[5].Result;

                // Get the cart count
                var cartCount = await GetCartCount();

                // Pass the cart count to the view
                ViewData["CartCount"] = cartCount;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving best-selling books: {ex.Message}");
                return View(new HomeViewModel()); // Return an empty view model on error
            }
        }



        public async Task<IActionResult> SearchBooks(string searchInput)
        {
    

          
                // Perform the search in FirebaseHelper based on the search input
                var searchResults = await FirebaseHelper.SearchBookProducts(searchInput);
                // Get best selling books for fantasy and science fiction
                var bestSellersFantasy = await FirebaseHelper.GetBestSellingBooksByGenre("Fantasy");
                var bestSellersScienceFiction = await FirebaseHelper.GetBestSellingBooksByGenre("Science Fiction");

            var bestSellersMystery = await FirebaseHelper.GetBestSellingBooksByGenre("Mystery");
            var bestSellersThriller = await FirebaseHelper.GetBestSellingBooksByGenre("Thriller");
            var bestSellersRomance = await FirebaseHelper.GetBestSellingBooksByGenre("Romance");
            var bestSellersHistoricalFiction = await FirebaseHelper.GetBestSellingBooksByGenre("Historical Fiction");

      

        // Pass the search results to the view
        var viewModel = new HomeViewModel
            {
                SearchResults = searchResults,
                BestSellersFantasy = bestSellersFantasy,
                    BestSellersScienceFiction = bestSellersScienceFiction,
                    BestSellersMystery = bestSellersMystery,
            BestSellersThriller= bestSellersThriller,
            BestSellersRomance = bestSellersRomance,
            BestSellersHistoricalFiction = bestSellersHistoricalFiction

        };
            // Get the cart count
            var cartCount = await GetCartCount();

            // Pass the cart count to the view
            ViewData["CartCount"] = cartCount;
            return View("Index", viewModel);
         
           
        }




        public async Task<IActionResult> OnPost(DataUserModel dataUserModel)
        {
            // Get user's authentication ID (UID) from Firebase
            var userAuthId = HttpContext.Session.GetString("_UserId");

            if (userAuthId != null && dataUserModel.UserType != null && dataUserModel.UserName != null)
            {
                // Create user data
                var result = await FirebaseHelper.CreateUserData(userAuthId, dataUserModel.UserType, dataUserModel.UserName);

                if (result)
                {
                    TempData["SuccessMessage"] = "User information added successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add user information.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "User authentication ID not found.";
            }
            // Get the cart count
            var cartCount = await GetCartCount();

            // Pass the cart count to the view
            ViewData["CartCount"] = cartCount;
            return RedirectToAction("Index");
        }

      



        public IActionResult LogOut()
        {
            // Remove the user token from the session
            HttpContext.Session.Remove("_UserToken");
            HttpContext.Session.Clear();
            // Remove the Remember Me cookies
            Response.Cookies.Delete("RememberMe");
            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("UserPassword");
            return RedirectToAction("SignIn");
        }
        public IActionResult SignIn()
        {
      
            return View();
        }

        public IActionResult GoogleSignIn()
        {
            // Redirect to the Google sign-in page provided by Firebase
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "Google");
        }


        [HttpPost]
        public async Task<IActionResult> SignIn(LoginModel loginModel , bool rememberMe)
        {
            try
            {

                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    ModelState.AddModelError(string.Empty, "No internet connection available.");
                    return View(loginModel);
                }

                //log in an existing user
                var fbAuthLink = await auth
                                .SignInWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);
                string token = fbAuthLink.FirebaseToken;
                // Retrieve user's claims
                var user = await auth.GetUserAsync(fbAuthLink.FirebaseToken);
                string userId = user.LocalId; // Assuming LocalId is the user ID
                if (user.IsEmailVerified)
                {   // Save the token and user ID to session variables
                    if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(loginModel.Email))
                    {
                        HttpContext.Session.SetString("_UserToken", token);
                        HttpContext.Session.SetString("_UserId", userId); // Store the user ID in session
                     
                        var userType = await FirebaseHelper.GetUserType(userId);
                      if (userType != null) { 
                            HttpContext.Session.SetString("_userType", userType); }
               
                    
                        // Check if Remember Me is checked
                        if (rememberMe)
                        {
                            // Store login information in a persistent storage (e.g., cookies)
                            Response.Cookies.Append("RememberMe", "true");

                            // Optionally, store other user information as well
                            Response.Cookies.Append("UserEmail", loginModel.Email);
                            // You might want to hash sensitive information before storing it in cookies
                        }
                      

                        TempData["UserEmail"] = loginModel.Email;
                        return RedirectToAction("Index");
                    }
                }
                else {
                    TempData["Message"] = "Email is not Verified";
                    return View();
                }
              

            }
            catch (FirebaseAuthException ex)
            { //validation with DB 
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                if (firebaseEx != null && firebaseEx.error != null && firebaseEx.error.message != null)
                {
                    ModelState.AddModelError(string.Empty, firebaseEx.error.message);
                }
                else
                {
                    // Handle the case where firebaseEx or its properties are null
                    ModelState.AddModelError(string.Empty, "An error occurred during authentication.");
                }


                return View(loginModel);
            }

            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        public IActionResult TermsAndConditions()
        {
            return View();
        }

        public IActionResult AdChoices()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                // Send password reset email
                await auth.SendPasswordResetEmailAsync(email);
                // Set success message
                TempData["Message"] = "Password reset email has been sent successfully.";
            }
            catch (FirebaseAuthException)
            {
                // Set error message
                TempData["ErrorMessage"] = "Failed to send password reset email.";
            }

            // Redirect back to the same page
            return RedirectToAction("ForgotPassword");
        }


        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(LoginModel loginModel)
        {
            try
            {
                // Create the user
                var fbAuthLink2 = await auth.CreateUserWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);

      
            


                // Log in the new user
               // var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);
                string token = fbAuthLink2.FirebaseToken;
                await auth.SendEmailVerificationAsync(token); // Send verification email

                // Saving the token in a session variable
                if (token != null)
                {
                    //HttpContext.Session.SetString("_UserToken", token);
                    var userAuthId = fbAuthLink2.User.LocalId;

                    if (userAuthId != null && loginModel.UserType != null && loginModel.UserName != null)
                    {
                        // Create user data
                        var result = await FirebaseHelper.CreateUserData(userAuthId, loginModel.UserType, loginModel.UserName);

                        if (result)
                        {
                            TempData["Message"] = "User registration successful! Please verify your email address.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to add user information.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "User authentication ID not found.";
                    }

                    return View();
                }
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                if (firebaseEx != null && firebaseEx.error != null && firebaseEx.error.message != null)
                {
                    ModelState.AddModelError(string.Empty, firebaseEx.error.message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred during authentication.");
                }

                return View(loginModel);
            }

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}