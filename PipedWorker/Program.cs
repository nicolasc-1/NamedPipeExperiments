using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Core;

var outPipe = new NamedPipeClientStream(".", $"worker_server_pipe_{args[0]}", PipeDirection.Out);
var inPipe = new NamedPipeClientStream(".", $"server_worker_pipe_{args[0]}", PipeDirection.In);

outPipe.Connect();
inPipe.Connect();

using var reader = new StreamReader(inPipe, Encoding.UTF8);
using var writer = new StreamWriter(outPipe, Encoding.UTF8);
try
{
    while (true)
    {
        // read request
        var json = reader.ReadLine();
        
        if (json != null)
        {
            var test = JsonSerializer.Deserialize<BenchmarkPayload>(json);
            // Console.WriteLine($"Received Payload: {payload?.Name}, {payload?.Timestamp}");
        }
        
        // send a new DTO
        writer.WriteLine(JsonSerializer.Serialize(BenchmarkPayload.FromRandom()));
    }
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}