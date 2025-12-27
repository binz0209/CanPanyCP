using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// AuditLog entity - Represents an audit log entry for security and tracking
/// </summary>
[BsonIgnoreExtraElements]
public class AuditLog : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }

    [BsonElement("userEmail")]
    public string? UserEmail { get; set; }

    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;

    [BsonElement("entityType")]
    public string? EntityType { get; set; }

    [BsonElement("entityId")]
    public string? EntityId { get; set; }

    [BsonElement("endpoint")]
    public string Endpoint { get; set; } = string.Empty;

    [BsonElement("httpMethod")]
    public string HttpMethod { get; set; } = string.Empty;

    [BsonElement("requestPath")]
    public string RequestPath { get; set; } = string.Empty;

    [BsonElement("queryString")]
    public string? QueryString { get; set; }

    [BsonElement("requestBody")]
    public string? RequestBody { get; set; }

    [BsonElement("responseStatusCode")]
    public int? ResponseStatusCode { get; set; }

    [BsonElement("responseBody")]
    public string? ResponseBody { get; set; }

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("duration")]
    public long? Duration { get; set; }

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    [BsonElement("stackTrace")]
    public string? StackTrace { get; set; }

    [BsonElement("changes")]
    public Dictionary<string, object>? Changes { get; set; }

    [BsonElement("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


