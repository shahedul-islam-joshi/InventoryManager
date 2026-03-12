namespace InventoryManager.Helpers
{
    // Helper methods for reading and applying the user's preferred language.
    public static class LocalizationHelper
    {
        // Supported culture codes — add more as .resx files are added
        private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
        {
            "en", "es", "pl"
        };

        // Returns the culture code to use for the given user preference.
        // Falls back to "en" for unsupported or null values.
        public static string Resolve(string? preference)
        {
            if (!string.IsNullOrWhiteSpace(preference) && Supported.Contains(preference))
                return preference;
            return "en";
        }
    }
}
