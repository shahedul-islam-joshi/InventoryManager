namespace InventoryManager.Helpers
{
    // Helper methods for reading and resolving the user's preferred UI theme.
    public static class ThemeHelper
    {
        // Valid theme names — must match the CSS files in wwwroot/css/
        public const string Dark = "dark";
        public const string Light = "light";

        public static string Resolve(string? preference)
        {
            return preference?.ToLowerInvariant() == Dark ? Dark : Light;
        }

        // Returns the CSS filename for the given theme
        public static string GetCssFile(string theme)
        {
            return theme == Dark ? "dark-theme.css" : "light-theme.css";
        }
    }
}
