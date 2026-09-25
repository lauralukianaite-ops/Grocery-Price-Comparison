namespace backend.DTOs;
public class SimilarItemsResponseDto
{
    public int BaseItemId { get; set; }
    public string BaseItemName { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public List<SimilarItemDto> SimilarItems { get; set; } = new();
    public int Count => SimilarItems.Count;
}