namespace Brimborium.Worker;

public interface IBWWorker : IBWMonitored {
}

public interface IBWWorkerWithQueue : IBWWorker, IBWMonitored {
    /// <summary>
    /// Starts the monitor
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stop this worker, if all work is done.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Waits until all messages are processed and <see cref="StopAsync(CancellationToken)"/> was called.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CompletionAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Process messages
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public interface IBWWorker<TMessage>
    : IBWWorker
    where TMessage : IBWMessage {
    /// <summary>
    /// Process a message
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteMessageAsync(TMessage message, CancellationToken cancellationToken);
}

/// <summary>
/// Base worker
/// </summary>
public class BWWorker : BWMonitored, IBWWorker {
    public BWWorker(
        BWIdentifier identifier,
        IBWMonitor monitor
        ) : base(identifier, monitor){
    }
}

public abstract class BWWorker<TMessage>
    : BWWorker
    , IBWWorker<TMessage>
    , IBWMiddleware<TMessage>
    where TMessage : IBWMessage {

    public BWWorker(
        BWMiddlewareBuilder<TMessage>? middlewareBuilder,
        BWIdentifier identifier, IBWMonitor monitor
    ) : base(
        identifier, monitor
    ) {
        this.Middleware = (
                middlewareBuilder
                ?? BWMiddlewareBuilder<TMessage>.CreateDefault(default)
            ).Build(this, monitor);
    }

    protected TBWInvoker RegisterNext<TBWInvoker>(
        TBWInvoker next,
        BWIdentifier identifier
    ) where TBWInvoker : IBWInvoker {
        next.SetCaller(this, identifier);
        return next;
    }

    protected BWMiddleware<TMessage> Middleware;

    public virtual async Task ExecuteMessageAsync(TMessage message, CancellationToken cancellationToken) {
        await this.Middleware.ProcessMessageAsync(message, cancellationToken);
    }

    Task IBWMiddleware<TMessage>.ProcessMessageAsync(TMessage message, CancellationToken cancellationToken) {
        return this.ExecuteLogic(message, cancellationToken);
    }

    public abstract Task ExecuteLogic(TMessage message, CancellationToken cancellationToken);
}
