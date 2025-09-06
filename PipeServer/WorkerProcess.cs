using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Core;

namespace NamedPipeExperiments;

public class WorkerProcess
{
    private const int NumIterations = 10000;
    
    public NamedPipeServerStream In { get; set; }
    public NamedPipeServerStream Out { get; set; }
    public Thread SendThread { get; set; }
    public Thread ReceiveThread { get; set; }
    public int Identity { get; set; }
    
    public WorkerProcess(int identity)
    {
        Identity = identity;
        In = new NamedPipeServerStream($"worker_server_pipe_{identity}", PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances);
        Out = new NamedPipeServerStream($"server_worker_pipe_{identity}", PipeDirection.Out, NamedPipeServerStream.MaxAllowedServerInstances);

        SendThread = new Thread(SendPayload);
        ReceiveThread = new Thread(ReceivePayload);
    }
    
    public void Start()
    {
        // Launch the PipedWorker process
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "PipedWorker.exe",
            Arguments = $"{Identity}",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        var process = Process.Start(processStartInfo);
        if (process == null)
        {
            throw new InvalidOperationException($"Failed to start worker process for identity {Identity}");
        }
        
        Console.WriteLine($"Server waiting for worker {Identity} to connect to pipes");
        
        // wait for the named pipes to be ready
        In.WaitForConnection();
        Out.WaitForConnection();
        
        Console.WriteLine($"Worker {Identity} connected to pipes");
        
        // start the processing threads
        SendThread.Start(this);
        ReceiveThread.Start(this);
    }

    public void Join()
    {
        SendThread.Join();
        ReceiveThread.Join();
    }
    
    void SendPayload(object? data)
    {
        var worker = (WorkerProcess)data!;
    
        Console.WriteLine($"Sending payload on worker {worker.Identity}");
        
        using var writer = new StreamWriter(worker.Out, Encoding.UTF8);
        writer.AutoFlush = true;
        for (int i = 0; i < NumIterations; i++)
        {
            writer.WriteLine(JsonSerializer.Serialize(BenchmarkPayload.FromRandom()));
        }
    }

    void ReceivePayload(object? data)
    {
        var worker = (WorkerProcess)data!;
        using var reader = new StreamReader(worker.In, Encoding.UTF8);

        bool finished = false;
        int numReceived = 0;
        while (!finished)
        {
            var line = reader.ReadLine();

            if (line != null)
            {
                _ = JsonSerializer.Deserialize<BenchmarkPayload>(line);
                // Console.WriteLine($"Received {test?.Name} on worker {worker.Identity}");
                numReceived++;
            }
            
            if (numReceived >= NumIterations)
            {
                finished = true;
                Console.WriteLine($"Worker {worker.Identity} received all {numReceived} payloads");
            }
        }
    }
}