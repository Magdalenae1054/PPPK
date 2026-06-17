using Apolon.Domain;

namespace Apolon.Data;

public static class DoctorSeeder
{
    public static void Seed(ApolonDbContext context)
    {
        if (context.Doctors.Any())
            return;

        var doctors = new List<Doctor>
        {
            new Doctor { FirstName = "Petar",    LastName = "Petrić",   Specialization = "Kardiologija" },
            new Doctor { FirstName = "Ivan",   LastName = "Ivić",    Specialization = "Radiologija" },
            new Doctor { FirstName = "Marija", LastName = "Marić",    Specialization = "Neurologija" },
            new Doctor { FirstName = "Ana",  LastName = "Anić",    Specialization = "Dermatologija" },
            new Doctor { FirstName = "Lucija", LastName = "Lucić",    Specialization = "Oftalmologija" }
        };

        context.Doctors.AddRange(doctors);
        context.SaveChanges();

        Console.WriteLine("Liječnici su dodani (prvo pokretanje).");
    }
}