
using Security.Dto.LogEntry;

namespace Logging.Service.Interface
{
    public interface IAppLoggerService<T>
    {
        public void Debug(string message, AppLogEntry? context = null);
        public void Info(string message, AppLogEntry? context = null);
        public void Warn(string message, AppLogEntry? context = null);
        public void Error(string message, AppLogEntry? context = null);
        public void Exception(Exception ex, string message, AppLogEntry? context = null);
    }
}
