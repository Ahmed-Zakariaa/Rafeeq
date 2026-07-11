using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Vehicles;
using Rafeeq.Domain.Vehicles.DTOs;
using Rafeeq.Domain.Vehicles.IService;
using Rafeeq.Infrastructure.Persistence;

namespace Rafeeq.Application.Vehicles;

public class VehicleService : IVehicleService
{
    private readonly RafeeqDbContext _db;
    private readonly ICurrentUser _currentUser;

    public VehicleService(RafeeqDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ResultViewModel<bool>> Create(VehicleCreateDto dto)
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");

        var vehicle = new Vehicle(userId, dto.Make, dto.Model, dto.Color, dto.PlateNumber, dto.SeatsCapacity);
        await _db.Vehicles.AddAsync(vehicle);
        await _db.SaveChangesAsync();

        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<List<VehicleResultDto>>> GetMine()
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");

        var vehicles = await _db.Vehicles.AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.Id)
            .Select(v => new VehicleResultDto
            {
                Id = v.Id,
                Make = v.Make,
                Model = v.Model,
                Color = v.Color,
                PlateNumber = v.PlateNumber,
                SeatsCapacity = v.SeatsCapacity,
            })
            .ToListAsync();

        return ResultViewModel<List<VehicleResultDto>>.Success(vehicles);
    }
}
