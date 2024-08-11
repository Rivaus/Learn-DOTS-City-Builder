using System.Collections.Generic;

namespace quentin.tran.models.grid
{
    public class RoadModel
    {
        public RoadIntersectionModel Start { get; set; }

        public RoadIntersectionModel End { get; set; }

        /// <summary>
        /// From start to stop.
        /// </summary>
        public List<GridCellModel> RoadCells { get; set; }

        public RoadModel(GridCellModel road)
        {
            this.RoadCells = new List<GridCellModel>() { road };
        }
    }
}

