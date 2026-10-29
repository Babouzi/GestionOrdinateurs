namespace GestionOrdinateurs.Shared;
public enum TypeOrdinateur
{
    Portable, Fixe, Server
}

public class Caracteristiques
{
    public int RamGo { get; set; }
    public TypeOrdinateur TypeOrdi { get; set; }
    public int StockageGo { get; set; }
    public string Processeur {get; set; } = "";


    public Caracteristiques(int ramGo, int stockageGo, TypeOrdinateur typeOrdi, string processeur)
    {
        RamGo = ramGo;
        StockageGo = stockageGo;
        TypeOrdi = typeOrdi;
        Processeur = processeur;
        
    }
}