using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace Brimborium.Worker;


/// <summary>
/// The methods that operate with <see cref="IBWMessage"/> must report to <see cref="IBWMonitor"/>.
/// </summary>
public interface IBWMessage {
    long GetId();

    bool TryGetBehaviour<T>([MaybeNullWhen(false)] out T behaviour);

    /*
    Task<TMessage> AddChildMessage<TMessage>(TMessage message)
        where TMessage : IBWMessage;

    Task<TMessage> AddNextMessage<TMessage>(TMessage message)
        where TMessage : IBWMessage;
    */

    IBWMessageWithValue<TNextValue> CreateWithValue<TNextValue>(TNextValue nextValue);
    IBWMessageWithResult<TNextResult> CreateWithResult<TNextResult>(IBWMessageResult<TNextResult> nextResult);
    IBWMessageWithScope<TNextScope> CreateWithScope<TNextScope>(TNextScope nextScope);
}

public static class BWMessageId {
    private static long _Id = 1;
    public static long GetId() {
        return System.Threading.Interlocked.Increment(ref _Id);
    }
}

public abstract class BWMessageBase : IBWMessage {
    private long _Id;

    public long GetId() {
        if (this._Id == 0) {
            this._Id = BWMessageId.GetId();
        }
        return this._Id;
    }

    /*
    private SemaphoreSlim? _SemaphoreRelatives;
    public SemaphoreSlim GetSemaphoreRelatives() {
        if (this._SemaphoreRelatives is null) {
            System.Threading.Interlocked.CompareExchange(
                ref this._SemaphoreRelatives,
                new(1, 1),
                null
            );
            return this._SemaphoreRelatives;
        } else {
            return this._SemaphoreRelatives;
        }

    }

    private Dictionary<IBWMessage, IBWMessage>? _ListChild;
    public async Task<TMessage> AddChildMessage<TMessage>(TMessage message)
        where TMessage : IBWMessage {
        var semaphore = this.GetSemaphoreRelatives();
        await semaphore.WaitAsync();
        try {
            (this._ListChild ??= new()).TryAdd(message, message);
            return message;
        } finally {
            semaphore.Release();
        }
    }

    private Dictionary<IBWMessage, IBWMessage>? _ListNext;
    public async Task<TMessage> AddNextMessage<TMessage>(TMessage message)
        where TMessage : IBWMessage {
        var semaphore = this.GetSemaphoreRelatives();
        await semaphore.WaitAsync();
        try {
            (this._ListNext ??= new()).TryAdd(message, message);
            return message;
        } finally {
            semaphore.Release();
        }
    }
    */

    public abstract IBWMessageWithValue<TNextValue> CreateWithValue<TNextValue>(TNextValue nextValue);

    public abstract IBWMessageWithResult<TNextResult> CreateWithResult<TNextResult>(IBWMessageResult<TNextResult> nextResult);

    public abstract IBWMessageWithScope<TNextScope> CreateWithScope<TNextScope>(TNextScope nextScope);

    private BWBehaviour _Behaviour = new();
    public bool TryGetBehaviour<T>([MaybeNullWhen(false)] out T behaviour) {
        throw new NotImplementedException();
    }
}

public struct BWBehaviour { }

public sealed class BWMessage : BWMessageBase {
    public BWMessage() {        
    }

    public override IBWMessageWithValue<TNextValue> CreateWithValue<TNextValue>(TNextValue nextValue) {
        return new BWMessageWithValue<TNextValue>(nextValue);
    }

    public override IBWMessageWithResult<TNextResult> CreateWithResult<TNextResult>(IBWMessageResult<TNextResult> nextResult) {
        return new BWMessageWithResult<TNextResult>(nextResult);
    }

    public override IBWMessageWithScope<TNextScope> CreateWithScope<TNextScope>(TNextScope nextScope) {
        return new BWMessageWithScope<TNextScope>(nextScope);
    }
}