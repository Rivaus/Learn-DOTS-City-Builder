using Unity.Mathematics;

namespace quentin.tran.models.grid
{
    public struct PathFindingNode
    {
        public int2 Index { get; set; }

        public int2 Previous { get; set; }

        private float gCost, hCost;

        /// <summary>
        /// Cost from start.
        /// </summary>
        public float GCost { get => gCost; set { gCost = value; ComputeFCost(); } }

        /// <summary>
        /// Heuristic : estimate cost from this node to end.
        /// </summary>
        public float HCost { get => hCost; set { hCost = value; ComputeFCost(); } }

        /// <summary>
        /// FCost = HCost + GCost
        /// </summary>
        public float FCost { get; private set; }

        public bool IsWalkable { get; set; }

        private void ComputeFCost()
        {
            FCost = gCost + hCost;
        }
    }
}

