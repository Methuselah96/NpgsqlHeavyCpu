using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace NpgsqlHeavyCpu
{
    public static class NpgsqlOrchestration
    {
        [FunctionName("NpgsqlOrchestration")]
        public static Task<string[]> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var tasks = Enumerable.Range(0, 25).Select(_ => context.CallActivityAsync<string>("NpgsqlOrchestration_Run", null));
            return Task.WhenAll(tasks);
        }

        [FunctionName("NpgsqlOrchestration_Run")]
        public static async Task<string> Run([ActivityTrigger] IDurableOrchestrationContext context, ExecutionContext executionContext, ILogger log)
        {
            //var heavyCpuResults = Enumerable.Range(0, 50).AsParallel().Select(index =>
            //{
            //    var watch = Stopwatch.StartNew();
            //    while (watch.ElapsedMilliseconds < 100)
            //    {
            //    }

            //    return index;
            //}).ToList();
            var bag = new ConcurrentBag<int>();

            var heavyCpuResults = Parallel.ForEach(Enumerable.Range(0, 50), index =>
            {
                var watch = Stopwatch.StartNew();
                while (watch.ElapsedMilliseconds < 100)
                {
                }

                bag.Add(index);
            });

            var stopwatch = Stopwatch.StartNew();
            using var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("PostgresConnectionString"));
            await connection.OpenAsync();
            return $"{executionContext.InvocationId}: {stopwatch.ElapsedMilliseconds}";
        }

        [FunctionName("NpgsqlOrchestration_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("NpgsqlOrchestration", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
