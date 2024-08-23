using quentin.tran.models.grid;
using quentin.tran.simulation.grid;
using Unity.Mathematics;
using UnityEngine;

namespace quentin.tran.debug
{
    public class RoadDebug : MonoBehaviour
    {
        public void Update()
        {
            /*Debug.Log("ROADS " + RoadGridManager.Instance.Roads.Count);

            foreach(RoadModel road in RoadGridManager.Instance.Roads)
            {
                Vector3 start = GridUtils.GetCellCenterPosition(road.RoadCells[0].Index) + new float3(0, .5f, 0);
                Vector3 end = (road.End != null ? GridUtils.GetCellCenterPosition(road.Start.Index) : GridUtils.GetCellCenterPosition(road.RoadCells[^1].Index)) + new float3(0, .5f, 0);

                Debug.DrawLine(start, end, road.Color);
            }*/
        }
    }
}
