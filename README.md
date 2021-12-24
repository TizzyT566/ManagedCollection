# ManagedGuidSet

A thread-safe HashSet<Guid> which stores upto a set capacity discarding older Guids as needed.

Useful for keeping track of old vs new things.

## Features

1. Loading saved Guids upon construction.
2. Save current Guids for later loading.
3. Set the capacity.
4. See if a Guid is present in collection.
5. Enumerable.

### Time Complexities

- Add: O(1)
- Remove: O(1)
- Contains: O(1)
- Clear: O(1)
- Save: O(n)
- Load: O(n)

## Usage

### Properties
```csharp
/// <summary>
/// The maximum amount of items until old items are discarded.
/// </summary>
public int Capacity
```
```csharp
/// <summary>
/// How many items are currently in the collection.
/// </summary>
public int Size
```

### Constructor
```csharp
/// <summary>
/// Creates a new ManagedLookup collection.
/// </summary>
/// <param name="capacity">The initial capacity of the collection.</param>
/// <param name="path">The path to load saved Guids from.</param>
public ManagedGuidSet(int capacity = 3, string? path = null)
```

### Methods
```csharp
/// <summary>
/// Checks to see if a specific Guid is already in the collection.
/// </summary>
/// <param name="guid">The Guid to check.</param>
/// <returns>true if the specified Guid is present in the collection, false otherwise.</returns>
public bool Contains(Guid guid)
```
```csharp
/// <summary>
/// Adds a Guid to the collection.
/// </summary>
/// <param name="guid">The Guid to be added.</param>
/// <returns>true if the Guid was added successfully, false otherwise.</returns>
public bool Add(Guid guid)
```
```csharp
/// <summary>
/// Tries to remove the oldest item from the collection.
/// </summary>
/// <param name="guid">The removed Guid.</param>
/// <returns>true if an item was removed from the collection, false otherwise.</returns>
public bool TryRemove(out Guid? guid)
```
```csharp
/// <summary>
/// Clears the collection.
/// </summary>
public void Clear()
```
```csharp
/// <summary>
/// Saves the collection to disk.
/// </summary>
/// <param name="path">The full save path.</param>
/// <returns>true if save was successful, false otherwise.</returns>
public async Task<bool> Save(string? path = null)
```

### Example
```csharp
// Create a ManagedGuidSet with capacity of 600
ManagedGuidSet mgs = new(600);

// Adding a Guid
Guid newGuid = Guid.NewGuid();
mgs.Add(newGuid);

// Checking to see if the Guid is present
bool isPresent =  mgs.Contains(newGuid);

// Removing the oldest Guid
if(mgs.TryRemove(out Guid? oldGuid))
{
    Console.WriteLine($"Removed old Guid: {oldGuid}");
}
else
{
    Console.WriteLine("Collection is Empty");
}

// Saving the collection to disk
string path = "some path here";
if(await msg.Save("Some Path here"))
    Console.WriteLine("Saved.");

// Loading saved collection on construction with a new capacity of 300
ManagedGuidSet mgs2 = new(300, path);
```