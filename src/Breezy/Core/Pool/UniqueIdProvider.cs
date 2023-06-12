using System.Collections.Concurrent;

namespace Breezy.Core.Pool;

public class UniqueIdProvider
{
    protected readonly ConcurrentQueue<int> _mFreeIds = new ();
    protected int _mHighestId;
    
    public UniqueIdProvider() {}
    
    public UniqueIdProvider(IEnumerable<int> freeIds)
    {
        foreach (var freeId in freeIds)
        {
            _mFreeIds.Enqueue(freeId);
        }
    }

    public virtual int Pop()
    {
        int id;

        if (!_mFreeIds.IsEmpty)
        {
            if (!_mFreeIds.TryDequeue(out id))
            {
                // if we can't dequeue, we return the next id
                return Next();
            }
        }
        else
            return Next();

        return id;
    }

    public virtual int Peek()
    {
        int id;

        if (!_mFreeIds.IsEmpty)
        {
            if (!_mFreeIds.TryPeek(out id))
            {
                // if we can't dequeue, we return the next id
                return _mHighestId + 1;
            }
        }
        else
            return _mHighestId + 1;

        return id;
    }

    /// <summary>
    /// Indicate that the given id is free
    /// </summary>
    /// <param name="freeId"></param>
    public virtual void Push(int freeId)
    {
        _mFreeIds.Enqueue(freeId);
    }

    protected virtual int Next()
    {
        return Interlocked.Increment(ref _mHighestId);
    }
}
