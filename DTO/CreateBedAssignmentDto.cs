namespace WebApplication2.DTO;

public class CreateBedAssignmentDto
{
    public DateTime From { get; set; }
    public DateTime? To { get; set; }     // opcjonalne (przykład 2 nie ma "to")
    public string BedType { get; set; } = null!;
    public string Ward { get; set; } = null!;
}