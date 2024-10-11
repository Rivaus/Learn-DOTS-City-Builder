using Unity.Entities;
using Unity.Rendering;

namespace quentin.tran.simulation.component.material
{
    [MaterialProperty("_SnowLevel")]
    public struct SnowLevel : IComponentData
    {
        public float level;
    }
}

