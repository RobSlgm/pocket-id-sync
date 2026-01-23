using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PocketIdSync.Utils;

sealed class YamlHelper
{
    public IDeserializer Deserializer { get; init; }
    public ISerializer Serializer { get; init; }

    public YamlHelper()
    {
        Deserializer = new StaticDeserializerBuilder(new SourceGenerationYamlContext())
            // .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTagMapping("tag:yaml.org,2002:binary", typeof(byte[]))
            .WithTypeConverter(new StaticByteArrayConverter())
            // .WithTagMapping("!binary", typeof(byte[]))
            .Build();
        Serializer = new StaticSerializerBuilder(new SourceGenerationYamlContext())
            .WithQuotingNecessaryStrings()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            // .WithTagMapping("!binary", typeof(byte[]))
            .WithTagMapping("tag:yaml.org,2002:binary", typeof(byte[]))
            .WithTypeConverter(new StaticByteArrayConverter())
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitEmptyCollections)
            .WithNewLine("\n")
            .WithIndentedSequences()
            .EnsureRoundtrip()
            .Build();
    }

    public string Write<T>(T data)
    {
        return Serializer.Serialize(data);
    }

    public async Task<T?> ReadAsync<T>(FileInfo file, CancellationToken ct) => await ReadAsync<T>(file.FullName, ct);

    public async Task<T?> ReadAsync<T>(string filename, CancellationToken ct)
    {
        var jsonString = await File.ReadAllTextAsync(filename, ct);
        if (string.IsNullOrEmpty(jsonString))
        {
            return default;
        }
        var data = Deserializer.Deserialize<T>(jsonString);
        return data;
    }
}
