namespace Brimborium.Worker;

public class BWWorkerWithQueue : BWWorker, IBWWorkerWithQueue, IBWMonitored {
    public BWWorkerWithQueue(
            BWIdentifier identifier,
            IBWMonitor monitor
        ) : base(identifier, monitor) {
    }

    protected SemaphoreSlim? _SemaphoreExecution;
    protected Task? _TaskExecute;
    public virtual async Task StartAsync(CancellationToken cancellationToken) {
        if (_TaskExecute is null) {
            System.Threading.Interlocked.CompareExchange(
                ref this._SemaphoreExecution,
                new(1, 1),
                null
            );
            await this._SemaphoreExecution.WaitAsync(cancellationToken);
            try {
                if (_TaskExecute is null) {
                    var monitorScope = await this.Monitor.ReportStart(this, "Execute", cancellationToken);
                    this._TaskExecute = this.ExecuteAsync(monitorScope, cancellationToken);
                }
            } finally {
                this._SemaphoreExecution.Release();
            }
        }
    }

    public virtual async Task ExecuteAsync(IBWMonitorScope monitorScope, CancellationToken cancellationToken) {
        monitorScope.Dispose();
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public Task CompletionAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}

public class BWWorkerWithQueue<TValue, TMessage>
    : BWWorkerWithQueue
    , IBWWorker<TValue, TMessage>
    where TMessage : IBWMessageWithValue<TValue> {
    private readonly IBWQueue<TValue, TMessage> _Queue;

    public BWWorkerWithQueue(
            IBWQueue<TValue, TMessage> queue,
            BWIdentifier identifier,
            IBWMonitor monitor
        ) : base(
            identifier,
            monitor
        ) {
        this._Queue = queue;
    }

    public async Task Execute(TMessage message, CancellationToken cancellationToken) {
        await this.Monitor.ReportEnqueue(this, message, cancellationToken);
        await this._Queue.Enqueue(message, cancellationToken);
    }

    public override async Task ExecuteAsync(IBWMonitorScope monitorScope, CancellationToken cancellationToken) {
        try {
            while (await this._Queue.WaitToReadAsync(cancellationToken)) {
                while (this._Queue.TryRead(out var message)) {
                    var messageScope = await this.Monitor.ReportExecuteMessage(this, message, cancellationToken);
                    try {
                        await this.ExecuteInner(message, cancellationToken);
                    } catch (Exception error) {
                        await messageScope.ReportError(error);
                    } finally {
                        messageScope.Dispose();
                    }
                }
            }
            await monitorScope.ReportSuccess();
        } catch (Exception error) {
            await monitorScope.ReportError(error);
        } finally {
            monitorScope.Dispose();
        }
    }

    protected virtual Task ExecuteInner(TMessage message, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
