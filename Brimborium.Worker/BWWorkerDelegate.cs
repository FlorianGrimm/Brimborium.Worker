namespace Brimborium.Worker;

public class BWWorkerDelegate<TMessageIn, TMessageOut>
    : BWWorker<TMessageIn>
    where TMessageIn : IBWMessage
    where TMessageOut : IBWMessage {
    protected readonly IBWInvoker<TMessageOut> _InvokerNext;
    protected readonly Func<TMessageIn, IBWInvoker<TMessageOut>, CancellationToken, Task> _InvokerDelegate;

    public BWWorkerDelegate(
        Func<TMessageIn, IBWInvoker<TMessageOut>, CancellationToken, Task> invokerDelegate,
        IBWInvoker<TMessageOut> invokerNext,
        BWMiddlewareBuilder<TMessageIn>? middlewareBuilder,
        BWIdentifier identifier, IBWMonitor monitor
    ) : base(
        middlewareBuilder,
        identifier, monitor
    ) {
        this._InvokerDelegate = invokerDelegate;
        this._InvokerNext = invokerNext;
    }

    public override async Task ExecuteLogic(TMessageIn message, CancellationToken cancellationToken) {
        await this._InvokerDelegate(message, this._InvokerNext, cancellationToken).ConfigureAwait(false);
    }
}