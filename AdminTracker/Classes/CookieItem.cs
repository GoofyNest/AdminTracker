namespace AdminTracker.Classes
{
    public class CookieItem
    {
        public string Domain { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
        public double ExpirationDate { get; set; }
    }
}
