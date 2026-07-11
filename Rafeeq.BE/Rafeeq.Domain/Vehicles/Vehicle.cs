using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;

namespace Rafeeq.Domain.Vehicles;

public class Vehicle : BaseEntity<int>
{
    public int UserId { get; private set; }            // the driver who owns it
    public User? User { get; private set; }
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public string PlateNumber { get; private set; } = string.Empty;
    public int SeatsCapacity { get; private set; }

    private Vehicle() { }

    public Vehicle(int userId, string make, string model, string color, string plateNumber, int seatsCapacity)
    {
        UserId = userId;
        Make = make;
        Model = model;
        Color = color;
        PlateNumber = plateNumber;
        SeatsCapacity = seatsCapacity;
    }
}
