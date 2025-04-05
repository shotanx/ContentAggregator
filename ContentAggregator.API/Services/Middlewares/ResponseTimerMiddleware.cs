using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace ContentAggregator.API.Services.Middlewares
{
    public class ResponseTimerMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseTimerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        //public async Task Invoke(HttpContext context)
        //{
        //    var originalBody = context.Response.Body;
        //    using (var responseBody = new MemoryStream())
        //    {
        //        context.Response.Body = responseBody;

        //        await _next(context);

        //        responseBody.Position = 0;
        //        using (var reader = new StreamReader(responseBody))
        //        {
        //            var responseBodyContent = await reader.ReadToEndAsync();
        //            responseBody.Position = 0;

        //            await responseBody.CopyToAsync(originalBody);
        //        }


        //        context.Response.Body = originalBody;
        //    }
        //}

        public async Task Invoke(HttpContext context)
        {
            var watch = new Stopwatch();
            watch.Start();

            context.Response.OnStarting(() =>
            {
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                context.Response.Headers["X-Response-Time"] = elapsedMs.ToString();

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
