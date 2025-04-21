using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using prediccion.Models;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace prediccion.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;

    public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    [HttpPost]
    public async Task<IActionResult> Index(IFormFile imagen)
    {
        if (imagen == null || imagen.Length == 0)
        {
            ViewBag.Resultado = "No se seleccionó ninguna imagen.";
            return View();
        }

        using var content = new MultipartFormDataContent();
        using var stream = imagen.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(imagen.ContentType);

        content.Add(fileContent, "file", imagen.FileName);

        var response = await _httpClient.PostAsync("http://127.0.0.1:8000/predict-digit/", content);

        if (response.IsSuccessStatusCode)
        {
            var resultado = await response.Content.ReadFromJsonAsync<PrediccionResponse>();
            ViewBag.Resultado = $"Número predicho: {resultado?.PredictedDigit}";
        }
        else
        {
            ViewBag.Resultado = "Error al predecir.";
        }

        return View();
    }

    public class PrediccionResponse
    {
        [JsonPropertyName("predicted_digit")]
        public int PredictedDigit { get; set; }
    }
}
