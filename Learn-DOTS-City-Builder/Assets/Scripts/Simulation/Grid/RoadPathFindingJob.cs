using quentin.tran.models.grid;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace quentin.tran.simulation.grid
{
    [BurstCompile]
    public partial struct RoadPathFindingJob : IJob
    {
        [ReadOnly]
        public int2 startIndex;

        [ReadOnly]
        public int2 endIndex;

        [ReadOnly]
        public int gridWidth;

        /// <summary>
        /// All possible directions to move.
        /// </summary>
        [ReadOnly]
        public NativeArray<int2>.ReadOnly directions;

        [ReadOnly]
        public NativeArray<RoadCell>.ReadOnly roads;

        /// <summary>
        /// Base graph nodes.
        /// </summary
        [ReadOnly]
        public NativeArray<PathFindingNode> baseNodes;

        /// <summary>
        /// Result from start to end.
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        public DynamicBuffer<Waypoint> result;

        [ReadOnly]
        public bool addExtraWaypoint;

        /// <summary>
        /// Extra waypoint added at the end of the path (so after <see cref="endIndex"/>) if <see cref="addExtraWaypoint"/> is set to true.
        /// </summary>
        [ReadOnly]
        public int2 extraWaypoint;

        [BurstCompile]
        public void Execute()
        {
            if (startIndex.Equals(endIndex))
            {
                if (addExtraWaypoint)
                    result.Add(new Waypoint() { cellIndex = extraWaypoint });

                return;
            }

            // 1. Init
            int endArrayIndex = GridIndexToArrayIndex(endIndex);
            int startArrayIndex = GridIndexToArrayIndex(startIndex);
            NativeArray<PathFindingNode> nodes = new(this.baseNodes.Length, Allocator.Temp);
            NativeArray<PathFindingNode>.Copy(this.baseNodes, nodes);

            InitHCosts(nodes); // Compute heuristic for every node.

            PathFindingNode startNode = nodes[startArrayIndex];
            startNode.GCost = 0;
            nodes[startArrayIndex] = startNode;

            NativeList<int> openList = new(Allocator.Temp); // List of node to evaluate
            NativeList<int> closeList = new(Allocator.Temp); // List of node already evaluated

            openList.Add(startArrayIndex);

            // 2. Main loop
            while (!openList.IsEmpty)
            {
                (int currentIndex, int pos) = GetNodeWithLowestFCost(openList, nodes);

                openList.RemoveAtSwapBack(pos);
                closeList.Add(currentIndex);

                if (currentIndex == endArrayIndex)
                {
                    break;
                }

                PathFindingNode current = nodes[currentIndex];

                foreach (int2 direction in directions)
                {
                    int cost = 1;
                    int2 neighbourIndex = current.Index + direction;

                    if (!IsInGridBound(neighbourIndex))
                        continue;

                    int neighbourArrayIndex = GridIndexToArrayIndex(neighbourIndex);

                    while (IsInGridBound(neighbourIndex) && roads[neighbourArrayIndex].Type == RoadType.Simple)
                    {
                        neighbourIndex += direction;
                        neighbourArrayIndex = GridIndexToArrayIndex(neighbourIndex);

                        cost++;
                    }

                    if (roads[neighbourArrayIndex].Type == RoadType.None)
                    {
                        neighbourIndex -= direction;
                        neighbourArrayIndex = GridIndexToArrayIndex(neighbourIndex);
                        cost--;
                    }

                    if (closeList.Contains(neighbourArrayIndex))
                        continue;

                    PathFindingNode neighbour = nodes[neighbourArrayIndex];
                    if (!neighbour.IsWalkable)
                        return;

                    // Neighbour is valid
                    float distanceToNeighbour = current.GCost + cost;
                    if (distanceToNeighbour < neighbour.GCost)
                    {
                        neighbour.GCost = distanceToNeighbour;
                        nodes[neighbourArrayIndex] = neighbour;

                        if (!openList.Contains(neighbourArrayIndex))
                            openList.Add(neighbourArrayIndex);

                        // Update all intermediate
                        for (int i = 1; i <= cost; i++)
                        {
                            int2 intermediateIndex = current.Index + i * direction;
                            int intermediateArrayIndex = GridIndexToArrayIndex(intermediateIndex);

                            PathFindingNode intermediate = nodes[intermediateArrayIndex];
                            intermediate.Previous = intermediateIndex - direction;
                            nodes[intermediateArrayIndex] = intermediate;
                        }
                    }
                }
            }

            // 3. Final check
            PathFindingNode endNode = nodes[endArrayIndex];

            if (endNode.Previous.x == -1)
            {
                Debug.Log("PATHFINDING FAILED");
            }
            else
            {
                int2 tmpIndex = endIndex;

                if (addExtraWaypoint)
                    result.Add(new Waypoint() { cellIndex = extraWaypoint });

                while (!tmpIndex.Equals(startIndex))
                {
                    result.Add(new Waypoint() { cellIndex = tmpIndex });
                    tmpIndex = nodes[GridIndexToArrayIndex(tmpIndex)].Previous;
                }

                result.Add(new Waypoint() { cellIndex = startIndex });
            }

            // Clear
            openList.Dispose();
            closeList.Dispose();
            nodes.Dispose();
        }

        [BurstCompile]
        private int GridIndexToArrayIndex(int2 index) => index.x + gridWidth * index.y;

        [BurstCompile]
        private bool IsInGridBound(int2 index)
        {
            if (index.x < 0 || index.y < 0)
                return false;

            if (index.x >= gridWidth || index.y >= gridWidth)
                return false;

            return true;
        }

        /// <summary>
        /// Find the element of <paramref name="list"/> with the lowest f cost. F costs are stored in <paramref name="nodes"/>.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        [BurstCompile]
        private (int index, int position) GetNodeWithLowestFCost(NativeList<int> list, NativeArray<PathFindingNode> nodes)
        {
            Debug.Assert(list.Length > 0);

            PathFindingNode lowest = nodes[list[0]];
            int pos = 0;

            PathFindingNode tmp;

            for (int i = 1; i < list.Length; i++)
            {
                tmp = nodes[list[i]];

                if (tmp.FCost < lowest.FCost)
                {
                    pos = i;
                    lowest = tmp;
                }
            }

            return (GridIndexToArrayIndex(lowest.Index), pos);
        }


        [BurstCompile]
        private void InitHCosts(NativeArray<PathFindingNode> nodes)
        {
            int k = 0;

            for (int i = 0; i < nodes.Length; i++)
            {
                PathFindingNode node = nodes[i];

                if (!node.IsWalkable)
                    continue;

                k++;
                node.HCost = GridUtils.ManhattanDistance(node.Index, endIndex);
                nodes[i] = node;
            }
        }
    }

    public struct Waypoint : IBufferElementData
    {
        public int2 cellIndex;
    }
}
