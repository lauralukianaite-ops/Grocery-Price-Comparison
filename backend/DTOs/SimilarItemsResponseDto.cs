namespace backend.DTOs;
public record SimilarItemsResponseDto(
    string BaseItemName,
    double Threshold,
    List<SimilarItemDto> SimilarItems
)
{
    public int Count => SimilarItems.Count;
}