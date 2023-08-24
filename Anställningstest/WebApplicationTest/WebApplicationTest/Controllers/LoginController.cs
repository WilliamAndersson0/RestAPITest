using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using WebApplicationTest.Models;
using System.Net.Http;


namespace WebApplicationTest.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _authClient;
        private readonly HttpClient _profileClient;

        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _authClient = CreateHttpClient(httpClientFactory, "https://services2.i-centrum.se/recruitment/auth");
            _profileClient = CreateHttpClient(httpClientFactory, "https://services2.i-centrum.se/recruitment/profile/avatar");
        }

        public IActionResult Index()
        {
            return View();
        }

        private HttpClient CreateHttpClient(IHttpClientFactory httpClientFactory, string baseAddress)
        {
            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginModel model)
        {
            string token = await GetTokenAsync(model);

            if (!string.IsNullOrEmpty(token))
            {
                string base64Image = await GetBase64ImageAsync(token);
                ViewBag.Base64Image = base64Image;
            }

            return View();
        }

        private async Task<string> GetTokenAsync(LoginModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _authClient.PostAsync("", content);

            if (response.IsSuccessStatusCode)
            {
                var responseObject = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return responseObject.RootElement.TryGetProperty("token", out var tokenElement) ? tokenElement.GetString() : null;
            }

            return null;
        }

        private async Task<string> GetBase64ImageAsync(string token)
        {
            _profileClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _profileClient.GetAsync("");

            if (response.IsSuccessStatusCode)
            {
                var responseObject = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return responseObject.RootElement.TryGetProperty("data", out var dataElement) ? dataElement.GetString() : null;
            }

            return null;
        }
    }

}
