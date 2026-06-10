namespace Common.Dto.Log
{
    public class AppLogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string Level { get; set; } = "Information";

        public string Message { get; set; } = string.Empty;

        public string? Service { get; set; }

        public string? Environment { get; set; }

        public string? Ip { get; set; }

        public string? UserId { get; set; }

        public string? Endpoint { get; set; }

        public string? TraceId { get; set; }

        public Exception? Exception { get; set; }

        public Dictionary<string, object>? Properties { get; set; }
    }
}
