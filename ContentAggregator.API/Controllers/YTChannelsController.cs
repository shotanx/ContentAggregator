using Microsoft.AspNetCore.Mvc;
using ContentAggregator.Core.Entities;
using ContentAggregator.Core.Models.YTModels;
using Newtonsoft.Json;
using ContentAggregator.Core.Interfaces;
using ContentAggregator.Core.Models.DTOs;
using static ContentAggregator.API.Program;
using System.Net.Http;

namespace ContentAggregator.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YTChannelsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IYTChannelRepository _yTChannelRepository;
        private readonly string _apiKey;

        public YTChannelsController(IHttpClientFactory httpClientFactory, IYTChannelRepository yTChannelRepository, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientNames.Default);
            _yTChannelRepository = yTChannelRepository;
            _apiKey = configuration["YoutubeAccessToken"]!;
        }

        // GET: api/YTChannels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<YTChannel>>> GetYTChannels(CancellationToken cancellationToken)
        {
            var result = await _yTChannelRepository.GetAllChannelsAsync(cancellationToken);

            return Ok(result);
        }

        // GET: api/YTChannels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<YTChannel>> GetYTChannel(string id, CancellationToken cancellationToken)
        {
            var yTChannel = await _yTChannelRepository.GetChannelByIdAsync(id, cancellationToken);

            if (yTChannel == null)
            {
                return NotFound();
            }

            return Ok(yTChannel);
        }

        // PUT: api/YTChannels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutYTChannel(string id, [FromHeader(Name = "Prefer")] string? preferHeader, [FromBody] YtChannelDto yTChannelDto, CancellationToken cancellationToken)
        {
            var existingChannel = await _yTChannelRepository.GetChannelByIdAsync(id, cancellationToken);
            if (existingChannel == null)
            {
                return NotFound();
            }

            // TODO: Use automapper instead.
            existingChannel.Name = yTChannelDto.ChannelTitle;
            existingChannel.Url = new Uri($"https://www.youtube.com/{yTChannelDto.ChannelSuffix}");
            existingChannel.ActivityLevel = yTChannelDto.ActivityLevel;
            existingChannel.TitleKeywords = yTChannelDto.TitleKeywords;

            existingChannel.UpdatedAt = DateTimeOffset.UtcNow;

            await _yTChannelRepository.SaveChangesAsync(cancellationToken);

            bool wantsMinimalResponse = preferHeader?.Contains("return=minimal") ?? false;

            return wantsMinimalResponse ? NoContent() : Ok(existingChannel);
        }

        // POST: api/YTChannels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<YTChannel>> PostYTChannel([FromBody] YtChannelDto channelDTO, CancellationToken cancellationToken)
        {
            try
            {
                var requestUrl = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={channelDTO.ChannelSuffix}&type=channel&key={_apiKey}";

                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var searchResponse = JsonConvert.DeserializeObject<YTSearchResponse>(content);
                    Item item;
                    if (searchResponse!.Items.Count > 1)
                    {
                        if (channelDTO.ChannelTitle == null)
                        {
                            return BadRequest($"Several channels with provided 'channelSuffix' were found. Please provide 'channelTitle' for clarity.");
                        }
                        item = searchResponse.Items.Single(x => x.Snippet.Title == channelDTO.ChannelTitle);
                    }
                    else
                    {
                        item = searchResponse.Items.Single();
                    }

                    if (await _yTChannelRepository.GetChannelByIdAsync(item.Snippet.ChannelId, cancellationToken) != null)
                    {
                        return BadRequest($"Channel with the Channel ID already exists.");
                    }

                    var yTChannel = new YTChannel
                    {
                        Name = item.Snippet.ChannelTitle,
                        Id = item.Snippet.ChannelId,
                        Url = new Uri($"https://www.youtube.com/{channelDTO.ChannelSuffix}"),
                        ActivityLevel = channelDTO.ActivityLevel,
                        TitleKeywords = channelDTO.TitleKeywords
                    };

                    await _yTChannelRepository.AddChannelAsync(yTChannel, cancellationToken);

                    return CreatedAtAction(nameof(GetYTChannel), new { id = yTChannel.Id }, yTChannel);
                }
                else
                {
                    // log error
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return BadRequest($"Error fetching contents from Youtube: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                return BadRequest($"Error occured: {ex.Message}");
            }
        }

        // DELETE: api/YTChannels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteYTChannel(string id, CancellationToken cancellationToken)
        {
            var result = await _yTChannelRepository.DeleteChannelAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
