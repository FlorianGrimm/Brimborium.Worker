using System.Net.Http.Headers;

namespace Brimborium.Worker;


public record class BWIdentifier(
    string Identifier
);

public interface IBWWorker : IBWMonitored {
}

public interface IBWWorkerWithQueue : IBWMonitored {
    /// <summary>
    /// Starts the monitor
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken);
    /// <summary>
    /// Stop if all work is done.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StopAsync(CancellationToken cancellationToken);
    Task CompletionAsync(CancellationToken cancellationToken);
}

public interface IBWWorker<TMessage>
    : IBWWorker
    where TMessage : IBWMessage {
    Task HandleMessage(TMessage message, CancellationToken cancellationToken);
}

public interface IBWWorker<TValue, TMessage>
    : IBWWorker
    where TMessage : IBWMessageWithValue<TValue> {
    Task Execute(TMessage message, CancellationToken cancellationToken);
}

public class BWWorker : IBWWorker, IBWMonitored {
    public BWWorker(
        BWIdentifier identifier,
        IBWMonitor monitor
        ) {
        this.Monitor = monitor;
        Identifier = identifier;
    }

    public IBWMonitor Monitor { get; set; }

    public BWIdentifier Identifier { get; set; }

}
public interface IBWMiddleware<TBWMessage>
    where TBWMessage : IBWMessage {
    Task ProcessMessage(TBWMessage message, CancellationToken cancellationToken);
}

//public interface IBWOutgoingware<TBWMessage>
//    where TBWMessage : IBWMessage {
//    Task Process(TBWMessage message, CancellationToken cancellationToken);
//}

public class BWWorker<TMessage>
    : BWWorker
    , IBWWorker<TMessage>
    , IBWMiddleware<TMessage>
    where TMessage : IBWMessage {
    public BWWorker(
            IBWMiddleware<TMessage>? middleware,
            BWIdentifier identifier, IBWMonitor monitor
        ) : base(
            identifier, monitor
        ) {
        this.Middleware = middleware ?? this;
    }
   
    protected IBWMiddleware<TMessage> Middleware;

    public virtual async Task HandleMessage(TMessage message, CancellationToken cancellationToken) {
        await this.Middleware.ProcessMessage(message, cancellationToken);
    }

    Task IBWMiddleware<TMessage>.ProcessMessage(TMessage message, CancellationToken cancellationToken) {
        return this.ExecuteMessage(message, cancellationToken);
    }

    public virtual Task ExecuteMessage(TMessage message, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
