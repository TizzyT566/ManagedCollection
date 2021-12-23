using System.Collections.Concurrent;

ManagedGuidSet mlu = new(600);
Console.WriteLine($"Capacity : {mlu.Capacity}\n");
bool _running = true;

ThreadPool.QueueUserWorkItem(_ =>
{
    while (_running)
    {
        mlu.Add(Guid.NewGuid());
    }
    Console.WriteLine("Second Thread Ended");
});

Console.WriteLine("Add Test:");
for (int i = 0; i < 18; i++)
{
    Guid newGuid = Guid.NewGuid();
    Console.WriteLine($"{i + 1,2} : {mlu.Add(newGuid), 5} : {newGuid}");
}

Guid duplicateTest = Guid.NewGuid();
Console.WriteLine($"{19,2} : {mlu.Add(duplicateTest), 5} : {duplicateTest}");
Console.WriteLine($"{20,2} : {mlu.Add(duplicateTest), 5} : {duplicateTest}\n");

Console.WriteLine("Stored Guids:");
int count = 1;
foreach(Guid guid in mlu)
    Console.WriteLine($"{count++,2} : {guid}");

Console.WriteLine($"\nSize : {mlu.Size}\n");

Console.WriteLine($"Capacity : {mlu.Capacity = 14}\n");

Console.WriteLine("Addition Adds");
for (int i = 0; i < 7; i++)
{
    Guid newGuid = Guid.NewGuid();
    Console.WriteLine($"{i + 1,2} : {mlu.Add(newGuid),5} : {newGuid}");
}

Console.WriteLine($"\nSize : {mlu.Size}\n");

Console.WriteLine("Stored Guids:");
count = 1;
foreach (Guid guid in mlu)
    Console.WriteLine($"{count++,2} : {guid}");

Console.Clear();

mlu.Save(@"C:\Users\tizzy\Desktop\guids.bin");
Console.WriteLine("\nStored Guids1:");
count = 1;
foreach (Guid guid in mlu)
    Console.WriteLine($"{count++,2} : {guid}");

ManagedGuidSet mlu2 = new(60, @"C:\Users\tizzy\Desktop\guids.bin");
Console.WriteLine("\nStored Guids2:");
count = 1;
foreach (Guid guid in mlu2)
    Console.WriteLine($"{count++,2} : {guid}");

_running = false;