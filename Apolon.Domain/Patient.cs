namespace Apolon.Domain;

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Oib { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }   //

    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string ResidenceAddress { get; set; } = string.Empty;   
    public string DomicileAddress { get; set; } = string.Empty;    
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public virtual ICollection<Examination> Examinations { get; set; } = new List<Examination>();

}