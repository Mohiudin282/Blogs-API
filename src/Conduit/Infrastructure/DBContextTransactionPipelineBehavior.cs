using Mediator;

public class DBContextTransactionPipelineBehavior<TRequest, TResponse>(ConduitContext context)
 : IPipelineBehavior<TRequest, TResponse>
 where TRequest : notnull, IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request is IReadOnlyRequest)
        {
            return await next(request, cancellationToken);
        }

        TResponse result;
        try
        {
            await context.BeginTransactionAsync(cancellationToken);
            result = await next(request, cancellationToken);
            await context.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await context.RollbackTransactionAsync();
            throw;
        }
        return result;
    }
}