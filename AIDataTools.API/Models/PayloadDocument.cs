namespace AIDataTools.API.Models;

/// <summary>
/// Document model for storing payload processing results
/// </summary>
public class PayloadDocument
{
    /// <summary>
    /// Document ID
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Operation type (anonymize, samples, schema, convert)
    /// </summary>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Original input payload
    /// </summary>
    public string InputPayload { get; set; } = string.Empty;
    
    /// <summary>
    /// Processed output payload
    /// </summary>
    public string OutputPayload { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp of when the processing occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }
}
