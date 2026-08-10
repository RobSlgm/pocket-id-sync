using PocketIdSync.Models;
using Riok.Mapperly.Abstractions;

namespace PocketIdSync.ModelSpecs;

[Mapper]
static partial class AppApiMapper
{
    [MapperIgnoreSource(nameof(ApiResponseDto.CreatedAt))]
    [MapperIgnoreSource(nameof(ApiResponseDto.Id))]
    private static partial AppApiSpec Map(ApiResponseDto data);

    // private static partial ApiResponseDto MapInto(AppApiSpec data);
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
                Name = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(spec.Name ?? ""),
                Namespace = ns is not null ? System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(ns) : null,
            },
            Spec = spec,
        };
        return kind;
    }


    public static ApiResponseDto FromKind(this AppApiSpec spec, ApiResponseDto? remote)
    {
        // if (remote is not null)
        // {
        //     var copy = MapInto(remote);
        //     copy.FriendlyName = spec.FriendlyName;
        //     copy.Name = spec.Name;
        //     copy.CustomClaims = spec.CustomClaims;
        //     return copy;
        // }
        var data = Map(spec);
        return data;
    }
}
