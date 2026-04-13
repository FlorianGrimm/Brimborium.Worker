namespace Brimborium.Worker;

public static partial class BWMiddlewareBuilderExtension {
    extension<TMessage>(BWMiddlewareBuilder<TMessage> that)
        where TMessage : IBWMessage {
        public BWMiddlewareBuilder<TMessage> AddBWMonitorMiddlewareBuilder(
                string scope
            ) {
            var builder = new BWMonitorMiddlewareBuilder<TMessage>(scope);
            return that.Add(builder);
        }
    }
}

public class BWMonitorMiddlewareBuilder<TMessage>
    : IBWMiddlewareBuilder<TMessage>
    where TMessage : IBWMessage {
    private readonly string _Scope;

    public BWMonitorMiddlewareBuilder(
            string scope
        ) {
        this._Scope = scope;
    }

    public IBWMiddleware<TMessage> CreateMiddleware(
            IBWMiddleware<TMessage> caller,
            IBWMiddleware<TMessage> next,
            IBWMonitor monitor
        ) {
        if (caller is not IBWMonitored monitored) {
            monitored = new BWMonitored(new BWIdentifier(""), monitor);
        }

        return new BWMonitorMiddleware<TMessage>(
            monitored,
            this._Scope,
            monitor,
            next);
    }
}

public class BWMonitorMiddleware<TMessage>
    : IBWMiddleware<TMessage>
    where TMessage : IBWMessage {
    private readonly IBWMonitored _Caller;
    private readonly string _Scope;
    private readonly IBWMonitor _Monitor;
    private readonly IBWMiddleware<TMessage> _Next;

    public BWMonitorMiddleware(
            IBWMonitored caller,
            string scope,
            IBWMonitor monitor,
            IBWMiddleware<TMessage> next
        ) {
        this._Caller = caller;
        this._Scope = scope;
        this._Monitor = monitor;
        this._Next = next;
    }

    public async Task ProcessMessageAsync(TMessage message, CancellationToken cancellationToken) {
        using (var monitorScope = await this._Monitor.ReportBlockStart(
            this._Caller,
            message,
            this._Scope,
            cancellationToken
            )) {
            message.SetBehaviour(0, monitorScope);
            await this._Next.ProcessMessageAsync(message, cancellationToken);
            message.RemoveBehaviour(monitorScope);
        }
    }
}