using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiDataSimulator.Models
{
    public class SkierDetailedSeason
    {
        public int? Id { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public DateOnly? Enddate { get; set; }
        public int? TotalSeasonRuns { get; set; }
        public string? CurrentSeason { get; set; }
        public int? TotalSeasonDays { get; set; }
        
    }
}
