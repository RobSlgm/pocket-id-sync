using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using PocketIdSync.Models;
using Spectre.Console;
using Spectre.Console.Json;

namespace PocketIdSync.Utils;

sealed class JsonHelper
{
    public JsonSerializerOptions Options { get; init; }

    public JsonHelper()
    {
        Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = SourceGenerationContext.Default,
        };
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public void WriteConsole<T>(T client)
    {
        var json = JsonSerializer.Serialize(client, Options);
        if (!string.IsNullOrWhiteSpace(json))
        {
            var jsonText = new JsonText(json);
            AnsiConsole.Write(jsonText);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No content[/]");
        }
        AnsiConsole.MarkupLine("");
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<T?> ReadAsync<T>(FileInfo file)
    {
        var jsonString = await File.ReadAllTextAsync(file.FullName);
        if (string.IsNullOrEmpty(jsonString))
        {
            AnsiConsole.MarkupLine($"[red]File `{file.FullName}` is empty[/]");
            return default;
        }
        var data = JsonSerializer.Deserialize<T>(jsonString, Options);
        return data;
    }
}
