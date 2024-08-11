using Unity.Entities;
using Unity.Transforms;

namespace quentin.tran.simulation.system.citizen
{
    [UpdateAfter(typeof(UpdateTimeSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class CitizenSystemGroup : ComponentSystemGroup
    {

    }
}

