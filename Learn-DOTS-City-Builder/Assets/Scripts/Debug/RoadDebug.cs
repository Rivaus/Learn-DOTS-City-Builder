using quentin.tran.models.grid;
using quentin.tran.simulation.grid;
using UnityEngine;

namespace quentin.tran.debug
{
    public class RoadDebug : MonoBehaviour
    {
        public void Update()
        {
            foreach(RoadModel road in GridManager.Instance.Roads)
            {
                Debug.DrawLine(road.Start.Position, road.End.Position);
            }
        }
    }
}
