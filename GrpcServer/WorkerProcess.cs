using System.Diagnostics;
using System.Globalization;
using Grpc.Net.Client;
using GrpcWorker;

namespace GrpcServer;

public class WorkerProcess
{
    private const int NumIterations = 10000;

    private GrpcChannel? grpcChannel;
    private readonly int identity;
    private readonly int port;
    private readonly Thread workThread;

    public WorkerProcess(int identity, int port)
    {
        this.identity = identity;
        this.port = port;
        this.workThread = new Thread(Work);
    }
    
    public void Start()
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "GrpcWorker.exe", // Path to the GrpcWorker executable
            Arguments = $"{this.identity} {this.port}", // Pass worker ID and port number
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(processStartInfo);
        if (process == null)
        {
            throw new InvalidOperationException($"Failed to start worker process for identity {this.identity}");
        }
        
        this.grpcChannel = GrpcChannel.ForAddress($"http://localhost:{this.port}");
        Console.WriteLine($"Worker {this.identity} connected to gRPC server on port {this.port}");
        
        this.workThread.Start();
    }
    
    public void Join()
    {
        this.workThread.Join();
    }

    public void Shutdown()
    {
        this.grpcChannel?.ShutdownAsync().Wait();
    }
    
    private void Work()
    {
        var client = new Worker.WorkerClient(this.grpcChannel);
        
        for (int j = 0; j < NumIterations; j++)
        {
            var randomData = Core.BenchmarkPayload.FromRandom();
            var payload = new BenchmarkPayload
            {
                Id = randomData.Id.ToString(),
                Name = randomData.Name,
                Timestamp = randomData.Timestamp.ToString(CultureInfo.InvariantCulture),
                Numbers = { randomData.Numbers },
                Metadata = { randomData.Metadata.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value.ToString()) },
                Details = new NestedObject
                {
                    Id = randomData.Details.Id,
                    Description = randomData.Details.Description,
                    Value = randomData.Details.Value
                },
                NestedList =
                {
                    randomData.NestedList.Select(n => new NestedObject
                    {
                        Id = n.Id,
                        Description = n.Description,
                        Value = n.Value
                    })
                },
                BinaryData = Google.Protobuf.ByteString.CopyFrom(randomData.BinaryData)
            };

            var result = client.WorkAsync(payload).ResponseAsync;
            // var unwrapResult = result.Id;
        }
        
        Console.WriteLine($"Worker {this.identity} completed work.");
    }
}