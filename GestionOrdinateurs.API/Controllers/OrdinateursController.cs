using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class OrdinateursController : ControllerBase
{
    private readonly IOrdinateurService _service;
    private readonly ILogger<OrdinateursController> _logger;
    private readonly IConfiguration _configuration;

    public OrdinateursController(IOrdinateurService service, ILogger<OrdinateursController> logger, IConfiguration configuration)
    {
        _service = service;
        _logger = logger;
        _configuration = configuration;


        //Réponse à votre feedback sur le TP2: On lie les évènements à des vraies méthodes nommées (déclarées en bas du fichier)
        _service.OrdinateurAjoute += SurOrdinateurAjoute;
        _service.OrdinateurMisAJour += SurOrdinateurMisAJour;
        _service.OrdinateurSupprime += SurOrdinateurSupprime;
    }
    
    [HttpGet]
    // Récupère la liste complète de tous les ordinateurs
    public async Task<IActionResult> GetComputers() 
    {
        _logger.LogInformation("Appel de l'endpoint GET : Récupération de tous les ordinateurs.");
        return Ok(await _service.ListerOrdinateursAsync()); 
    }

    [HttpGet("{id}")]
    // Récupère un ordinateur spécifique grâce à son identifiant
    public async Task<IActionResult> GetComputerById(int id)
    {
        Ordinateur? ordinateurTrouve = await _service.RenvoyerOrdinateurAsync(id);
        if(ordinateurTrouve == null)
        {
            return NotFound();
        }
        _logger.LogInformation($"Appel de l'endpoint Get pour l'ordi {id}");
        return Ok(ordinateurTrouve);    
    }

    [HttpPost]
    // Ajoute un nouvel ordinateur au parc
    public async Task<IActionResult> AjouterOrdinateur([FromBody] Ordinateur nouvelOrdinateur) // Ajout de async Task<>
    {
        if (await _service.AjouterUnOrdinateur(nouvelOrdinateur)) 
        {
            return Created();
        }
        else
        {
            _logger.LogWarning($"L'ordinateur à id :{nouvelOrdinateur.Nom} n'a pas pu être ajouté");
            return BadRequest();
        }
    }

    [HttpPut("{id}")]
    // Met à jour les informations d'un ordinateur existant.
    public async Task<IActionResult> MajOrdinateur(int id, [FromBody] Ordinateur ordinateurModifie) 
    {
        if (ordinateurModifie.Id != id)
        {
            _logger.LogWarning($"L'Id spécifié n'est pas identique à celui de l'objet mis à jour");
            return BadRequest();
        }
        else
        {
            if (await _service.MettreAJourOrdinateurAsync(ordinateurModifie)) 
            {
                _logger.LogInformation("Objet ajouté avec succès");
                return Ok(); // code 200 (succès)
            }
            else
            {
                return NotFound(); // code 404 (ordinateur non trouvé)
            }
        }
    }

    [HttpDelete("{id}")]
    // Supprime un ordinateur du parc informatique via son identifiant
    public async Task<IActionResult> SupprimerOrdinateur(int id) 
    {
        if (await _service.SupprimerOrdinateurAsync(id)) 
        {
            _logger.LogInformation("Ordinateur supprimé avec succès");
            return NoContent(); // Code 204 (Opération réussie)
        }
        else
        {
            return NotFound();  // Code 404 (Ordinateur pas trouvé)
        }
    }
    [HttpGet("disponibles")]
    // Retourne uniquement les ordinateurs en stock (Stock > 0)
    public async Task<IActionResult> GetOrdinateursDisponibles()
    {
        List<Ordinateur> tousLesOrdis = await _service.ListerOrdinateursAsync();
        List<Ordinateur> ordisDispos = tousLesOrdis.Where(o => o.Stock > 0).ToList();
        
        return Ok(ordisDispos);
    }

    [HttpGet("prix-decroissant")]
    // Retourne les ordinateurs triés du plus cher au moins cher
    public async Task<IActionResult> GetOrdinateursParPrix()
    {
        List<Ordinateur> tousLesOrdis = await _service.ListerOrdinateursAsync();
        List<Ordinateur> ordisTries = tousLesOrdis.OrderByDescending(o => o.Prix).ToList();
        
        return Ok(ordisTries);
    }
      
    // Bonus
    [HttpGet("marque/{marque}")]
    // Recherche et retourne les ordinateurs correspondants à une marque donnée
    public async Task<IActionResult> GetComputersByMarque(string marque) 
    {
        List<Ordinateur> tousLesOrdis = await _service.ListerOrdinateursAsync(); 


        List<Ordinateur> ordisFiltres = tousLesOrdis
            .Where(o => o.Marque.ToLower() == marque.ToLower())
            .ToList();
        
        if (ordisFiltres.Count == 0)
        {
            _logger.LogWarning($"Aucun ordi de la marque {marque}");
            return NotFound(); 
        }

        // On renvoie la liste remplie avec un code 200 OK
        return Ok(ordisFiltres);
    }

    [HttpGet("info")]
    /// <summary>Affiche les informations sur l'application prises dans le fichier appsettings.json</summary>
    public IActionResult GetAppInfo()
    {
        var nomApp = _configuration["Application:Name"];
        var versionApp = _configuration["Application:Version"];

        _logger.LogInformation("Récupération des informations de l'application.");

        return Ok(new { Nom = nomApp, Version = versionApp });
    }

    [HttpGet("{id}/diagnostic")]
public async Task<ActionResult<string>> ObtenirDiagnostic(int id)
{
    //On cherche l'ordinateur dans la base de données
    var ordi = await _service.RenvoyerOrdinateurAsync(id);
    
    //S'il n'existe pas, on renvoie une erreur 404
    if (ordi == null)
    {
        return NotFound("Ordinateur introuvable");
    }
    string resultatDiagnostic = _service.Diagnostiquer(ordi);
    
    return Ok(resultatDiagnostic);
}

[HttpGet("statistiques")]
public ActionResult ObtenirStatistiques()
{
    // On "déconstruit" le tuple pour récupérer les 4 variables
    var (total, enStock, valeurStock, nbReparations) = _service.CalculerStatistiques();
    
    // On les renvoie au format JSON
    return Ok(new 
    { 
        TotalOrdinateurs = total, 
        OrdinateursEnStock = enStock, 
        ValeurTotale = valeurStock, 
        TotalReparations = nbReparations 
    });
}


[HttpGet("inventaire/{nom}")]
public async Task<IActionResult> ChercherDansInventaire(string nom)
{
    var inventaire = new InventaireOrdinateurs();
    
  
    var listeOrdis = await _service.ListerOrdinateursAsync(); 
    inventaire.Ordinateurs = listeOrdis.ToList();

    var ordinateur = inventaire[nom]; 

    if (ordinateur == null)
    {
        return NotFound($"Aucun ordinateur trouvé avec le nom '{nom}'");
    }

    return Ok(ordinateur);
}

    // 
    // Méthodes pour les évènements.
    // 
    
    private void SurOrdinateurAjoute(object? sender, OrdinateurAjouteEventArgs args)
    {
        Console.WriteLine($"[EVENEMENT]: Un ordinateur nommé '{args.Nom}' a été ajouté.");
    }

    private void SurOrdinateurMisAJour(object? sender, OrdinateurMisAJourEventArgs args)
    {
        Console.WriteLine($"[EVENEMENT]: L'ordinateur à l'id : '{args.Id}' a été modifié avec le nom '{args.NouveauNom}'");
    }

    private void SurOrdinateurSupprime(object? sender, OrdinateurSupprimeEventArgs args)
    {
        Console.WriteLine($"[EVENEMENT] L'ordinateur à l'id: {args.Id} a été supprimé");
    }




}