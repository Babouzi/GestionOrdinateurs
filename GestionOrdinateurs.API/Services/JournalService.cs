namespace GestionOrdinateurs.API.Services;
using System.IO;

public class JournalService
{
    // On passe le chemin du fichier en paramètre pour écrire au bon endroit
    public void Ecrire(string cheminFichier, string message)
    {
        var dossier = Path.GetDirectoryName(cheminFichier);
        if (!Directory.Exists(dossier))
        {
            Directory.CreateDirectory(dossier!);
        }

        // On écrit dans le fichier
        using var writer = new StreamWriter(cheminFichier, append: true);
        
        // On formate avec la date du jour.
        writer.WriteLine($"{DateTime.Now:dd/MM/yyyy} - {message}");
    }
}