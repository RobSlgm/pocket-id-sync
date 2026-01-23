namespace PocketIdSync.ModelSpecs;

class KubernetesSpec<T> : KubernetesBase
{
    public T? Spec { get; set; }
}
