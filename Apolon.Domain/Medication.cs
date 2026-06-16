namespace Apolon.Domain;

public class Medication
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;        
    public string Frequency { get; set; } = string.Empty;   

    public int MedicalRecordId { get; set; }
    public virtual MedicalRecord MedicalRecord { get; set; } = null!;
}