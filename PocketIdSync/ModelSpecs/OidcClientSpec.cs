using Generator.Equals;
using PocketIdSync.Models;
using PocketIdSync.Utils;

namespace PocketIdSync.ModelSpecs;

[Equatable]
sealed partial class OidcClientSpec
{
    public string? Id { get; set; }
    public string? Name { get; set; }

    [CustomEquality(typeof(StringEmptyEqualityComparer))]
    public string? Description { get; set; }

    [UnorderedEquality(System.StringComparison.OrdinalIgnoreCase)]
    public string[] CallbackURLs { get; set; } = [];

    [UnorderedEquality(System.StringComparison.OrdinalIgnoreCase)]
    public string[] LogoutCallbackURLs { get; set; } = [];

    public string? LaunchURL { get; set; }

    public bool? IsPublic { get; set; }

    public bool? PkceEnabled { get; set; }

    public bool? PkceSupported { get; set; }

    public bool? RequiresReauthentication { get; set; }

    public bool? RequiresPushedAuthorizationRequests { get; set; }

    public bool? SkipConsent { get; set; }

    public OidcClientCredentialsDto? Credentials { get; set; }

    [UnorderedEquality(System.StringComparison.OrdinalIgnoreCase)]
    public string[] AllowedGroups { get; set; } = [];

    public string? LogoPath { get; set; }

    [IgnoreEquality]
    public byte[]? LogoContent { get; set; }

    public string? LogoDarkPath { get; set; }

    [IgnoreEquality]
    public byte[]? LogoDarkContent { get; set; }

    public int? AccessTokenDurationMinutes { get; set; }

    public int? RefreshTokenDurationMinutes { get; set; }

    [UnorderedEquality]
    public AppApiPermission[]? UserDelegatedPermission { get; set; }

    [UnorderedEquality]
    public AppApiPermission[]? ClientPermission { get; set; }
}

sealed class OidcClientKind : KubernetesSpec<OidcClientSpec> { }
