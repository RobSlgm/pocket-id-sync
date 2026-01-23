using Generator.Equals;

namespace PocketIdSync.Models;

[Equatable]
sealed partial class CustomClaimDto
{
    public string? Key { get; set; }
    public string? Value { get; set; }
}
