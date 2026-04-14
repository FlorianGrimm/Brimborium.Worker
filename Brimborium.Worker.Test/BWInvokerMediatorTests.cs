using Foundatio.Mediator;

using Microsoft.Extensions.DependencyInjection;

namespace Brimborium.Worker;

public class BWInvokerMediatorTests {
    [Test]
    public async Task BWInvokerMediatorTest001() {
        CancellationTokenSource cts = new CancellationTokenSource();
        Microsoft.Extensions.DependencyInjection.ServiceCollection serviceBuilder = new();
        IBWMonitor monitor = new BWMonitorConsole();
        serviceBuilder.AddSingleton<IBWMonitor>(monitor);
        serviceBuilder.AddMediator((options) => {
            options.AddAssembly<IBWMonitor>();
            options.AddAssembly<BWInvokerMediatorTests>();
        });
        serviceBuilder.AddBrimboriumWorker();
        serviceBuilder.AddBrimboriumWorkerTest();
        var globalServiceProvider = serviceBuilder.BuildServiceProvider();
        using var scope = globalServiceProvider.CreateScope();
        var scopeServiceProvider = scope.ServiceProvider;
        IMediator mediator = scopeServiceProvider.GetRequiredService<IMediator>();
        BWInvokerMediator1Command cmd1 = new(1);
        await mediator.InvokeAsync(cmd1, cts.Token);
        var invokerMediator3Worker = scopeServiceProvider.GetRequiredService<BWInvokerMediator3Worker>();
        var actList = invokerMediator3Worker.List.Select(item => item.Value).ToList();
        await Assert.That(actList).IsEquivalentTo(new List<int>() { 3, 4 });

    }
}

public class BWInvokerMediator1Command(
        int value
    ) : BWMessageBase
    , Foundatio.Mediator.ICommand {
    public int Value { get; } = value;
}

public class BWInvokerMediator1Worker : BWWorker<BWInvokerMediator1Command> {
    [Injectio.Attributes.RegisterServices]
    public static void Register(IServiceCollection services) {
        services.AddSingleton<BWInvokerMediator1Worker>(
            (sp) => {
                IBWMonitor monitor = sp.GetRequiredService<IBWMonitor>();
                IMediator mediator = sp.GetRequiredService<IMediator>();
                BWInvokerMediatorCommand<BWInvokerMediator2Command> next = new(mediator, default, monitor);
                var middlewareBuilder = new BWMiddlewareBuilder<BWInvokerMediator1Command>()
                    .AddMonitorMiddlewareBuilder(BWMonitor.ScopeExecute);
                return new BWInvokerMediator1Worker(next, middlewareBuilder, monitor);
            });
    }

    [Foundatio.Mediator.Handler]
    public static async Task HandleAsync(
        BWInvokerMediator1Command command,
        BWInvokerMediator1Worker worker,
        CancellationToken cancellationToken
        ) {
        await worker.ExecuteMessageAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public static readonly BWIdentifier ClassIdentifier = new(nameof(BWInvokerMediator1Worker));
    private readonly IBWInvoker<BWInvokerMediator2Command> _Next;

    public BWInvokerMediator1Worker(
            IBWInvoker<BWInvokerMediator2Command> next,
            BWMiddlewareBuilder<BWInvokerMediator1Command> middlewareBuilder,
            IBWMonitor monitor
        ) : base(
            middlewareBuilder,
            ClassIdentifier, monitor
        ) {
        this._Next = this.RegisterNext(next, this.Identifier.CreateChild("Next"));
    }

    protected override async Task ExecuteLogic(BWInvokerMediator1Command message, CancellationToken cancellationToken) {
        if (message.Value <= 0) {
            await this.Monitor.ReportEvent(this, message, "", "", cancellationToken);
            return;
        }

        {
            BWInvokerMediator2Command next = new(message.Value * 2);
            await this._Next.InvokeAsync(message, next, this.Monitor, cancellationToken);
        }
        {
            BWInvokerMediator2Command next = new(message.Value * 2 + 1);
            await this._Next.InvokeAsync(message, next, this.Monitor, cancellationToken);
        }
    }
}

public class BWInvokerMediator2Command(
        int value
    ) : BWMessageBase
    , Foundatio.Mediator.ICommand {
    public int Value { get; } = value;
}


public class BWInvokerMediator2Worker : BWWorker<BWInvokerMediator2Command> {
    [Injectio.Attributes.RegisterServices]
    public static void Register(IServiceCollection services) {
        services.AddSingleton<BWInvokerMediator2Worker>(
            (sp) => {
                IBWMonitor monitor = sp.GetRequiredService<IBWMonitor>();
                IMediator mediator = sp.GetRequiredService<IMediator>();
                BWInvokerMediatorCommand<BWInvokerMediator3Command> next = new(mediator, default, monitor);
                var middlewareBuilder = new BWMiddlewareBuilder<BWInvokerMediator2Command>()
                    .AddMonitorMiddlewareBuilder(BWMonitor.ScopeExecute);
                return new BWInvokerMediator2Worker(next, middlewareBuilder, monitor);
            });
    }

