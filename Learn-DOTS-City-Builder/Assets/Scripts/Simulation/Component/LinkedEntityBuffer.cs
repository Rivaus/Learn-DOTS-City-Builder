using Unity.Entities;
using Unity.Mathematics;

namespace quentin.tran.simulation.component
{
    public struct LinkedEntityBuffer : IBufferElementData
    {
        public Entity entity;
    }
}