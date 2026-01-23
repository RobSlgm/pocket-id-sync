using Generator.Equals;

namespace PocketIdSync.Models;

[Equatable]
sealed partial class OidcClientCredentialsDto
{
    [UnorderedEquality]
    public OidcClientFederatedIdentityDto[] FederatedIdentities { get; set; } = [];
}
