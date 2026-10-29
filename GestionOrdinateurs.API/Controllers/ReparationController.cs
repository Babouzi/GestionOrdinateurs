using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API.Controllers;
public record HistoriqueReparationRecord(
    int OrdinateurId,
    string NomOrdinateur,
    string Technicien,
    string TypeReparation,
    DateTime DateIntervention
);

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReparationController : ControllerBase
{
    private readonly IReparationService _reparService;
    private readonly IOrdinateurService _ordinateurService;
    private readonly StockService _stockService; 

    
    public ReparationController(IReparationService reparService, IOrdinateurService ordinateurService, StockService stockService)
    {
        _reparService = reparService;
        _ordinateurService = ordinateurService;
        _stockService = stockService; 
    }
    
    [HttpPost("simuler")]
    public async Task<IActionResult> SimulerReparation()
    {
        // Récupération de la liste d'ordinateurs
        List<Ordinateur> listOrdinateurs = await _ordinateurService.ListerOrdinateursAsync();

        if (listOrdinateurs.Count < 5)
        {
            return BadRequest("Pas assez d'ordinateurs en base pour simuler 5 réparations.");
        }

    for(int i = 0; i < 5; i++)
    {
    await _reparService.SimulerReparationAsync(listOrdinateurs[i]);
    }
        return Ok("Les 5 réparations ont été simulées avec succès.");
    }

    [HttpGet]
    public async Task<IActionResult> GetReparations()
    {
        List<Reparation> reparations = await _reparService.ListerReparationsAsync();
        return Ok(reparations);
    }

    [HttpGet("stock-ssd")]
    public IActionResult GetStockSSD()
    {
        // On lance 10 fois la méthode en parallèle.
        Parallel.For(0,10,i => _stockService.ConsommerSSD());

        Console.WriteLine($"Stock restant : {_stockService.StockSSD}");
        return Ok(_stockService.StockSSD);
    }

    [HttpGet("historique")]
public async Task<ActionResult<List<HistoriqueReparationRecord>>> ObtenirHistorique()
{
    
    var historique = await _reparService.ObtenirHistoriqueAsync();
    
    // Renvoie le résultat avec un code HTTP 200 (Ok)
    return Ok(historique);
}
}