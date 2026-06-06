using Microsoft.AspNetCore.Mvc;
using WebApplication2.DTO;
using WebApplication2.Service;

namespace WebApplication2.Controllers;

public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    
    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }
    
    [HttpGet("api/patients")]
    public async Task<IActionResult> GetPatients ([FromQuery] string? search)
    {
        var patients = await _patientService.GetPatientsAsync(search);
        return Ok(patients);
    }
    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(string pesel, [FromBody] CreateBedAssignmentDto dto)
    {
        var result = await _patientService.AssignBedAsync(pesel, dto);

        return result.Status switch
        {
            AssignBedStatus.Created => StatusCode(201, new
            {
                message = result.Message,
                bedAssignmentId = result.BedAssignmentId,
                bedId = result.BedId
            }),
            AssignBedStatus.InvalidDates    => BadRequest(new { message = result.Message }),
            AssignBedStatus.PatientNotFound => NotFound(new { message = result.Message }),
            AssignBedStatus.WardNotFound    => NotFound(new { message = result.Message }),
            AssignBedStatus.BedTypeNotFound => NotFound(new { message = result.Message }),
            AssignBedStatus.NoBedAvailable  => NotFound(new { message = result.Message }),
            _ => StatusCode(500)
        };
    }
}