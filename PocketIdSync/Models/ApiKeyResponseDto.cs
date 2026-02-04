namespace PocketIdSync.Models;

sealed class ApiKeyResponseDto
{
    public ApiKeyDto? ApiKey { get; set; }
    public string? Token { get; set; }
}
