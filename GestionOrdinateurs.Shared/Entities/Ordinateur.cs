using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GestionOrdinateurs.Shared;
public enum EtatOrdinateur 
{ 
    EnService, 
    EnPanne, 
    EnMaintenance, 
    HorsService, 
    Lent, 
    Reparable 
}
[Table("Ordinateurs")]
public class Ordinateur
{
public int Id { get; private set; }
public  string Nom { get; set;} = "";
public string Marque {get;  set;} = "";
public Caracteristiques Caracteristiques {get;set;} = null!;
[DefaultValue(0)]
public int Stock { get; set; }
[DefaultValue(0)]
public decimal Prix { get; set; } = 500; 
public bool Vendu { get; set; } = false;
public EtatOrdinateur EtatOrdinateur { get; set; }


// L'attribut [JsonIgnore] empêche la conversion de cette liste en JSON sur les endpoints de l'API.
// Cela permet d'éviter une erreur Object cycle que j'ai eu :
// L'ordinateur affiche ses réparations -> qui affichent l'ordinateur -> qui affiche les réparations, etc.
// (Honnêtement la solution a été trouvé grace à l'ia).
[JsonIgnore]
public List<Reparation> Reparations { get; set; } = new();

// Constructeur vide pour Ef core
public Ordinateur() { }

public Ordinateur(int id, string nom, string marque, EtatOrdinateur etat,int stock,decimal prix, Caracteristiques caracteristiques)
    {
        Id = id;
        Nom = nom;
        Marque = marque;
        EtatOrdinateur = etat;
        Stock = stock;
        Prix = prix;
        Caracteristiques = caracteristiques;


        
    }


public virtual void Allumer()
    {
        EtatOrdinateur = EtatOrdinateur.EnService;
        
    }

public virtual void Eteindre()
    {
        EtatOrdinateur = EtatOrdinateur.HorsService;
        
    }
public virtual void Afficher()
    {
        Console.WriteLine($"Id = {Id} Nom = {Nom} marque = {Marque}");
    }
    
}
