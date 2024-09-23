using quentin.tran.authoring.building;
using quentin.tran.models.grid;
using quentin.tran.simulation.component;
using quentin.tran.simulation.grid;
using quentin.tran.simulation.system.grid;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static quentin.tran.gameplay.buildingTool.BuilderController;

namespace quentin.tran.gameplay.buildingTool
{
    public class DeleteBuilderController : IBuilderModule
    {
        private RoadBuilderController roadBuilder;

        private List<IBuildingCellCommand> commandBuffer = new List<IBuildingCellCommand>();

        public DeleteBuilderController(RoadBuilderController roadBuilder)
        {
            this.roadBuilder = roadBuilder;
        }

        IEnumerable<IBuildingCellCommand> IBuilderModule.Handle(int x, int y)
        {
            this.commandBuffer.Clear();

            GridCellModel cell = GridManager.Instance.GetCell(x, y);

            switch (cell.Type)
            {
                case GridCellType.None:
                    return this.commandBuffer;
                case GridCellType.Road:
                    RoadGridManager.Instance.Destroy(x, y);
                    break;
            }

            this.commandBuffer.Add(new DeleteBuildCellCommand { index = new int2(x, y), buildingType = cell.Type });
            this.roadBuilder.UpdateRoadNeighbours(x, y, this.commandBuffer);
            RoadGridManager.Instance.UpdateGraph();

            GridManager.Instance.Destroy(x, y);

            return this.commandBuffer;
        }
    }
}
