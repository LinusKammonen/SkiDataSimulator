using SkidataWpf.Models;

namespace SkiDataSimulator.Repositories;

public class DbRepository
{
    public async Task<Lift> GetRandomSkiLiftFromResortAsync(Resort resort)
    {
        /* Här ska ni hämta en slumpmässig lift som skidåkaren åker i en viss anläggning
         * Hur slumpar man? Ett tips är att hämta alla id för liftar och lägga i en array som
         * ni sedan slumpar värden från och returnerar
         */


        throw new NotImplementedException();
    }

    public async Task<List<SkiPass>> GetRandomSkiPassesAsync(int numberOfSkipasses)
    {
        /* Om ni vill simulera att säg 20 skidåkare åker en dag i alla anläggningar
         * då måste ni hitta 20 lifkort från databasen. Ni behöver inte ta hänsyn till
         * gilgithetsdatumet om ni inte vill
         * Kika gärna på ORDER BY RANDOM() i PostgreSQL
         */

        throw new NotImplementedException();
    }
    public async Task<Resort> GetRandomResortAsync()
    {
        /*  Leta upp en slumpmässig anläggning (resort)
         *  Här kan ni använda er av slumpgeneratorn som finns inbyggd i PostgreSQL
         *  ORDER BY RANDOM()
         */

        throw new NotImplementedException();

    }
    public async Task<Season> GetSeasonByDateAsync(DateTime date)
    {
        /* Vilken säsong är egentligen 2023-12-08 eller 2021-04-01
         * Det ska ni kontrollera i er databas or returnera
         */

        throw new NotImplementedException();
    }

    public async Task SaveSkiRunsAsync(List<SkiRun> skiRuns)
    {
        /* Skicka in alla åk som har genererats till databasen med 
         * hjälp av SQL-frågor
         */

        throw new NotImplementedException();
    }
}
