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

    Task ReportSuccess(IBWMonitored caller, IBWMessage message, string scope, CancellationToken cancellationToken);

    Task ReportError(IBWMonitored caller, IBWMessage message, string scope, Exception error, CancellationToken cancellationToken);
    Task ReportNextMessage(IBWMessage prevMessage, IBWMessage nextMessage, CancellationToken cancellationToken);
}

public class BWMonitor : IBWMonitor {
    public static readonly string ScopeExecute = "Execute";
    public static readonly string ScopeInvoke = "Invoke";
    public static readonly string ScopeQueue = "Queue";

    public virtual Task ReportLog(IBWMonitored caller, IBWMessage message, string scope, string eventName, Exception? error, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public virtual async Task ReportEvent(IBWMonitored caller, IBWMessage message, string scope, string eventName, CancellationToken cancellationToken) {
        await this.ReportLog(caller, message, scope, eventName, null, cancellationToken);
    }

    public virtual async Task<IBWMonitorScope> ReportBlockStart(IBWMonitored caller, IBWMessage message, string scope, CancellationToken cancellationToken) {
        await this.ReportEvent(caller, message, scope, "Start", cancellationToken);
        return new BWMonitorScope(this, caller, message, scope);
    }

    public virtual async Task ReportSuccess(IBWMonitored caller, IBWMessage message, string scope, CancellationToken cancellationToken) {
        await this.ReportLog(caller, message, scope, "Success", null, cancellationToken);
    }

    public virtual async Task ReportError(IBWMonitored caller, IBWMessage message, string scope, Exception error, CancellationToken cancellationToken) {
        await this.ReportLog(caller, message, scope, "Error", error, cancellationToken);
    }

    public virtual Task ReportNextMessage(IBWMessage prevMessage, IBWMessage nextMessage, CancellationToken cancellationToken) {
        // await this.ReportLog(caller, message, scope, "Success", null, cancellationToken);
        return Task.CompletedTask;
    }
}

public class BWMonitorConsole : BWMonitor {
    public override Task ReportLog(IBWMonitored caller, IBWMessage message, string scope, string eventName, Exception? error, CancellationToken cancellationToken) {
        if (error is null) {
            System.Console.Out.WriteLine($"{caller.Identifier.Identifier} - {message.GetType().Name}#{message.GetId()} - {scope} - {eventName}");
        } else {
            System.Console.Error.WriteLine($"{caller.Identifier.Identifier} - {message.GetType().Name}#{message.GetId()} - {scope} - {eventName} - {error.ToString()}");
        }
        return Task.CompletedTask;
    }

    public override Task ReportNextMessage(IBWMessage prevMessage, IBWMessage nextMessage, CancellationToken cancellationToken) {
        System.Console.Out.WriteLine($"{prevMessage.GetType().Name}#{prevMessage.GetId()} - {nextMessage.GetType().Name}#{nextMessage.GetId()}");
        return Task.CompletedTask;
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

    public Task ReportSuccess(IBWMonitored caller, IBWMessage message, string scope, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public Task ReportError(IBWMonitored caller, IBWMessage message, string scope, Exception error, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    public Task ReportNextMessage(IBWMessage prevMessage, IBWMessage nextMessage, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}

public sealed class BWMonitorScope : IBWMonitorScope {
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

            this._Monitor.ReportEvent(this._Caller, this._Message, this._Scope, "End", CancellationToken.None);
        }
    }

    public async Task ReportError(Exception error, CancellationToken cancellationToken) {
        await this._Monitor.ReportError(this._Caller, this._Message, this._Scope, error, cancellationToken);
    }

    public async Task ReportSuccess(CancellationToken cancellationToken) {
        await this._Monitor.ReportSuccess(this._Caller, this._Message, this._Scope, cancellationToken);
    }
}