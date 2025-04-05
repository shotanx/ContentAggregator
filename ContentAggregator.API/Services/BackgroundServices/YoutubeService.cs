using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using ContentAggregator.Infrastructure.Data;
using ContentAggregator.Core.Entities;
using ContentAggregator.Core.Interfaces;
using ContentAggregator.Core.Models.YTModels;
using static ContentAggregator.API.Program;

namespace ContentAggregator.API.Services.BackgroundServices
{
    public class YoutubeService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _apiKey;
        private readonly ILogger<YoutubeService> _logger;

        public YoutubeService(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider, IConfiguration configuration, ILogger<YoutubeService> logger)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientNames.Default);
            _serviceProvider = serviceProvider;
            _logger = logger;
            _apiKey = configuration["YoutubeAccessToken"]!;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // temporary while testing...

                using (var scope = _serviceProvider.CreateScope())
                {
                    var channelRepository = scope.ServiceProvider.GetRequiredService<IYTChannelRepository>();
                    var yTRepository = scope.ServiceProvider.GetRequiredService<IYoutubeContentRepository>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                    _logger.LogInformation("Getting all channels from database.");
                    var channels = await channelRepository.GetAllChannelsAsync(stoppingToken);

                    foreach (var channel in channels)
                    {
                        _logger.LogInformation("Getting contents from youtube.");
                        var yTContentEntities = await FetchYoutubeContents(channel);

                        if (!yTContentEntities.IsNullOrEmpty())
                        {
                            _logger.LogInformation($"Youtube query returned {yTContentEntities.Count} entities");
                            using (var transaction = await dbContext.Database.BeginTransactionAsync())
                            {
                                try
                                {
                                    await yTRepository.AddYTContents(yTContentEntities);

                                    // Update the YTChannel entity
                                    channel.LastPublishedAt = yTContentEntities.Max(x => x.VideoPublishedAt);
                                    await channelRepository.UpdateChannelAsync(channel, stoppingToken);

                                    await transaction.CommitAsync();
                                }
                                catch (Exception ex)
                                {
                                    await transaction.RollbackAsync();
                                    _logger.LogWarning(ex, $"{nameof(YoutubeService)} threw an exception: {ex}");

                                    // Handle the exception (e.g., log the error)
                                }

                            }
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Adjust the interval as needed
            }
        }

        private async Task<List<YoutubeContent>> FetchYoutubeContents(YTChannel channel)
        {
            try
            {
                string requestUrl;
                if (channel.LastPublishedAt != null)
                {
                    DateTimeOffset? date = channel.LastPublishedAt;
                    string formattedDate = date.Value.AddSeconds(1).ToString("yyyy-MM-ddTHH:mm:ssZ"); // Youtube demands RFC 3339 format for this endpoint.
                    requestUrl = $"https://youtube.googleapis.com/youtube/v3/search?key={_apiKey}&channelId={channel.Id}&part=snippet&order=date&publishedAfter={formattedDate}";
                }
                else
                {
                    requestUrl = $"https://youtube.googleapis.com/youtube/v3/search?key={_apiKey}&channelId={channel.Id}&part=snippet&order=date&maxResults=3";
                }
                var response = await _httpClient.GetAsync(requestUrl);
                var yTContentEntities = new List<YoutubeContent>();

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var searchResponse = JsonConvert.DeserializeObject<YTSearchResponse>(content);
                    var items = searchResponse!.Items;

                    if (channel.TitleKeywords != null)
                    {
                        items = items.Where(x => x.Snippet.Title.Contains(channel.TitleKeywords)).ToList();
                    }

                    var idLengthPairs = await FetchLongerVideos(items.Select(x => x.Id.VideoId), TimeSpan.FromMinutes(30));

                    if (idLengthPairs.IsNullOrEmpty())
                    {
                        return null;
                    }

                    foreach (var idLengthPair in idLengthPairs)
                    {
                        var searchResponseItem = items.Single(x => x.Id.VideoId == idLengthPair.Key);
                        var youtubeContent = new YoutubeContent
                        {
                            VideoId = idLengthPair.Key,
                            VideoTitle = searchResponseItem.Snippet.Title, // Replace with actual title
                            ChannelId = channel.Id, // Replace with actual channel ID
                            VideoLength = idLengthPair.Value, // Replace with actual video length
                            VideoPublishedAt = searchResponseItem.Snippet.PublishedAt, // Google API returns UTC, so doesn't need special conversion
                            SubtitlesEngSRT = null // youtube's captions API doesn't allow access to other channels' captions
                        };

                        yTContentEntities.Add(youtubeContent);
                    }

                    return yTContentEntities;
                }
                else
                {
                    // log error
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.Error.WriteLine($"Error fetching YouTube contents: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.Error.WriteLine($"Exception occurred while fetching YouTube contents: {ex.Message}");
                throw;
            }
        }

        private async Task<List<KeyValuePair<string, TimeSpan>>> FetchLongerVideos(IEnumerable<string> videoIds, TimeSpan videoLength)
        {
            if (videoIds.IsNullOrEmpty())
            {
                return null;
            }
            var idLengthPairs = new List<KeyValuePair<string, TimeSpan>>();

            var videoIdString = "";
            foreach (var id in videoIds)
            {
                videoIdString += id + ",";
            }
            videoIdString.TrimEnd(',');

            var requestUrl = $"https://youtube.googleapis.com/youtube/v3/videos?id={videoIdString}&key={_apiKey}&part=contentDetails";
            var response = await _httpClient.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var videosResponse = JsonConvert.DeserializeObject<YTVideosResponse>(content)!;

                foreach (var item in videosResponse.Items)
                {
                    var duration = ParseIso8601Duration(item.ContentDetails.Duration);
                    if (duration > videoLength)
                    {
                        idLengthPairs.Add(new KeyValuePair<string, TimeSpan>(item.Id, duration));
                    }
                }
            }
            return idLengthPairs;
        }

        private TimeSpan ParseIso8601Duration(string duration) // TODO: ამას unit test უნდა დავუწერო.
        {
            if (duration == "P0D") // live-ებს აქვთ ეს duration. TODO: Risk of missing lives if LastPublishedDate is set to higher than live launch date.
            {
                // Think of possible solutions. 1. message broker.
                // 2. Setting LastPublishedDate to lower than live. Risks duplicating newer videos that were added after live started but before live finished.
                // 2. might need additional DB trips to check whether such videos exist. Otherwise, don't add newer than livestream videos.
                return new TimeSpan();
            }
            if (!duration.StartsWith("PT"))
                throw new FormatException("Invalid duration format. Expected 'PT' prefix.");

            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            string timePart = duration.Substring(2); // Remove "PT"
            var matches = Regex.Matches(timePart, @"(\d+)(H|M|S)");

            foreach (Match match in matches)
            {
                int value = int.Parse(match.Groups[1].Value);
                string unit = match.Groups[2].Value;

                switch (unit)
                {
                    case "H":
                        hours = value;
                        break;
                    case "M":
                        minutes = value;
                        break;
                    case "S":
                        seconds = value;
                        break;
                }
            }

            return new TimeSpan(hours, minutes, seconds);
        }
    }
}