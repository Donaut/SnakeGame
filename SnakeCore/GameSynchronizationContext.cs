namespace SnakeCore.Internal;

public class GameSynchronizationContext : SynchronizationContext
{
    internal Queue<(SendOrPostCallback callback, object? state)> frontPostQueue = new();
    internal Queue<(SendOrPostCallback callback, object? state)> backPostQueue = new();

    public override SynchronizationContext CreateCopy()
    {
        throw new NotImplementedException("CreateCopy not implemented!");
        return base.CreateCopy();
    }

    public override void OperationCompleted()
    {
        throw new NotImplementedException("OperationCompleted not implemented!");
        base.OperationCompleted();
    }

    public override void OperationStarted()
    {
        throw new NotImplementedException("OperationStarted not implemented!");
        base.OperationStarted();
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        frontPostQueue.Enqueue((d, state));
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        throw new NotImplementedException("Send not implemented!");
        base.Send(d, state);
    }

    public override int Wait(nint[] waitHandles, bool waitAll, int millisecondsTimeout)
    {
        throw new NotImplementedException("Wait not implemented!");
        return base.Wait(waitHandles, waitAll, millisecondsTimeout);
    }

    public void ExecutePostQueue()
    {
        var temp = frontPostQueue;
        frontPostQueue = backPostQueue;
        backPostQueue = temp;

        while(backPostQueue.Count > 0) {
            var (callback, state) = backPostQueue.Dequeue();
            callback(state);
        }
    }
}
