using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Data.SqlClient;
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
            var tasks = Enumerable.Range(0, 100).Select(_ => context.CallSubOrchestratorAsync<string>("PostgresOrchestration_Sub", null));
            return Task.WhenAll(tasks);
        }

        [FunctionName("PostgresOrchestration_Sub")]
        public static async Task<string> RunOrchestratorSub(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync("NpgsqlOrchestration_RunCpu", null);
            return await context.CallActivityAsync<string>("NpgsqlOrchestration_RunAsync", null);
        }

        [FunctionName("NpgsqlOrchestration_RunCpu")]
        public static void RunCpu([ActivityTrigger] IDurableOrchestrationContext context)
        {
            for (var i = 0; i < 50; i++)
            {
                var watch = Stopwatch.StartNew();
                while (watch.ElapsedMilliseconds < 100)
                {
                }
            }

            // You can either run the CPU function and the Npgsql function in the same function or separate functions

            // var stopwatch = Stopwatch.StartNew();
            // await using var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("PostgresConnectionString"));
            // await connection.OpenAsync();
            // return $"{executionContext.InvocationId}: {stopwatch.ElapsedMilliseconds}";
        }

        [FunctionName("NpgsqlOrchestration_RunAsync")]
        public static async Task<string> RunAsync([ActivityTrigger] IDurableOrchestrationContext context, ExecutionContext executionContext)
        {
            var stopwatch = Stopwatch.StartNew();
            await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlServerConnectionString"));
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
            string instanceId = await starter.StartNewAsync("NpgsqlOrchestration");

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
