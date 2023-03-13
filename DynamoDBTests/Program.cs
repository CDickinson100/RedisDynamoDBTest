using System.Diagnostics;
using System.Reactive.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using DynamoDBTests;
using static Common.Constants;

var client = new AmazonDynamoDBClient();
var context = new DynamoDBContext(client);

Console.WriteLine("Connected to DB");

var times = new List<long>();

var random = new Random();

Console.WriteLine($"Sending {Amount} Updates every {Interval}ms for {Time} seconds");

var observableData = Observable.Interval(TimeSpan.FromMilliseconds(Interval))
    .SelectMany(_ => Enumerable.Range(1, Amount))
    .Select(id => new Data(id, random.NextDouble()));

using (observableData.Subscribe(async data => await AddToDb(data)))
{
    await Task.Delay(TimeSpan.FromSeconds(Time));
}

WriteSummary();

async Task AddToDb(Data data)
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();
    await context.SaveAsync(data);
    stopwatch.Stop();
    times.Add(stopwatch.ElapsedMilliseconds);
}

void WriteSummary()
{
    var timesCopy = new List<long>(times);
    Console.WriteLine("-------------------------------------------------------------------");
    Console.WriteLine($"Finished sending {timesCopy.Count}/{Amount * (Time * 1000 / Interval)} values to DynamoDB");
    Console.WriteLine($"Average Response Time {timesCopy.Average()}ms");
    Console.WriteLine($"Capable of sending {timesCopy.Count / Time}/second");
    Console.WriteLine("-------------------------------------------------------------------");
}