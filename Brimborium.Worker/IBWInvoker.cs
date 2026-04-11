using Foundatio.Mediator;

namespace Brimborium.Worker;

public interface IBWInvoker<TMessage>
    where TMessage : IBWMessage {
    Task ExecuteAsync(TMessage message, CancellationToken cancellationToken);
}

public class BWInvokerMediator<TMessage>
    : IBWInvoker<TMessage>
    where TMessage : IBWMessage {
    private readonly IMediator _Mediator;

    public BWInvokerMediator(
            IMediator mediator
        ) {
        this._Mediator = mediator;
    }

    public async Task ExecuteAsync(
        TMessage message,
        CancellationToken cancellationToken) {
        await this._Mediator.InvokeAsync(message, cancellationToken);
    }
}

public class BWInvokerToList<TMessage>
    : IBWInvoker<TMessage>
    where TMessage : IBWMessage {
    public readonly List<TMessage> ListMessages;

    public BWInvokerToList(
            List<TMessage>? listMessages = default
        ) {
        this.ListMessages = listMessages ?? new();
    }

    public Task ExecuteAsync(
        TMessage message,
        CancellationToken cancellationToken) {
        lock (this.ListMessages) {
            this.ListMessages.Add(message);
        }
        return Task.CompletedTask;
    }
}