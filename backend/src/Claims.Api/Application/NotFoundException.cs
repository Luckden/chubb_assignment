namespace Claims.Api.Application;

/// <summary>The requested resource does not exist (HTTP 404).</summary>
public sealed class NotFoundException(string message) : Exception(message);
