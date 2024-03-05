

namespace FirebaseLoginAuth.Models
{
    // ViewModel class
    public class HomeViewModel
    {
        public List<BookProduct>? BestSellersFantasy { get; set; }
        public List<BookProduct>? BestSellersScienceFiction { get; set; }

        public List<BookProduct>? BestSellersMystery { get; set; }
        public List<BookProduct>? BestSellersThriller { get; set; }
        public List<BookProduct>? BestSellersRomance { get; set; }
        public List<BookProduct>? BestSellersHistoricalFiction { get; set; }
        public List<BookProduct>? SearchResults { get; set; }
        public string ImageUrl { get; set; }
        public List<string> ImageUrls { get; set; }

    }

}
