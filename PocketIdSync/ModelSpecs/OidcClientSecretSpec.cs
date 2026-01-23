using System;
using System.Text;

namespace PocketIdSync.ModelSpecs;

sealed class OidcClientSecretSpec
{
    public string? ClientId
    {
        get;
        set
        {
            field = value is not null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) : null;
        }
    }
    public string? ClientSecret
    {
        get;
        set
        {
            field = value is not null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) : null;
        }
    }
    public string? IssuerUrl
    {
        get;
        set
        {
            field = value is not null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) : null;
        }
    }
}

sealed class OidcClientSecretKind : KubernetesSecret<OidcClientSecretSpec> { }
