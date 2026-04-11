namespace Brimborium.Worker;

// TODO: seams useless in this stage...

public interface IBWMiddlewareBuilder<TMessage>
    where TMessage : IBWMessage {
    IBWMiddleware<TMessage> CreateMiddleware(
            IBWMiddleware<TMessage> caller,
            IBWMiddleware<TMessage> next
        );
}

public sealed class BWMiddlewareBuilder<TMessage>
    where TMessage : IBWMessage
    {
    private readonly List<IBWMiddlewareBuilder<TMessage>> _ListBuilder = new();
    private bool _Frozen;

    public BWMiddlewareBuilder(params IBWMiddlewareBuilder<TMessage>[] builder) {
        this._ListBuilder.AddRange(builder);
    }

    public BWMiddlewareBuilder<TMessage> Add(
            IBWMiddlewareBuilder<TMessage> builder
        ) {
        if (this._Frozen) {
            throw new InvalidOperationException("frozen");
        }
        this._ListBuilder.Add(builder);
        return this;
    }



    public BWMiddlewareBuilder<TMessage> AddRange(
            params IBWMiddlewareBuilder<TMessage>[] builder
        ) {
        if (this._Frozen) {
            throw new InvalidOperationException("frozen");
        }
        this._ListBuilder.AddRange(builder);
        return this;
    }

    public BWMiddlewareBuilder<TMessage> Freeze() {
        this._Frozen = true;
        return this;
    }

    public IBWMiddleware<TMessage> Build(
            IBWMiddleware<TMessage> caller
        ) {
        this._Frozen = true;
        {
            var result = caller;
            for (int index = this._ListBuilder.Count - 1;
                0 <= index;
                --index) {
                var builder = this._ListBuilder[index];
                result = builder.CreateMiddleware(caller, result);
            }
            return result;
        }
    }
}
