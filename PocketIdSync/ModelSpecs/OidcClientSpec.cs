using Generator.Equals;
using PocketIdSync.Models;

namespace PocketIdSync.ModelSpecs;

[Equatable]
sealed partial class OidcClientSpec
{
    public string? Id { get; set; }
    public string? Name { get; set; }

    [UnorderedEquality]
    public string[] CallbackURLs { get; set; } = [];

    [UnorderedEquality]
    public string[] LogoutCallbackURLs { get; set; } = [];

    public string? LaunchURL { get; set; }

    public bool? IsPublic { get; set; }

    public bool? PkceEnabled { get; set; }

    public bool? RequiresReauthentication { get; set; }

    public OidcClientCredentialsDto? Credentials { get; set; }

    [UnorderedEquality]
    public string[] AllowedGroups { get; set; } = [];

    public string? LogoPath { get; set; }

    [IgnoreEquality]
    public byte[]? LogoContent { get; set; }

    public string? LogoDarkPath { get; set; }

    [IgnoreEquality]
    public byte[]? LogoDarkContent { get; set; }
}

sealed class OidcClientKind : KubernetesSpec<OidcClientSpec> { }
