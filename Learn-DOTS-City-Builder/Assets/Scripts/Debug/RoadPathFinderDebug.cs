using quentin.tran.models.grid;
using quentin.tran.simulation.grid;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace quentin.tran.debug
{
    public class RoadPathFinderDebug : MonoBehaviour
    {
        public Vector2 start = Vector2.zero, end = Vector2.zero;

        List<GameObject> debugObjects = new();

        private void Start()
        {
            /*QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 30;*/
        }

        [ContextMenu("Pathfinding")]
        public void PathFinding()
        {
            foreach (GameObject go in debugObjects)
            {
                GameObject.Destroy(go);
            }
            debugObjects.Clear();

            int2 startIndex = new((int)start.x, (int)start.y);
            int2 endIndex = new((int)end.x, (int)end.y);

            RoadGridManager manager = RoadGridManager.Instance;

            NativeArray<PathFindingNode> nodes = new(manager.GetGraphSize(), Allocator.TempJob);
            NativeArray<RoadCell>.ReadOnly roadsArray = manager.RoadGridArray;

            Entity debugEntity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity();
            DynamicBuffer<Waypoint> result = World.DefaultGameObjectInjectionWorld.EntityManager.AddBuffer<Waypoint>(debugEntity);
            manager.GetGraphNode(nodes);

            RoadPathFindingJob job = new()
            {
                startIndex = startIndex,
                endIndex = endIndex,
                roads = roadsArray,
                gridWidth = manager.GetGridSize().x,
                directions = manager.MovementDirections,
                baseNodes = nodes,
                result = result
            };

            JobHandle handle = job.Schedule();
            handle.Complete();


            Debug.Log("Pathfinding " + (job.result.Length > 0));

            foreach (Waypoint res in job.result)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = GridUtils.GetCellCenterPosition(res.cellIndex);

                debugObjects
                    .Add(go);
            }

        }
    }
}
