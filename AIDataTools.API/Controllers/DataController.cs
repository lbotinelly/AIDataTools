using AIDataTools.API.Extensions;
using AIDataTools.API.Models;
using AIDataTools.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIDataTools.API.Controllers;

/// <summary>
/// Controller for data processing operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class DataController : ControllerBase
{
    private readonly ILlmService _llmService;
    // private readonly IMongoService _mongoService;
    private readonly ILogger<DataController> _logger;
    private readonly MetricsService _metrics;

    /// <summary>
    /// Constructor
    /// </summary>
    public DataController(
        ILlmService llmService,
        //IMongoService mongoService,
        ILogger<DataController> logger,
        MetricsService metrics)
    {
        _llmService = llmService;
        //_mongoService = mongoService;
        _logger = logger;
        _metrics = metrics;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        _logger.LogDebug("HealthCheck endpoint called");
        var health = new
        {
            status = "healthy",
            services = new
            {
                api = "up",
                //mongodb = _mongoService.IsConnected() ? "up" : "down",
                llm = _llmService.IsAvailable() ? "up" : "down"
            },
            version = "1.0.0"
        };

        return Ok(health);
    }

    /// <summary>
    /// Anonymize a data payload by removing PII
    /// </summary>
    [HttpGet("downloadModel")]
    public async Task<IActionResult> DownloadModel()
    {
        _logger.LogDebug("DownloadModel endpoint called");
        try
        {
            await _llmService.DownloadModel();
            _logger.LogDebug("Model downloaded successfully");
            return Ok(new { message = "Model downloaded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading model");
            return StatusCode(500, "Error downloading model");
        }
    }

    /// <summary>
    /// Freeform text processing
    /// </summary>
    [HttpPost("freeform")]
    public async Task<IActionResult> Freeform([FromBody] FreeformRequest request)
    {
        _logger.LogDebug("Freeform endpoint called with request: {Request}", request);
        if (request == null || string.IsNullOrEmpty(request.Text))
        {
            _logger.LogWarning("Freeform request is invalid");
            return BadRequest("Freeform text is required.");
        }

        // Process the freeform text using the LLM service
        var result = await _llmService.ProcessFreeformText(request.Text);
        _logger.LogDebug("Freeform text processed successfully");

        return Ok(new { result });
    }

    [HttpPost("anonymize")]
    public async Task<IActionResult> Anonymize([FromBody] PayloadRequest request)
    {
        _logger.LogInformation("Processing anonymize request, payload size: {Size}", request.Payload.Length);
        _metrics.IncrementCounter("anonymize_requests");
        _metrics.IncrementCounter("total_payload_size", request.Payload.Length);
        _metrics.StartTimer("anonymize_operation");

        try
        {
            var prompt = "Anonymize the following data by replacing all PII with fictional equivalents. " +
                         "Maintain the same data structure and format. " +
                         "Replace names, emails, phone numbers, addresses, IDs, and any other personally identifiable information. " +
                         "Ensure the anonymized data is realistic and consistent.";

            _metrics.StartTimer("llm_processing");
            var result = await _llmService.ProcessAsync(prompt, request.Payload);
            var llmTime = _metrics.StopTimer("llm_processing");
            _logger.LogDebug("LLM processing took {LlmTime}ms", llmTime);

            var processingTime = _metrics.StopTimer("anonymize_operation");
            //await _mongoService.SaveResultAsync("anonymize", request.Payload, result);

            _logger.LogInformation("Anonymize operation completed in {ElapsedMs}ms", processingTime);

            // Add debug information if enabled
            if (HttpContext.RequestServices.GetService<IConfiguration>().GetValue("api:enableDebug", false))
            {
                return Ok(new
                {
                    result,
                    debug = new
                    {
                        processingTimeMs = processingTime,
                        llmTimeMs = llmTime,
                        payloadSize = request.Payload.Length,
                        timestamp = DateTime.UtcNow
                    }
                });
            }

            return Ok(new { result });
        }
        catch (Exception ex)
        {
            _metrics.IncrementCounter("anonymize_errors");
            _logger.LogError(ex, "Error processing anonymize request");
            throw;
        }
    }

    /// <summary>
    /// Create sample data based on the provided payload and criteria
    /// </summary>
    [HttpPost("samples")]
    public async Task<IActionResult> CreateSamples([FromBody] SampleRequest request)
    {
        _logger.LogInformation("Processing samples request, payload size: {Size}, criteria: {Criteria}",
            request.Payload.Length, request.Criteria);
        var startTime = DateTime.UtcNow;

        try
        {
            var prompt = $"Create {request.Count} sample data entries following the exact same structure and format as the provided data. " +
                         $"Apply these criteria to the generated samples: {request.Criteria}. " +
                         $"Ensure the samples are realistic, diverse, and follow the same schema as the original data.";

            var result = await _llmService.ProcessAsync(prompt, request.Payload);

            var processingTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            //await _mongoService.SaveResultAsync("samples", request.Payload, result);

            _logger.LogInformation("Samples operation completed in {ElapsedMs}ms", processingTime);

            return Ok(new { result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing samples request");
            throw;
        }
    }

    /// <summary>
    /// Generate a schema representation of the provided data
    /// </summary>
    [HttpPost("schema")]
    public async Task<IActionResult> GenerateSchema([FromBody] PayloadRequest request)
    {
        _logger.LogInformation("Processing schema request, payload size: {Size}", request.Payload.Length);
        var startTime = DateTime.UtcNow;

        try
        {
            var prompt = "Generate a schema representation for this data. " +
                         "Identify all fields, their data types, and any patterns or constraints. " +
                         "If the data appears to be JSON, provide a JSON Schema. " +
                         "If it's XML, provide an XML Schema. " +
                         "For YAML, provide a schema in YAML format. " +
                         "Include descriptions for each field based on the data content.";

            var result = await _llmService.ProcessAsync(prompt, request.Payload);

            var processingTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            //await _mongoService.SaveResultAsync("schema", request.Payload, result);

            _logger.LogInformation("Schema operation completed in {ElapsedMs}ms", processingTime);

            return Ok(new { result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing schema request");
            throw;
        }
    }

    /// <summary>
    /// Convert data from one format to another
    /// </summary>
    [HttpPost("convert")]
    public async Task<IActionResult> Convert([FromBody] ConvertRequest request)
    {
        _logger.LogInformation("Processing convert request, payload size: {Size}, target format: {Format}",
            request.Payload.Length, request.TargetFormat);
        var startTime = DateTime.UtcNow;

        try
        {
            var prompt = $"Convert this data from its current format to {request.TargetFormat} format. " +
                         $"Preserve all data, structure, and relationships. " +
                         $"Ensure the output is valid {request.TargetFormat.ToUpperInvariant()} syntax.";

            var result = await _llmService.ProcessAsync(prompt, request.Payload);

            var processingTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            //await _mongoService.SaveResultAsync("convert", request.Payload, result);

            _logger.LogInformation("Convert operation completed in {ElapsedMs}ms", processingTime);

            return Ok(new { result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing convert request");
            throw;
        }
    }

}
