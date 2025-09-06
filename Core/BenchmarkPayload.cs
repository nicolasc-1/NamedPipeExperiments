using Bogus;

namespace Core;

/// <summary>
/// Represents a complex payload for benchmarking the connection.
/// </summary>
public class BenchmarkPayload
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public List<int> Numbers { get; set; } = new();
    public Dictionary<Guid, Guid> Metadata { get; set; } = new();
    public NestedObject Details { get; set; } = new();
    public List<NestedObject> NestedList { get; set; } = new();
    public byte[] BinaryData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Represents a nested object within the payload.
    /// </summary>
    public class NestedObject
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    public static BenchmarkPayload FromRandom()
    {
        var faker = new Faker();

        return new BenchmarkPayload
        {
            Id = Guid.NewGuid(),
            Name = faker.Commerce.ProductName(),
            Timestamp = faker.Date.Recent(),
            Numbers = faker.Random.ListItems(Enumerable.Range(1, 100).ToList(), faker.Random.Int(5, 20)).ToList(),
            Metadata = Enumerable.Range(1, 5).ToDictionary(_ => Guid.NewGuid(), _ => Guid.NewGuid()),
            Details = new NestedObject
            {
                Id = faker.Random.Int(1, 1000),
                Description = faker.Lorem.Sentence(),
                Value = faker.Random.Double(1, 100)
            },
            NestedList = faker.Make(3, () => new NestedObject
            {
                Id = faker.Random.Int(1, 1000),
                Description = faker.Lorem.Sentence(),
                Value = faker.Random.Double(1, 100)
            }).ToList(),
            BinaryData = faker.Random.Bytes(50)
        };
    }
}