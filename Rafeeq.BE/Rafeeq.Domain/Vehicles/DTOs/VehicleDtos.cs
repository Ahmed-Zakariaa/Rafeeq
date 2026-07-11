namespace Rafeeq.Domain.Vehicles.DTOs;

public class VehicleCreateDto
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public int SeatsCapacity { get; set; }
}

public class VehicleResultDto
{
    public int Id { get; set; }   // int (used as TripCreateDto.VehicleId in a POST body, not a URL)
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public int SeatsCapacity { get; set; }
}
