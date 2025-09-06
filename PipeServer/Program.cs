using System.Diagnostics;
using NamedPipeExperiments;

const int numWorkers = 4;

var workers = new List<WorkerProcess>(4);

var stopwatch = Stopwatch.StartNew();
for (int i = 0; i < numWorkers; i++)
{
    var worker = new WorkerProcess(i);
    workers.Add(worker);
    
    worker.Start();
}

// join threads
foreach (var worker in workers)
{
    worker.Join();
}
stopwatch.Stop();
Console.WriteLine($"All workers completed. Total time: {stopwatch.ElapsedMilliseconds} ms");