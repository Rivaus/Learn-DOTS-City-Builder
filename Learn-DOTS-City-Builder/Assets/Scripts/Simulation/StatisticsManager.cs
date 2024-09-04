using quentin.tran.authoring.building;
using quentin.tran.authoring.citizen;
using quentin.tran.common;
using quentin.tran.gameplay;
using quentin.tran.simulation.system.citizen;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static quentin.tran.simulation.Statistics;

namespace quentin.tran.simulation
{
    /// <summary>
    /// Updates all <see cref="Statistics"/> data.
    /// </summary>
    public class StatisticsManager : ISingleton<StatisticsManager>
    {
        public static StatisticsManager Instance { get; private set; }

        public Statistics Statistics { get; private set; }

        #region Queries

        private EntityQuery citizensQuery, babiesQuery, childrenQuery, teenagersQuery, adultsQuery, seniorsQuery;

        private EntityQuery nbOfHouseBuildingsQuery, housesQuery, jobBuildingsQuery, workersQuery;

        #endregion

        public StatisticsManager()
        {
            Instance = this;

            this.Statistics = new Statistics();

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            this.citizensQuery = entityManager.CreateEntityQuery(typeof(Citizen));
            this.babiesQuery = entityManager.CreateEntityQuery(typeof(CitizenBaby));
            this.childrenQuery = entityManager.CreateEntityQuery(typeof(CitizenBaby));
            this.teenagersQuery = entityManager.CreateEntityQuery(typeof(CitizenTeenager));
            this.adultsQuery = entityManager.CreateEntityQuery(typeof(CitizenAdult));
            this.seniorsQuery = entityManager.CreateEntityQuery(typeof(CitizenSenior));

            this.nbOfHouseBuildingsQuery = entityManager.CreateEntityQuery(typeof(Building));
            this.housesQuery = entityManager.CreateEntityQuery(typeof(House));
            this.jobBuildingsQuery = entityManager.CreateEntityQuery(typeof(OfficeBuilding));
            this.workersQuery = entityManager.CreateEntityQuery(typeof(Citizen), typeof(CitizenJob));

            UpdateData();
        }

        private async void UpdateData()
        {
            Debug.LogError("TO IMPROVE : no need to query each iteration for house data");

            while (true)
            {
                await Awaitable.WaitForSecondsAsync(1/10f);

                ComputeCitizensData();
                ComputeHousesData();
                ComputeJobData();

                Application.exitCancellationToken.ThrowIfCancellationRequested();
            }
        }

        private void ComputeCitizensData()
        {
            using NativeArray<Citizen> citizens = this.citizensQuery.ToComponentDataArray<Citizen>(Allocator.Temp);

            CitizenStatistics stats = this.Statistics.citizenStatistics;
            stats.NumberOfCitizens = citizens.Length;

            stats.NumberOfBabies = this.babiesQuery.CalculateEntityCount();
            stats.NumberOfChildren = this.childrenQuery.CalculateEntityCount();
            stats.NumberOfTeenagers = this.teenagersQuery.CalculateEntityCount();
            stats.NumberOfAdults = this.adultsQuery.CalculateEntityCount();
            stats.NumberOfSeniors = this.seniorsQuery.CalculateEntityCount();
        }

        private void ComputeHousesData()
        {
            this.Statistics.NumberOfHouseBuildings = this.nbOfHouseBuildingsQuery.CalculateEntityCount();

            int nbOfHouses = 0;
            int nbOfFreeHousePlaces = 0;
            int nbOfFreeHouses = 0;

            using NativeArray<House> houses = this.housesQuery.ToComponentDataArray<House>(Allocator.Temp);

            foreach (House house in houses)
            {
                nbOfHouses++;
                if (house.nbOfResidents == 0)
                {
                    nbOfFreeHouses++;
                    nbOfFreeHousePlaces += house.capacity;
                }
            }

            this.Statistics.NumberOfHouses = nbOfHouses;
            this.Statistics.NumberOfEmptyHouses = nbOfFreeHouses;
        }

        private void ComputeJobData()
        {
            Statistics.NumberOfJobBuildings = this.jobBuildingsQuery.CalculateEntityCount();
            
            using NativeArray<OfficeBuilding> offices = this.jobBuildingsQuery.ToComponentDataArray<OfficeBuilding>(Allocator.Temp);

            int nbAvailableJobs = 0;
            for (int i = 0; i < offices.Length; i++)
            {
                nbAvailableJobs += offices[i].nbOfAvailableJob;
            }

            Statistics.NumberOfJobAvailable = nbAvailableJobs;
            Statistics.NumberOfWorkers = this.workersQuery.CalculateEntityCount();
            Statistics.NumberOfUnemployed = Statistics.citizenStatistics.NumberOfAdults - Statistics.NumberOfWorkers;
        }

        public void Clear()
        {
            Instance = null;

            this.citizensQuery.Dispose();
            this.babiesQuery.Dispose();
            this.childrenQuery.Dispose();
            this.teenagersQuery.Dispose();
            this.adultsQuery.Dispose();
            this.seniorsQuery.Dispose();

            this.nbOfHouseBuildingsQuery.Dispose();
            this.housesQuery.Dispose();
            this.jobBuildingsQuery.Dispose();
            this.workersQuery.Dispose();
        }
    }
}

