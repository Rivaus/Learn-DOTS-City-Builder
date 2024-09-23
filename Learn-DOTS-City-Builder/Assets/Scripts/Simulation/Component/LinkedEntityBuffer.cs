using Unity.Entities;

namespace quentin.tran.simulation.component
{
    public struct LinkedEntityBuffer : IBufferElementData
    {
        public Entity entity;
    }
}