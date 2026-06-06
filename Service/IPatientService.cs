using WebApplication2.DTO;

namespace WebApplication2.Service;

public interface IPatientService
{ 
    Task<List<PatientDto>> GetPatientsAsync(string? search);
    Task<AssignBedResult> AssignBedAsync(string pesel, CreateBedAssignmentDto dto);
}