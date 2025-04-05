using System.Diagnostics;
using System.Runtime.InteropServices;
using ContentAggregator.Core.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ContentAggregator.API.Services.BackgroundServices
{
    public class SubtitleService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SubtitleService> _logger;

        public SubtitleService(IServiceProvider serviceProvider, ILogger<SubtitleService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string tempDir = CreateTempDirectory(Path.GetTempPath());
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var yTRepository = scope.ServiceProvider.GetRequiredService<IYoutubeContentRepository>();

                        var youtubeContents = await yTRepository.GetYTContentsWithoutEngSRT();

                        foreach (var content in youtubeContents)
                        {
                            var subtitleSRTFile = await DownloadSubtitlesAsync(content.VideoId, tempDir);
                            if (subtitleSRTFile.IsNullOrEmpty())
                            {
                                continue;
                            }
                            string subtitleSRTString = await File.ReadAllTextAsync(subtitleSRTFile!);
                            string[] subtitleSRTLines = await File.ReadAllLinesAsync(subtitleSRTFile!);
                            if (!string.IsNullOrEmpty(subtitleSRTString))
                            {
                                content.SubtitlesEngSRT = subtitleSRTString;
                                content.SubtitlesFiltered = SRTToText(subtitleSRTLines);
                                await yTRepository.UpdateYTContentsAsync(content);
                                _logger.LogInformation("Subtitles downloaded and filtered successfully.");
                            }

                            var random = new Random();
                            await Task.Delay(TimeSpan.FromSeconds(random.Next(50, 70)), stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"{nameof(SubtitleService)} threw an exception: {ex}");
                    // Log the exception (logging mechanism not shown here)
                }
                finally
                {
                    Directory.Delete(tempDir, recursive: true);
                }
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        private async Task<string?> DownloadSubtitlesAsync(string videoId, string tempDir)
        {
            string tempDirForSingleSub = CreateTempDirectory(tempDir);
            string executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "yt-dlp.exe" : "yt-dlp";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine("tools", executableName),
                //FileName = Constants.ytdlpLocation,
                Arguments = $"--write-auto-sub --sub-lang en --convert-subs srt --output \"subtitle.%(ext)s\" --skip-download https://www.youtube.com/watch?v={videoId}",
                WorkingDirectory = tempDirForSingleSub,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    throw new Exception($"yt-dlp error: {error}");
                }

                if (output.Contains("There are no subtitles for the requested languages"))
                {
                    _logger.LogInformation($"yt-dlp error: No subtitle found for videoId {videoId}");
                    return null;
                }

                return Directory.GetFiles(tempDirForSingleSub, "*.srt").Single();
            }
        }

        private string CreateTempDirectory(string initialPath)
        {
            string tempDir = Path.Combine(initialPath, Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        private string SRTToText(string[] lines)
        {
            var cleanedLines = new List<string>();
            string? previousLine = null;

            foreach (var line in lines)
            {
                if (IsSubtitleMetadata(line)) continue;

                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                if (trimmed != previousLine)
                {
                    cleanedLines.Add(trimmed);
                    previousLine = trimmed;
                }
            }

            var cleanedString = string.Join(Environment.NewLine, cleanedLines);

            return cleanedString;
        }

        private bool IsSubtitleMetadata(string line)
        {
            // Check for sequence numbers (e.g., "1") or timestamps (e.g., "00:00:01,000 --> 00:00:04,000")
            return string.IsNullOrWhiteSpace(line) ||
                   int.TryParse(line.Trim(), out _) ||
                   line.Contains("-->");
        }
    }
}