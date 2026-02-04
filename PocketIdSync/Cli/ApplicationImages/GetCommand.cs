using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.ApplicationImages;

[CliCommand(
    Description = "Get application images from Pocket ID",
    Name = "get",
    Parent = typeof(ApplicationImagesCommand)
)]
sealed class GetCommand(IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliArgument(Description = "Image type", Arity = CliArgumentArity.ZeroOrOne)]
    public ApplicationImageType ImageType { get; set; } = ApplicationImageType.All;

    [CliOption(Description = "Root directory of file store for configuration", Alias = "d", ValidationRules = CliValidationRules.ExistingDirectory)]
    public DirectoryInfo StoreRoot { get; set; } = new DirectoryInfo(".");

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
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
        var imgPath = Directory.CreateDirectory(Path.Combine(StoreRoot.FullName, "images"));
        foreach (var imgType in imageTypes)
        {
            var image = await GetImageAsync(pocketId, imgType, context.CancellationToken);
            if (image is null)
            {
                AnsiConsole.MarkupLine($"[red]✗ Failed to read {imgType} image[/]");
                return ExitCode.BadRequest;
            }
            AnsiConsole.MarkupLine($"[Green]✓ Image for {imgType} read. (Type: {image.MimeType}, Size: {image.ContentLength} bytes)[/]");
            if (image.Data is null || image.Data.Length == 0)
            {
                return ExitCode.BadRequest;
            }
            try
            {
                var extension = MimeTypeUtil.FromMimeType(image.MimeType);
                var fileInfo = new FileInfo(Path.Combine(imgPath.FullName, $"{imgType}{extension}"));
                var isEqual = false;
                if (fileInfo.Exists)
                {
                    var localContent = await File.ReadAllBytesAsync(fileInfo.FullName, context.CancellationToken);
                    if (localContent.SequenceEqual(image.Data))
                    {
                        isEqual = true;
                    }
                }
                if (!isEqual)
                {
                    await File.WriteAllBytesAsync(fileInfo.FullName, image.Data, context.CancellationToken);
                    AnsiConsole.MarkupLine($"[Orange1]✓ {imgType} written to {fileInfo.FullName}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[Green1]✓ {imgType} skipped as unchanged from {fileInfo.FullName}[/]");
                }
            }
            catch
            {
                AnsiConsole.MarkupLine($"[red]✗ Failed to write image to local file store[/]");
                return ExitCode.FatalError;
            }
        }
        return ExitCode.Success;
    }

    private static async Task<ApiResult<byte[]>?> GetImageAsync(PocketIdClient pocketId, ApplicationImageType imageType, CancellationToken ct)
    {
        ApiResult<byte[]>? image;
        switch (imageType)
        {
            case ApplicationImageType.Background:
                image = await pocketId.ApplicationImages.BackgroundImage.GetAsync(ct);
                break;
            case ApplicationImageType.DefaultProfile:
                image = await pocketId.ApplicationImages.DefaultProfilePicture.GetAsync(ct);
                break;
            case ApplicationImageType.Email:
                image = await pocketId.ApplicationImages.EmailLogo.GetAsync(ct);
                break;
            case ApplicationImageType.Favicon:
                image = await pocketId.ApplicationImages.FavIcon.GetAsync(ct);
                break;
            case ApplicationImageType.LogoLight:
                image = await pocketId.ApplicationImages.Logo(LogoThemeMode.Light).GetAsync(ct);
                break;
            case ApplicationImageType.LogoDark:
                image = await pocketId.ApplicationImages.Logo(LogoThemeMode.Dark).GetAsync(ct);
                break;
            default:
                return default;
        }
        return image;
    }
}
