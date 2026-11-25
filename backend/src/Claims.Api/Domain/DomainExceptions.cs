namespace Claims.Api.Domain;

/// <summary>The claim is not in a state that allows the requested change (HTTP 409).</summary>
public sealed class InvalidTransitionException(string message) : Exception(message);

/// <summary>The acting user is not allowed to make the requested change (HTTP 403).</summary>
public sealed class NotPermittedException(string message) : Exception(message);
