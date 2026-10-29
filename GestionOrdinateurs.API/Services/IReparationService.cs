using GestionOrdinateurs.API.Controllers;
using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Services; 

public interface IReparationService
{
    Task SimulerReparationAsync(Ordinateur ordinateur);
    Task<List<Reparation>> ListerReparationsAsync();
    Task<List<HistoriqueReparationRecord>> ObtenirHistoriqueAsync();
}