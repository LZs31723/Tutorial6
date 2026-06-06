using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.DTO;
using WebApplication2.Entities;

namespace WebApplication2.Service;

public class PatientService : IPatientService
{
    private readonly AppDbContext _context;

    public PatientService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PatientDto>> GetPatientsAsync(string? search)
    {
        var query = _context.Patients.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p =>
                p.FirstName.Contains(search) || p.LastName.Contains(search));
        }

        return await query
            .Select(p => new PatientDto
            {
                Pesel = p.Pesel,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Age = p.Age,
                Sex = p.Sex ? "Male" : "Female",
                Admissions = p.Admissions.Select(a => new AdmissionDto
                {
                    Id = a.Id,
                    AdmissionDate = a.AdmissionDate,
                    DischargeDate = a.DischargeDate,
                    Ward = new WardDto
                    {
                        Id = a.Ward.Id,
                        Name = a.Ward.Name,
                        Description = a.Ward.Description
                    }
                }).ToList(),
                BedAssignments = p.BedAssignments.Select(ba => new BedAssignmentDto
                {
                    Id = ba.Id,
                    From = ba.From,
                    To = ba.To,
                    Bed = new BedDto
                    {
                        Id = ba.Bed.Id,
                        BedType = new BedTypeDto
                        {
                            Id = ba.Bed.BedType.Id,
                            Name = ba.Bed.BedType.Name,
                            Description = ba.Bed.BedType.Description
                        },
                        Room = new RoomDto
                        {
                            Id = ba.Bed.Room.Id,
                            HasTv = ba.Bed.Room.HasTv,
                            Ward = new WardDto
                            {
                                Id = ba.Bed.Room.Ward.Id,
                                Name = ba.Bed.Room.Ward.Name,
                                Description = ba.Bed.Room.Ward.Description
                            }
                        }

                    }

                }).ToList()

            }).ToListAsync();
    }

    public async Task<AssignBedResult> AssignBedAsync(string pesel, CreateBedAssignmentDto dto)
{
    if (dto.To.HasValue && dto.To.Value <= dto.From)
        return new AssignBedResult
        {
            Status = AssignBedStatus.InvalidDates,
            Message = "The 'to' date must be later than the 'from' date."
        };
    
    var patientExists = await _context.Patients.AnyAsync(p => p.Pesel == pesel);
    if (!patientExists)
        return new AssignBedResult
        {
            Status = AssignBedStatus.PatientNotFound,
            Message = $"Patient with PESEL '{pesel}' was not found."
        };
    
    var ward = await _context.Wards.FirstOrDefaultAsync(w => w.Name == dto.Ward);
    if (ward == null)
        return new AssignBedResult
        {
            Status = AssignBedStatus.WardNotFound,
            Message = $"Ward '{dto.Ward}' was not found."
        };
    
    var bedType = await _context.BedTypes.FirstOrDefaultAsync(bt => bt.Name == dto.BedType);
    if (bedType == null)
        return new AssignBedResult
        {
            Status = AssignBedStatus.BedTypeNotFound,
            Message = $"Bed type '{dto.BedType}' was not found."
        };
    
    var requestedFrom = dto.From;
    var requestedTo = dto.To;

    var freeBed = await _context.Beds
        .Where(b => b.BedTypeId == bedType.Id && b.Room.WardId == ward.Id)
        .Where(b => !b.BedAssignments.Any(ba =>
            (requestedTo == null || ba.From < requestedTo) &&
            (ba.To == null || requestedFrom < ba.To)))
        .FirstOrDefaultAsync();

    if (freeBed == null)
        return new AssignBedResult
        {
            Status = AssignBedStatus.NoBedAvailable,
            Message = $"No available '{dto.BedType}' bed in ward '{dto.Ward}' for the requested period."
        };
    
    var assignment = new BedAssignment
    {
        PatientPesel = pesel,
        BedId = freeBed.Id,
        From = dto.From,
        To = dto.To
    };
    _context.BedAssignments.Add(assignment);
    await _context.SaveChangesAsync();

    return new AssignBedResult
    {
        Status = AssignBedStatus.Created,
        Message = $"Bed {freeBed.Id} assigned to patient '{pesel}'.",
        BedAssignmentId = assignment.Id,
        BedId = freeBed.Id
    };
}
}