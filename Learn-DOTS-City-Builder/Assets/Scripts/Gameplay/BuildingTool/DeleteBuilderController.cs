using quentin.tran.simulation.grid;
using System.Collections.Generic;
using Unity.Mathematics;
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

            if (!GridManager.Instance.IsCellBuildable(x, y))
            {
                GridManager.Instance.Destroy(x, y);

                if (RoadGridManager.Instance.IsRoad(x, y))
                {
                    RoadGridManager.Instance.Destroy(x, y);
                    RoadGridManager.Instance.UpdateGraph();
                }

                this.commandBuffer.Add(new DeleteBuildCellCommand { index = new int2(x, y) });
                this.roadBuilder.UpdateRoadNeighbours(x, y, this.commandBuffer);
            }

            return this.commandBuffer;
        }
    }

}
