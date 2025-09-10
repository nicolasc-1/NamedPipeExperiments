using System.Globalization;
using Grpc.Core;

namespace GrpcWorker.Services;

public class WorkerService : Worker.WorkerBase
{
    public override Task<BenchmarkPayload> Work(BenchmarkPayload request, ServerCallContext context)
    {
        var randomData = Core.BenchmarkPayload.FromRandom();
        return Task.FromResult(new BenchmarkPayload
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
        });
    }
    
    public override Task<ShutdownResponse> Shutdown(ShutdownRequest request, ServerCallContext context)
    {
        Console.WriteLine("Shutdown request received. Stopping server...");
        
        Environment.Exit(request.ExitCode);
        return Task.FromResult(new ShutdownResponse
        {
            Message = "Goodbye!"
        });
    }
}