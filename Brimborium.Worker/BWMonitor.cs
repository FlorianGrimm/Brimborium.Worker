namespace Brimborium.Worker;

public interface IBWMonitored {
    BWIdentifier Identifier { get; }
    IBWMonitor Monitor { get; }
}

public class BWMonitored : IBWMonitored {
    public BWMonitored(
            BWIdentifier identifier,
            IBWMonitor monitor 
        ) {
        this.Identifier = identifier;
        this.Monitor = monitor;
    }
    public BWIdentifier Identifier { get; set; }

    public IBWMonitor Monitor { get; set; }
}


public interface IBWMonitorScope : IDisposable {
    Task ReportError(Exception error, CancellationToken cancellationToken);
    Task ReportSuccess(CancellationToken cancellationToken);
}

public interface IBWMonitor {
    Task ReportEvent(IBWMonitored caller, IBWMessage message, string scope, string eventName, CancellationToken cancellationToken);
    Task<IBWMonitorScope> ReportBlockStart(IBWMonitored caller, IBWMessage message, string scope, CancellationToken cancellationToken);
}

public class BWMonitor : IBWMonitor {
    public static readonly string ScopeExecute = "Execute";
    public static readonly string ScopeInvoke = "Invoke";

    public virtual Task ReportEvent(IBWMonitored caller, IBWMessage message, string scope, string eventName, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public virtual async Task<IBWMonitorScope> ReportBlockStart(IBWMonitored caller, IBWMessage message, string scope, CancellationToken cancellationToken) {
        await this.ReportEvent(caller, message, scope, "Start", cancellationToken);
        return new BWMonitorScope(this, caller, message, scope);
    }
}

public class BWMonitorNull : IBWMonitor {
    private static IBWMonitor? _Instance;

    public static IBWMonitor Instance => _Instance ??= new BWMonitorNull();

    private BWMonitorNull() {

    }

    public Task ReportEvent(IBWMonitored caller, IBWMessage message, string scope, string eventName, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public async Task<IBWMonitorScope> ReportBlockStart(IBWMonitored caller, IBWMessage message, string scope, CancellationToken cancellationToken) {
        await this.ReportEvent(caller, message, scope, "Start", cancellationToken);
        return new BWMonitorScope(this, caller, message, scope);
    }
}
public class BWMonitorScope : IBWMonitorScope {
    private bool _IsDisposed;
    private readonly IBWMonitor _Monitor;
    private readonly IBWMonitored _Caller;
    private readonly IBWMessage _Message;
    private readonly string _Scope;

    public BWMonitorScope(IBWMonitor monitor, IBWMonitored caller, IBWMessage message, string scope) {
        this._Monitor = monitor;
        this._Caller = caller;
        this._Message = message;
        this._Scope = scope;
    }

    public void Dispose() {
        if (this._IsDisposed) {
        } else {
            this._IsDisposed = true;
        }
    }

    public Task ReportError(Exception error, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public Task ReportSuccess(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}