
namespace ShopingCart.Models.Base
{
    public class Settings
    {
        public Service CartAPI { get; set; }
        public Service StockAPI { get; set; }
        public Service IDP { get; set; }
    }
    public class Service
    {
        public string BaseURL { get; set; }
    }
}
