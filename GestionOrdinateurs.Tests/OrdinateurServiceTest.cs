using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services;

namespace GestionOrdinateurs.Tests;

public class OrdinateurServiceTest
{
    private ParcInformatiqueContext CreerContexte()
    {
        var options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new ParcInformatiqueContext(options);
    }
    
    [Fact]
    // Vérifie que l'ajout d'un nouvel ordinateur fonctionne correctement.
    public async Task TesterAjoutOrdinateur()
    {
        var context = CreerContexte();
        OrdinateurService service = new OrdinateurService(context);

        var caracAjout = new Caracteristiques(16, 512, TypeOrdinateur.Portable, "Intel Core i5");
        Ordinateur nouvelOrdi = new Ordinateur(999, "PC-Test", "Asus", EtatOrdinateur.EnService, 1, 500m, caracAjout);
        
        bool resultat = await service.AjouterUnOrdinateur(nouvelOrdi);
        
        Assert.True(resultat);
        Assert.NotNull(await service.RenvoyerOrdinateurAsync(999)); 
    }

    [Fact]
    // Vérifie la bonne suppression d'un ordinateur quand l'ID fourni existe
    public async Task TesterSuppression_OrdinateurExistant() 
    {
        var context = CreerContexte();
        OrdinateurService service = new OrdinateurService(context);
        var caracSuppr = new Caracteristiques(32, 1024, TypeOrdinateur.Fixe, "AMD Ryzen 7");
        Ordinateur ordiATester = new Ordinateur(888, "PC-Dell", "Dell", EtatOrdinateur.HorsService, 1, 500m, caracSuppr);
        // On attend l'ajout
        await service.AjouterUnOrdinateur(ordiATester);
    
        
        int idReel = ordiATester.Id; 
        
      
        bool resultat = await service.SupprimerOrdinateurAsync(idReel);
    
        Assert.True(resultat);
        Assert.Null(await service.RenvoyerOrdinateurAsync(idReel)); 
    }

    [Fact]
    // Vérifie que la suppression renvoie false quand l'ID fourni n'existe pas.
    public async Task TesterSuppression_OrdinateurNonExistant() 
    {
        var context = CreerContexte();
        OrdinateurService service = new OrdinateurService(context);
        
     
        bool resultat = await service.SupprimerOrdinateurAsync(777777); 
        
        Assert.False(resultat);
    }

    [Fact]
public void TesterCreationReparation()
{
    // 1. On crée un ordinateur fictif
    var carac = new Caracteristiques(16, 512, TypeOrdinateur.Portable, "Intel Core i7");
    var ordi = new Ordinateur(10, "Dell XPS", "Dell", EtatOrdinateur.EnService, 1, 1500m, carac);
    
    //On crée la réparation avec le nouveau paramètre "Technicien"
  
    var reparation = new Reparation(1, ordi, "Changement batterie", DateTime.Now, "Terminé", "Bob le Réparateur");
    
    // On vérifie que les données sont bien assignées
    Assert.Equal("Changement batterie", reparation.TypeReparation);
    Assert.Equal("Dell XPS", reparation.UnOrdinateur.Nom);
    Assert.Equal("Bob le Réparateur", reparation.Technicien);
}
}