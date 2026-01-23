namespace PocketIdSync.Models;

class OidcClientCreateDto : OidcClientUpdateDto
{
    /// <summary>
    /// min=2,max=128
    /// </summary>
    public required string Id { get; set; }
}
