using quentin.tran.common;
using Unity.Mathematics;

namespace quentin.tran.simulation.grid
{
    public static class GridUtils
    {
        /// <summary>
        /// Returns center world position of a grid cell.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float3 GetCellCenterPosition(int x, int y)
        {
            return new float3(x, 0, y) * GridProperties.GRID_CELL_SIZE + new float3(1, 0, 1) * GridProperties.GRID_SIZE / 2f;
        }
    }
}

