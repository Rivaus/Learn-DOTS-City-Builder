using quentin.tran.common;
using quentin.tran.gameplay;
using quentin.tran.models.grid;
using System;

namespace quentin.tran.simulation.grid
{
    public class GridManager : ISingleton<GridManager>
    {
        public static GridManager Instance { get; private set; }

        private GridCellModel[,] grid;

        public GridManager()
        {
            Instance = this;

            this.grid = new GridCellModel[GridProperties.GRID_SIZE, GridProperties.GRID_SIZE];

            for (int i = 0; i < GridProperties.GRID_SIZE; i++)
            {
                for (int j = 0; j < GridProperties.GRID_SIZE; j++)
                {
                    this.grid[i, j] = new GridCellModel() { Index = new(i, j), Type = GridCellType.None };
                }
            }
        }

        public bool IsCellBuildable(int x, int y)
        {
            CheckGrid(nameof(IsCellBuildable), x, y);

            return this.grid[x, y].Type == GridCellType.None;
        }

        /// <summary>
        /// Build a new cell in the grid
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="cell"></param>
        public void Build(int x, int y, GridCellModel cell)
        {
            CheckGrid(nameof(Build), x, y);

            this.grid[x, y] = cell;
        }

        public void Destroy(int x, int y)
        {
            CheckGrid(nameof(Destroy), x, y);

            this.grid[x, y].Type = GridCellType.None;
        }

#nullable enable
        public GridCellModel GetCell(int x, int y)
        {
            if (x >= GridProperties.GRID_SIZE || y >= GridProperties.GRID_SIZE || x < 0 || y < 0)
            {
                return new GridCellModel() { Type = GridCellType.None };
            }
            else
            {
                return this.grid[x, y];
            }
        }

#nullable disable

        public void Clear()
        {

        }

        private void CheckGrid(string methodName, int x, int y)
        {
            if (x >= GridProperties.GRID_SIZE || y >= GridProperties.GRID_SIZE || x < 0 || y < 0)
            {
                throw new GridOutOfRangeException(methodName, x, y);
            }
        }

        private class GridOutOfRangeException : Exception
        {
            public GridOutOfRangeException(string methodName, int x, int y) : base($"GridOutOfRange.{methodName} : {x}; {y} for {GridProperties.GRID_SIZE}; {GridProperties.GRID_SIZE} ")
            {

            }
        }
    }

}

