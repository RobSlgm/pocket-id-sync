using YamlDotNet.Serialization;

namespace PocketIdSync.ModelSpecs;

interface IKubernetes
{
    string? ApiVersion { get; set; }
    string? Kind { get; set; }
    KubernetesMetadata? Metadata { get; set; }
}

class KubernetesBase : IKubernetes
{
    [YamlMember(Order = -10)]
    public string? ApiVersion { get; set; }

    [YamlMember(Order = -9)]
    public string? Kind { get; set; }

    [YamlMember(Order = -8)]
    public KubernetesMetadata? Metadata { get; set; }
}
