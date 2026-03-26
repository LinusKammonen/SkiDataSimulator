using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiDataSimulator.Repositories
{
    public static class DbExceptions
    {
        public static Exception Translate(PostgresException exception)
        {
            switch (exception.ConstraintName)
            {
                case "chk_firstname_valid_chars":
                    return new Exception("Namnet får enbart innehålla bokstäver.", exception);

                case "chk_lastname_valid_chars":
                    return new Exception("Efternamnet får enbart innehålla bokstäver.", exception);

                case "chk_name_valid_chars_username":
                    return new Exception("Användarnamnet får enbart innehålla siffror och bokstäver", exception);

                case "chk_email_valid_chars":
                    return new Exception("Fel format på mejladressen", exception);

                case "skier_email_key":
                    return new Exception("Mejladressen används redan", exception);

                case "skier_username_key":
                    return new Exception("Användarnamnet används redan", exception);
                case "chk_card_number_length":
                    return new Exception("Fel, Kortnummret ska enbart innehålla siffror och vara 9 tecken långt.");
            }

            switch (exception.SqlState)
            {
                case PostgresErrorCodes.UniqueViolation:
                    return new Exception($"Värde {exception.Source} måste vara unikt. {exception.Message}", exception);

                case PostgresErrorCodes.NotNullViolation:
                    return new Exception($"Fältet {exception.ColumnName} får inte vara tomt. {exception.Message}", exception);

                case PostgresErrorCodes.CheckViolation:
                    return new Exception($"Ett värde bryter mot en databasregel. {exception.Message}", exception);

                default:
                    return new Exception($"Ett okänt databasfel har inträffat. {exception.Message}", exception);

            }
        }
        
    }
}
