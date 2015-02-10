using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nymphicus.Model
{
    public class Geo
    {
        public string CityOrEqual { get; set; }
        public string StreetName { get; set; }

        public string FullLocation
        {
            get
            {
                if (!string.IsNullOrEmpty(StreetName))
                {
                    return CityOrEqual + ", " + StreetName;
                }
                else
                {
                    return CityOrEqual;
                }
            }
        }
    }
}
