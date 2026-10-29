namespace GestionOrdinateurs.Shared;
public class Reparation
{
    public int Id { get; set; }
    public Ordinateur? UnOrdinateur { get; set; }
    public string TypeReparation { get; set; } = "";
    public DateTime DateDebut { get; set; }
    public string Statut { get; set; } = "";  
    public string Technicien { get; set; } = "";

    //Constructeur vide pour EF
    public Reparation() { }
    public Reparation(int id, Ordinateur ordi, string type, DateTime date, string statut, string technicien)
    {
        Id = id;
        UnOrdinateur = ordi;
        TypeReparation = type;
        DateDebut = date;
        Statut = statut;
        Technicien = technicien;
    }
}