using Npgsql;
using SkiDataSimulator.Models;
using SkidataWpf.Models;
using System.Windows.Controls.Primitives;
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
        try
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
                Enddate = reader.GetOrdinal("slutdatum"),
                TotalSeasonRuns = reader.GetOrdinal("total_season_runs"),
                CurrentSeason = reader.GetOrdinal("season"),
                TotalSeasonDays = ("total_season_days")
            };

            while (await reader.ReadAsync())
            {
                SkierDetailedSeason skier = new SkierDetailedSeason
                {
                    Enddate = reader.GetFieldValue<DateOnly?>(ordinals.Enddate),
                    TotalSeasonRuns = reader.GetFieldValue<int?>(ordinals.TotalSeasonRuns),
                    CurrentSeason = reader.GetFieldValue<string?>(ordinals.CurrentSeason),
                    TotalSeasonDays = reader.GetFieldValue<int?>(ordinals.TotalSeasonRuns)
                };
                return skier;
            }
            return null;
        }
        catch (PostgresException exception)
        {

            throw;
        }

    }
    public async Task<SkiPass> GetSkipassByCardnumber(int number)
    {
        try
        {
            string query = "Select * from ski_pass WHERE card_number = @number";

            await using var command = _dataSource.CreateCommand(query);

            command.Parameters.AddWithValue("number", number);

            await using var reader = await command.ExecuteReaderAsync();

            var ordinals = new
            {
                Id = reader.GetOrdinal("id"),
                CardNumber = reader.GetOrdinal("card_number"),
                SkierId = reader.GetOrdinal("skier_id"),
                ValidFrom = reader.GetOrdinal("valid_from"),
                ValidTo = reader.GetOrdinal("valid_to"),
                DestinationId = reader.GetOrdinal("destination_id")
            };

            while (await reader.ReadAsync())
            {
                SkiPass skipass = new SkiPass
                {
                    Id = reader.GetFieldValue<int>(ordinals.Id),
                    CardNumber = reader.GetFieldValue<int>(ordinals.CardNumber),
                    SkierId = reader.GetFieldValue<int>(ordinals.SkierId),
                    ValidFrom = reader.GetFieldValue<DateOnly>(ordinals.ValidFrom),
                    ValidTo = reader.GetFieldValue<DateOnly?>(ordinals.ValidTo),
                    DestinationId = reader.GetFieldValue<int>(ordinals.DestinationId)
                };
                return skipass;
            }
            return null;
        }
        catch (PostgresException exception)
        {

            throw;
        }

    }
    public async Task<SkierLeaderboardDetails> GetLeaderboardDetails(int id)
    {
        try
        {
            string query = "SELECT firstname, lastname, SUM(lift.vertical_drop) as total_vertical_drop," +
                        "  COUNT(distinct country.id) as total_countries from skier JOIN ski_pass ON " +
                        "ski_pass.skier_id = skier.id JOIN ski_run ON ski_run.ski_pass_id = ski_pass.id " +
                        "JOIN lift ON ski_run.lift_id = lift.id JOIN resort ON resort.id = lift.resort_id " +
                        "JOIN destination ON resort.destination_id = destination.id JOIN country " +
                        "ON destination.country_id = country.id WHERE skier.id = @id GROUP BY skier.id";


            await using var command = _dataSource.CreateCommand(query);

            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();

            var ordinals = new
            {
                TotalVerticalDrop = reader.GetOrdinal("total_vertical_drop"),
                TotalCountries = reader.GetOrdinal("total_countries")
            };
            while (await reader.ReadAsync())
            {
                SkierLeaderboardDetails skier = new SkierLeaderboardDetails
                {
                    TotalVerticalDrop = reader.GetFieldValue<int?>(ordinals.TotalVerticalDrop),
                    TotalCountries = reader.GetFieldValue<int?>(ordinals.TotalCountries)
                };
                return skier;
            }
            return null;
            
        }
        catch (PostgresException exception)
        {

            throw;
        }

    }
    public async Task<List<Skier>> SearchSkier(string name)
    {
        try
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
        catch (PostgresException exception)
        {

            throw;
        }

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
    public async Task<List<Lift>> FindLiftsByResortId(int id)
    {
        try
        {
            string query = "SELECT * FROM lift WHERE resort_id = @resort_id";

            await using var command = _dataSource.CreateCommand(query);

            command.Parameters.AddWithValue("resort_id", id);

            await using var reader = await command.ExecuteReaderAsync();

            var ordinals = new
            {
                Id = reader.GetOrdinal("id"),
                Name = reader.GetOrdinal("name"),
                VerticalDrop = reader.GetOrdinal("vertical_drop"),
                ResortId = reader.GetOrdinal("resort_id"),
                LiftTypeId = reader.GetOrdinal("lift_type_id")
            };

            List<Lift> lifts = [];

            while (await reader.ReadAsync())
            {
                Lift lift = new Lift
                {
                    Id = reader.GetFieldValue<int>(ordinals.Id),
                    Name = reader.GetFieldValue<string>(ordinals.Name),
                    VerticalDrop = reader.GetFieldValue<int?>(ordinals.VerticalDrop),
                    ResortId = reader.GetFieldValue<int>(ordinals.ResortId),
                    LiftTypeId = reader.GetFieldValue<int>(ordinals.LiftTypeId)
                };
                lifts.Add(lift);
            }
            return lifts;
        }
        catch (PostgresException exception)
        {

            throw;
        }


    }
    public async Task<List<Resort>> FindResortByDestinationID(int id)
    {
        try
        {
            string query = "SELECT * FROM resort WHERE destination_id = @destination_id";

            await using var command = _dataSource.CreateCommand(query);

            command.Parameters.AddWithValue("destination_id", id);

            await using var reader = await command.ExecuteReaderAsync();

            var ordinals = new
            {
                Id = reader.GetOrdinal("id"),
                Name = reader.GetOrdinal("name"),
                Destination_id = reader.GetOrdinal("destination_id"),
            };

            List<Resort> resorts = [];

            while (await reader.ReadAsync())
            {
                Resort resort = new Resort
                {
                    Id = reader.GetFieldValue<int>(ordinals.Id),
                    Name = reader.GetFieldValue<string>(ordinals.Name),
                    DestinationId = reader.GetFieldValue<int>(ordinals.Destination_id)
                };
                resorts.Add(resort);
            }
            return resorts;
        }
        catch (PostgresException exception)
        {

            throw;
        }

    }
    public async Task<Season> GetSeasonByTime(DateTime time)
    {
        string query = "SELECT * FROM season WHERE @date BETWEEN start_date AND end_date";

        await using var command = _dataSource.CreateCommand(query);

        command.Parameters.AddWithValue("date", time);

        await using var reader = await command.ExecuteReaderAsync();

        var ordinals = new
        {
            Id = reader.GetOrdinal("id"),
            StartDate = reader.GetOrdinal("start_date"),
            EndDate = reader.GetOrdinal("end_date"),
            Name = reader.GetOrdinal("name")
        };

        while (await reader.ReadAsync())
        {
            Season season = new Season
            {
                Id = reader.GetFieldValue<int>(ordinals.Id),
                Name = reader.GetFieldValue<string?>(ordinals.Name),
                StartDate = reader.GetFieldValue<DateTime>(ordinals.StartDate),
                EndDate = reader.GetFieldValue<DateTime>(ordinals.EndDate)
            };
            return season;
        }
        return null;

    }
    public async Task<bool> RegisterRideByLiftId(SkiRun skiRun)
    {
        string query = "INSERT INTO ski_run(ski_pass_id, lift_id, season_id, \"timestamp\") VALUES (@ski_pass_id, @lift_id, @season_id, @timestamp)";

        await using var command = _dataSource.CreateCommand(query);

        command.Parameters.AddWithValue("ski_pass_id", skiRun.SkipassId);
        command.Parameters.AddWithValue("lift_id", skiRun.LiftId);
        command.Parameters.AddWithValue("season_id", skiRun.SeasonId);
        command.Parameters.AddWithValue("timestamp", skiRun.Timestamp);

        var result = await command.ExecuteNonQueryAsync();

        return result == 1;

    }
    public async Task<Skier> FindSkierByUsername(string username)
    {
        string query = "SELECT * FROM skier WHERE username = @username";

        await using var command = _dataSource.CreateCommand(query);

        command.Parameters.AddWithValue("username", username);

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
        while (await reader.ReadAsync())
        {
            Skier skier = new Skier
            {
                Id = reader.GetFieldValue<int>(ordinals.Id),
                Firstname = reader.GetFieldValue<string>(ordinals.Firstname),
                Lastname = reader.GetFieldValue<string>(ordinals.Lastname),
                Email = reader.GetFieldValue<string>(ordinals.Email),
                Image_url = reader.IsDBNull(ordinals.Image_url) ? null : reader.GetFieldValue<string?>(ordinals.Image_url)
            };
            return skier;
        }
        return null;

    }
    public async Task<bool> BuySkiPass(SkiPass skipass)
    {
        // https://stackoverflow.com/questions/6817266/how-to-get-the-current-date-without-the-time 
        //översätta datetime till dateonly 
        string query = "INSERT INTO ski_pass(card_number, skier_id, valid_from, valid_to, destination_id) " +
                       "VALUES (@card_number, @skier_id, @valid_from, @valid_to, @destination_id)";

        await using var command = _dataSource.CreateCommand(query);

        command.Parameters.AddWithValue("card_number", skipass.CardNumber);
        command.Parameters.AddWithValue("skier_id", skipass.SkierId);
        command.Parameters.AddWithValue("valid_from", skipass.ValidFrom);
        command.Parameters.AddWithValue("valid_to", skipass.ValidTo);
        command.Parameters.AddWithValue("destination_id", skipass.DestinationId);

        var result = await command.ExecuteNonQueryAsync();

        return result == 1; 
    }
    public async Task<List<Destination>> GetDestinations()
    {
        string query = "SELECT * FROM destination";

        await using var command = _dataSource.CreateCommand(query);
        await using var reader = await command.ExecuteReaderAsync();


        var ordinals = new
        {
            Id = reader.GetOrdinal("id"),
            Name = reader.GetOrdinal("name"),
            CountryId = reader.GetOrdinal("country_id"), 
        };

        List<Destination> destinations = [];
        while (await reader.ReadAsync())
        {
            Destination destination = new Destination
            {
                Id = reader.GetFieldValue<int>(ordinals.Id),
                Name = reader.IsDBNull(ordinals.Name) ? null : reader.GetFieldValue<string?>(ordinals.Name),
                CountryId = reader.GetFieldValue<int>(ordinals.Id)
            };
            destinations.Add(destination);
        }
        return destinations;

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
