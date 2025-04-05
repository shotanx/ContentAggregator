using Azure;
using Azure.AI.Translation.Text;
using ContentAggregator.Core.Interfaces;
using ContentAggregator.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace ContentAggregator.API.Services.BackgroundServices
{
    public class TranslatorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _apiKey;
        private readonly string _azureTranslatorURL;
        private readonly ILogger<TranslatorService> _logger;

        public TranslatorService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<TranslatorService> logger)
        {
            _serviceProvider = serviceProvider;
            _apiKey = configuration["AzureTranslatorToken"]!;
            _azureTranslatorURL = configuration["AzureTranslatorURL"]!;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var yTRepository = scope.ServiceProvider.GetRequiredService<IYoutubeContentRepository>();

                        var contents = await yTRepository.GetYTContentsWithoutGeoSummaries();

                        _logger.LogInformation($"Translator DB query returned {contents.Count} non-translated entities.");

                        if (!contents.IsNullOrEmpty())
                        {
                            foreach (var content in contents)
                            {
                                content.VideoSummaryGeo = await TranslateSummaryAsync(content.VideoSummaryEng!, stoppingToken);
                                _logger.LogInformation("Azure translator API call returned successfully.");
                            }

                            await yTRepository.UpdateYTContentsRangeAsync(contents);

                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"{nameof(TranslatorService)} threw an exception: {ex}");
                    // Log the exception (logging mechanism not shown here)
                }
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        private async Task<string> TranslateSummaryAsync(string engSummary, CancellationToken stoppingToken)
        {
            var client = new TextTranslationClient(new AzureKeyCredential(_apiKey), new Uri(_azureTranslatorURL));

            var response = await client.TranslateAsync(targetLanguage: "ka", content: new[] { engSummary }, sourceLanguage: "en", stoppingToken);

            return response.Value.Single().Translations.Single().Text;
        }
    }
}
