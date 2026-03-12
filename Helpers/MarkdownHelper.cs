namespace InventoryManager.Helpers
{
    // Converts Markdown text to safe HTML using a minimal built-in approach.
    // In production you would swap this for Markdig or similar.
    public static class MarkdownHelper
    {
        public static string ToHtml(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            // Very basic transformation — replace newlines with <br> and bold/italic markers.
            // Replace with a proper Markdig call when the package is added.
            var html = System.Net.WebUtility.HtmlEncode(markdown)
                .Replace("\r\n", "\n")
                .Replace("\n\n", "</p><p>")
                .Replace("\n", "<br/>");

            return $"<p>{html}</p>";
        }
    }
}
