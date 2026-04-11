using System.Diagnostics.CodeAnalysis;

namespace Brimborium.Worker;

public sealed class BWMessageVoid : IBWMessage {
    private static BWMessageVoid? _Instance;
    public static BWMessageVoid Instance => _Instance ??= new();

    public long GetId() => 0;

    public bool TryAddBehaviour<T>(T behaviour) where T : IBWBehaviour
        => false;

    public bool TryGetBehaviour<T>([MaybeNullWhen(false)] out T behaviour) where T : IBWBehaviour {
        behaviour = default;
        return false;
    }
}