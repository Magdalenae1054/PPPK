namespace Apolon.Domain;

public class Doctor
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;

    public virtual ICollection<Examination> Examinations { get; set; } = new List<Examination>();
}