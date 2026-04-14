namespace Brimborium.Worker;

/// <summary>
/// middleware for processing message
/// </summary>
/// <typeparam name="TBWMessage"></typeparam>
public interface IBWMiddleware<TBWMessage>
    where TBWMessage : IBWMessage {
    Task ProcessMessageAsync(TBWMessage message, CancellationToken cancellationToken);
}

public interface IBWMiddlewareBuilder<TMessage>
    where TMessage : IBWMessage {
    IBWMiddleware<TMessage> CreateMiddleware(
            IBWMiddleware<TMessage> caller,
            IBWMiddleware<TMessage> next, 
            IBWMonitor monitor
        );
}

public sealed class BWMiddlewareBuilder<TMessage>
    where TMessage : IBWMessage
    {
    public static BWMiddlewareBuilder<TMessage> CreateDefault(
            string? scope
        ) {
        BWMiddlewareBuilder<TMessage> result = new();
        result.AddMonitorMiddlewareBuilder(scope ?? BWMonitor.ScopeExecute);
        return result;
    }

    private readonly List<IBWMiddlewareBuilder<TMessage>> _ListBuilder = new();
    private bool _Frozen;

    public BWMiddlewareBuilder(params IBWMiddlewareBuilder<TMessage>[] builder) {
        this._ListBuilder.AddRange(builder);
    }

    public BWMiddlewareBuilder<TMessage> Add(
            IBWMiddlewareBuilder<TMessage> builder
        ) {
        if (this._Frozen) {
            throw new InvalidOperationException("frozen");
        }
        this._ListBuilder.Add(builder);
        return this;
    }

    public BWMiddlewareBuilder<TMessage> AddRange(
            params IBWMiddlewareBuilder<TMessage>[] builder
        ) {
        if (this._Frozen) {
            throw new InvalidOperationException("frozen");
        }
        this._ListBuilder.AddRange(builder);
        return this;
    }

    public BWMiddlewareBuilder<TMessage> Freeze() {
        this._Frozen = true;
        return this;
    }

    public BWMiddleware<TMessage> Build(
            IBWMiddleware<TMessage> caller, 
            IBWMonitor monitor
        ) {
        this._Frozen = true;
        {
            var result = caller;
            for (int index = this._ListBuilder.Count - 1;
                0 <= index;
                --index) {
                var builder = this._ListBuilder[index];
                result = builder.CreateMiddleware(caller, result, monitor);
            }
            return new(result);
        }
    }
}

public struct BWMiddleware<TBWMessage> : IBWMiddleware<TBWMessage>
    where TBWMessage : IBWMessage {

    public BWMiddleware(IBWMiddleware<TBWMessage> middleware) {
        this.Middleware = middleware;
    }

    public IBWMiddleware<TBWMessage> Middleware { get; set; }

    public Task ProcessMessageAsync(TBWMessage message, CancellationToken cancellationToken) {
        return this.Middleware.ProcessMessageAsync(message, cancellationToken);
    }

    public void SetCaller(IBWWorker caller, BWIdentifier identifier) {
        if (ReferenceEquals(caller, this.Middleware)) { return; }
        if (this.Middleware is IBWInvoker invoker) { 
            invoker.SetCaller(caller, identifier);
        }
    }
}
