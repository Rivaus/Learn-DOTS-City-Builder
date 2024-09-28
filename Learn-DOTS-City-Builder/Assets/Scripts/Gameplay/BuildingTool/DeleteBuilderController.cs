using quentin.tran.models.grid;
using quentin.tran.simulation.grid;
using System.Collections.Generic;
using Unity.Mathematics;
using static quentin.tran.gameplay.buildingTool.BuilderController;

namespace quentin.tran.gameplay.buildingTool
{
    public class DeleteBuilderController : IBuilderModule
    {
        private RoadBuilderController roadBuilder;

        private List<IBuildingEntityCommand> commandBuffer = new List<IBuildingEntityCommand>();

        public DeleteBuilderController(RoadBuilderController roadBuilder)
        {
            this.roadBuilder = roadBuilder;
        }

        IEnumerable<IBuildingEntityCommand> IBuilderModule.Handle(int x, int y)
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

            this.commandBuffer.Add(new DeleteBuildEntityCommand { index = new int2(x, y), buildingType = cell.Type });

            GridManager.Instance.Destroy(x, y);
            this.roadBuilder.UpdateRoadNeighbours(x, y, this.commandBuffer);
            RoadGridManager.Instance.UpdateGraph();

            return this.commandBuffer;
        }
    }
}
