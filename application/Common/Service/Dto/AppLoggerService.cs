using Common.Dto.Log;
using Logging.Service.Interface;
using Microsoft.Extensions.Logging;

namespace Logging.Service.Dto
{
    public class AppLoggerService<T> : IAppLoggerService<T>
    {
        private readonly ILogger<T> _logger;

        public AppLoggerService(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void Debug(string message, AppLogEntry? context = null)
        {
            var log = Build(message, context);
            log.Level = "DEBUG";

            _logger.LogDebug("{@Log}", log);
        }

        public void Info(string message, AppLogEntry? context = null)
        {
            var log = Build(message, context);
            log.Level = "INFO";

            _logger.LogInformation("{@Log}", Build(message, context));
        }

        public void Warn(string message, AppLogEntry? context = null)
        {
            var log = Build(message, context);
            log.Level = "WARN";

            _logger.LogWarning("{@Log}", Build(message, context));
        }

        public void Error(string message, AppLogEntry? context = null)
        {
            var log = Build(message, context);
            log.Level = "ERROR";

            _logger.LogError("{@Log}", Build(message, context));
        }

        public void Exception(Exception ex, string message, AppLogEntry? context = null)
        {
            var log = Build(message, context);
            log.Level = "CRITICAL";
            log.Exception = ex;

            _logger.LogCritical(ex, "{@Log}", log);
        }

        private AppLogEntry Build(string message, AppLogEntry? context)
        {
            context ??= new AppLogEntry();

            context.Message = message;
            context.Environment ??= Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            context.Service ??= typeof(T).Name;

            return context;
        }
    }
}
