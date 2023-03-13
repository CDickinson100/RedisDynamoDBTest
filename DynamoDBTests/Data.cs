using Amazon.DynamoDBv2.DataModel;

namespace DynamoDBTests;

[DynamoDBTable("test-data-table")]
public record Data
{
    [DynamoDBHashKey] public string Id { get; init; }
    [DynamoDBRangeKey] public DateTime Date { get; init; } = DateTime.Now;
    public double Price { get; init; }

    public Data(int Id, double Price)
    {
        this.Id = Id.ToString();
        this.Price = Price;
    }

    public Data()
    {
    }
}