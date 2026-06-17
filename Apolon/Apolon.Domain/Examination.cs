namespace Apolon.Domain;

public class Examination
{
    public int Id { get; set; }
    public ExaminationType Type { get; set; }
    public DateTime ScheduledAt { get; set; }  

    public int PatientId { get; set; }
    public virtual Patient Patient { get; set; } = null!;

    public int DoctorId { get; set; }
    public virtual Doctor Doctor { get; set; } = null!;
}