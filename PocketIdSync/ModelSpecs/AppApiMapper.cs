using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using PocketIdSync.Models;
using Riok.Mapperly.Abstractions;

namespace PocketIdSync.ModelSpecs;

[Mapper]
static partial class AppApiMapper
{
    [MapperIgnoreSource(nameof(ApiResponseDto.CreatedAt))]
    [MapperIgnoreSource(nameof(ApiResponseDto.Id))]
    private static partial AppApiSpec Map(ApiResponseDto data);

    [MapperIgnoreTarget(nameof(ApiResponseDto.CreatedAt))]
    [MapperIgnoreTarget(nameof(ApiResponseDto.Id))]
    private static partial ApiResponseDto Map(AppApiSpec data);

    [MapperIgnoreSource(nameof(ApiResponseDto.CreatedAt))]
    [MapperIgnoreSource(nameof(ApiResponseDto.Id))]
    [MapperIgnoreSource(nameof(ApiResponseDto.Permissions))]
    private static partial ApiCreateDto MapForCreate(ApiResponseDto data);

    [MapperIgnoreSource(nameof(ApiResponseDto.CreatedAt))]
    [MapperIgnoreSource(nameof(ApiResponseDto.Id))]
    [MapperIgnoreSource(nameof(ApiResponseDto.Permissions))]
    [MapperIgnoreSource(nameof(ApiResponseDto.Resource))]
    private static partial ApiUpdateDto MapForUpdate(ApiResponseDto data);

    [MapperIgnoreSource(nameof(ApiPermissionResponseDto.Id))]
    private static partial ApiPermissionInputDto Map(ApiPermissionResponseDto data);

    [MapperIgnoreTarget(nameof(ApiPermissionResponseDto.Id))]
    private static partial ApiPermissionResponseDto Map(ApiPermissionInputDto data);

    // private static partial ApiResponseDto MapInto(ApiResponseDto data);

    public static ApiUpdateDto ToUpdateRequest(this ApiResponseDto data)
    {
        return MapForUpdate(data);
    }

    public static ApiCreateDto ToCreateRequest(this ApiResponseDto data)
    {
        return MapForCreate(data);
    }

    public static AppApiKind ToKind(this ApiResponseDto data, string? ns = null) => ToKind(Map(data), ns);

    public static AppApiKind ToKind(AppApiSpec spec, string? ns = null)
    {
        var kind = new AppApiKind
        {
            ApiVersion = "pocketid.closure.ch/v1",
            Kind = "ApplicationApi",
            Metadata = new KubernetesMetadata
            {
                Name = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(ToSafeName(spec.Resource)),
                Namespace = ns is not null ? System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(ns) : null,
            },
            Spec = spec,
        };
        return kind;
    }

    public static string ToSafeName(string? uri)
    {
        if (string.IsNullOrEmpty(uri))
        {
            return string.Empty;
        }
        var lowerInput = uri.ToLowerInvariant();
        return AllowedNameCharacterset.Replace(lowerInput, "-");
    }

    public static ApiResponseDto FromKind(this AppApiSpec spec, ApiResponseDto? remote)
    {
        if (remote is not null)
        {
            // var copy = MapInto(remote);
            // copy.Resource = spec.Resource;
            // copy.Name = spec.Name;
            // // copy.Permissions = [..spec.Permissions ?? []];
            // return copy;
        }
        var data = Map(spec);
        return data;
    }

    [GeneratedRegex("[^a-z0-9.\\-]+", RegexOptions.None, matchTimeoutMilliseconds: 100)]
    private static partial Regex AllowedNameCharacterset { get; }
}
