using quentin.tran.common;
using quentin.tran.models.grid;
using quentin.tran.simulation.grid;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static quentin.tran.gameplay.buildingTool.BuilderController;

namespace quentin.tran.gameplay.buildingTool
{
    public class BuildingBuilderController : IBuilderModule
    {
        private List<IBuildingCellCommand> commandsBuffer = new();

        IEnumerable<IBuildingCellCommand> IBuilderModule.Handle(int x, int y)
        {
            this.commandsBuffer.Clear();

            if (!GridManager.Instance.IsCellBuildable(x, y))
            {
                return this.commandsBuffer;
            }

            if (!IsThereRoadAround(x, y))
            {
                Debug.Log($"{ nameof(BuildingBuilderController) }.Handle : buildings must be connect to a road.");
                return this.commandsBuffer;
            }

            float3 position = new float3(x, 0, y) * GridProperties.GRID_CELL_SIZE + new float3(GridProperties.GRID_CELL_SIZE, 0, GridProperties.GRID_CELL_SIZE) / 2f;
            quaternion rotation = Quaternion.Euler(0, 90 * UnityEngine.Random.Range(0, 4), 0);

            this.commandsBuffer.Add(new CreateBuildCellCommand
            {
                cellKey = GridCellKeys.SIMPLE_HOUSE_01,
                index = new int2(x, y),
                position = position,
                rotation = rotation
            });

            GridManager.Instance.Build(x, y, new models.grid.GridCellModel()
            {
                Index = new int2(x, y),
                Key = GridCellKeys.SIMPLE_HOUSE_01,
                Type = models.grid.GridCellType.Building,
                RotationOffset = rotation
            });

            return this.commandsBuffer;
        }

        private bool IsThereRoadAround(int x, int y)
        {

            List<GridCellModel> directions = new();

            // Top
            GridCellModel tmp = GridManager.Instance.GetCell(x, y + 1);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                return true;

            // Right
            tmp = GridManager.Instance.GetCell(x + 1, y);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                return true;

            // Bottom
            tmp = GridManager.Instance.GetCell(x, y - 1);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                return true;

            // Left
            tmp = GridManager.Instance.GetCell(x - 1, y);
            if (tmp is not null && tmp.Type == GridCellType.Road)
                return true;

            return false;
        }
    }

}
