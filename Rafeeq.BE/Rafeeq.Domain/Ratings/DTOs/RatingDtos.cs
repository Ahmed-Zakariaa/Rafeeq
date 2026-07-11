namespace Rafeeq.Domain.Ratings.DTOs;

public class RateDto
{
    public string BookingUniqueId { get; set; } = string.Empty;
    public int Stars { get; set; }            // 1–5
    public string? Comment { get; set; }
}
