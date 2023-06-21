using Newtonsoft.Json;

namespace ShopingCart.Const
{
    public static class CartDateTime
    {
        public static DateTime Now => TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id));

        public static class CartJsonConfig
        {
            public static JsonSerializerSettings Settings => new()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                NullValueHandling = NullValueHandling.Ignore
            };

            public static JsonSerializerSettings SettingsExtend => new()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>() { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };
        }
    }
}
