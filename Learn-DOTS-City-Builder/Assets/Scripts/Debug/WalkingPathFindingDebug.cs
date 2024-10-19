using quentin.tran.common;
using quentin.tran.simulation.component;
using quentin.tran.simulation.grid;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace quentin.tran.debug
{
    public class WalkingPathFindingDebug : MonoBehaviour
    {
        HashSet<GameObject> debugSpheres = new();

        [ContextMenu("Display Network")]
        private void DisplayNetwork()
        {
            foreach(GameObject go in this.debugSpheres)
            {
                GameObject.Destroy(go);
            }

            using var nodes = WalkingNetworkManager.Instance.Nodes.GetValueArray(Allocator.Temp);

            foreach (PathFindingNode node in nodes)
            {
                DrawDebug(node.position);
            }
        }

        public Vector2 start, end;

        [ContextMenu("PathFinding")]
        private void PathFinding()
        {
            foreach (GameObject go in this.debugSpheres)
            {
                GameObject.Destroy(go);
            }

            Entity debugEntity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity();
            DynamicBuffer<Waypoint> result = World.DefaultGameObjectInjectionWorld.EntityManager.AddBuffer<Waypoint>(debugEntity);

            PathFindingJob job = new()
            {
                baseNodes = WalkingNetworkManager.Instance.Nodes,
                startPosition = new float3(start.x, 0, start.y) * GridProperties.GRID_CELL_SIZE + new float3(1, 0, 1) * GridProperties.GRID_CELL_SIZE/2f,
                endPosition = new float3(end.x, 0, end.y) * GridProperties.GRID_CELL_SIZE + new float3(1, 0, 1) * GridProperties.GRID_CELL_SIZE / 2f,
                result = result
            };
            job.Schedule().Complete();

            Debug.Log("Result " + result.Length);
            for (int i = 0; i < result.Length; i++)
                DrawDebug(result[i].position);

            World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(debugEntity);
        }

        private void DrawDebug(Vector3 position)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.SetParent(this.transform);
            go.transform.localScale = Vector3.one * .2f;
            go.transform.position = position;

            debugSpheres.Add(go);
        }
    }
}

