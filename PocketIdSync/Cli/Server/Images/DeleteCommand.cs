using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using Spectre.Console;

namespace PocketIdSync.Cli.Server.Images;

[CliCommand(
    Description = "Delete default profile picture from Pocket ID",
    Name = "delete",
    Parent = typeof(ServerImagesCommand)
)]
sealed class DeleteCommand(IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliArgument(Description = "Image type", Arity = CliArgumentArity.ExactlyOne, Required = true)]
    public required ApplicationImageType ImageType { get; set; }

    public async Task<int> RunAsync(CliContext context)
    {
        if (ImageType != ApplicationImageType.DefaultProfile)
        {
            AnsiConsole.MarkupLine($"[red]{ImageType} image doesn't support delete[/]");
            return ExitCode.BadRequest;
        }

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
        foreach (var imgType in imageTypes)
        {
            ApiResult<int> result = imgType switch
            {
                ApplicationImageType.DefaultProfile => await pocketId.ApplicationImages.DefaultProfilePicture.DeleteAsync(context.CancellationToken),
                _ => throw new InvalidOperationException(),
            };
            if (!result.IsSuccessful)
            {
                if (result.Status != System.Net.HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine($"[red]✗ {imgType} image delete failed: {result.Status} {result.Data}[/]");
                    return ExitCode.FatalError;
                }
                AnsiConsole.MarkupLine($"[Orange1]✓ {imgType} image not found, nothing to do[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[Green1]✓ {imgType} image deleted[/]");
            }
        }
        return ExitCode.Success;
    }
}
