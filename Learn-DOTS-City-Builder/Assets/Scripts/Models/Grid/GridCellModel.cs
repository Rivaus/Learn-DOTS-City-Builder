using Unity.Mathematics;

namespace quentin.tran.models.grid
{
    public class GridCellModel
    {
        /// <summary>
        /// Cell position in grid.
        /// </summary>
        public int2 Index { get; set; }

        /// <summary>
        /// <see cref="common.GridCellKeys"/>
        /// </summary>
        public uint Key { get; set; }

        /// <summary>
        /// 3D Models rotation offset.
        /// </summary>
        public quaternion RotationOffset { get; set; }

        /// <summary>
        /// Cell type <see cref="GridCellType"/>.
        /// </summary>
        public GridCellType Type { get; set; }
    }

    public enum GridCellType
    {
        None,
        Road,
        House,
        Office,
        School
    }
}