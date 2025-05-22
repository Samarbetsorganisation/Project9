namespace MerchStore.Infrastructure.External.ReviewApi;

public class ProductWithDetailsRequestDto
{
    public string Mode { get; set; } = "withDetails";
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public List<string> Tags { get; set; }
}