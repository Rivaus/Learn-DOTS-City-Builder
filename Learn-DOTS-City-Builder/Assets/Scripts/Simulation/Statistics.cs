using Unity.Mathematics;
using Unity.Properties;

namespace quentin.tran.simulation
{
    /// <summary>
    /// Contains all data about the current city.
    /// </summary>
    public class Statistics
    {
        [CreateProperty]
        public int2 HoveredCell { get; set; }

        #region Houses

        [CreateProperty]
        public int NumberOfHouseBuildings { get; set; } = 0;

        [CreateProperty]
        public int NumberOfHouses { get; set; } = 0;

        [CreateProperty]
        public int NumberOfEmptyHouses { get; set; } = 0;

        #endregion

        #region Citizens

        public CitizenStatistics citizenStatistics = new();

        public class CitizenStatistics
        {
            [CreateProperty]
            public int NumberOfCitizens { get; set; }

            public int NumberOfBabies { get; set; }

            public int NumberOfChildren { get; set; }

            public int NumberOfTeenagers { get; set; }

            public int NumberOfAdults { get; set; }

            public int NumberOfSeniors { get; set; }

            public CitizenStatistics Copy()
            {
                return new CitizenStatistics
                {
                    NumberOfCitizens = NumberOfCitizens,
                    NumberOfBabies = NumberOfBabies,
                    NumberOfChildren = NumberOfChildren,
                    NumberOfTeenagers = NumberOfTeenagers,
                    NumberOfAdults = NumberOfAdults,
                    NumberOfSeniors = NumberOfSeniors
                };
            }

            public bool Equals(CitizenStatistics other)
            {
                if (other is null)
                    return false;

                if (other.NumberOfCitizens != NumberOfCitizens) return false;
                else if (other.NumberOfBabies != NumberOfBabies) return false;
                else if (other.NumberOfChildren != NumberOfChildren) return false;
                else if (other.NumberOfTeenagers != NumberOfTeenagers) return false;
                else if (other.NumberOfAdults != NumberOfAdults) return false;
                else if (other.NumberOfSeniors != NumberOfSeniors) return false;

                return true;
            }
        }

        #endregion

        #region Jobs

        [CreateProperty]
        public int NumberOfJobBuildings { get; set; }

        [CreateProperty]
        public int NumberOfJobAvailable { get; set; }

        [CreateProperty]
        public int NumberOfWorkers { get; set; }

        [CreateProperty]
        public int NumberOfUnemployed { get; set; }

        #endregion
    }
}
