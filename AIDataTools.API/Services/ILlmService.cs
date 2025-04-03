namespace AIDataTools.API.Services;

/// <summary>
/// Interface for LLM service that handles communication with Ollama
/// </summary>
public interface ILlmService
{
    /// <summary>
    /// Process a prompt with the LLM
    /// </summary>
    /// <param name="instruction">The instruction for the LLM</param>
    /// <param name="payload">The data payload to process</param>
    /// <returns>The processed result from the LLM</returns>
    Task<string> ProcessAsync(string instruction, string payload);
    
    /// <summary>
    /// Check if the LLM service is available
    /// </summary>
    /// <returns>True if the service is available, false otherwise</returns>
    bool IsAvailable();

    /// <summary>
    /// Processes freeform text using the LLM service.
    /// </summary>
    /// <param name="text">The freeform text to process.</param>
    /// <returns>The processed result.</returns>
    Task<string> ProcessFreeformText(string text);

    /// <summary>
    /// Downloads the LLM model.
    /// </summary>
    Task DownloadModel();
}
