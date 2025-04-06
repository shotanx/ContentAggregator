using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ContentAggregator.Core.Entities;
using ContentAggregator.Core.Interfaces;
using ContentAggregator.Core.Models;
using static ContentAggregator.API.Program;

namespace ContentAggregator.API.Services.BackgroundServices
{
    public class SummarizerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SummarizerService> _logger;
        private readonly string _lMStudioApiURL;

        public SummarizerService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<SummarizerService> logger)
        {
            _serviceProvider = serviceProvider;
            _httpClient = httpClientFactory.CreateClient(HttpClientNames.LongTimeout);
            _logger = logger;
            _lMStudioApiURL = configuration["LMStudioApiURL"]!;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        _logger.LogInformation($"{DateTime.Now}: Starting LLM Service");
                        var yTRepository = scope.ServiceProvider.GetRequiredService<IYoutubeContentRepository>();
                        var featureRepository = scope.ServiceProvider.GetRequiredService<IFeatureRepository>();

                        var youtubeContents = await yTRepository.GetYTContentsWithoutEngSummaries();
                        var features = await featureRepository.GetAllFeaturesAsync(stoppingToken);

                        foreach (var content in youtubeContents)
                        {
                            if (content.VideoSummaryEng == null)
                            {
                                _logger.LogInformation($"{DateTime.Now}: LLM Service requesting summary for youtube content with ID: {content.Id}.");
                                var generatedSummaryWithParticipants = await GenerateSummaryAsync(content.SubtitlesFiltered);

                                var firstLineEndIndex = generatedSummaryWithParticipants.IndexOf('\n');
                                content.VideoSummaryEng = generatedSummaryWithParticipants.Substring(firstLineEndIndex + 1).Trim();
                                ParseParticipants(generatedSummaryWithParticipants, content, features);

                                await yTRepository.UpdateYTContentsAsync(content);
                                _logger.LogInformation($"{DateTime.Now}: LLM Service successfully generated and saved summary for youtube content with ID: {content.Id}.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"{nameof(SummarizerService)} threw an exception: {ex}");
                    // Log the exception (logging mechanism not shown here)
                }
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        private async Task<string> GenerateSummaryAsync(string subtitles)
        {
            var request = new
            {
                model = "meta-llama-3.1-8b-instruct",  // Match your loaded model in LM Studio
                messages = new[]
                {
                    new { role = "system", content = SummarizeInstruction },
                    new { role = "user", content = subtitles }
                },
                temperature = 0.7,
                max_tokens = 1000
            };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_lMStudioApiURL}chat/completions", content);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            Debug.WriteLine(responseBody);

            var deserializedResponse = JsonSerializer.Deserialize<CompletionResponse>(responseBody, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            return deserializedResponse.Choices[0].Message.Content;
        }

        private void ParseParticipants(string summary, YoutubeContent yTContent, IEnumerable<Feature> features)
        {
            var firstLineEndIndex = summary.IndexOf('\n');
            string participants = summary.Substring(0, firstLineEndIndex).Trim();
            yTContent.AdditionalComments = participants;
            var listOfParticipants = participants.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var participant in listOfParticipants)
            {
                var participantTrimmed = participant.Trim();
                foreach (var feature in features)
                {
                    if (string.Equals(participantTrimmed, feature.LastNameEng, StringComparison.OrdinalIgnoreCase))
                    {
                        var contentFeature = new YoutubeContentFeature
                        {
                            YoutubeContentId = yTContent.Id,
                            FeatureId = feature.Id
                        };
                        yTContent.YoutubeContentFeatures.Add(contentFeature);
                    }
                }
            }
        }

        private class CompletionResponse
        {
            public Choice[] Choices { get; set; }

            public class Choice
            {
                public int Index { get; set; }
                public Message Message { get; set; }
                public string FinishReason { get; set; }
            }

            public class Message
            {
                public string Role { get; set; }
                public string Content { get; set; }
            }
        }

        private const string SummarizeInstruction = """
Your job is to summarize radio shows, podcasts and other conversations, usually involving 2 participants.
As the first line/paragraph of your response, return names of participants of the conversation, separated by a comma and Nothing else on that line.

From the next paragraph, summarize the conversation by focusing on themes, ideas, and key points without mentioning any specific participants(e.g., avoid names like 'John Doe argues...'). Instead:

Use general terms like 'the participants,' 'the speakers,' or 'they say.'

Frame statements passively (e.g., 'It is mentioned that...', 'The podcast discusses...').

Focus on content, not who said it.

Examples:
❌ Avoid: "Dr. Smith explained that climate change accelerates biodiversity loss."
✅ Use: "The podcast highlights that climate change accelerates biodiversity loss," or "It is mentioned that biodiversity loss is linked to rising temperatures."

❌ Avoid: "Sarah and Mark debated the ethics of AI."
✅ Use: "The participants debated the ethics of AI," or "They discussed differing views on AI ethics."

Additional Guidelines:

Do not attribute statements to named individuals, even if roles(host / guest) are clear.
Use terms like 'the participants,' 'the speakers,' or 'they' to refer to contributors.
Phrase claims as 'It is argued that...', 'The podcast explores...', or 'They emphasize... max_tokens:2000
""";
    }
}