using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AIDataTools.API.Services;

/// <summary>
/// Response model for Ollama API
/// </summary>
public class OllamaResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;
}

/// <summary>
/// Service for communicating with Ollama LLM
/// </summary>
public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LlmService> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public LlmService(IConfiguration config, ILogger<LlmService> logger)
    {
        _configuration = config;
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(config.GetValue("llm:timeoutSeconds", 300))
        };

        var model = _configuration["llm:model"] ?? "codellama";
        _logger.LogInformation("Using LLM model: {Model}", model);
    }

    /// <inheritdoc/>
    public async Task<string> ProcessAsync(string instruction, string payload)
    {
        var endpoint = _configuration["llm:endpoint"] ?? "http://ollama:11434/api/generate";
        var model = _configuration["llm:model"] ?? "codellama";
        var temperature = _configuration.GetValue("llm:temperature", 0.2f);
        var maxTokens = _configuration.GetValue("llm:maxTokens", 2048);

        var systemPrompt = "You are DataForgeAI, a tool for processing data payloads.";
        var fullPrompt = $"{systemPrompt}\n\n{instruction}\n\n{payload}";

        var request = new
        {
            model,
            prompt = fullPrompt,
            temperature,
            max_tokens = maxTokens,
            stream = false
        };

        try
        {
            _logger.LogInformation("Sending request to LLM, model: {Model}, endpoint: {Endpoint}, prompt length: {Length}", model, endpoint, fullPrompt.Length);
            _logger.LogDebug("LLM request: {Request}", request);

            var response = await _httpClient.PostAsJsonAsync(endpoint, request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("LLM request failed: {StatusCode}, {Error}, request: {Request}",
                    response.StatusCode, error, request);
                throw new Exception($"LLM request failed: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
            return result?.Response ?? string.Empty;
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("LLM request timed out after {Timeout} seconds",
                _httpClient.Timeout.TotalSeconds);
            throw new Exception("LLM request timed out");
        }
        catch (Exception ex) when (ex is not TaskCanceledException)
        {
            _logger.LogError(ex, "Error processing LLM request");
            throw;
        }
    }

    /// <inheritdoc/>
    public bool IsAvailable()
    {
        try
        {
            var endpoint = _configuration["llm:endpoint"] ?? "http://ollama:11434/api/generate";
            var baseUrl = new Uri(endpoint).GetLeftPart(UriPartial.Authority);
            var response = _httpClient.GetAsync($"{baseUrl}/api/version").Result;
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<string> ProcessFreeformText(string text)
    {
        var prompt = "Process the following freeform text: " + text;
        return await ProcessAsync("Freeform Text Processing", prompt);
    }

    /// <inheritdoc />
    public async Task DownloadModel()
    {
        var model = _configuration["llm:model"] ?? "codellama";
        var endpoint = _configuration["llm:endpoint"] ?? "http://ollama:11434/api/generate";
        var baseUrl = new Uri(endpoint).GetLeftPart(UriPartial.Authority);
        var pullEndpoint = $"{baseUrl}/api/pull";

        _logger.LogInformation("Downloading LLM model: {Model}", model);

        var request = new
        {
            name = model,
            stream = false
        };

        try
        {
            _logger.LogInformation("Sending pull request to LLM, model: {Model}, endpoint: {Endpoint}", model, pullEndpoint);
            _logger.LogDebug("LLM pull request: {Request}", request);

            var response = await _httpClient.PostAsJsonAsync(pullEndpoint, request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("LLM pull request failed: {StatusCode}, {Error}, request: {Request}",
                    response.StatusCode, error, request);
                throw new Exception($"LLM pull request failed: {response.StatusCode}");
            }

            _logger.LogInformation("Model download completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading model");
            throw;
        }
    }
}
