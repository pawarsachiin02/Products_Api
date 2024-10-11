

namespace Products_Api.Model
{
    public class Product
    {        
            public string title { get; set; }
            public int price { get; set; }
            public string[] sizes { get; set; }
            public string description { get; set; }
    }

    public class ProductsDetails
    {
        public List<Product> products { get; set; } = new List<Product>();
       
    }
}
