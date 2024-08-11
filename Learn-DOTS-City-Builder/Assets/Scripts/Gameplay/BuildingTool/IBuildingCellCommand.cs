using Unity.Mathematics;

namespace quentin.tran.gameplay.buildingTool
{
    public interface IBuildingCellCommand
    {

    }

    public struct CreateBuildCellCommand : IBuildingCellCommand
    {
        public uint cellKey;

        public int2 index;

        public float3 position;

        public quaternion rotation;
    }

    public struct DeleteBuildCellCommand : IBuildingCellCommand
    {
        public int2 index;
    }
}

