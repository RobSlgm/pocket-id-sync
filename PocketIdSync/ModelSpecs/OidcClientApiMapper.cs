using System.Linq;
using PocketIdSync.Models;
using PocketIdSync.Utils;
using Riok.Mapperly.Abstractions;

namespace PocketIdSync.ModelSpecs;

[Mapper]
static partial class OidcClientApiMapper
{
    [MapperIgnoreSource(nameof(ApiResponseDto.CreatedAt))]
    [MapperIgnoreSource(nameof(ApiResponseDto.Id))]
    private static partial OidcClientApiSpec Map(ApiResponseDto data);

    [MapperIgnoreTarget(nameof(ApiResponseDto.CreatedAt))]
    [MapperIgnoreTarget(nameof(ApiResponseDto.Id))]
    private static partial ApiResponseDto Map(OidcClientApiSpec data);

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

    public static ApiPermissionInputDto[] ToUpdateRequest(this ApiPermissionResponseDto[] data)
    {
        return [.. data.Select(p => Map(p))];
    }

    public static OidcClientApiKind ToKind(this ApiResponseDto data, string? ns = null) => ToKind(Map(data), ns);

    public static OidcClientApiKind ToKind(OidcClientApiSpec spec, string? ns = null)
    {
        var kind = new OidcClientApiKind
        {
            ApiVersion = "pocketid.closure.ch/v1",
            Kind = "OidcClientApi",
            Metadata = new KubernetesMetadata
            {
                Name = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(StringNameConverter.ToSafeName(spec.Resource)),
                Namespace = ns is not null ? System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(ns) : null,
            },
            Spec = spec,
        };
        return kind;
    }

    public static ApiResponseDto FromKind(this OidcClientApiSpec spec, ApiResponseDto? remote)
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
}
