using quentin.tran.authoring.building;
using quentin.tran.models.grid;
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
                    break;
                case GridCellType.Road:
                    RoadGridManager.Instance.Destroy(x, y);
                    break;
                case GridCellType.Building:
                    // 1. Delete building
                    // 1.1 Remove job for every citizen who were working here
                    // 2. Delete house
                    // 2.1 Remove every worker from their jobs
                    // 2.2 Remove every child from school and student from university
                    // 2.3 Delete citizens

                    Debug.Log("TODO : Build");

                    break;
            }

            GridManager.Instance.Destroy(x, y);

            this.commandBuffer.Add(new DeleteBuildCellCommand { index = new int2(x, y) });
            this.roadBuilder.UpdateRoadNeighbours(x, y, this.commandBuffer);

            return this.commandBuffer;
        }
    }

    [BurstCompile]
    public partial struct DeleteOfficeBuildingJob : IJobEntity
    {
        [ReadOnly]
        public int index;

        [BurstCompile]
        public void Execute(in GridCellComponent cell, in OfficeBuilding building)
        {
            if (!cell.index.Equals(this.index))
                return;


        }
    }

}
