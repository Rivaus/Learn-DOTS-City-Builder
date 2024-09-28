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
        private List<IBuildingEntityCommand> commandsBuffer = new();

        public uint CurrentBuildingKey;

        public GridCellType BuildingType;

        IEnumerable<IBuildingEntityCommand> IBuilderModule.Handle(int x, int y)
        {
            this.commandsBuffer.Clear();

            int roads = GridUtils.GetNeighboursOfTypeCount(x, y, GridCellType.Road);

            if (roads <= 0)
            {
                Debug.Log($"{nameof(BuildingBuilderController)}.Handle : buildings must be connect to a road.");
                return this.commandsBuffer;
            }

            float3 position = new float3(x, 0, y) * GridProperties.GRID_CELL_SIZE + new float3(GridProperties.GRID_CELL_SIZE, 0, GridProperties.GRID_CELL_SIZE) / 2f;
            quaternion rotation = Quaternion.Euler(0, 90 * UnityEngine.Random.Range(0, 4), 0);

            this.commandsBuffer.Add(new CreateBuildingEntityCommand
            {
                cellKey = CurrentBuildingKey,
                index = new int2(x, y),
                position = position,
                rotation = rotation
            });

            GridManager.Instance.Build(x, y, new GridCellModel()
            {
                Index = new int2(x, y),
                Key = CurrentBuildingKey,
                Type = BuildingType,
                RotationOffset = rotation
            });

            return this.commandsBuffer;
        }
    }
}
