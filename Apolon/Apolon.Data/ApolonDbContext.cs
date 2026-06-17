using Apolon.Domain;
using Microsoft.EntityFrameworkCore;

namespace Apolon.Data;

public class ApolonDbContext : DbContext
{
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<Examination> Examinations { get; set; }

    public ApolonDbContext(DbContextOptions<ApolonDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.Id);                       
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);  
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Oib).IsRequired().HasMaxLength(11);
            entity.HasIndex(p => p.Oib).IsUnique();        
            entity.Property(p => p.ResidenceAddress).IsRequired();
            entity.Property(p => p.DomicileAddress).IsRequired();
        });

        // DOCTOR
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(d => d.LastName).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Specialization).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Condition).IsRequired();

            entity.HasOne(m => m.Patient)
                  .WithMany(p => p.MedicalRecords)
                  .HasForeignKey(m => m.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);  
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).IsRequired();
            entity.Property(m => m.Dose).IsRequired();
            entity.Property(m => m.Frequency).IsRequired();

            entity.HasOne(m => m.MedicalRecord)
                  .WithMany(mr => mr.Medications)
                  .HasForeignKey(m => m.MedicalRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Examination>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Patient)
                  .WithMany(p => p.Examinations)
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Doctor)
                  .WithMany(d => d.Examinations)
                  .HasForeignKey(e => e.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);  
        });

        base.OnModelCreating(modelBuilder);
    }
}