using ShopingCart.Models.Base;

namespace ShopingCart.Services
{
    public class ServiceInit
    {
        public static dynamic GetCartInstance(Service providerInfo)
        {
            return new Cart(providerInfo);
        }
    }
}
