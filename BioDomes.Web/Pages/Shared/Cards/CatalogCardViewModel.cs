namespace BioDomes.Web.Pages.Shared.Cards;

public class CatalogCardViewModel
{
public string Title { get; set; } = string.Empty;
public string? ImagePath { get; set; }
public string? Badge { get; set; }

public IReadOnlyList<CatalogCardMetaItem> Meta { get; set; } = [];
    
public string? EditPage { get; set; }
public IDictionary<string, string>? EditRouteValues { get; set; }

public string? DeletePage { get; set; }
public IDictionary<string, string>? DeleteRouteValues { get; set; }
}

public class CatalogCardMetaItem
{
public string Label { get; set; } = string.Empty;
public string Value { get; set; } = string.Empty;
}
