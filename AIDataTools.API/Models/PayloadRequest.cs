using System.ComponentModel.DataAnnotations;

namespace AIDataTools.API.Models;

/// <summary>
/// Base request model for payload processing
/// </summary>
public class PayloadRequest
{
    /// <summary>
    /// The data payload to process
    /// </summary>
    [Required]
    public string Payload { get; set; } = string.Empty;
}

/// <summary>
/// Request model for sample generation
/// </summary>
public class SampleRequest : PayloadRequest
{
    /// <summary>
    /// Number of samples to generate
    /// </summary>
    [Range(1, 10)]
    public int Count { get; set; } = 3;
    
    /// <summary>
    /// Criteria for sample generation
    /// </summary>
    [Required]
    public string Criteria { get; set; } = string.Empty;
}

/// <summary>
/// Request model for format conversion
/// </summary>
public class ConvertRequest : PayloadRequest
{
    /// <summary>
    /// Target format to convert to (json, xml, yaml)
    /// </summary>
    [Required]
    public string TargetFormat { get; set; } = string.Empty;
}
