using quentin.tran.common;
using quentin.tran.models.grid;
using quentin.tran.simulation.grid;
using static quentin.tran.gameplay.buildingTool.BuilderController;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace quentin.tran.gameplay.buildingTool
{
    /// <summary>
    /// Class responsible for building roads
    /// </summary>
    public class RoadBuilderController : IBuilderModule
    {
        private List<IBuildingCellCommand> commandBuffer = new();

        public RoadBuilderController()
        {

        }

        IEnumerable<IBuildingCellCommand> IBuilderModule.Handle(int x, int y)
        {
            this.commandBuffer.Clear();

            if (!GridManager.Instance.IsCellBuildable(x, y))
            {
                return this.commandBuffer;
            }

            int2 index = new int2(x, y);

            quaternion rotation;
            float3 position = new float3(x, 0, y) * GridProperties.GRID_CELL_SIZE + new float3(GridProperties.GRID_CELL_SIZE, 0, GridProperties.GRID_CELL_SIZE) / 2f;

            uint roadKey = GetRoadType(x, y, out rotation);

            if (roadKey != 0)
            {
                // 1. Create new road
                GridManager.Instance.Build(x, y, new GridCellModel { Index = index, Type = GridCellType.Road, Key = roadKey, RotationOffset = rotation });

                this.commandBuffer.Add(new CreateBuildCellCommand()
                {
                    index = index,
                    cellKey = roadKey,
                    position = position,
                    rotation = rotation
                });

                // 2. Check if neighbours need to be updated
                UpdateRoadNeighbours(x, y, this.commandBuffer);
            }

            return this.commandBuffer;
        }

        internal void UpdateRoadNeighbours(int x, int y, List<IBuildingCellCommand> commands)
        {
            quaternion rotation;
            uint roadKey;
            float3 position;

            foreach (GridCellModel neighbour in GetRoadNeighbours(x, y))
            {
                roadKey = GetRoadType(neighbour.Index.x, neighbour.Index.y, out rotation);
                position = new float3(neighbour.Index.x, 0, neighbour.Index.y) * GridProperties.GRID_CELL_SIZE + new float3(GridProperties.GRID_CELL_SIZE, 0, GridProperties.GRID_CELL_SIZE) / 2f;

                if (roadKey != neighbour.Key || !rotation.Equals(neighbour.RotationOffset))
                {
                    neighbour.Key = roadKey;
                    neighbour.RotationOffset = rotation;

                    commands.Add(new DeleteBuildCellCommand()
                    {
                        index = neighbour.Index,
                    });

                    commands.Add(new CreateBuildCellCommand()
                    {
                        index = neighbour.Index,
                        cellKey = roadKey,
                        position = position,
                        rotation = rotation
                    });
                }
            }
        }

        /// <summary>
        /// Returns the type of route to build considering the cell's neighbours
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private uint GetRoadType(int x, int y, out quaternion rotation)
        {
            List<GridDirection> directions = GetRoadNeighboursDirection(x, y);

            uint roadType = 0;

            rotation = quaternion.identity;


            switch (directions.Count)
            {
                case 0:
                    roadType = GridCellKeys.ROAD_2x2_NO_NEIGHBOUR;
                    break;
                case 1:
                    roadType = GridCellKeys.ROAD_2x2_ONE_NEIGHBOUR;

                    switch (directions[0])
                    {
                        case GridDirection.Top:
                            break;
                        case GridDirection.Right:
                            rotation = quaternion.Euler(0, math.radians(90), 0);
                            break;
                        case GridDirection.Bottom:
                            rotation = quaternion.Euler(0, math.radians(180), 0);
                            break;
                        case GridDirection.Left:
                            rotation = quaternion.Euler(0, math.radians(-90), 0);
                            break;
                        default:
                            break;
                    }

                    break;
                case 2:

                    if (directions[0] == GridDirection.Top && directions[1] == GridDirection.Bottom)
                    {
                        rotation = quaternion.identity;
                        roadType = GridCellKeys.ROAD_2x2;
                    }
                    else if (directions[0] == GridDirection.Right && directions[1] == GridDirection.Left)
                    {
                        rotation = quaternion.Euler(0, math.radians(90), 0);
                        roadType = GridCellKeys.ROAD_2x2;
                    }
                    else if (directions[0] == GridDirection.Right && directions[1] == GridDirection.Bottom)
                    {
                        rotation = quaternion.identity;
                        roadType = GridCellKeys.ROAD_2x2_TURN;
                    }
                    else if (directions[0] == GridDirection.Bottom && directions[1] == GridDirection.Left)
                    {
                        rotation = quaternion.Euler(0, math.radians(90), 0);
                        roadType = GridCellKeys.ROAD_2x2_TURN;
                    }
                    else if (directions[0] == GridDirection.Top && directions[1] == GridDirection.Left)
                    {
                        rotation = quaternion.Euler(0, math.radians(180), 0);
                        roadType = GridCellKeys.ROAD_2x2_TURN;
                    }
                    else if (directions[0] == GridDirection.Top && directions[1] == GridDirection.Right)
                    {
                        rotation = quaternion.Euler(0, math.radians(-90), 0);
                        roadType = GridCellKeys.ROAD_2x2_TURN;
                    }
                    else
                    {
                        Debug.LogError("Impossible case " + directions[0] + " - " + directions[1]);
                    }

                    break;

                case 3:
                    roadType = GridCellKeys.ROAD_2x2_T_TURN;

                    if (directions[0] == GridDirection.Right && directions[1] == GridDirection.Bottom && directions[2] == GridDirection.Left)
                    {

                    }
                    else if (directions[0] == GridDirection.Top && directions[1] == GridDirection.Bottom && directions[2] == GridDirection.Left)
                    {
                        rotation = quaternion.Euler(0, math.radians(90), 0);
                    }
                    else if (directions[0] == GridDirection.Top && directions[1] == GridDirection.Right && directions[2] == GridDirection.Left)
                    {
                        rotation = quaternion.Euler(0, math.radians(180), 0);
                    }
                    else if (directions[0] == GridDirection.Top && directions[1] == GridDirection.Right && directions[2] == GridDirection.Bottom)
                    {
                        rotation = quaternion.Euler(0, math.radians(-90), 0);
                    }
                    else
                    {
                        Debug.LogError("Impossible case " + directions[0] + " - " + directions[1] + " - " + directions[2]);
                    }

                    break;
                case 4:
                    roadType = GridCellKeys.ROAD_2x2_CROSSROAD;
                    break;
            }

            return roadType;
        }

        /// <summary>
        /// Returns locations of road neighbours around (x; y)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private List<GridDirection> GetRoadNeighboursDirection(int x, int y)
        {
            List<GridDirection> directions = new();

            // Top
            GridCellModel tmp = GridManager.Instance.GetCell(x, y + 1);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                directions.Add(GridDirection.Top);

            // Right
            tmp = GridManager.Instance.GetCell(x + 1, y);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                directions.Add(GridDirection.Right);

            // Bottom
            tmp = GridManager.Instance.GetCell(x, y - 1);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                directions.Add(GridDirection.Bottom);

            // Left
            tmp = GridManager.Instance.GetCell(x - 1, y);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                directions.Add(GridDirection.Left);

            return directions;
        }

        /// <summary>
        /// Returns all roads around (x; y)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private List<GridCellModel> GetRoadNeighbours(int x, int y)
        {
            List<GridCellModel> directions = new();

            // Top
            GridCellModel tmp = GridManager.Instance.GetCell(x, y + 1);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                directions.Add(tmp);

            // Right
            tmp = GridManager.Instance.GetCell(x + 1, y);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                directions.Add(tmp);

            // Bottom
            tmp = GridManager.Instance.GetCell(x, y - 1);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                directions.Add(tmp);

            // Left
            tmp = GridManager.Instance.GetCell(x - 1, y);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                directions.Add(tmp);

            return directions;
        }
    }
}


