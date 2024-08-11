using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace quentin.tran.models.grid
{
    public class RoadIntersectionModel
    {
        public float3 Position;

        public List<RoadModel> Roads { get; set; }

        public GridCellModel Cell { get; set; }
    }
}