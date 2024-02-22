

namespace FirebaseLoginAuth.Models
{
    // ViewModel class
    public class HomeViewModel
    {
        public List<BookProduct>? BestSellersFantasy { get; set; }
        public List<BookProduct>? BestSellersScienceFiction { get; set; }
        public List<BookProduct>? SearchResults { get; set; }
        public string ImageUrl { get; set; }
        public List<string> ImageUrls { get; set; }
    }

}
