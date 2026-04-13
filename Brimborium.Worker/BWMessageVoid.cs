using System.Diagnostics.CodeAnalysis;

namespace Brimborium.Worker;

public sealed class BWMessageVoid : IBWMessage {
    private static BWMessageVoid? _Instance;
    public static BWMessageVoid Instance => _Instance ??= new();

    public List<long>? ListChildMessage => throw new NotImplementedException();

    public List<long>? ListNextMessage => throw new NotImplementedException();

    public void AddChildMessage<TMessage>(TMessage childMessage) where TMessage : IBWMessage { }

    public void AddNextMessage<TMessage>(TMessage nextMessage) where TMessage : IBWMessage { }


    public long GetId() => 0;


    public void SetBehaviour<T>(int index, T behaviour) where T : class {    }

    public bool TryGetBehaviour<T>(int index, [MaybeNullWhen(false)] out T behaviour) where T : class {
        behaviour = default;
        return false;
    }
    public void RemoveBehaviour<T>(T behaviour) where T : class {    }
}