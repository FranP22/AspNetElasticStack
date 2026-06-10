using Logging.Service.Interface;
using Microsoft.AspNetCore.Http;
using Security.Dto.LogEntry;
using Security.Service.Dto;
using Security.Service.Interface;

namespace Common.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAppLoggerService<ExceptionMiddleware> _logger;
        private readonly IClientIpService _clientIpService;

        public ExceptionMiddleware(
            RequestDelegate next,
            IAppLoggerService<ExceptionMiddleware> logger,
            IClientIpService clientIpService
            )
        {
            _next = next;
            _logger = logger;
            _clientIpService = clientIpService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.Exception(
                    ex,
                    "Unhandled exception occurred",
                    new AppLogEntry
                    {
                        Ip = _clientIpService.GetIp(),
                        Endpoint = context.Request.Path,
                        TraceId = context.TraceIdentifier,
                        UserId = context.User?.Identity?.Name
                    });

                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new
                {
                    Message = "Internal server error"
                });
            }
        }
    }
}
