using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ApiGateway.API.Controllers;

[ApiController]
[Route("api/{*catchall}")]
[Authorize]
public class ProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProxyController> _logger;

    public ProxyController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    [HttpPatch]
    public async Task<IActionResult> ProxyRequest(string catchall)
    {
        try
        {
            // Get the target API configuration from the route
            var apiConfig = GetApiConfiguration(catchall);
            if (apiConfig == null)
            {
                return NotFound($"API configuration not found for path: {catchall}");
            }

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(GetHttpMethod(), $"{apiConfig.BaseUrl}/{catchall}");

            // Forward the authorization header
            if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authHeader.ToString().Replace("Bearer ", ""));
            }

            // Forward the request body if present
            if (Request.ContentLength != null && Request.ContentLength > 0)
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                request.Content = new StringContent(body, System.Text.Encoding.UTF8, Request.ContentType ?? "application/json");
            }

            // Forward all headers except Authorization (already handled) and Host
            foreach (var header in Request.Headers)
            {
                if (header.Key != "Authorization" && header.Key != "Host")
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToString());
                }
            }

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Forwarded request to {catchall} with status code {response.StatusCode}");

            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = responseContent,
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error forwarding request to {catchall}");
            return StatusCode(500, "Error forwarding request to the target service");
        }
    }

    private HttpMethod GetHttpMethod()
    {
        return Request.Method switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            "PATCH" => HttpMethod.Patch,
            _ => HttpMethod.Get
        };
    }

    private ApiConfig? GetApiConfiguration(string path)
    {
        // Get all API configurations from appsettings.json
        var apiConfigs = _configuration.GetSection("ApiConfigurations").Get<List<ApiConfig>>();
        if (apiConfigs == null) return null;

        // Find the matching API configuration based on the path prefix
        return apiConfigs.FirstOrDefault(config => path.StartsWith(config.PathPrefix, StringComparison.OrdinalIgnoreCase));
    }
}

public class ApiConfig
{
    public string Name { get; set; } = string.Empty;
    public string PathPrefix { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
} 