using Apolon.Data;
using Apolon.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Apolon.ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("ApolonDb");

        var options = new DbContextOptionsBuilder<ApolonDbContext>()
            .UseLazyLoadingProxies()       
            .UseNpgsql(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;

        using var context = new ApolonDbContext(options);
        DoctorSeeder.Seed(context);

        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("===== APOLON - Medicinski sustav =====");
            Console.WriteLine("1. Prikaži sve pacijente");
            Console.WriteLine("2. Dodaj pacijenta");
            Console.WriteLine("3. Uredi pacijenta");
            Console.WriteLine("4. Obriši pacijenta");
            Console.WriteLine("5. Pretraži pacijente (po prezimenu)");
            Console.WriteLine("6. Prikaži liječnike");
            Console.WriteLine("7. Dodaj karton i pregled pacijentu");
            Console.WriteLine("8. Detalji pacijenta (EAGER loading)");
            Console.WriteLine("9. Detalji pacijenta (LAZY loading)");
    //        Console.WriteLine("10. Demonstracija change trackinga");
            Console.WriteLine("0. Izlaz");
            Console.Write("Odabir: ");

            var choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1": PrikaziPacijente(context); break;
                case "2": DodajPacijenta(context); break;
                case "3": UrediPacijenta(context); break;
                case "4": ObrisiPacijenta(context); break;
                case "5": PretraziPacijente(context); break;
                case "6": PrikaziLijecnike(context); break;
                case "7": DodajKartonIPregled(context); break;
                case "8": DetaljiPacijentaEager(context); break;
                case "9": DetaljiPacijentaLazy(context); break;
    //            case "10": DemoChangeTracking(context); break;
                case "0": running = false; break;
                default: Console.WriteLine("Nepoznat odabir."); break;
            }
        }
    }
    static void PrikaziPacijente(ApolonDbContext context)
    {
        var pacijenti = context.Patients
            .OrderBy(p => p.LastName)        
            .ToList();

        if (!pacijenti.Any())
        {
            Console.WriteLine("Nema pacijenata u bazi.");
            return;
        }

        foreach (var p in pacijenti)
        {
            Console.WriteLine($"[{p.Id}] {p.FirstName} {p.LastName} | OIB: {p.Oib} " +
                              $"| Rođen: {p.DateOfBirth} | Spol: {p.Gender}");
        }
    }

    static void DodajPacijenta(ApolonDbContext context)
    {
        Console.Write("Ime: ");
        var ime = Console.ReadLine() ?? "";
        Console.Write("Prezime: ");
        var prezime = Console.ReadLine() ?? "";
        Console.Write("OIB (11 znamenki): ");
        var oib = Console.ReadLine() ?? "";
        Console.Write("Datum rođenja (yyyy-mm-dd): ");
        var datumStr = Console.ReadLine() ?? "";
        Console.Write("Spol (Musko/Zensko): ");
        var spolStr = Console.ReadLine() ?? "";
        Console.Write("Adresa boravišta: ");
        var boraviste = Console.ReadLine() ?? "";
        Console.Write("Adresa prebivališta: ");
        var prebivaliste = Console.ReadLine() ?? "";

        if (!DateOnly.TryParse(datumStr, out var datum))
        {
            Console.WriteLine("Neispravan datum.");
            return;
        }
        if (!Enum.TryParse<Gender>(spolStr, out var spol))
        {
            Console.WriteLine("Neispravan spol.");
            return;
        }

        var pacijent = new Patient
        {
            FirstName = ime,
            LastName = prezime,
            Oib = oib,
            DateOfBirth = datum,
            Gender = spol,
            ResidenceAddress = boraviste,
            DomicileAddress = prebivaliste
        };

        context.Patients.Add(pacijent);
        context.SaveChanges();

        Console.WriteLine($"Pacijent dodan (Id: {pacijent.Id}).");
    }
    static void UrediPacijenta(ApolonDbContext context)
    {
        Console.Write("Id pacijenta za uređivanje: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Neispravan Id.");
            return;
        }

        var pacijent = context.Patients.Find(id);
        if (pacijent == null)
        {
            Console.WriteLine("Pacijent nije pronađen.");
            return;
        }

        Console.Write($"Novo ime ({pacijent.FirstName}): ");
        var ime = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(ime)) pacijent.FirstName = ime;

        Console.Write($"Novo prezime ({pacijent.LastName}): ");
        var prezime = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(prezime)) pacijent.LastName = prezime;

        Console.Write($"Nova adresa boravišta ({pacijent.ResidenceAddress}): ");
        var boraviste = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(boraviste)) pacijent.ResidenceAddress = boraviste;

        context.SaveChanges();
        Console.WriteLine("Pacijent ažuriran.");
    }
    static void ObrisiPacijenta(ApolonDbContext context)
    {
        Console.Write("Id pacijenta za brisanje: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Neispravan Id.");
            return;
        }

        var pacijent = context.Patients.Find(id);
        if (pacijent == null)
        {
            Console.WriteLine("Pacijent nije pronađen.");
            return;
        }

        context.Patients.Remove(pacijent); 
        context.SaveChanges();
        Console.WriteLine("Pacijent obrisan.");
    }
    static void PretraziPacijente(ApolonDbContext context)
    {
        Console.Write("Unesi prezime (ili dio): ");
        var pojam = Console.ReadLine() ?? "";

        var rezultati = context.Patients
            .Where(p => p.LastName.ToLower().Contains(pojam.ToLower()))  
            .OrderBy(p => p.LastName)
            .ToList();

        if (!rezultati.Any())
        {
            Console.WriteLine("Nema rezultata.");
            return;
        }

        foreach (var p in rezultati)
            Console.WriteLine($"[{p.Id}] {p.FirstName} {p.LastName} | OIB: {p.Oib}");
    }
    static void PrikaziLijecnike(ApolonDbContext context)
    {
        var lijecnici = context.Doctors.OrderBy(d => d.LastName).ToList();
        foreach (var d in lijecnici)
            Console.WriteLine($"[{d.Id}] dr. {d.FirstName} {d.LastName} - {d.Specialization}");
    }

    static void DodajKartonIPregled(ApolonDbContext context)
    {
        Console.Write("Id pacijenta: ");
        if (!int.TryParse(Console.ReadLine(), out var pacijentId))
        {
            Console.WriteLine("Neispravan Id.");
            return;
        }

        var pacijent = context.Patients.Find(pacijentId);
        if (pacijent == null)
        {
            Console.WriteLine("Pacijent nije pronađen.");
            return;
        }

        Console.Write("Dijagnoza/stanje: ");
        var stanje = Console.ReadLine() ?? "";

        var karton = new MedicalRecord
        {
            Condition = stanje,
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            PatientId = pacijent.Id
        };

        Console.Write("Naziv lijeka: ");
        var lijek = Console.ReadLine() ?? "";
        Console.Write("Doza (npr. 500 mg): ");
        var doza = Console.ReadLine() ?? "";
        Console.Write("Učestalost (npr. 3 puta dnevno): ");
        var ucestalost = Console.ReadLine() ?? "";

        karton.Medications.Add(new Medication
        {
            Name = lijek,
            Dose = doza,
            Frequency = ucestalost
        });

        Console.Write("Tip pregleda (CT/MR/ULTRA/EKG/ECHO/OKO/DERM/DENTA/MAMMO/EEG): ");
        var tipStr = Console.ReadLine() ?? "";
        Console.Write("Id liječnika: ");
        int.TryParse(Console.ReadLine(), out var doctorId);

        if (Enum.TryParse<ExaminationType>(tipStr, out var tip) &&
            context.Doctors.Any(d => d.Id == doctorId))
        {
            var pregled = new Examination
            {
                Type = tip,
                ScheduledAt = DateTime.UtcNow.AddDays(7),
                PatientId = pacijent.Id,
                DoctorId = doctorId
            };
            context.Examinations.Add(pregled);
        }
        else
        {
            Console.WriteLine("Pregled preskočen (neispravan tip ili liječnik).");
        }

        context.MedicalRecords.Add(karton);

        context.SaveChanges();
        Console.WriteLine("Karton, lijek i pregled dodani.");
    }

    static void DetaljiPacijentaEager(ApolonDbContext context)
    {
        Console.Write("Id pacijenta: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Neispravan Id.");
            return;
        }

        var pacijent = context.Patients
            .Include(p => p.MedicalRecords)        
                .ThenInclude(m => m.Medications)  
            .Include(p => p.Examinations)         
                .ThenInclude(e => e.Doctor)      
            .FirstOrDefault(p => p.Id == id);

        if (pacijent == null)
        {
            Console.WriteLine("Pacijent nije pronađen.");
            return;
        }

        IspisiDetalje(pacijent);
    }

    static void DetaljiPacijentaLazy(ApolonDbContext context)
    {
        Console.Write("Id pacijenta: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Neispravan Id.");
            return;
        }

        var pacijent = context.Patients.FirstOrDefault(p => p.Id == id);

        if (pacijent == null)
        {
            Console.WriteLine("Pacijent nije pronađen.");
            return;
        }

           IspisiDetalje(pacijent);
    }

    static void IspisiDetalje(Patient p)
    {
        Console.WriteLine($"\nPacijent: {p.FirstName} {p.LastName} (OIB: {p.Oib})");

        Console.WriteLine("Kartoni (povijest bolesti):");
        foreach (var k in p.MedicalRecords)
        {
            Console.WriteLine($"  - {k.Condition} (od {k.StartDate})");
            foreach (var lijek in k.Medications)
                Console.WriteLine($"      lijek: {lijek.Name}, {lijek.Dose}, {lijek.Frequency}");
        }

        Console.WriteLine("Pregledi:");
        foreach (var e in p.Examinations)
            Console.WriteLine($"  - {e.Type} kod dr. {e.Doctor.LastName} ({e.ScheduledAt:dd.MM.yyyy})");
    }

    /*static void DemoChangeTracking(ApolonDbContext context)
    {
        Console.Write("Id pacijenta za demonstraciju: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Neispravan Id.");
            return;
        }

        var pacijent = context.Patients.FirstOrDefault(p => p.Id == id);
        if (pacijent == null)
        {
            Console.WriteLine("Pacijent nije pronađen.");
            return;
        }

        Console.WriteLine("\n--- 1) Stanje odmah nakon dohvaćanja ---");
        IspisiStanja(context);

        var staroPrezime = pacijent.LastName;
        pacijent.LastName = staroPrezime + "_IZMJENA";
        Console.WriteLine($"\nPromijenili smo prezime: '{staroPrezime}' -> '{pacijent.LastName}'");

        Console.WriteLine("\n--- 2) Stanje nakon izmjene (prije SaveChanges) ---");
        IspisiStanja(context);

        context.SaveChanges();

        Console.WriteLine("\n--- 3) Stanje nakon SaveChanges ---");
        IspisiStanja(context);

        pacijent.LastName = staroPrezime;
        context.SaveChanges();
        Console.WriteLine("\n(Prezime vraćeno na izvornu vrijednost.)");
    }

    static void IspisiStanja(ApolonDbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries())
        {
            Console.WriteLine($"  {entry.Entity.GetType().Name} (Id pristup preko entiteta) - stanje: {entry.State}");
        }
    }
    */

}