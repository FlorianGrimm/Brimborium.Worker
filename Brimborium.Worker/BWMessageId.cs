namespace Brimborium.Worker;

public static class BWMessageId {
    private static long _Id = 1;
    public static long GetId() {
        return System.Threading.Interlocked.Increment(ref _Id);
    }
}

/*
public class BWMessageCommand
    : BWMessageBase
    , Foundatio.Mediator.ICommand {
    public BWMessageCommand() {
    }

    [Foundatio.Mediator.Handler]
    public virtual async Task HandleAsync(
        BWMessageCommand command,
        Foundatio.Mediator.IMediator mediator,
        CancellationToken cancellationToken
        ) {
        await command.InvokeAsync(mediator, cancellationToken);
    }

    protected async Task InvokeAsync(IMediator mediator, CancellationToken cancellationToken) {
        await mediator.InvokeAsync(this, cancellationToken);
    }
}
*/