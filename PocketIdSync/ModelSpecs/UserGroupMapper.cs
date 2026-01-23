using PocketIdSync.Models;
using Riok.Mapperly.Abstractions;

namespace PocketIdSync.ModelSpecs;

[Mapper]
static partial class UserGroupMapper
{
    [MapperIgnoreSource(nameof(UserGroupDto.AllowedOidcClients))]
    [MapperIgnoreSource(nameof(UserGroupDto.Users))]
    [MapperIgnoreSource(nameof(UserGroupDto.UserCount))]
    [MapperIgnoreSource(nameof(UserGroupDto.CreatedAt))]
    [MapperIgnoreSource(nameof(UserGroupDto.Id))]
    [MapperIgnoreSource(nameof(UserGroupDto.LdapId))]
    private static partial UserGroupSpec Map(UserGroupDto data);

    [MapperIgnoreSource(nameof(UserGroupMinimalDto.UserCount))]
    [MapperIgnoreSource(nameof(UserGroupMinimalDto.CreatedAt))]
    [MapperIgnoreSource(nameof(UserGroupMinimalDto.Id))]
    [MapperIgnoreSource(nameof(UserGroupMinimalDto.LdapId))]
    private static partial UserGroupSpec Map(UserGroupMinimalDto data);

    [MapperIgnoreTarget(nameof(UserGroupDto.AllowedOidcClients))]
    [MapperIgnoreTarget(nameof(UserGroupDto.Users))]
    public static partial UserGroupDto MapInto(UserGroupMinimalDto data);

    [MapperIgnoreTarget(nameof(UserGroupDto.AllowedOidcClients))]
    [MapperIgnoreTarget(nameof(UserGroupDto.Users))]
    [MapperIgnoreTarget(nameof(UserGroupDto.UserCount))]
    [MapperIgnoreTarget(nameof(UserGroupDto.CreatedAt))]
    [MapperIgnoreTarget(nameof(UserGroupDto.Id))]
    [MapperIgnoreTarget(nameof(UserGroupDto.LdapId))]
    private static partial UserGroupDto Map(UserGroupSpec data);

    public static UserGroupKind ToKind(this UserGroupDto data, string? ns = null) => ToKind(Map(data), ns);

    public static UserGroupKind ToKind(this UserGroupMinimalDto data, string? ns = null) => ToKind(Map(data), ns);

    public static UserGroupKind ToKind(UserGroupSpec spec, string? ns = null)
    {
        var kind = new UserGroupKind
        {
            ApiVersion = "pocketid.closure.ch/v1",
            Kind = "UserGroup",
            Metadata = new KubernetesMetadata
            {
                Name = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(spec.Name ?? ""),
                Namespace = ns is not null ? System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(ns) : null,
            },
            Spec = spec,
        };
        return kind;
    }

    public static UserGroupDto FromKind(this UserGroupSpec spec, UserGroupDto? remote)
    {
        if (remote is not null)
        {
            var copy = MapInto(remote);
            copy.FriendlyName = spec.FriendlyName;
            copy.Name = spec.Name;
            copy.CustomClaims = spec.CustomClaims;
            return copy;
        }
        var data = Map(spec);
        return data;
    }
}
