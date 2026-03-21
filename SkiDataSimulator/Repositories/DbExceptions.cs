using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiDataSimulator.Repositories
{
    static class DbExceptions
    {
        public static Exception Translate(PostgresException exception)
        {
            return null;
        }
    }
}
