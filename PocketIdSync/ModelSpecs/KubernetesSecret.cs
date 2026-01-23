namespace PocketIdSync.ModelSpecs;

class KubernetesSecret<T> : KubernetesBase
{
    public string Type { get; set; } = "Opaque";
    public T? Data { get; set; }
}
