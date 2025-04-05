using ContentAggregator.Core.Interfaces;
using ContentAggregator.Core.Models;
using ContentAggregator.Core.Services;

namespace ContentAggregator.API.Services.BackgroundServices
{
    public class FacebookService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly FbPoster _fbPoster;
        private readonly ILogger<FacebookService> _logger;
        private readonly string _fbPageId;

        public FacebookService(IServiceProvider serviceProvider, FbPoster fbPoster, IConfiguration configuration, ILogger<FacebookService> logger)
        {
            _serviceProvider = serviceProvider;
            _fbPoster = fbPoster;
            _logger = logger;
            _fbPageId = configuration["FbPageId"]!;
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

                        var youtubeContents = await yTRepository.GetYTContentsForFBPost();
                        _logger.LogInformation($"{DateTime.Now}: DB query returned {youtubeContents.Count} items ready to be posted on FB.");

                        if (youtubeContents == null || !youtubeContents.Any())
                        {
                            foreach (var content in youtubeContents)
                            {
                                var postUrl = $"https://www.youtube.com/watch?v={content.VideoId}";
                                var message = (content.VideoSummaryGeo ?? content.VideoSummaryEng) + $"\n\n{Constants.AISummaryDisclaimer}";
                                await _fbPoster.SharePost(_fbPageId, postUrl, message);
                                _logger.LogInformation($"{DateTime.Now}: Posting on FB.");

                                content.FbPosted = true;
                            }
                            await yTRepository.UpdateYTContentsRangeAsync(youtubeContents);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"{nameof(FacebookService)} threw an exception: {ex}");
                    // Log the exception (logging mechanism not shown here)
                }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Adjust the interval as needed
            }
        }
    }
}
