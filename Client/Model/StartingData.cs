using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Model
{
    public class StartingData
    {
        public int PlanetId { get; set; }

        public StartingData(int planetId)
        {
            PlanetId = planetId;
        }
    }
}
