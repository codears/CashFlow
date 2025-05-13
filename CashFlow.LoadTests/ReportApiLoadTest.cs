using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CashFlow.LoadTests
{
    public class ReportApiLoadTest
    {
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _httpClient;
        private readonly string _reportApiUrl;
        
        // Obtém a URL da API de relatório da variável de ambiente ou usa um valor padrão

        public ReportApiLoadTest(ITestOutputHelper output)
        {
            _output = output;
            _httpClient = new HttpClient();
            
            // Usa variável de ambiente TARGET_REPORT_API se disponível, senão usa localhost
            _reportApiUrl = Environment.GetEnvironmentVariable("TARGET_REPORT_API") ?? "http://localhost:55677";
            _output.WriteLine($"Usando URL da API de relatório: {_reportApiUrl}");
            
            // Configura timeout mais longo para ambientes de contêiner
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public async Task ReportApi_Should_Handle_500_RequestsPerSecond()
        {
            // Today's date for the request
            var today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
            var url = $"{_reportApiUrl}/api/Report/{today}";

            // Aguarda até que a API esteja online e respondendo (com tempo limite de 30 segundos)
            await WaitForApiReadiness(url, maxRetries: 10, retryDelayMs: 3000);
            
            // Configuration
            var targetRps = 500; // Target requests per second
            var testDurationSeconds = 10; // Run test for 10 seconds
            var totalRequests = targetRps * testDurationSeconds;

            _output.WriteLine($"Starting load test against {url}");
            _output.WriteLine($"Target: {targetRps} requests per second for {testDurationSeconds} seconds");

            // Create tasks list for parallel execution
            var tasks = new List<Task<bool>>();
            var stopwatch = new Stopwatch();
            var successCount = 0;
            var failureCount = 0;
            var responseTimes = new List<long>();

            stopwatch.Start();

            // Launch all requests in parallel batches to simulate load
            for (int i = 0; i < totalRequests; i++)
            {
                // Control the request rate to match target RPS
                var targetTime = TimeSpan.FromMilliseconds((i * 1000.0) / targetRps);
                var currentTime = stopwatch.Elapsed;
                if (currentTime < targetTime)
                {
                    var sleepTime = targetTime - currentTime;
                    if (sleepTime.TotalMilliseconds > 0)
                        await Task.Delay(sleepTime);
                }

                tasks.Add(SendRequest(url, responseTimes));

                // Processing results in batches to avoid overwhelming memory
                if (tasks.Count >= 100 || i == totalRequests - 1)
                {
                    var results = await Task.WhenAll(tasks);
                    successCount += results.Count(r => r);
                    failureCount += results.Count(r => !r);
                    tasks.Clear();
                }
            }

            stopwatch.Stop();
            var totalTime = stopwatch.Elapsed;

            // Calculate statistics
            var actualRps = totalRequests / totalTime.TotalSeconds;
            var successRate = (double)successCount / totalRequests;
            var averageResponseTime = responseTimes.Count > 0 ? responseTimes.Average() : 0;
            var maxResponseTime = responseTimes.Count > 0 ? responseTimes.Max() : 0;
            var p95ResponseTime = CalculatePercentile(responseTimes, 95);
            var p99ResponseTime = CalculatePercentile(responseTimes, 99);

            // Output results
            _output.WriteLine($"Test completed in: {totalTime.TotalSeconds:F2} seconds");
            _output.WriteLine($"Actual requests per second: {actualRps:F1} RPS");
            _output.WriteLine($"Total requests: {totalRequests}");
            _output.WriteLine($"Successful requests: {successCount}");
            _output.WriteLine($"Failed requests: {failureCount}");
            _output.WriteLine($"Success rate: {successRate:P2}");
            _output.WriteLine($"Average response time: {averageResponseTime:F2} ms");
            _output.WriteLine($"Maximum response time: {maxResponseTime} ms");
            _output.WriteLine($"P95 response time: {p95ResponseTime} ms");
            _output.WriteLine($"P99 response time: {p99ResponseTime} ms");

            // Verify success rate meets requirement (95%)
            Assert.True(successRate >= 0.95, $"Success rate of {successRate:P2} is below the required 95%");
        }

        private async Task<bool> SendRequest(string url, List<long> responseTimes)
        {
            try
            {
                var requestStopwatch = new Stopwatch();
                requestStopwatch.Start();

                var response = await _httpClient.GetAsync(url);

                requestStopwatch.Stop();
                lock (responseTimes)
                {
                    responseTimes.Add(requestStopwatch.ElapsedMilliseconds);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Request failed: {ex.Message}");
                return false;
            }
        }

        private long CalculatePercentile(List<long> responseTimes, int percentile)
        {
            if (responseTimes == null || !responseTimes.Any())
                return 0;

            var orderedTimes = responseTimes.OrderBy(t => t).ToList();
            var index = (int)Math.Ceiling((percentile / 100.0) * orderedTimes.Count) - 1;
            return orderedTimes[Math.Max(0, index)];
        }
        
        // Método para aguardar até que a API esteja pronta, com retry
        private async Task WaitForApiReadiness(string url, int maxRetries, int retryDelayMs)
        {
            _output.WriteLine($"Verificando disponibilidade da API em: {url}");
            
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        _output.WriteLine($"API está pronta e respondendo com status {response.StatusCode}");
                        return;
                    }
                    _output.WriteLine($"Tentativa {i+1}/{maxRetries}: API respondeu com status {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Tentativa {i+1}/{maxRetries}: Erro ao conectar à API: {ex.Message}");
                }
                
                if (i < maxRetries - 1)
                {
                    _output.WriteLine($"Aguardando {retryDelayMs}ms antes da próxima tentativa...");
                    await Task.Delay(retryDelayMs);
                }
            }
            
            _output.WriteLine("Não foi possível conectar à API após todas as tentativas. Continuando com os testes mesmo assim.");
        }

        [Fact]
        public async Task ReportApi_StressTest_IncreaseConcurrentUsers()
        {
            // Today's date for the request
            var today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
            var url = $"{_reportApiUrl}/api/Report/{today}";

            // Configuration for increasing load test
            var stages = new[]
            {
                (rps: 100, duration: 5), // 100 RPS for 5 seconds
                (rps: 300, duration: 5), // 300 RPS for 5 seconds
                (rps: 500, duration: 5), // 500 RPS for 5 seconds
                (rps: 800, duration: 5)  // 800 RPS for 5 seconds
            };

            _output.WriteLine($"Starting stress test against {url}");
            _output.WriteLine("Gradually increasing load to find breaking point");

            var successCount = 0;
            var failureCount = 0;
            var allResponseTimes = new List<long>();
            var stageMetrics = new List<(int rps, double successRate, double avgResponse)>();

            foreach (var (targetRps, durationSeconds) in stages)
            {
                _output.WriteLine($"\nStage: {targetRps} RPS for {durationSeconds} seconds");

                var totalRequests = targetRps * durationSeconds;
                var tasks = new List<Task<bool>>();
                var responseTimes = new List<long>();
                var stageSuccessCount = 0;
                var stopwatch = new Stopwatch();

                stopwatch.Start();

                // Launch all requests in parallel batches to simulate load
                for (int i = 0; i < totalRequests; i++)
                {
                    // Control the request rate to match target RPS
                    var targetTime = TimeSpan.FromMilliseconds((i * 1000.0) / targetRps);
                    var currentTime = stopwatch.Elapsed;
                    if (currentTime < targetTime)
                    {
                        var sleepTime = targetTime - currentTime;
                        if (sleepTime.TotalMilliseconds > 0)
                            await Task.Delay(sleepTime);
                    }

                    tasks.Add(SendRequest(url, responseTimes));

                    // Processing results in batches to avoid overwhelming memory
                    if (tasks.Count >= 100 || i == totalRequests - 1)
                    {
                        var results = await Task.WhenAll(tasks);
                        stageSuccessCount += results.Count(r => r);
                        tasks.Clear();
                    }
                }

                stopwatch.Stop();

                // Calculate statistics for this stage
                var actualRps = totalRequests / stopwatch.Elapsed.TotalSeconds;
                var stageSuccessRate = (double)stageSuccessCount / totalRequests;
                var stageAvgResponseTime = responseTimes.Count > 0 ? responseTimes.Average() : 0;

                successCount += stageSuccessCount;
                failureCount += (totalRequests - stageSuccessCount);
                allResponseTimes.AddRange(responseTimes);

                stageMetrics.Add((targetRps, stageSuccessRate, stageAvgResponseTime));

                _output.WriteLine($"Actual RPS: {actualRps:F1}");
                _output.WriteLine($"Success rate: {stageSuccessRate:P2}");
                _output.WriteLine($"Average response time: {stageAvgResponseTime:F2} ms");

                // If success rate falls below 50%, we've found the breaking point
                if (stageSuccessRate < 0.5)
                {
                    _output.WriteLine("\nBreaking point detected - stopping test");
                    break;
                }
            }

            // Output overall results
            var totalRequestsProcessed = successCount + failureCount;
            var overallSuccessRate = (double)successCount / totalRequestsProcessed;
            var p95ResponseTime = CalculatePercentile(allResponseTimes, 95);
            var p99ResponseTime = CalculatePercentile(allResponseTimes, 99);

            _output.WriteLine("\nStress Test Summary:");
            _output.WriteLine($"Total requests: {totalRequestsProcessed}");
            _output.WriteLine($"Successful requests: {successCount}");
            _output.WriteLine($"Failed requests: {failureCount}");
            _output.WriteLine($"Overall success rate: {overallSuccessRate:P2}");
            _output.WriteLine($"P95 response time: {p95ResponseTime} ms");
            _output.WriteLine($"P99 response time: {p99ResponseTime} ms");

            _output.WriteLine("\nBreaking point analysis:");
            foreach (var (rps, successRate, avgResponse) in stageMetrics)
            {
                _output.WriteLine($"At {rps} RPS: Success rate = {successRate:P2}, Avg response = {avgResponse:F2} ms");
            }
        }
    }
}
