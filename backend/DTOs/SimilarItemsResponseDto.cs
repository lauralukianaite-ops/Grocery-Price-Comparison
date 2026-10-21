namespace backend.DTOs;
public class SimilarItemsResponseDto(
    string BaseItemName,
    double Threshold,
    List<SimilarItemDto> SimilarItems
)
{
    public int Count => SimilarItems.Count;
}