namespace Brimborium.Worker;

/// <summary>
/// TODO
/// </summary>
public interface IBWInvoker {
    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="worker"></param>
    /// <param name="identifier"></param>
    void SetCaller(IBWWorker worker, BWIdentifier identifier);
}

/// <summary>
/// TODO
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public interface IBWInvoker<TMessage>
    : IBWInvoker
    where TMessage : IBWMessage {
    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteAsync(TMessage message, CancellationToken cancellationToken);
}

public abstract class BWInvoker<TMessage>
    : IBWInvoker<TMessage>
    , IBWMonitored
    , IBWMiddleware<TMessage>
    where TMessage : IBWMessage {
    protected readonly IBWMonitor _Monitor;
    protected IBWMonitored _Caller;
    protected BWIdentifier _Identifier;
    protected BWMiddleware<TMessage> Middleware;

    public BWIdentifier Identifier => this._Identifier;

    public IBWMonitor Monitor => this.Monitor;

    public BWInvoker(
            BWMiddlewareBuilder<TMessage>? middlewareBuilder,
            IBWMonitor monitor
        ) {
        this._Caller = this;
        this._Monitor = monitor;
        this._Identifier = new BWIdentifier("BWInvoker");

        this.Middleware = (middlewareBuilder is { })
            ? middlewareBuilder.Build(this, monitor)
            : new BWMiddleware<TMessage>(this);
    }

    public void SetCaller(IBWWorker caller, BWIdentifier identifier) {
        this._Caller = caller;
        this._Identifier = identifier;
        this.Middleware.SetCaller(caller, identifier);
    }

    public async Task ExecuteAsync(TMessage message, CancellationToken cancellationToken) {
        using (var monitorScope = await this._Monitor.ReportBlockStart(
                this._Caller,
                message,
                BWMonitor.ScopeInvoke,
                cancellationToken)) {
            try {
                message.SetBehaviour(0, monitorScope);
                await this.Middleware.ProcessMessageAsync(message, cancellationToken);
                await monitorScope.ReportSuccess(cancellationToken);
            } catch (Exception error) {
                await monitorScope.ReportError(error, cancellationToken);
            }
        }
    }

    Task IBWMiddleware<TMessage>.ProcessMessageAsync(TMessage message, CancellationToken cancellationToken) {
        return this.ExecuteLogicAsync(message, cancellationToken);
    }

    public abstract Task ExecuteLogicAsync(TMessage message, CancellationToken cancellationToken);
}

public static class IBWInvokerExtension {
    extension<TMessage>(IBWInvoker<TMessage> that)
        where TMessage : IBWMessage {
        //public async Task ExecuteAsync<TIncomingMessage>(
        //    TIncomingMessage incomingMessage,
        //    TMessage nextMessage, 
        //    CancellationToken cancellationToken)
        //    where TIncomingMessage : IBWMessage {
        //    incomingMessage.AddNextMessage(nextMessage);
        //    await that.ExecuteAsync(nextMessage, cancellationToken);
        //}

        public async Task InvokeAsync<TIncomingMessage>(
            TIncomingMessage incomingMessage,
            TMessage nextMessage,
            IBWMonitor monitor,
            CancellationToken cancellationToken)
            where TIncomingMessage : IBWMessage {
            await monitor.ReportNextMessage(incomingMessage, nextMessage, cancellationToken);
            await that.ExecuteAsync(nextMessage, cancellationToken);
        }
    }
}

/// <summary>
/// TODO
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public class BWInvokerMediatorCommand<TMessage>
    : BWInvoker<TMessage>
    where TMessage : IBWMessage, Foundatio.Mediator.ICommand {
    private readonly IMediator _Mediator;

    public BWInvokerMediatorCommand(
        IMediator mediator,
        BWMiddlewareBuilder<TMessage>? middlewareBuilder,
        IBWMonitor monitor
    ) :base(
        middlewareBuilder,
        monitor
    ) {
        this._Mediator = mediator;
    }
    public override async Task ExecuteLogicAsync(TMessage message, CancellationToken cancellationToken) {      
        await this._Mediator.InvokeAsync(message, cancellationToken);
    }
}
/// <summary>
/// TODO
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public class BWInvokerMediatorPublish<TMessage>
    : BWInvoker<TMessage>
    where TMessage : IBWMessage, Foundatio.Mediator.INotification {
    private readonly IMediator _Mediator;

    public BWInvokerMediatorPublish(
        IMediator mediator,
        BWMiddlewareBuilder<TMessage>? middlewareBuilder,
        IBWMonitor monitor
    ) : base(
        middlewareBuilder,
        monitor
    ) {
        this._Mediator = mediator;
    }
    public override async Task ExecuteLogicAsync(TMessage message, CancellationToken cancellationToken) {
        await this._Mediator.PublishAsync(message, cancellationToken);
    }
}


public class BWInvokerToList<TMessage>
    : BWInvoker<TMessage>
    where TMessage : IBWMessage {
    public readonly List<TMessage> ListMessages;

    public BWInvokerToList(
            List<TMessage>? listMessages,
            BWMiddlewareBuilder<TMessage>? middlewareBuilder,
            IBWMonitor monitor
        ) : base(
            middlewareBuilder,
            monitor
        ) {
        this.ListMessages = listMessages ?? new();
    }

    public override async Task ExecuteLogicAsync(TMessage message, CancellationToken cancellationToken) {        
        // await this.Monitor.ReportEvent(this._Caller, message, "sink", "done", cancellationToken);
        lock (this.ListMessages) {
            this.ListMessages.Add(message);
        }
        await message.SetCompletionAsync(cancellationToken);
    }
}