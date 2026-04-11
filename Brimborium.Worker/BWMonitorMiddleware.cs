namespace Brimborium.Worker;


public class BWMonitorMiddlewareBuilder<TMessage>
    : IBWMiddlewareBuilder<TMessage>
    where TMessage : IBWMessage {
    private readonly string _Scope;
    private readonly IBWMonitor _Monitor;

    public BWMonitorMiddlewareBuilder(
            string scope,
            IBWMonitor monitor
        ) {
        this._Scope = scope;
        this._Monitor = monitor;
    }

    public IBWMiddleware<TMessage> CreateMiddleware(
        IBWMiddleware<TMessage> caller,
        IBWMiddleware<TMessage> next) {
        if (caller is IBWMonitored monitored) {
            return new BWMonitorMiddleware<TMessage>(
                monitored,
                this._Scope,
                this._Monitor,
                next);
        } else {
            return next;
        }
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

    public async Task ProcessMessage(TMessage message, CancellationToken cancellationToken) {
        using (var monitorScope = this._Monitor.ReportBlockStart(
            this._Caller,
            message,
            this._Scope,
            cancellationToken
            )) { 
            await this._Next.ProcessMessage(message, cancellationToken);
        }
    }
}