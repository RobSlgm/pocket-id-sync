using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PocketIdSync.Utils;


public class StaticByteArrayConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(byte[]);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        if (!parser.Accept<Scalar>(out var scalar))
        {
            parser.MoveNext();
            return Array.Empty<byte>();
        }

        parser.Consume<Scalar>();
        if (string.IsNullOrEmpty(scalar.Value)) return Array.Empty<byte>();
        try
        {
            string cleanBase64 = scalar.Value.Replace("\n", "").Replace("\r", "").Trim();
            return Convert.FromBase64String(cleanBase64);
        }
        catch (FormatException)
        {
            throw new YamlException(scalar.Start, scalar.End, "Invalid Base64 format for byte array.");
        }
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer rootSerializer)
    {
        if (value == null) return;

        var bytes = (byte[])value;
        var base64 = Convert.ToBase64String(bytes, Base64FormattingOptions.None);
        // var base64 = Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks);

        emitter.Emit(new Scalar(
            default,
            "tag:yaml.org,2002:binary",
            base64,
            ScalarStyle.Folded,
            isPlainImplicit: true,
            isQuotedImplicit: false
        ));
    }
}
