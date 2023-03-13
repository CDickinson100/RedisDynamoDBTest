using System.Diagnostics;
using System.Reactive.Linq;
using RedisTests;
using StackExchange.Redis;
using static Common.Constants;

var options = new ConfigurationOptions
{
    EndPoints = { "EndpointHere" },
    Ssl = true,
    SslProtocols = System.Security.Authentication.SslProtocols.None,
    CheckCertificateRevocation = false,
};
var redis = ConnectionMultiplexer.Connect(options);

var db = redis.GetDatabase();

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
    await db.ListRightPushAsync(data.Id.ToString(), data.Price);
    stopwatch.Stop();
    times.Add(stopwatch.ElapsedMilliseconds);
}

void WriteSummary()
{
    var timesCopy = new List<long>(times);
    Console.WriteLine("-------------------------------------------------------------------");
    Console.WriteLine($"Finished sending {timesCopy.Count}/{Amount * (Time * 1000 / Interval)} values to Redis");
    Console.WriteLine($"Average Response Time {timesCopy.Average()}ms");
    Console.WriteLine($"Capable of sending {timesCopy.Count / Time}/second");
    Console.WriteLine("-------------------------------------------------------------------");
}