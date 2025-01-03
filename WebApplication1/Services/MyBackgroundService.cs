namespace WebApplication1.Services
{
    public class MyBackgroundService : BackgroundService
    {
        private readonly ILogger<MyBackgroundService> _logger;

        public MyBackgroundService(ILogger<MyBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Background task is running at: {time}", DateTimeOffset.Now);
                // Simulate some background work
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}