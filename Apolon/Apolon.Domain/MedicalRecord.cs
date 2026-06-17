namespace Apolon.Domain;

public class MedicalRecord
{
    public int Id { get; set; }
    public string Condition { get; set; } = string.Empty;   
    public DateOnly StartDate { get; set; }                
    public DateOnly? EndDate { get; set; }                

    public int PatientId { get; set; }
    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();
}