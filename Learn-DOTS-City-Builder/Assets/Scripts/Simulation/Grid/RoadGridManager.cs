using quentin.tran.common;
using quentin.tran.gameplay;
using quentin.tran.models.grid;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace quentin.tran.simulation.grid
{
    public class RoadGridManager : ISingleton<RoadGridManager>
    {
        public static RoadGridManager Instance { get; private set; }

        public RoadCell[,] roadGrid;

        private NativeArray<PathFindingNode> roadPathFindingNodes;

        private NativeArray<RoadCell> roadGridArray;

        public NativeArray<RoadCell>.ReadOnly RoadGridArray => roadGridArray.AsReadOnly();

        public readonly NativeArray<int2> movementDirections;

        public NativeArray<int2>.ReadOnly MovementDirections => movementDirections.AsReadOnly();

        public RoadGridManager()
        {
            Instance = this;

            this.roadGrid = new RoadCell[GridProperties.GRID_SIZE, GridProperties.GRID_SIZE];
            this.roadPathFindingNodes = new(this.roadGrid.Length, Allocator.Persistent);
            this.roadGridArray = new(this.roadGrid.Length, Allocator.Persistent);
            this.movementDirections = new(4, Allocator.Persistent);

            for (int i = 0; i < this.movementDirections.Length; i++)
            {
                this.movementDirections[i] = GridUtils.CARDINAL_DIRECTION[i];
            }

            UpdateGraph();
        }

        /// <summary>
        /// Build a new road cell.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="intersection"></param>
        /// <exception cref="System.Exception"></exception>
        public void Build(int x, int y, bool intersection)
        {
            if (roadGrid[x, y].Type != RoadType.None)
                throw new System.Exception("RoadGridManager.Build : cell not buildable " + x + "; " + y);

            roadGrid[x, y].Type = intersection ? RoadType.Intersection : RoadType.Simple;
        }

        /// <summary>
        /// Destroy a road cell.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <exception cref="System.Exception"></exception>
        public void Destroy(int x, int y)
        {
            if (roadGrid[x, y].Type == RoadType.None)
                throw new System.Exception("RoadGridManager.Destroy : cell not destroyable " + x + "; " + y);

            roadGrid[x, y].Type = RoadType.None;
            UpdateGraph();
        }

        public bool IsRoad(int x, int y) => this.roadGrid[x, y].Type != RoadType.None;

        public int GetGraphSize() => this.roadPathFindingNodes.Length;
        public int2 GetGridSize() => new(this.roadGrid.GetLength(0), this.roadGrid.GetLength(1));

        public void GetGraphNode(NativeArray<PathFindingNode> nodes)
        {
            Debug.Assert(nodes.Length == this.roadPathFindingNodes.Length);
            NativeArray<PathFindingNode>.Copy(this.roadPathFindingNodes, nodes);
        }

        public void UpdateGraph()
        {
            this.roadGridArray.Dispose();
            this.roadPathFindingNodes.Dispose();
            this.roadPathFindingNodes = new(this.roadGrid.Length, Allocator.Persistent);
            this.roadGridArray = new(this.roadGrid.Length, Allocator.Persistent);

            int width = roadGrid.GetLength(0), height = roadGrid.GetLength(1);

            RoadCell cell;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cell = roadGrid[x, y];

                    PathFindingNode node = new PathFindingNode()
                    {
                        Index = new int2(x, y),
                        Previous = new int2(-1, 0),
                        GCost = float.PositiveInfinity,
                        IsWalkable = cell.Type != RoadType.None
                    };

                    this.roadPathFindingNodes[x + y * width] = node;

                    roadGridArray[x + y * width] = cell;
                }
            }
        }

        public void Clear()
        {
            this.roadGridArray.Dispose();
            this.roadPathFindingNodes.Dispose();
            this.movementDirections.Dispose();
        }
    }

    public struct RoadCell
    {
        public RoadType Type { get; set; }
    }

    public enum RoadType
    {
        None, Simple, Intersection
    }
}