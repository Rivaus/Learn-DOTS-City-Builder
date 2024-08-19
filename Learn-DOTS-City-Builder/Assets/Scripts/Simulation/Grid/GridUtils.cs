using NUnit.Framework;
using quentin.tran.common;
using quentin.tran.models.grid;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

namespace quentin.tran.simulation.grid
{
    public static class GridUtils
    {
        public static readonly List<int2> CARDINAL_DIRECTION = new() { new int2(0, 1), new int2(1, 0), new int2(0, -1), new int2(-1, 0) };

        /// <summary>
        /// Returns center world position of a grid cell.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float3 GetCellCenterPosition(int x, int y)
        {
            return new float3(x, 0, y) * GridProperties.GRID_CELL_SIZE + new float3(GridProperties.GRID_CELL_SIZE, 0, GridProperties.GRID_CELL_SIZE) / 2f;
        }

        /// <summary>
        /// Returns center world position of a grid cell.
        /// </summary>
        public static float3 GetCellCenterPosition(int2 index) => GetCellCenterPosition(index.x, index.y);

        public static void GetNeighboursOfType(int x, int y, GridCellType type, List<GridCellModel> neighbours)
        {
            // Top
            GridCellModel tmp = GridManager.Instance.GetCell(x, y + 1);
            if (tmp is not null && tmp.Type == type)
                neighbours.Add(tmp);

            // Right
            tmp = GridManager.Instance.GetCell(x + 1, y);
            if (tmp is not null && tmp.Type == type)
                neighbours.Add(tmp);

            // Bottom
            tmp = GridManager.Instance.GetCell(x, y - 1);
            if (tmp is not null && tmp.Type == type)
                neighbours.Add(tmp);

            // Left
            tmp = GridManager.Instance.GetCell(x - 1, y);
            if (tmp is not null && tmp.Type == type)
                neighbours.Add(tmp);
        }

        public static int GetNeighboursOfTypeCount(int x, int y, GridCellType type)
        {
            int neighbours = 0;

            // Top
            GridCellModel tmp = GridManager.Instance.GetCell(x, y + 1);
            if (tmp is not null && tmp.Type == type)
                neighbours++;

            // Right
            tmp = GridManager.Instance.GetCell(x + 1, y);
            if (tmp is not null && tmp.Type == type)
                neighbours++;

            // Bottom
            tmp = GridManager.Instance.GetCell(x, y - 1);
            if (tmp is not null && tmp.Type == type)
                neighbours++;

            // Left
            tmp = GridManager.Instance.GetCell(x - 1, y);
            if (tmp is not null && tmp.Type == type)
                neighbours++;

            return neighbours;
        }

        public static float ManhattanDistance(int2 start, int2 end)
        {
            float dx = math.abs(start.x - end.x);
            float dy = math.abs(start.y - end.y);

            return dx + dy;
        }
    }
}

