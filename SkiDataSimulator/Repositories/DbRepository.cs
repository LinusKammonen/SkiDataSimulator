using Npgsql;
using SkiDataSimulator.Models;
using SkidataWpf.Models;
using System.Xml.Linq;
namespace SkiDataSimulator.Repositories;

public class DbRepository
{
    public readonly NpgsqlDataSource _dataSource;

    public DbRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }
    public async Task<Lift> GetRandomSkiLiftFromResortAsync(Resort resort)
    {
        /* Här ska ni hämta en slumpmässig lift som skidåkaren åker i en viss anläggning
         * Hur slumpar man? Ett tips är att hämta alla id för liftar och lägga i en array som
         * ni sedan slumpar värden från och returnerar
         */


        throw new NotImplementedException();
    }
    public async Task<SkierDetailedSeason?> GetSkierDetailedSeason(int id)
    {
        string query = "SELECT skier.id as skier_id, firstname, lastname," +
                        " ski_pass.valid_to as slutdatum, COUNT(ski_run.id) as total_season_runs, " +
                        "season.name as season, COUNT(distinct DATE_PART('day', ski_run.timestamp)) " +
                        "as total_season_days FROM skier left JOIN ski_pass ON ski_pass.skier_id = skier.id " +
                        "left JOIN ski_run ON ski_run.ski_pass_id = ski_pass.id left JOIN season ON ski_run.season_id" +
                        " = season.id WHERE skier.id = @id AND DATE_PART('year', season.end_date) = DATE_PART('year', CURRENT_DATE) " +
                        "GROUP BY skier.id, ski_pass.id, season.id";

        await using var command = _dataSource.CreateCommand(query);

        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        var ordinals = new
        {
            Id = reader.GetOrdinal("skier_id"),
            Firstname = reader.GetOrdinal("firstname"),
            Lastname = reader.GetOrdinal("lastname"),
            Enddate = reader.GetOrdinal("slutdatum"),
            TotalSeasonRuns = reader.GetOrdinal("total_season_runs"),
            CurrentSeason = reader.GetOrdinal("season"),
            TotalSeasonDays = ("total_season_days")
        };

        while (await reader.ReadAsync())
        {
            SkierDetailedSeason skier = new SkierDetailedSeason
            {
                Id = reader.GetFieldValue<int?>(ordinals.Id),
                Firstname = reader.GetFieldValue<string?>(ordinals.Firstname),
                Lastname = reader.GetFieldValue<string?>(ordinals.Lastname),
                Enddate = reader.GetFieldValue<DateOnly?>(ordinals.Enddate),
                TotalSeasonRuns = reader.GetFieldValue<int?>(ordinals.TotalSeasonRuns),
                CurrentSeason = reader.GetFieldValue<string?>(ordinals.CurrentSeason),
                TotalSeasonDays = reader.GetFieldValue<int?>(ordinals.TotalSeasonRuns)
            };
            return skier;
        }
        return null;

    }
    public async Task<List<Skier>> SearchSkier(string name)
    {
        string query = "SELECT id, firstname, lastname, email, username, image_url FROM skier" +
                       " WHERE firstname ILIKE @name or lastname ILIKE @name";

        await using var command = _dataSource.CreateCommand(query);

        command.Parameters.AddWithValue("name", $"%{name}%");
        
        await using var reader = await command.ExecuteReaderAsync();

        var ordinals = new
        {
            Id = reader.GetOrdinal("id"),
            Firstname = reader.GetOrdinal("firstname"),
            Lastname = reader.GetOrdinal("lastname"),
            Email = reader.GetOrdinal("email"),
            Username = reader.GetOrdinal("username"),
            Image_url = reader.GetOrdinal("image_url")
        };

        List<Skier> skiers = [];

        while (await reader.ReadAsync())
        {
            Skier skier = new Skier
            {
                Id = reader.GetFieldValue<int>(ordinals.Id),
                Firstname = reader.GetFieldValue<string>(ordinals.Firstname),
                Lastname = reader.GetFieldValue<string>(ordinals.Lastname),
                Email = reader.GetFieldValue<string>(ordinals.Email),
                Username = reader.GetFieldValue<string>(ordinals.Username),
                Image_url = reader.IsDBNull(ordinals.Image_url) ? null : reader.GetFieldValue<string?>(ordinals.Image_url)
            };
            skiers.Add(skier);
        }
        return skiers;
    } 
    public async Task<bool> RegisterSkier(Skier skier)
    {
        string query = "insert into skier" +
                       "(firstname, lastname, email, username, image_url)" +
                       " values (@firstname, @lastname, @email, @username, @image_url)";

        await using var command = _dataSource.CreateCommand(query);

        command.Parameters.AddWithValue("firstname", skier.Firstname);
        command.Parameters.AddWithValue("lastname", skier.Lastname);
        command.Parameters.AddWithValue("email", skier.Email);
        command.Parameters.AddWithValue("username", skier.Username);
        command.Parameters.AddWithValue("image_url", skier.Image_url);

        var result = await command.ExecuteNonQueryAsync();

        return result == 1;

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
