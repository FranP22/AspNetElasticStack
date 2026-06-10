using Microsoft.AspNetCore.Http;

namespace Security.Middleware
{
    public class ClientIpMiddleware
    {
        private readonly RequestDelegate _next;

        public ClientIpMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            string? clientIp;

            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                clientIp = forwardedFor.Split(",")[0].Trim();
            }
            else
            {
                clientIp = context.Connection.RemoteIpAddress?.ToString();
            }

            context.Items["ClientIp"] = clientIp;

            await _next(context);
        }
    }
}
