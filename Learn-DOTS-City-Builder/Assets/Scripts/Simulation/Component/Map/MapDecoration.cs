using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace quentin.tran.simulation.component.map
{
    public struct MapDecoration : IComponentData
    {
        public int2 index;
    }
}
