using quentin.tran.authoring;
using quentin.tran.authoring.citizen;
using quentin.tran.simulation;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace quentin.tran.debug
{
    public class HouseAndJobDebug : MonoBehaviour
    {
        public static int nbOfHouseBuildings = 0;
        public static int nbOfHouses = 0;
        public static int nbOfFreeHouses = 0;
        public static int nbOfFreeHousePlaces = 0;

        private int nbOfCitizens = 0;

        public static int2 currentCellHovered = 0;

        private EntityQuery nbCitizensQuery;

        private async void Start()
        {
            this.nbCitizensQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(Citizen));

            float timeScale;

            while (true)
            {
                timeScale = TimeManagerMonoHandler.time.timeScale;

                await Awaitable.WaitForSecondsAsync(timeScale == 0 ? 2f : (5f / timeScale));

                this.nbOfCitizens = this.nbCitizensQuery.CalculateEntityCount();
            }
        }

        private void OnDestroy()
        {
            this.nbCitizensQuery.Dispose();
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(0, 0, 300, 100), "");

            GUI.Label(new Rect(0, 0, 300, 20), $"Number of house buildings { nbOfHouseBuildings }");
            GUI.Label(new Rect(0, 15, 300, 20), $"Number of houses {nbOfHouses}");
            GUI.Label(new Rect(0, 30, 300, 20), $"Number of free houses { nbOfFreeHouses }");
            GUI.Label(new Rect(0, 45, 300, 20), $"Number of free places (in free houses) { nbOfFreeHousePlaces }");
            GUI.Label(new Rect(0, 60, 300, 20), $"Citizens { nbOfCitizens }");
            GUI.Label(new Rect(0, 75, 300, 20), $"Cell {currentCellHovered}");
        }
    }
}


