using Microsoft.EntityFrameworkCore;
using GestionOrdinateurs.Shared; 

namespace GestionOrdinateurs.API.Data; 

/// <summary>
/// Contexte de base de données pour la gestion du parc informatique avec EF Core.
/// </summary> 
public class ParcInformatiqueContext : DbContext
{
    public ParcInformatiqueContext() 
    { 
    }

    public ParcInformatiqueContext(DbContextOptions<ParcInformatiqueContext> options) : base(options) 
    { 
    }

    // Regroupement des tables
    public DbSet<Ordinateur> Ordinateurs { get; set; }
    public DbSet<Reparation> Reparations { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // On ne configure SQLite que si aucune autre option n'a été passée.
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=parcinfo.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ordinateur>()
            .OwnsOne(o => o.Caracteristiques);

        modelBuilder.Entity<Ordinateur>()
            .HasMany(o => o.Reparations)
            .WithOne(r => r.UnOrdinateur);

        // --- AJOUT TP : Force le login à être unique en base de données ---
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();
    }
}