namespace ECommerce.API.Pages.Shared;

public class PaginationViewModel
{
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public string PageName { get; set; } = string.Empty;
    public Dictionary<string, string?> RouteValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public string GetUrl(int page)
    {
        var targetPage = Math.Max(1, page);
        var queryParams = new List<string>();

        foreach (var kvp in RouteValues)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Value) &&
                !string.Equals(kvp.Key, "page", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(kvp.Key, "pageNumber", StringComparison.OrdinalIgnoreCase))
            {
                queryParams.Add($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
            }
        }

        queryParams.Add($"page={targetPage}");

        var path = string.IsNullOrWhiteSpace(PageName) ? "" : (PageName.StartsWith("/") ? PageName : "/" + PageName);
        return $"{path}?{string.Join("&", queryParams)}";
    }

    public Dictionary<string, string> RouteValuesFor(int page)
    {
        var targetPage = Math.Max(1, page);
        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in RouteValues)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Value) &&
                !string.Equals(kvp.Key, "page", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(kvp.Key, "pageNumber", StringComparison.OrdinalIgnoreCase))
            {
                merged[kvp.Key] = kvp.Value;
            }
        }
        merged["page"] = targetPage.ToString();
        merged["pageNumber"] = targetPage.ToString();
        return merged;
    }
}
