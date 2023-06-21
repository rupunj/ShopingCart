using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShopingCart.Extentions;
using ShopingCart.Filters;
using System.Text.Json.Serialization;

JObject Settings = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("./appsettings.json"));

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(Settings["Settings"]["BaseURL"].ToString());
builder.WebHost.UseKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1073741824;
    options.Limits.MaxConcurrentConnections = 5000;
});

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

#region Default Thread Settings for parallel processing
int minWorker, minIOC;
ThreadPool.GetMinThreads(out minWorker, out minIOC);
ThreadPool.SetMinThreads(3000, minIOC);
#endregion

builder.Services.AddMvc()
    .AddViewOptions(options =>
    {
        options.HtmlHelperOptions.ClientValidationEnabled = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.Secure = CookieSecurePolicy.Always;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMvc(options =>
{
    options.Filters.Add(new AuthenticationFilter(builder.Configuration));
    options.Filters.Add(new ExceptionFilter(builder.Configuration));
});
builder.Services.Configure<ShopingCart.Models.Base.Settings>(builder.Configuration.GetSection("Settings"));
builder.Services.AddAntiforgery();
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1073741824;
    options.Limits.MaxConcurrentConnections = 5000;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseCookiePolicy();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseEnableRequestRewindMiddleware();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapControllers();
});
app.Use((context, next) => { context.Request.Scheme = "https"; return next(); });
app.Run();
