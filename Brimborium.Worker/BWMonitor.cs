namespace Brimborium.Worker;

public interface IBWMonitored {
    BWIdentifier Identifier { get; }
    IBWMonitor Monitor { get; }
}

public interface IBWMonitorScope : IDisposable {
    Task ReportError(Exception error);
    Task ReportSuccess();
}

public interface IBWMonitor {
    Task<IBWMonitorScope> ReportStart(IBWMonitored caller, string Scope, CancellationToken cancellationToken);
    Task ReportEnqueue(IBWMonitored caller, IBWMessage message, CancellationToken cancellationToken);
    Task<IBWMonitorScope> ReportExecuteMessage(IBWMonitored caller, IBWMessage message, CancellationToken cancellationToken);
}

public class BWMonitor : IBWMonitor {
    public Task<IBWMonitorScope> ReportStart(IBWMonitored caller, string Scope, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public Task ReportEnqueue(IBWMonitored caller, IBWMessage message, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    public Task<IBWMonitorScope> ReportExecuteMessage(IBWMonitored caller, IBWMessage message, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
}

public class MonitorDefault : IBWMonitor {
    private static IBWMonitor? _Instance;

    public static IBWMonitor Instance => _Instance ??= new MonitorDefault();

    private MonitorDefault() {

    }

    public Task<IBWMonitorScope> ReportStart(IBWMonitored caller, string Scope, CancellationToken cancellationToken) {
        return Task.FromResult<IBWMonitorScope>(new BWMonitorScope());
    }

    public Task ReportEnqueue(IBWMonitored caller, IBWMessage message, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public Task<IBWMonitorScope> ReportExecuteMessage(IBWMonitored caller, IBWMessage message, CancellationToken cancellationToken) {
        return Task.FromResult<IBWMonitorScope>(new BWMonitorScope());
    }
}
public class BWMonitorScope : IBWMonitorScope {
    private bool _IsDisposed;

    public void Dispose() {
        if (this._IsDisposed) {
        } else { 
            this._IsDisposed = true;
        }
    }

    public Task ReportError(Exception error) {
        return Task.CompletedTask;
    }

    public Task ReportSuccess() {
        return Task.CompletedTask;
    }
}