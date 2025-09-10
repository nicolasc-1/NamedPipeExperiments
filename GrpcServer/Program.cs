using System.Diagnostics;
using GrpcServer;

const int numWorkers = 2;
const int startingPort = 5000;

var workers = new List<WorkerProcess>(numWorkers);

var stopwatch = Stopwatch.StartNew();
for (int i = 0; i < numWorkers; i++)
{
    var worker = new WorkerProcess(i, startingPort + i);
    workers.Add(worker);
    
    worker.Start();
}

// join threads
foreach (var worker in workers)
{
    worker.Join();
}
stopwatch.Stop();

foreach (var worker in workers)
{
    worker.Shutdown();
}

Console.WriteLine($"All workers completed. Total time: {stopwatch.ElapsedMilliseconds} ms");