namespace GestionOrdinateurs.API;

public class OrdinateurAjouteEventArgs : EventArgs
{
    public string Nom { get; set; } = "";
}

/// <summary>Données transmises lors de la suppression d'un ordinateur.</summary>
public class OrdinateurSupprimeEventArgs : EventArgs
{
    public int Id { get; set; }
}


public class OrdinateurMisAJourEventArgs : EventArgs
{
    public int Id { get; set; }
    public string NouveauNom { get; set; } = "";
}