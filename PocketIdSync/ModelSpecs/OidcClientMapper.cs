using System;
using System.Collections.Generic;
using System.Linq;
using PocketIdSync.Models;
using PocketIdSync.Sync;
using Riok.Mapperly.Abstractions;

namespace PocketIdSync.ModelSpecs;

[Mapper]
static partial class OidcClientMapper
{
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.AllowedUserGroups))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.ClientPermissionIds))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.UserDelegatedPermissionIds))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.HasLogo))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.HasDarkLogo))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.IsGroupRestricted))]
    [MapperIgnoreTarget(nameof(OidcClientSpec.AllowedGroups))]
    [MapperIgnoreTarget(nameof(OidcClientSpec.ClientPermission))]
    [MapperIgnoreTarget(nameof(OidcClientSpec.UserDelegatedPermission))]
    [MapperIgnoreTarget(nameof(OidcClientSpec.LogoPath))]
    [MapperIgnoreTarget(nameof(OidcClientSpec.LogoContent))]
    [MapperIgnoreTarget(nameof(OidcClientSpec.LogoDarkPath))]
    [MapperIgnoreTarget(nameof(OidcClientSpec.LogoDarkContent))]
    private static partial OidcClientSpec Map(OidcClientCompleteDto data);

    [MapperIgnoreTarget(nameof(OidcClientCompleteDto.AllowedUserGroups))]
    [MapperIgnoreTarget(nameof(OidcClientCompleteDto.ClientPermissionIds))]
    [MapperIgnoreTarget(nameof(OidcClientCompleteDto.UserDelegatedPermissionIds))]
    [MapperIgnoreTarget(nameof(OidcClientCompleteDto.HasLogo))]
    [MapperIgnoreTarget(nameof(OidcClientCompleteDto.HasDarkLogo))]
    [MapperIgnoreTarget(nameof(OidcClientCompleteDto.IsGroupRestricted))]
    [MapperIgnoreSource(nameof(OidcClientSpec.AllowedGroups))]
    [MapperIgnoreSource(nameof(OidcClientSpec.ClientPermission))]
    [MapperIgnoreSource(nameof(OidcClientSpec.UserDelegatedPermission))]
    [MapperIgnoreSource(nameof(OidcClientSpec.LogoPath))]
    [MapperIgnoreSource(nameof(OidcClientSpec.LogoContent))]
    [MapperIgnoreSource(nameof(OidcClientSpec.LogoDarkPath))]
    [MapperIgnoreSource(nameof(OidcClientSpec.LogoDarkContent))]
    private static partial OidcClientCompleteDto Map(OidcClientSpec data);

    private static OidcClientSpec ToSpec(OidcClientCompleteDto data, AppApiResolver? apiResolver)
    {
        var spec = Map(data);
        if (data.HasDarkLogo == true)
        {
            spec.LogoDarkPath = $"{data.Id}-dark.jpg";
        }
        if (data.HasLogo == true)
        {
            spec.LogoPath = $"{data.Id}.jpg";
        }
        spec.ClientPermission = ToPermissions(apiResolver, data.ClientPermissionIds);
        spec.UserDelegatedPermission = ToPermissions(apiResolver, data.UserDelegatedPermissionIds);
        return spec;
    }

    private static AppApiPermission[]? ToPermissions(AppApiResolver? resolver, string[]? permissionIds)
    {
        if (resolver is null || permissionIds is null || permissionIds.Length == 0)
        {
            return null;
        }
        var permissions = new List<AppApiPermission>();
        foreach (var pid in permissionIds)
        {
            var permission = resolver.Find(pid);
            if (permission is not null)
            {
                permissions.Add(permission);
            }
        }
        return permissions.Count > 0 ? [.. permissions] : null;
    }

    public static OidcClientKind ToKind(this OidcClientCompleteDto data, string? ns = null, AppApiResolver? apiResolver = null) => ToKind(ToSpec(data, apiResolver), ns, [.. data.AllowedUserGroups.Select(c => c.Name!)]);

    public static OidcClientKind ToKind(this OidcClientCompleteDto data, OidcClientKind? other, AppApiResolver? apiResolver)
    {
        var groups = data.AllowedUserGroups.Select(g => g.Name!).ToArray();
        if (other is null || other.Metadata is null)
        {
            return ToKind(ToSpec(data, apiResolver), ns: null, groups);
        }
        var kind = ToKind(ToSpec(data, apiResolver), other.Metadata.Namespace, groups);
        if (kind.Spec is null)
        {
            throw new InvalidOperationException("YAML malformed");
        }
        if (data.HasDarkLogo == true)
        {
            kind.Spec.LogoDarkPath = other.Spec?.LogoDarkPath ?? kind.Spec.LogoDarkPath;
        }
        if (data.HasLogo == true)
        {
            kind.Spec.LogoPath = other.Spec?.LogoPath ?? kind.Spec.LogoPath;
        }
        return kind;
    }

    public static OidcClientKind ToKind(OidcClientSpec spec, string? ns = null, string[]? groups = null)
    {
        var kind = new OidcClientKind
        {
            ApiVersion = "pocketid.closure.ch/v1",
            Kind = "OidcClient",
            Metadata = new KubernetesMetadata
            {
                Name = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(spec.Id ?? ""),
                Namespace = ns is not null ? System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(ns) : null,
            },
            Spec = spec,
        };
        if (groups is not null)
        {
            kind.Spec.AllowedGroups = groups;
        }
        return kind;
    }

    public static OidcClientCompleteDto FromKind(this OidcClientSpec spec, Dictionary<string, UserGroupMinimalDto> userGroups, AppApiResolver? apiResolver)
    {
        var data = Map(spec);
        if (spec.AllowedGroups is not null && spec.AllowedGroups.Length > 0)
        {
            foreach (var groupName in spec.AllowedGroups)
            {
                if (userGroups.TryGetValue(groupName, out var ug))
                {
                    data.AllowedUserGroups = [.. data.AllowedUserGroups, ug];
                }
            }
        }
        data.HasLogo = !string.IsNullOrEmpty(spec.LogoPath);
        data.HasDarkLogo = !string.IsNullOrEmpty(spec.LogoDarkPath);
        data.IsGroupRestricted = spec.AllowedGroups?.Length > 0;
        return data;
    }
}
