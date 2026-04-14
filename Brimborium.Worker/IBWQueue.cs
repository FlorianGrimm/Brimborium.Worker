using System.Threading.Channels;

namespace Brimborium.Worker;

public interface IBWQueue<TMessage>
    where TMessage : IBWMessage {
    Task Enqueue(TMessage message, CancellationToken cancellationToken);
    ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken);
    bool TryRead([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TMessage item);
    Task StopAsync(CancellationToken cancellationToken);
}

public sealed class BWQueue<TMessage>
    : IBWQueue<TMessage>
    where TMessage : IBWMessage {
    private readonly Channel<TMessage> _Channel;

    public BWQueue(
        Channel<TMessage> channel
        ) {
        this._Channel = channel;
    }

    public Channel<TMessage> Channel => this._Channel;

    public async Task Enqueue(TMessage message, CancellationToken cancellationToken) {
        await this._Channel.Writer.WriteAsync(message, cancellationToken);
    }

    public bool TryRead([MaybeNullWhen(false)] out TMessage item) {
        return this._Channel.Reader.TryRead(out item);
    }

    public async ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken) {
        return await this._Channel.Reader.WaitToReadAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        this._Channel.Writer.TryComplete();
        return Task.CompletedTask;
    }
}