namespace WebApplication2.DTO;

public class PatientDto
{
    public string Pesel { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName = null!;
    public int Age { get; set; }
    public string Sex { get; set; } = null!;
    public List<AdmissionDto> Admissions { get; set; } = new();
    public List<BedAssignmentDto> BedAssignments { get; set; } = new();
    
}