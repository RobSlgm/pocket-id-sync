using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.Sync;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.ApplicationImages;

[CliCommand(
    Description = "Put application images to Pocket ID",
    Name = "put",
    Parent = typeof(ApplicationImagesCommand)
)]
sealed class PutCommand(IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliArgument(Description = "Image type", Arity = CliArgumentArity.ExactlyOne, Required = true)]
    public required ApplicationImageType ImageType { get; set; }

    [CliOption(Description = "Root directory of file store for configuration", Alias = "d", ValidationRules = CliValidationRules.ExistingDirectory, Required = false)]
    public DirectoryInfo? StoreRoot { get; set; }

    [CliOption(Description = "Full file path", Alias = "f", ValidationRules = CliValidationRules.ExistingFile, Required = false)]
    public FileInfo? ImageFile { get; set; }

    [CliOption(Description = "Image Uri", Alias = "img", ValidationRules = CliValidationRules.LegalUri, Required = false)]
    public Uri? ImageUri { get; set; }

    public async Task<int> RunAsync(CliContext context)
    {
        var overdefined = (StoreRoot is not null ? 1 : 0) + (ImageFile is not null ? 1 : 0) + (ImageUri is not null ? 1 : 0);
        if (overdefined != 1)
        {
            if (overdefined > 1)
            {
                AnsiConsole.MarkupLine($"[red]At least one of StoreRoot, ImageFile or ImageUri can be set[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Either StoreRoot, ImageFile or ImageUri must be set[/]");
            }
            return ExitCode.InvalidConfiguration;
        }

        if (StoreRoot is null && ImageType == ApplicationImageType.All)
        {
            AnsiConsole.MarkupLine($"[red]ImageType All is only allowed with StoreRoot[/]");
            return ExitCode.BadRequest;
        }

        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);

        if (ImageUri is not null)
        {
            AnsiConsole.MarkupLine($"[Orange2]ImageUri not yet implemented[/]");
            return ExitCode.Unauthorized;
            // TODO: Download file and set ImageFile to temporary file
            //    return await UploadAsync(pocketId, ImageFile, ImageType, context.CancellationToken);
        }

        if (ImageFile is not null)
        {
            if (!ImageFile.Exists)
            {
                AnsiConsole.MarkupLine($"[red]Not found {ImageFile.FullName}[/]");
                return ExitCode.BadRequest;
            }
            return await UploadAsync(pocketId, ImageFile, ImageType, context.CancellationToken);
        }

        List<ApplicationImageType> imageTypes = [];
        switch (ImageType)
        {
            case ApplicationImageType.All:
                imageTypes.AddRange(Enum.GetValues<ApplicationImageType>());
                imageTypes.Remove(ApplicationImageType.All);
                break;

            default:
                imageTypes.Add(ImageType);
                break;
        }
        var imgPath = Directory.CreateDirectory(Path.Combine(StoreRoot!.FullName, "images"));
        foreach (var imgType in imageTypes)
        {
            var hit = Directory.EnumerateFileSystemEntries(imgPath.FullName, $"{imgType}.*").FirstOrDefault();
            if (hit is null)
            {
                AnsiConsole.MarkupLine($"[Orange1]✗ {imgType} not found in store, skipping[/]");
                continue;
            }
            var success = await UploadAsync(pocketId, new FileInfo(hit), imgType, context.CancellationToken);
            if (success != ExitCode.Success)
            {
                AnsiConsole.MarkupLine($"Abort");
                return success;
            }
        }
        return ExitCode.Success;
    }

    public static async Task<int> UploadAsync(PocketIdClient pocketId, FileInfo localFile, ApplicationImageType imageType, CancellationToken ct)
    {
        if (localFile is null || !localFile.Exists)
        {
            AnsiConsole.MarkupLine($"[red]✗ Not found {localFile?.FullName}[/]");
            return ExitCode.FatalError;
        }
        var result = await ImagePutAsync(pocketId, localFile, imageType, ct);
        if (result.ExitCode != ExitCode.Success)
        {
            AnsiConsole.MarkupLine($"[red]✗ {imageType} image upload failed[/]");
            return result.ExitCode;
        }
        AnsiConsole.MarkupLine($"[Green1]✓ {imageType} image uploaded. (Type: {result.Mimetype}, Size: {result.Content?.Length ?? 0} bytes)[/]");
        return ExitCode.Success;
    }

    private static async Task<ConfigStoreFile> ImagePutAsync(PocketIdClient pocketId, FileInfo localFile, ApplicationImageType imageType, CancellationToken ct)
    {
        try
        {
            var localContent = await File.ReadAllBytesAsync(localFile.FullName, ct);
            var request = new ConfigStoreFile(localContent, MimeTypeUtil.ToMimeType(localFile.Extension), localFile.Name, isSidecar: true);
            ApiResult<int> result = imageType switch
            {
                ApplicationImageType.Background => await pocketId.ApplicationImages.BackgroundImage.PutAsync(request, ct),
                ApplicationImageType.LogoLight => await pocketId.ApplicationImages.Logo(LogoThemeMode.Light).PutAsync(request, ct),
                ApplicationImageType.LogoDark => await pocketId.ApplicationImages.Logo(LogoThemeMode.Dark).PutAsync(request, ct),
                ApplicationImageType.Favicon => await pocketId.ApplicationImages.FavIcon.PutAsync(request, ct),
                ApplicationImageType.Email => await pocketId.ApplicationImages.EmailLogo.PutAsync(request, ct),
                ApplicationImageType.DefaultProfile => await pocketId.ApplicationImages.DefaultProfilePicture.PutAsync(request, ct),
                _ => throw new InvalidOperationException(),
            };
            if (result is null || !result.IsSuccessful)
            {
                request = request with { ExitCode = ExitCode.FatalError, };
                return request;
            }
            return request with { ExitCode = ExitCode.Success };
        }
        catch
        {
            return new ConfigStoreFile(default, MimeTypeUtil.ToMimeType(localFile.Extension), localFile.Name, isSidecar: true, ExitCode: ExitCode.GeneralError);
        }
    }
}
