using quentin.tran.authoring.building;
using quentin.tran.authoring.citizen;
using quentin.tran.debug;
using Unity.Burst;
using Unity.Entities;

namespace quentin.tran.simulation
{
    partial struct DebugSystem : ISystem
    {
        EntityQuery nbOfHouseBuildingsQuery;

        public void OnCreate(ref SystemState state)
        {
            this.nbOfHouseBuildingsQuery = state.EntityManager.CreateEntityQuery(typeof(Building));
        }

        public void OnUpdate(ref SystemState state)
        {
            int nbOfHouses = 0;
            int nbOfFreeHousePlaces = 0;
            int nbOfFreeHouses = 0;

            foreach (RefRO<House> house in SystemAPI.Query<RefRO<House>>())
            {
                nbOfHouses++;
                if (house.ValueRO.nbOfResidents == 0)
                {
                    nbOfFreeHouses++;
                    nbOfFreeHousePlaces += house.ValueRO.capacity;
                }
            }

            HouseAndJobDebug.nbOfHouseBuildings = this.nbOfHouseBuildingsQuery.CalculateEntityCount();
            HouseAndJobDebug.nbOfHouses = nbOfHouses;
            HouseAndJobDebug.nbOfFreeHouses = nbOfFreeHouses;
            HouseAndJobDebug.nbOfFreeHousePlaces = nbOfFreeHousePlaces;
        }

        [BurstCompile]
        public void OnDestroy()
        {
            this.nbOfHouseBuildingsQuery.Dispose();
        }
    }
}

