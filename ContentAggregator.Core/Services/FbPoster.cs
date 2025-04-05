using Facebook;

namespace ContentAggregator.Core.Services
{
    public class FbPoster
    {
        private readonly FacebookClient _fb;

        public FbPoster(string accessToken)
        {
            _fb = new FacebookClient(accessToken);
        }

        public async Task<string> SharePost(string pageId, string? postUrl, string? message = null)
        {
            if (postUrl == null && message == null)
            {
                throw new InvalidOperationException("Either postUrl or message must be provided.");
            }

            var postParameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(postUrl))
            {
                postParameters["link"] = postUrl;
            }

            if (!string.IsNullOrEmpty(message))
            {
                postParameters["message"] = message;
            }

            try
            {
                dynamic result = await _fb.PostTaskAsync($"/{pageId}/feed", postParameters);
                return "Post shared successfully with ID: " + result.id;
            }
            catch (FacebookApiException ex)
            {
                return "Facebook API Error: " + ex.Message;
            }
            catch (Exception ex)
            {
                return "An unexpected error occurred: " + ex.Message;
            }
        }
    }
}
