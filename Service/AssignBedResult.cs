namespace WebApplication2.Service;

public enum AssignBedStatus
{
    Created,
    InvalidDates,
    PatientNotFound,
    WardNotFound,
    BedTypeNotFound,
    NoBedAvailable
}

public class AssignBedResult
{
    public AssignBedStatus Status { get; set; }
    public string Message { get; set; } = "";
    public int? BedAssignmentId { get; set; }
    public int? BedId { get; set; }
}