    [Foundatio.Mediator.Handler]
    public static async Task HandleAsync(
        BWInvokerMediator2Command command,
        BWInvokerMediator2Worker worker,
        CancellationToken cancellationToken
        ) {
        await worker.ExecuteMessageAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public static readonly BWIdentifier ClassIdentifier = new(nameof(BWInvokerMediator2Worker));
    private readonly IBWInvoker<BWInvokerMediator3Command> _Next;

    public BWInvokerMediator2Worker(
            IBWInvoker<BWInvokerMediator3Command> next,
            BWMiddlewareBuilder<BWInvokerMediator2Command> middlewareBuilder,
            IBWMonitor monitor
        ) : base(
            middlewareBuilder,
            ClassIdentifier, monitor
        ) {
        this._Next = this.RegisterNext(next, this.Identifier.CreateChild("Next"));
    }

    protected override async Task ExecuteLogic(BWInvokerMediator2Command message, CancellationToken cancellationToken) {
        BWInvokerMediator3Command next = new(message.Value + 1);
        await this._Next.InvokeAsync(message, next, this.Monitor, cancellationToken);
    }
}

public class BWInvokerMediator3Command(
        int value
    ) : BWMessageBase
    , Foundatio.Mediator.ICommand {
    public int Value { get; } = value;
}

public class BWInvokerMediator3Worker : BWWorker<BWInvokerMediator3Command> {
    [Injectio.Attributes.RegisterServices]
    public static void Register(IServiceCollection services) {
        services.AddSingleton<BWInvokerMediator3Worker>(
            (sp) => {
                IBWMonitor monitor = sp.GetRequiredService<IBWMonitor>();
                IMediator mediator = sp.GetRequiredService<IMediator>();
                var middlewareBuilder = new BWMiddlewareBuilder<BWInvokerMediator3Command>()
                    .AddMonitorMiddlewareBuilder(BWMonitor.ScopeExecute);
                return new BWInvokerMediator3Worker(middlewareBuilder, monitor);
            });
    }

    [Foundatio.Mediator.Handler]
    public static async Task HandleAsync(
        BWInvokerMediator3Command command,
        BWInvokerMediator3Worker worker,
        CancellationToken cancellationToken
        ) {
        await worker.ExecuteMessageAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public static readonly BWIdentifier ClassIdentifier = new(nameof(BWInvokerMediator3Worker));
    public readonly List<BWInvokerMediator3Command> List = new();

    public BWInvokerMediator3Worker(
            BWMiddlewareBuilder<BWInvokerMediator3Command> middlewareBuilder,
            IBWMonitor monitor
        ) : base(
            middlewareBuilder,
            ClassIdentifier, monitor
        ) {
    }

    protected override async Task ExecuteLogic(BWInvokerMediator3Command message, CancellationToken cancellationToken) {
        lock (this.List) {
            this.List.Add(message);
        }

        System.Console.Out.WriteLine($"{this.Identifier.Identifier} - Result - {message.Value}");
    }
}
