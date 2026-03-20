using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiDataSimulator.Models
{
    public class SkierLeaderboardDetails
    {
        public int? Id { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public int? TotalVerticalDrop { get; set; }
        public int? TotalCountries { get; set; }

    }
}
