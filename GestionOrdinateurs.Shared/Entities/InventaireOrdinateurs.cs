namespace GestionOrdinateurs.Shared;

public class InventaireOrdinateurs
{
    // liste qui va contenir tous les ordinateurs
    public List<Ordinateur> Ordinateurs { get; set; } = new();

    // 1er Indexeur : Recherche par ID
    public Ordinateur? this[int id]
    {
        get { return Ordinateurs.FirstOrDefault(o => o.Id == id); }
    }

    // 2ème Indexeur : Recherche par Nom
    public Ordinateur? this[string nom]
    {
        get { return Ordinateurs.FirstOrDefault(o => o.Nom == nom); }
    }
}