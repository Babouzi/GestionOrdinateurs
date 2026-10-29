using GestionOrdinateurs.API.Controllers;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public class ReparationService : IReparationService
{
    private readonly ParcInformatiqueContext _context;
    
    // 1. On crée un objet Lock
    private static readonly object _cadenas = new object();

    public ReparationService(ParcInformatiqueContext context)
    {
        _context = context;
    }

   
    public async Task SimulerReparationAsync(Ordinateur ordinateur)
    {
        lock (_cadenas) 
        {
            Reparation test = new Reparation(0, ordinateur, "Maintenance standard", DateTime.Now, "En cours", "Auto");

            _context.Reparations.Add(test);
            
            _context.SaveChanges(); 
        
            Thread.Sleep(2000); 
            
            test.Statut = "Terminée";

            _context.SaveChanges(); 
            
            Console.WriteLine($"{ordinateur.Nom} réparé");

            var journal = new JournalService();
            journal.Ecrire(@"Logs\Reparations\reparations.log", $"{DateTime.Now:dd/MM/yyyy} - {ordinateur.Nom} réparé");
        }
    }

    public async Task<List<Reparation>> ListerReparationsAsync()
    {
        return await _context.Reparations.Include(r => r.UnOrdinateur).ToListAsync();
    }

    public async Task<List<HistoriqueReparationRecord>> ObtenirHistoriqueAsync()
{
    // On prend la table des réparations
    var historique = await _context.Reparations
        // Pour chaque réparation r, on crée un nouveau record
        .Select(r => new HistoriqueReparationRecord(
           
           // Id de l'ordinateur.
            r.UnOrdinateur.Id, 

            // Nom de l'ordinateur.
            r.UnOrdinateur.Nom,

            r.Technicien,

            r.TypeReparation,
            
            r.DateDebut
            
        ))
        .ToListAsync(); // On transforme le tout en liste asynchrone

    return historique;
}
}