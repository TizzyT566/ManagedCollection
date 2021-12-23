using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Concurrent;

/// <summary>
/// A thread-safe collection which stores unique guids and stores only up to the set capacity.
/// Discards oldest items first to make room for new items when collection is full.
/// </summary>
public class ManagedGuidSet : IEnumerable<Guid>
{
    private readonly Queue<Guid> _order;
    private readonly HashSet<Guid> _lookup;
    private readonly string? _backlogPath;
    private volatile int _capacity, _size;
    private int _lock = 0;
    /// <summary>
    /// The maximum amount of items until old items are discarded.
    /// </summary>
    public int Capacity
    {
        get => _capacity;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(Capacity), "Must be greater than 0.");
            while (Interlocked.Exchange(ref _lock, 1) == 1) ;
            int removeCount = _size - value;
            for (int i = 0; i < removeCount; i++)
                if (_order.TryDequeue(out Guid id) && _lookup.Remove(id))
                    _size--;
                else
                    break;
            _capacity = value;
            Interlocked.Exchange(ref _lock, 0);
        }
    }
    /// <summary>
    /// How many items are currently in the collection.
    /// </summary>
    public int Size => _size;

    /// <summary>
    /// Creates a new ManagedLookup collection.
    /// </summary>
    /// <param name="capacity">The initial capacity of the collection.</param> // Chose 3 because Dictionary initializes with 3
    /// <param name="path">The path to load saved Guids from.</param>
    public ManagedGuidSet(int capacity = 3, string? path = null)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Must be greater than 0.");
        _backlogPath = path;
        _capacity = capacity;
        _size = 0;
        _order = new();
        _lookup = new();
        if (path != null)
        {
            try
            {
                using FileStream fs = new(path, FileMode.Open);
                byte[] guidBytes = new byte[16];
                int readBytes = 0;
                while ((readBytes = fs.Read(guidBytes, 0, 16)) > 0)
                {
                    int crntReadBytes = 0;
                    while (readBytes < 16 && (crntReadBytes = fs.Read(guidBytes, readBytes, 16 - readBytes)) > 0)
                        readBytes += crntReadBytes;
                    if (readBytes == 16)
                    {
                        Guid newGuid = new(guidBytes);
                        _lookup.Add(newGuid);
                        _order.Enqueue(newGuid);
                        _size++;
                    }
                    else
                        break;
                }
                int removeCount = _size - _capacity;
                for (int i = 0; i < removeCount; i++)
                    if (_order.TryDequeue(out Guid id) && _lookup.Remove(id))
                        _size--;
                    else
                        break;
            }
            catch (Exception) { }
        }
    }

    /// <summary>
    /// Checks to see if a specific Guid is already in the collection.
    /// </summary>
    /// <param name="guid">The Guid to check.</param>
    /// <returns>true if the specified Guid is present in the collection, false otherwise.</returns>
    public bool Contains(Guid guid)
    {
        while (Interlocked.Exchange(ref _lock, 1) == 1) ;
        bool contains = _lookup.Contains(guid);
        Interlocked.Exchange(ref _lock, 0);
        return contains;
    }

    /// <summary>
    /// Adds a Guid to the collection.
    /// </summary>
    /// <param name="guid">The Guid to be added.</param>
    /// <returns>true if the Guid was added successfully, false otherwise.</returns>
    public bool Add(Guid guid)
    {
        while (Interlocked.Exchange(ref _lock, 1) == 1) ;
        bool success;
        if (success = _lookup.Add(guid))
        {
            _order.Enqueue(guid);
            _size++;
        }
        while (_size > _capacity)
        {
            _lookup.Remove(_order.Dequeue());
            _size--;
        }
        Interlocked.Exchange(ref _lock, 0);
        return success;
    }

    /// <summary>
    /// Tries to remove the oldest item from the collection.
    /// </summary>
    /// <param name="guid">The removed Guid.</param>
    /// <returns>true if an item was removed from the collection, false otherwise.</returns>
    public bool TryRemove(out Guid? guid)
    {
        guid = null;
        bool result;
        while (Interlocked.Exchange(ref _lock, 1) == 1) ;
        if (result = _order.TryDequeue(out Guid rGuid) && _lookup.Remove(rGuid))
            guid = rGuid;
        Interlocked.Exchange(ref _lock, 0);
        return result;
    }

    /// <summary>
    /// Clears the collection.
    /// </summary>
    public void Clear()
    {
        while (Interlocked.Exchange(ref _lock, 1) == 1) ;
        _lookup.Clear();
        _order.Clear();
        _size = 0;
        Interlocked.Exchange(ref _lock, 0);
    }

    /// <summary>
    /// Saves the collection to disk.
    /// </summary>
    /// <param name="path">The full save path.</param>
    /// <returns>true if save was successful, false otherwise.</returns>
    public async Task<bool> Save(string? path = null)
    {
        if (path == null)
        {
            if (_backlogPath == null)
                return false;
            else
                path = _backlogPath;
        }
        FileStream? fs = null;
        try
        {
            while (Interlocked.Exchange(ref _lock, 1) == 1) ;
            fs = new(path, FileMode.Create);
            foreach (Guid guid in _order)
                await fs.WriteAsync(guid.ToByteArray());
            Interlocked.Exchange(ref _lock, 0);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            fs?.Close();
        }
    }

    /// <summary>
    /// Enumerates through the collection.
    /// </summary>
    /// <returns>Guids in order which they were added.</returns>
    /// <remarks>Enumeration blocks.</remarks>
    public IEnumerator<Guid> GetEnumerator()
    {
        while (Interlocked.Exchange(ref _lock, 1) == 1) ;
        foreach (Guid guid in _order)
            yield return guid;
        Interlocked.Exchange(ref _lock, 0);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        while (Interlocked.Exchange(ref _lock, 1) == 1) ;
        foreach (Guid guid in _order)
            yield return guid;
        Interlocked.Exchange(ref _lock, 0);
    }
}