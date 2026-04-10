using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace Brimborium.Worker;

public interface IBWQueue<TValue, TMessage>
    where TMessage : IBWMessageWithValue<TValue> {
    Task Enqueue(TMessage message, CancellationToken cancellationToken);
    ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken);
    bool TryRead([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TMessage item);
}

public class BWQueue<TValue, TMessage>
    : IBWQueue<TValue, TMessage>
    where TMessage : IBWMessageWithValue<TValue> {
    private readonly Channel<TMessage> _Channel;

    public BWQueue(
        Channel<TMessage> channel
        ) {
        this._Channel = channel;
    }

    public async Task Enqueue(TMessage message, CancellationToken cancellationToken) {
        await this._Channel.Writer.WriteAsync(message, cancellationToken);
    }

    public bool TryRead([MaybeNullWhen(false)] out TMessage item) {
        return this._Channel.Reader.TryRead(out item);
    }

    public async ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken) {
        return await this._Channel.Reader.WaitToReadAsync(cancellationToken);
    }
}