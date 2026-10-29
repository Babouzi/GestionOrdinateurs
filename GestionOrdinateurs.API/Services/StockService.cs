namespace GestionOrdinateurs.API.Services;


public class StockService
{
    public int StockSSD { get; set; } = 10;
    private readonly object _verrou = new object();

    public void ConsommerSSD()
    {
        Task.Delay(10).Wait(); 
        
        // On pose un verrou sur le changement de la valeur
        lock (_verrou)
        {
            StockSSD = StockSSD - 1;
        }
    }
}