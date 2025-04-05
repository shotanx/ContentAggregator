//namespace ContentAggregator.API.Services.Middlewares
//{
//    public class CancellationTokenMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<CancellationTokenMiddleware> _logger;

//        public CancellationTokenMiddleware(RequestDelegate next, ILogger<CancellationTokenMiddleware> logger)
//        {
//            _next = next;
//            _logger = logger;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            try
//            {
//                await _next(context);
//            }
//            catch (OperationCanceledException ex)
//            {
//                _logger.LogInformation(ex, "Request was canceled by the client.");
//                context.Response.StatusCode = 499; // Client Closed Request
//                                                   // Optionally, write a response body for logging purposes
//                await context.Response.WriteAsync("Client closed the request.");
//            }
//        }
//    }
//}
