using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SkiDataSimulator.Models
{
    public class Liftkort
    {
        public string Name { get; set; }
        public int Time { get; set; }

        public List<Liftkort> GetPasses()
        {
            List<Liftkort> liftkort = new List<Liftkort>();
            Liftkort heldag = new Liftkort
            {
                Name = "Heldag",
                Time = 1,
            };
            liftkort.Add(heldag);
            Liftkort tvådag = new Liftkort
            {
                Name = "Tvådagars",
                Time = 2,
            };
            liftkort.Add(tvådag);
            Liftkort vecka = new Liftkort
            {
                Name = "Vecka",
                Time = 7,
            };
            liftkort.Add(vecka);
            Liftkort månad = new Liftkort
            {
                Name = "Månad",
                Time = 30,
            };
            liftkort.Add(månad);
            Liftkort säsong = new Liftkort
            {
                Name = "Säsong",
                Time = 0,
            };
            liftkort.Add(säsong);
            return liftkort;

        }
    }
}
