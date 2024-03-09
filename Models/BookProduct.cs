using Humanizer.Localisation;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using System.Security.Policy;
using System.Xml.Linq;

namespace FirebaseLoginAuth.Models
{
    public class BookProduct
    {
        public string? BookId { get; set; }
        public string? AdminId { get; set; }

        public int? SoldBooks  { get; set; }
    public string? Name { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        [NotMapped] 
        public string? ReleaseDateFormatted { get; set; }
        public DateTime?  ReleaseDate { get; set; }
        public int?   NumberOfAvailability { get; set; }
        public decimal? Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string? ISBN { get; set; }
        public string? Description { get; set; }
        public int? Pages { get; set; }
        public string? Language { get; set; }
        public string? Genre { get; set; }
        public bool? IsBestseller { get; set; }
        public bool? IsOnSell { get; set; }
        public string? Format { get; set; }
        public string? Country { get; set; } 
        public string? Dimensions { get; set; } 
        public string? Weight { get; set; }
        public string? Edition { get; set; }
        public int? AgeLimitation { get; set; }
        public string? ImageUrl { get; set; }
        public string? SecondImageUrl { get; set; }

        public BookProduct()
        {
            
        }

        // Constructor with parameters
        public BookProduct(bool isOnSell,int ageLimitation,string name, string author, string publisher, DateTime releaseDate,int numberOfAvailability, decimal price, decimal oldPrice, string isbn, string description, int pages, string language, string genre, bool isBestseller, string format, string country, string dimensions, string weight, string edition)
        {
            // Generate a random 6-digit number
            Random rand = new();
            int randomNumber = rand.Next(100000, 999999);
            ReleaseDateFormatted = releaseDate.ToString("MM/dd/yyyy");

            // Get the current timestamp
            DateTime currentTime = DateTime.Now;
       
            // Combine the random number and current timestamp to form the ID
            string timestamp = currentTime.ToString("yyyyMMddHHmmss");
            string combinedId = timestamp + randomNumber.ToString();

            // Parse the combined ID to an integer
            IsOnSell = isOnSell;
            BookId = combinedId;
            AgeLimitation = ageLimitation;
            Name = name;
            Author = author;
            Publisher = publisher;
            ReleaseDate = releaseDate;
            NumberOfAvailability = numberOfAvailability;
            Price = price;
            ISBN = isbn;
            Description = description;
            Pages = pages;
            Language = language;
            Genre = genre;
            IsBestseller = isBestseller;
            Format = format;
            Country = country;
            Dimensions = dimensions;
            Weight = weight;
            Edition = edition;
            OldPrice = oldPrice;
        }

   
    }


    public class BookCustomerAssociation
    {
        public string? BookId { get; set; }
        public string? CustomerId { get; set; }
        public decimal? Price { get; set; }
        public string? BookName { get; set; }
        public string? CustomerName { get; set; }
    }

}
