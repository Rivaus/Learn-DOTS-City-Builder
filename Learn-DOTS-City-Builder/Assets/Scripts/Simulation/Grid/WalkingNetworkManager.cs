using quentin.tran.common;
using quentin.tran.gameplay;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace quentin.tran.simulation.grid
{
    [BurstCompile]
    public class WalkingNetworkManager : ISingleton<WalkingNetworkManager>
    {
        public static WalkingNetworkManager Instance;

        private NativeHashMap<int, PathFindingNode> nodes;

        public NativeHashMap<int, PathFindingNode>.ReadOnly Nodes => this.nodes.AsReadOnly();

        public WalkingNetworkManager()
        {
            this.nodes = new NativeHashMap<int, PathFindingNode>(4 * GridProperties.GRID_SIZE *  GridProperties.GRID_SIZE, Allocator.Persistent);

            Instance = this;
        }

        [BurstCompile]
        public void AddRoad(in int2 cellIndex)
        {
            float border = GridProperties.GRID_CELL_SIZE * .1f;
            NativeArray<int2> neighbourCellsToConsider = new(2, Allocator.Temp);

            int baseCellId = GridIndexToId(cellIndex);
            for (int i = 0; i < 4; i++)
            {
                float3 offset = float3.zero;

                switch (i)
                {
                    case 0: // bottom left
                        offset = new float3(border, 0, border);
                        break;
                    case 1: // bottom right
                        offset = new float3(GridProperties.GRID_CELL_SIZE - border, 0, border);
                        break;
                    case 2: // top right
                        offset = new float3(GridProperties.GRID_CELL_SIZE - border, 0, GridProperties.GRID_CELL_SIZE - border);
                        break;
                    case 3: // top left
                        offset = new float3(border, 0, GridProperties.GRID_CELL_SIZE - border);
                        break;
                    default:
                        break;
                }

                switch (i)
                {
                    case 0: // bottom left
                        neighbourCellsToConsider[0] = new int2(cellIndex.x - 1, cellIndex.y); // Search -> left
                        neighbourCellsToConsider[1] = new int2(cellIndex.x, cellIndex.y - 1); // Search -> bottom
                        break;
                    case 1: // bottom right
                        neighbourCellsToConsider[0] = new int2(cellIndex.x + 1, cellIndex.y); // Search -> right
                        neighbourCellsToConsider[1] = new int2(cellIndex.x, cellIndex.y - 1); // Search -> bottom
                        break;
                    case 2: // top right
                        neighbourCellsToConsider[0] = new int2(cellIndex.x + 1, cellIndex.y); // Search -> right
                        neighbourCellsToConsider[1] = new int2(cellIndex.x, cellIndex.y + 1); // Search -> top
                        break;
                    case 3: // top left
                        neighbourCellsToConsider[0] = new int2(cellIndex.x - 1, cellIndex.y); // Search -> left
                        neighbourCellsToConsider[1] = new int2(cellIndex.x, cellIndex.y + 1); // Search -> top
                        break;
                    default:
                        break;
                }

                float3 nodePosition = new float3(cellIndex.x, 0, cellIndex.y) * GridProperties.GRID_CELL_SIZE + offset;
                AddNode(nodePosition, i, neighbourCellsToConsider, ref this.nodes);

                // Add links inside the cell
                int id = baseCellId + i;
                int nextNeighbour = id + 1;
                if (nextNeighbour > baseCellId + 3)
                    nextNeighbour = baseCellId;
                int previousNeighbour = id - 1;
                if (previousNeighbour < baseCellId)
                    previousNeighbour = baseCellId + 3;

                this.nodes[id].neighbours.Add(new PathFindingLink() { to = nextNeighbour, cost = GridProperties.GRID_CELL_SIZE });
                this.nodes[id].neighbours.Add(new PathFindingLink() { to = previousNeighbour, cost = GridProperties.GRID_CELL_SIZE });
            }

            neighbourCellsToConsider.Dispose();
        }

        [BurstCompile]
        public static int AddNode(in float3 nodePosition, int subId, in NativeArray<int2> neighbourCellsToConsider, ref NativeHashMap<int, PathFindingNode> nodes)
        {
            UnityEngine.Debug.Assert(subId >= 0 && subId < 4, "Sub id must be in range [0; 4[");

            int2 cellIndex = new int2((int)(nodePosition.x / GridProperties.GRID_CELL_SIZE), (int)(nodePosition.z / GridProperties.GRID_CELL_SIZE));
            int baseCellId = GridIndexToId(cellIndex);
            int id = baseCellId + subId;

            nodes.Add(id, new()
            {
                id = id,
                position = nodePosition,
                cellIndex = cellIndex,
                neighbours = new NativeList<PathFindingLink>(4, Allocator.Persistent),
                GCost = float.MaxValue,
                HCost = 0
            });

            foreach(int2 neighbourIndex in neighbourCellsToConsider)
            {
                int tmp = GetNeighbourInNeighbourCell(neighbourIndex, nodePosition, nodes);

                if (tmp != -1)
                {
                    float distance = math.distance(nodes[id].position, nodes[tmp].position);

                    nodes[id].neighbours.Add(new PathFindingLink() { cost = distance, to = tmp });
                    nodes[tmp].neighbours.Add(new PathFindingLink() { cost = distance, to = id });
                }
            }

            return id;
        }

        [BurstCompile]
        public void Clear()
        {
            Instance = null;

            foreach (var entry in this.nodes)
            {
                entry.Value.Dispose();
            }

            this.nodes.Dispose();
        }

        public void RemoveRoad(int2 cellIndex)
        {
            for (int i = 0; i < 4; i++)
            {
                int id = GridIndexToId(cellIndex) + i;

                if (!this.nodes.ContainsKey(id))
                {
                    UnityEngine.Debug.LogError($"{nameof(WalkingNetworkManager)}.{nameof(RemoveRoad)} Collection doesn't contain a node with id {id}.");
                    continue;
                }

                var neighbours = this.nodes[id].neighbours;

                for (int j = 0; j < neighbours.Length; j++)
                {
                    using var neighbourNeighbours = this.nodes[neighbours[j].to].neighbours.ToArray(Allocator.Temp);

                    for (int k = 0; k < neighbourNeighbours.Length; k++)
                    {
                        PathFindingLink link = neighbourNeighbours[k];

                        if (link.to == id)
                        {
                            this.nodes[neighbours[j].to].neighbours.RemoveAt(k);
                            break;
                        }
                    }
                }

                this.nodes[id].Dispose();
                this.nodes.Remove(id);
            }
        }

        [BurstCompile]
        private static int GetNeighbourInNeighbourCell(in int2 neighbourCell, in float3 position, in NativeHashMap<int, PathFindingNode> nodes)
        {
            int id = GridIndexToId(neighbourCell);

            int closestId = -1;
            float tmpDistance = float.MaxValue;

            if (nodes.ContainsKey(id))
            {
                for (int i = 0; i < 4; i++)
                {
                    float sqDistance = math.distancesq(position, nodes[id + i].position);

                    if (sqDistance < tmpDistance)
                    {
                        closestId = id + i;
                        tmpDistance = sqDistance;
                    }
                }
            }

            return closestId;
        }

        [BurstCompile]
        public static int GridIndexToId(in int2 index) => 4 * (index.x * GridProperties.GRID_SIZE + index.y);
    }

    public struct PathFindingNode : IDisposable
    {
        public int id;

        public int2 cellIndex;

        public float3 position;

        public int previous;

        public NativeList<PathFindingLink> neighbours;

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

        private void ComputeFCost()
        {
            FCost = gCost + hCost;
        }

        public void Dispose() => neighbours.Dispose();
    }

    public struct PathFindingLink
    {
        public int to;

        public float cost;
    }
}
