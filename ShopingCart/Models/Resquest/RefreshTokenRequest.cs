using System.ComponentModel.DataAnnotations;

namespace ShopingCart.Models.Resquest
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
