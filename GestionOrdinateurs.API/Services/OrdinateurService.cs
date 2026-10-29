using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public class OrdinateurService : IOrdinateurService
{
    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;
    // Déclaration du contexte de la base de données
    private readonly ParcInformatiqueContext _context;

    // Injection du contexte par le constructeur
    public OrdinateurService(ParcInformatiqueContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retourne la liste de tous les ordinateurs depuis la base de données.
    /// </summary>
    public async Task<List<Ordinateur>> ListerOrdinateursAsync()
{
    return await _context.Ordinateurs.ToListAsync();
}

    /// <summary>
    /// Recherche un ordinateur par son Id.</summary>
    /// <summary>
    public async Task<Ordinateur?> RenvoyerOrdinateurAsync(int id)
{
    return await _context.Ordinateurs.FirstOrDefaultAsync(o => o.Id == id);
}

    /// <summary>
    /// Ajoute un ordinateur dans la base de données
    /// </summary>
    public async Task<bool> AjouterUnOrdinateur(Ordinateur ordinateur)
    {
        bool existeDeja = await _context.Ordinateurs.AnyAsync(o => o.Id == ordinateur.Id);
        if (existeDeja && ordinateur.Id != 0) 
        {
            return false;
        }

        _context.Ordinateurs.Add(ordinateur);

        await _context.SaveChangesAsync(); //l'ordi est écrit dans le fichier .db
        OrdinateurAjoute?.Invoke(this, new OrdinateurAjouteEventArgs { Nom = ordinateur.Nom });

        //log
        var journal = new JournalService();
        journal.Ecrire(@"Logs\Ordinateurs\actions.log", $"{DateTime.Now:dd/MM/yyyy} - Ajout de l'ordinateur {ordinateur.Nom}");
        return true;
    }
     
    /// <summary>
    /// Supprime un ordinateur de la base de données.
    /// </summary>
    public async Task<bool> SupprimerOrdinateurAsync(int idSuppression)
{
    Ordinateur? ordiASupprimer = await RenvoyerOrdinateurAsync(idSuppression);
    if (ordiASupprimer != null)
    {
        _context.Ordinateurs.Remove(ordiASupprimer);
        await _context.SaveChangesAsync(); 
        OrdinateurSupprime?.Invoke(this, new OrdinateurSupprimeEventArgs { Id = idSuppression });
        
        //Logs
        var journal = new JournalService();
            journal.Ecrire(@"Logs\Ordinateurs\actions.log", $"{DateTime.Now:dd/MM/yyyy} - Suppression de l'ordinateur {ordiASupprimer.Nom}");
        return true; 
    }
    return false;
}
    /// <summary>Met à jour un ordinateur.</summary>
    public async Task<bool> MettreAJourOrdinateurAsync(Ordinateur ordinateurModifie)
{
    Ordinateur? ordiMaj = await RenvoyerOrdinateurAsync(ordinateurModifie.Id);
    
    if (ordiMaj != null)
    {
        // On met à jour les propriétés 
        ordiMaj.Nom = ordinateurModifie.Nom;
        ordiMaj.Marque = ordinateurModifie.Marque;
        ordiMaj.EtatOrdinateur = ordinateurModifie.EtatOrdinateur;
        ordiMaj.Stock = ordinateurModifie.Stock;
        ordiMaj.Prix = ordinateurModifie.Prix;
        ordiMaj.Vendu = ordinateurModifie.Vendu;
        ordiMaj.Caracteristiques = ordinateurModifie.Caracteristiques;
        
        await _context.SaveChangesAsync(); 
        OrdinateurMisAJour?.Invoke(this, new OrdinateurMisAJourEventArgs {Id = ordinateurModifie.Id, NouveauNom = ordinateurModifie.Nom});

        //log
        var journal = new JournalService();
            journal.Ecrire(@"Logs\Ordinateurs\actions.log", $"{DateTime.Now:dd/MM/yyyy} - Modification de l'ordinateur {ordinateurModifie.Nom}");

        return true;
    }
    return false;

    }
    public string Diagnostiquer(Ordinateur ordinateur)
{
    return ordinateur switch
    {
        { EtatOrdinateur: EtatOrdinateur.HorsService } 
            => "Remplacement",

        { 
            EtatOrdinateur: EtatOrdinateur.Lent, 
            Caracteristiques: { RamGo: < 8 } 
        } 
            => "Ajouter RAM",

        
        _ 
            => "Aucune action"
    };
}

public (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations) CalculerStatistiques()
{
    // Nombre total d'ordinateurs
    int total = _context.Ordinateurs.Count();
    
    // On compte ceux qui ont au moins 1 en stock
    int dispos = _context.Ordinateurs.Count(o => o.Stock > 0);
    
    // On additionne (Prix * Stock) pour tous les ordis
    decimal valeur = _context.Ordinateurs.Sum(o => o.Prix * o.Stock);
    
    // Nombre total de réparations
    int nbRepar = _context.Reparations.Count();

    return (total, dispos, valeur, nbRepar);
}
}