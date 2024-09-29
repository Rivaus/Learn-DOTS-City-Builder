using quentin.tran.gameplay.buildingTool;
using quentin.tran.simulation.system.grid;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace quentin.tran.gameplay.camera
{
    [UpdateBefore(typeof(CreateBuildingSystem))]
    partial struct CameraSelectEntitiesSystem : ISystem
    {
        private static bool select;

        public void OnCreate(ref SystemState state)
        {
            InputManager.OnClick += Select;
        }

        private void Select() => select = true;

        public void OnUpdate(ref SystemState state)
        {
            if (BuilderController.Instance.Mode != BuilderController.BuildingMode.None)
                return;

            if (select)
            {

                UnityEngine.Ray rayFromCamera = BuilderController.Instance.RayFromCamera();

                Entity selected = Raycast(rayFromCamera.origin, rayFromCamera.origin + rayFromCamera.direction * 50);

                select = false;
            }
        }

        public Entity Raycast(float3 from, float3 to)
        {
            EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

            using EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
            var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            RaycastInput input = new RaycastInput()
            {
                Start = from,
                End = to,
                Filter = CollisionFilter.Default
            };

            RaycastHit hit = new RaycastHit();

            bool haveHit = collisionWorld.CastRay(input, out hit);

            if (haveHit)
                return hit.Entity;

            return Entity.Null;
        }

        public void OnDestroy(ref SystemState state)
        {
            InputManager.OnClick -= Select;
        }
    }
}

