﻿using FirebaseLoginAuth.Helpers;
using FirebaseLoginAuth.Models;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using System;

namespace FirebaseLoginAuth.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add_Remove_Product_Admin()
        {
            return View("Add_Remove_Product_Admin");
        }


        public async Task<IActionResult> BoughtBooks()
        {
         
            var userAuthId = HttpContext.Session.GetString("_UserId");

            if (!string.IsNullOrEmpty(userAuthId))
            {
                // Retrieve the list of products bought by users
                var boughtBooks = await FirebaseHelper.GetAdminBoughtBooks(userAuthId);

                return View(boughtBooks);

            }
            else
            {
                return RedirectToAction("Add_Remove_Product_Admin");
            }
        }


        public async Task<IActionResult> GetBooksProducts()
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");

            if (!string.IsNullOrEmpty(userAuthId))
            {
                var bookProducts = await FirebaseHelper.GetAllBookedAdminBookProducts(userAuthId);
                return View("BookProductsList", bookProducts);
            }
            else
            {
                return RedirectToAction("Add_Remove_Product_Admin");
            }
        }
        public async Task<IActionResult> Edit_Book_Product(string id)
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");

            if (!string.IsNullOrEmpty(userAuthId))
            {
                var bookProduct = await FirebaseHelper.GetBookProductById( id);
                if (bookProduct != null)
                {
                     // Ensure that the ReleaseDate is properly formatted to display only the date part
            if (bookProduct.ReleaseDate.HasValue)
            {
                bookProduct.ReleaseDateFormatted = bookProduct.ReleaseDate.Value.ToString("yyyy-MM-dd");
            }
                    return View(bookProduct);
                }
                else
                {
                    TempData["ErrorMessage"] = "Book product not found.";
                    return RedirectToAction("BookProducts");
                }
            }
            else
            {
                return RedirectToAction("Add_Remove_Product_Admin");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete_Book_Product(string id)
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");

            if (!string.IsNullOrEmpty(userAuthId))
            {
                bool result = await FirebaseHelper.DeleteBookProduct(userAuthId, id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Book product deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete book product.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "User authentication failed.";
            }

            return RedirectToAction("GetBooksProducts");
        }


        [HttpPost]
        public async Task<IActionResult> EditBookProductAction(BookProduct bookProduct, IFormFile image, IFormFile secondImage)
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");

            if (!string.IsNullOrEmpty(userAuthId))
            {
                if (ModelState.IsValid)
                {
                    bool result = await FirebaseHelper.UpdateBookProduct(bookProduct, image, secondImage,userAuthId);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Book product updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update book product.";
                    }
                }
                else
                {
                    bool result = await FirebaseHelper.UpdateBookProduct(bookProduct, image, secondImage, userAuthId);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Book product updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update book product.";
                    }
                
            }

                if (bookProduct.BookId != null)
                {
                    return RedirectToAction("Edit_Book_Product", new { id = bookProduct.BookId });
                }
                else
                {
                    return RedirectToAction("Add_Remove_Product_Admin");
                }
            }
            else
            {
                return RedirectToAction("Add_Remove_Product_Admin");
            }
        }

        public async Task<IActionResult> SearchBooks(string searchString)
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");

            if (!string.IsNullOrEmpty(userAuthId))
            {
                // Perform the search in FirebaseHelper based on the search input
                var searchResults = await FirebaseHelper.SearchBookProducts(searchString);


                return View("BookProductsList", searchResults);
            }
            else
            {
                return RedirectToAction("Add_Remove_Product_Admin");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add_Book_Product(string id, int amount)
        {
            var userAuthId = HttpContext.Session.GetString("_UserId");

            if (!string.IsNullOrEmpty(userAuthId))
            {
         
                bool updated = await FirebaseHelper.UpdateBookProductAvailability(userAuthId, id, amount);
                var bookProducts = await FirebaseHelper.GetAllBookedAdminBookProducts(userAuthId);
          
                if (updated)
                {

                 
                    return View("BookProductsList", bookProducts);
                }
                else
                {
              
                    return RedirectToAction("BookProductsList", bookProducts); 
                }
            }
            else
            {
                return RedirectToAction("Add_Remove_Product_Admin");
            }
        }



        [HttpPost]
        public async Task<IActionResult> SaveBookProductOrder(BookProduct bookProduct, IFormFile image , IFormFile secondImage)
        {
            try
            {
                var userAuthId = HttpContext.Session.GetString("_UserId");

                if (!string.IsNullOrEmpty(userAuthId))
                {
                    if (bookProduct != null &&
                        bookProduct.ReleaseDate != null &&
                        bookProduct.Author != null
                        && bookProduct.Name != null &&
                        bookProduct.Publisher != null
                            && bookProduct.NumberOfAvailability != null &&
                        bookProduct.Price != null
                            && bookProduct.ISBN != null &&
                        bookProduct.Description != null
                            && bookProduct.Pages != null &&
                        bookProduct.Language != null
                            && bookProduct.Genre != null &&
                        bookProduct.IsBestseller != null
                            && bookProduct.Format != null &&
                        bookProduct.Country != null
                            && bookProduct.Dimensions != null &&
                        bookProduct.Weight != null
                            && bookProduct.Edition != null
                               && bookProduct.AgeLimitation != null
                                 && bookProduct.IsOnSell != null
                                  && bookProduct.OldPrice != null

                        )
                    {
                        // Initialize a new instance of BookProduct
                        BookProduct newBookProduct = new(
                     
                            name: bookProduct.Name,
                            isOnSell: (bool)bookProduct.IsOnSell,
                            author: bookProduct.Author,
                            publisher: bookProduct.Publisher,
                            releaseDate: (DateTime)bookProduct.ReleaseDate,
                            numberOfAvailability: (int)bookProduct.NumberOfAvailability,
                            price: (decimal)bookProduct.Price,
                             oldPrice: (decimal)bookProduct.OldPrice,
                            isbn: bookProduct.ISBN,
                            description: bookProduct.Description,
                            pages: (int)bookProduct.Pages,
                            language: bookProduct.Language,
                            genre: bookProduct.Genre,
                            isBestseller: (bool)bookProduct.IsBestseller,
                            format: bookProduct.Format,
                            country: bookProduct.Country,
                            dimensions: bookProduct.Dimensions,
                            weight: bookProduct.Weight,
                            edition: bookProduct.Edition,
                              ageLimitation: (int)bookProduct.AgeLimitation
                        );
                        var token = HttpContext.Session.GetString("_UserToken");
                        if (image != null && image.Length > 0 && token!=null)
                        {

                      
                            // Upload image to Firebase Storage and get the URL
                            var imageUrl = await FirebaseHelper.UploadImage(image, token);

                            // Set the image URL in the bookProduct object
                            newBookProduct.ImageUrl = imageUrl;
                        }
                        if (image != null && image.Length > 0 && token != null)
                        {
                            // Upload first image to Firebase Storage and get the URL
                            var imageUrl = await FirebaseHelper.UploadImage(image, token);
                            newBookProduct.ImageUrl = imageUrl;

                            // Check if the format is "Softcover and Hardback" and upload second image if available
                            if (bookProduct.Format == "Softcover and Hardback" && secondImage != null && secondImage.Length > 0)
                            {
                                var secondImageUrl = await FirebaseHelper.UploadImage(secondImage, token);
                                newBookProduct.SecondImageUrl = secondImageUrl;
                            }
                        }
                        else {
                            TempData["ErrorMessage"] = "Book product image not saved!";
                        }


                        // Call the method to save the book product data
                        bool result = await FirebaseHelper.CreateBookProductData(newBookProduct, userAuthId);

                        if (result)
                        {
                            TempData["SuccessMessage"] = "Book product saved successfully!";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to save book product.";
                        }

                        return RedirectToAction("Add_Remove_Product_Admin");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Book product is null.";
                    }
                }
                else
                {
                    return RedirectToAction("Add_Remove_Product_Admin");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error saving book product: {ex.Message}";
            }

            return RedirectToAction("Add_Remove_Product_Admin");
        }
    }
}
