namespace Brimborium.Worker;


/// <summary>
/// The methods that operate with <see cref="IBWMessage"/> must report to <see cref="IBWMonitor"/>.
/// </summary>
public interface IBWMessage {
    List<long>? ListChildMessage { get; }

    List<long>? ListNextMessage { get; }

    long GetId();

    bool TryGetBehaviour<T>(int index, [MaybeNullWhen(false)] out T behaviour) where T : class;

    void SetBehaviour<T>(int index, T behaviour) where T : class;

    void RemoveBehaviour<T>(T behaviour) where T : class;

    void AddNextMessage<TMessage>(TMessage nextMessage) where TMessage : IBWMessage;

    void AddChildMessage<TMessage>(TMessage childMessage) where TMessage : IBWMessage;
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
    private BWMessageBehaviour _Behaviour = new();

    public BWMessageBehaviour Behaviour => this._Behaviour;

    public bool TryGetBehaviour<T>(int index, [MaybeNullWhen(false)] out T behaviour) where T : class {
        lock (this) {
            return this._Behaviour.TryGetBehaviour<T>(index, out behaviour);
        }
    }

    public void SetBehaviour<T>(int index, T behaviour) where T : class {
        lock (this) {
            BWMessageBehaviour.SetBehaviour<T>(ref this._Behaviour, index, behaviour);
        }
    }

    public void RemoveBehaviour<T>(T behaviour) where T : class {
        lock (this) {
            this._Behaviour.RemoveBehaviour<T>(behaviour);
        }
    }

    private List<long>? _ListNextMessage;
    public List<long>? ListNextMessage => this._ListNextMessage;
    public void AddNextMessage<TMessage>(TMessage nextMessage) where TMessage : IBWMessage {
        (this._ListNextMessage ??= new()).Add(nextMessage.GetId());
    }

    private List<long>? _ListChildMessage;
    public List<long>? ListChildMessage => this._ListChildMessage;
    public void AddChildMessage<TMessage>(TMessage childMessage) where TMessage : IBWMessage {
        (this._ListChildMessage ??= new()).Add(childMessage.GetId());
        //childMessage.SetParentMessage(this)
    }
}

public class BWMessage : BWMessageBase {
    public BWMessage() {
    }
}

/*
public class BWMessageCommand
    : BWMessageBase
    , Foundatio.Mediator.ICommand {
    public BWMessageCommand() {
    }

    [Foundatio.Mediator.Handler]
    public virtual async Task HandleAsync(
        BWMessageCommand command,
        Foundatio.Mediator.IMediator mediator,
        CancellationToken cancellationToken
        ) {
        await command.InvokeAsync(mediator, cancellationToken);
    }

    protected async Task InvokeAsync(IMediator mediator, CancellationToken cancellationToken) {
        await mediator.InvokeAsync(this, cancellationToken);
    }
}
*/