using GestionOrdinateurs.Shared; 

namespace GestionOrdinateurs.API.Services; 

/// <summary>
/// Interface définissant la classe pour qui sert à la gestion des ordinateurs.
/// </summary>
public interface IOrdinateurService
{
    event EventHandler<OrdinateurAjouteEventArgs> OrdinateurAjoute;
    event EventHandler<OrdinateurSupprimeEventArgs> OrdinateurSupprime;
    event EventHandler<OrdinateurMisAJourEventArgs> OrdinateurMisAJour;

    /// <summary>Retourne la liste complète de tous les ordinateurs.</summary>
    Task<List<Ordinateur>> ListerOrdinateursAsync();

    /// <summary>Retourne un ordinateur via son Id s'il existe, null sinon.</summary>
    Task<Ordinateur?> RenvoyerOrdinateurAsync(int idCherche);

    /// <summary>Ajoute un ordinateur s'il n'existe pas déjà. Retourne true si l'ajout a réussi.</summary>
    Task<bool> AjouterUnOrdinateur(Ordinateur ordinateur);

    /// <summary>Supprime un ordinateur. Retourne true si la suppression a réussi, false sinon.</summary>
    Task<bool> SupprimerOrdinateurAsync(int idSuppression);

    /// <summary>Met à jour un ordinateur. Retourne true en cas de succès, false sinon.</summary>
    Task<bool> MettreAJourOrdinateurAsync(Ordinateur ordinateurModifie);

/// <summary>Diagnostique un ordinateur. Retourne une suggestion de réparation en fonction de l'état de l'ordinateur./// </summary>
    string Diagnostiquer(Ordinateur ordinateur);

/// <summary> Retourne un tuple avec des statistiques sur les ordinaeurs./// </summary>

    public (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations) CalculerStatistiques();
    
}