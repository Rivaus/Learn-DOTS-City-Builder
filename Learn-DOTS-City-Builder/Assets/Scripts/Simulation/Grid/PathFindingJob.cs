using quentin.tran.common;
using quentin.tran.models.grid;
using quentin.tran.simulation.component;
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
    public struct PathFindingJob : IJob
    {
        [ReadOnly]
        public float3 startPosition;

        [ReadOnly]
        public float3 endPosition;

        /// <summary>
        /// Base graph nodes.
        /// </summary
        [NativeDisableContainerSafetyRestriction]
        public NativeHashMap<int, PathFindingNode>.ReadOnly baseNodes;

        /// <summary>
        /// Result from start to end.
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        public DynamicBuffer<Waypoint> result;

        [BurstCompile]
        public void Execute()
        {
            int2 startIndex = new int2((int)(startPosition.x / GridProperties.GRID_CELL_SIZE), (int)(startPosition.z / GridProperties.GRID_CELL_SIZE));
            int2 endIndex = new int2((int)(endPosition.x / GridProperties.GRID_CELL_SIZE), (int)(endPosition.z / GridProperties.GRID_CELL_SIZE));

            if (startIndex.Equals(endIndex))
            {
                result.Add(new Waypoint() { position = endPosition });
                return;
            }


            // 1.Create a copy of base nodes
            NativeHashMap<int, PathFindingNode> nodes = new(this.baseNodes.Count, Allocator.Temp);
            using NativeKeyValueArrays<int, PathFindingNode> data = this.baseNodes.GetKeyValueArrays(Allocator.Temp);

            foreach (int id in data.Keys)
            {
                PathFindingNode baseNode = this.baseNodes[id];
                PathFindingNode copyNode = new PathFindingNode()
                {
                    id = id,
                    position = baseNode.position,
                    cellIndex = baseNode.cellIndex,
                    neighbours = new NativeList<PathFindingLink>(4, Allocator.Temp),
                    GCost = float.MaxValue,
                };

                foreach(PathFindingLink link in baseNode.neighbours)
                    copyNode.neighbours.Add(link);
                nodes.Add(id, copyNode);
            }

            // 2. Add start and end to the graph
            if (!baseNodes.ContainsKey(WalkingNetworkManager.GridIndexToId(startIndex)))
            {
                var startNeighbourCells = GetNeighbourCells(startIndex, Allocator.Temp);
                int start = WalkingNetworkManager.AddNode(startPosition, 0, startNeighbourCells, ref nodes);
                startNeighbourCells.Dispose();
            }

            int endId = WalkingNetworkManager.GridIndexToId(endIndex);
            if (!baseNodes.ContainsKey(endId))
            {
                var endNeighbourCells = GetNeighbourCells(endIndex, Allocator.Temp);
                endId = WalkingNetworkManager.AddNode(endPosition, 0, endNeighbourCells, ref nodes);
                endNeighbourCells.Dispose();
            }

            InitHCosts(nodes, new float3(endIndex.x, 0, endIndex.y) * GridProperties.GRID_CELL_SIZE); // Compute heuristic for every node.

            //3.Select start node
            int startId = WalkingNetworkManager.GridIndexToId(startIndex);
            PathFindingNode startNode = nodes[startId];
            startNode.GCost = 0;
            nodes[startId] = startNode;

            NativeList<int> openList = new(Allocator.Temp); // List of node to evaluate
            NativeList<int> closeList = new(Allocator.Temp); // List of node already evaluated

            openList.Add(startId);

            // 4. Main loop
            while (!openList.IsEmpty)
            {
                (int currentIndex, int pos) = GetNodeWithLowestFCost(openList, nodes);

                openList.RemoveAtSwapBack(pos);
                closeList.Add(currentIndex);

                if (currentIndex == endId)
                {
                    break;
                }

                PathFindingNode current = nodes[currentIndex];

                foreach(PathFindingLink link in current.neighbours)
                {
                    if (closeList.Contains(link.to))
                        continue;

                    PathFindingNode neighbour = nodes[link.to];

                    float distanceToNeighbour = current.GCost + math.distance(current.position, neighbour.position);
                    if (distanceToNeighbour < neighbour.GCost)
                    {
                        neighbour.GCost = distanceToNeighbour;
                        neighbour.previous = currentIndex;
                        nodes[neighbour.id] = neighbour;

                        if (!openList.Contains(neighbour.id))
                            openList.Add(neighbour.id);
                    }
                }
            }

            if (nodes[endId].previous == -1)
            {
                Debug.Log("PATHFINDING FAILED");
            }
            else
            {
                int tmpId = endId;

                while (!tmpId.Equals(startId))
                {
                    result.Add(new Waypoint() { position = nodes[tmpId].position });
                    tmpId = nodes[tmpId].previous;
                }

                result.Add(new Waypoint() { position = nodes[startId].position });
            }

            foreach (var entry in nodes)
            {
                entry.Value.Dispose();
            }
            nodes.Dispose();
            openList.Dispose();
            closeList.Dispose();
        }

        [BurstCompile]
        private void InitHCosts(NativeHashMap<int, PathFindingNode> nodes, float3 endPosition)
        {
            using NativeKeyValueArrays<int, PathFindingNode> data = nodes.GetKeyValueArrays(Allocator.Temp);

            foreach (int id in data.Keys)
            {
                PathFindingNode node = nodes[id];
                node.HCost = math.distance(node.position, endPosition);
                node.previous = -1;
                nodes[id] = node;
            }
        }

        [BurstCompile]
        private (int index, int position) GetNodeWithLowestFCost(NativeList<int> list, NativeHashMap<int, PathFindingNode> nodes)
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

            return (lowest.id, pos);
        }

        [BurstCompile]
        private NativeArray<int2> GetNeighbourCells(int2 cellIndex, Allocator allocator)
        {
            NativeArray<int2> cells = new(4, allocator);
            cells[0] = cellIndex + new int2(-1, 0);
            cells[1] = cellIndex + new int2(1, 0);
            cells[2] = cellIndex + new int2(0, 1);
            cells[3] = cellIndex + new int2(0, -1);

            return cells;
        }
    }
}
