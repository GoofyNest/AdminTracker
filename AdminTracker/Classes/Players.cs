using System;

namespace AdminTracker
{
    public class Players
    {
        public string steamName { get; set; }
        public string steamID { get; set; }
        public long lastSeen { get; set; } = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
    }
}
