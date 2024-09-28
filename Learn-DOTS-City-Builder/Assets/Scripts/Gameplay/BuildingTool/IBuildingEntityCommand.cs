using quentin.tran.models.grid;
using Unity.Mathematics;

namespace quentin.tran.gameplay.buildingTool
{
    public interface IBuildingEntityCommand
    {

    }

    public struct CreateBuildingEntityCommand : IBuildingEntityCommand
    {
        public uint cellKey;

        public int2 index;

        public float3 position;

        public quaternion rotation;
    }

    public struct DeleteBuildEntityCommand : IBuildingEntityCommand
    {
        public int2 index;

        public GridCellType buildingType;
    }
}

