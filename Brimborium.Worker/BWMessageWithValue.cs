namespace Brimborium.Worker;

public interface IBWMessageWithValue<TValue> : IBWMessage {
    TValue Value { get; }
}

public sealed class BWMessageWithValue<TValue>
    : BWMessageBase
    , IBWMessageWithValue<TValue> {
    public TValue Value { get; }

    public BWMessageWithValue(TValue value) {
        this.Value = value;
    }

    public override IBWMessageWithValue<TNextValue> CreateWithValue<TNextValue>(TNextValue nextValue) {
        return new BWMessageWithValue<TNextValue>(nextValue);
    }

    public override IBWMessageWithResult<TNextResult> CreateWithResult<TNextResult>(IBWMessageResult<TNextResult> nextResult) {
        return new BWMessageWithValueResult<TValue, TNextResult>(this.Value, nextResult);
    }

    public override IBWMessageWithScope<TNextScope> CreateWithScope<TNextScope>(TNextScope nextScope) {
        return new BWMessageWithValueScope<TValue, TNextScope>(this.Value, nextScope);
    }
}

