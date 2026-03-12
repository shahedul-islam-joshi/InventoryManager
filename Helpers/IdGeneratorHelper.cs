namespace InventoryManager.Helpers
{
    // Utility for generating random alphanumeric strings used in custom ID segments.
    public static class IdGeneratorHelper
    {
        private const string AlphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string RandomAlphanumeric(int length)
        {
            var random = new Random();
            return new string(
                Enumerable.Range(0, length)
                          .Select(_ => AlphanumericChars[random.Next(AlphanumericChars.Length)])
                          .ToArray());
        }

        // Generates a simple GUID-based short ID (first 8 hex chars)
        public static string ShortGuid() => Guid.NewGuid().ToString("N")[..8].ToUpper();
    }
}
