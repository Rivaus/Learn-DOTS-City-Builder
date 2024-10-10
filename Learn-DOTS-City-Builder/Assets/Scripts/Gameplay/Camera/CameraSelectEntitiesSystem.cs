using quentin.tran.gameplay.buildingTool;
using quentin.tran.simulation.component;
using quentin.tran.simulation.system.grid;
using quentin.tran.ui;
using quentin.tran.ui.popup;
using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

namespace quentin.tran.gameplay.camera
{
    [UpdateBefore(typeof(CreateBuildingSystem))]
    partial struct CameraSelectEntitiesSystem : ISystem
    {
        private static bool select;

        private static Entity hovered;

        public void OnCreate(ref SystemState state)
        {
            InputManager.OnClick += WanToToSelect;
            InputManager.OnClickRelease += DontWanToToSelect;
        }

        private void WanToToSelect() => select = true;

        private void DontWanToToSelect() => select = false;

        public void OnUpdate(ref SystemState state)
        {
            if (BuilderController.Instance.Mode != BuilderController.BuildingMode.None || !GameplayUIManager.CursorOnGameplay)
                return;

            Hover(BuilderController.Instance.RayFromCamera(), ref state);

            if (select && hovered != Entity.Null)
            {
                Select(hovered, ref state);

                select = false;
            }
        }

        [BurstCompile]
        private void Hover(UnityEngine.Ray rayFromCamera, ref SystemState state)
        {
            Entity hoveredEntity = Raycast(rayFromCamera.origin, rayFromCamera.origin + rayFromCamera.direction * 50);

            if (hovered != hoveredEntity)
            {
                EntityCommandBuffer cmd = new(Allocator.Temp);

                if (hovered != Entity.Null)
                {
                }

                hovered = hoveredEntity;

                if (hoveredEntity != Entity.Null)
                {
                }

                cmd.Playback(state.EntityManager);
                cmd.Dispose();
            }
        }


        [BurstCompile]
        private Entity Raycast(float3 from, float3 to)
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
            InputManager.OnClick -= WanToToSelect;
            select = false;
            hovered = Entity.Null;
        }

        private void Select(Entity entity, ref SystemState state)
        {
            if (entity == Entity.Null)
                return;

            InfoPopupEntry data = null;

            if (SystemAPI.HasComponent<Citizen>(entity))
            {
                RefRO<Citizen> citizen = SystemAPI.GetComponentRO<Citizen>(entity);

                data = new InfoPopupEntry()
                {
                    Title = "Citizen",
                    Entries = new()
                    {
                        new()
                        {
                            Title = citizen.ValueRO.name.ToString(),
                            Description = GenerateCitizenDescription(citizen.ValueRO)
                        }
                    }
                };
            }
            else if (SystemAPI.HasComponent<HouseBuilding>(entity))
            {
                DynamicBuffer<LinkedEntityBuffer> houseEntities = SystemAPI.GetBuffer<LinkedEntityBuffer>(entity);
                List<InfoPopupSubEntry> entries = new();

                for (int i = 0; i < houseEntities.Length; i++)
                {
                    Entity house = houseEntities[i].entity;
                    House houseData = SystemAPI.GetComponent<House>(house);
                    DynamicBuffer<LinkedEntityBuffer> inhabitants = SystemAPI.GetBuffer<LinkedEntityBuffer>(house);

                    StringBuilder description = new();

                    for (int j = 0; j < inhabitants.Length; j++)
                    {
                        Entity inhabitant = inhabitants[j].entity;
                        Citizen c = SystemAPI.GetComponent<Citizen>(inhabitant);

                        description.AppendLine(c.name.ToString());
                        description.AppendLine(GenerateCitizenDescription(c));
                        description.AppendLine("-------------------");
                    }

                    entries.Add(new() { Title = inhabitants.Length == 0 ? $"Empty home with {houseData.capacity} places." : "Home", Description = description.ToString() });
                }

                data = new InfoPopupEntry()
                {
                    Title = "Building",
                    Entries = entries
                };
            }
            else if (SystemAPI.HasComponent<OfficeBuilding>(entity))
            {
                List<InfoPopupSubEntry> entries = new();

                OfficeBuilding office = SystemAPI.GetComponent<OfficeBuilding>(entity);
                DynamicBuffer<LinkedEntityBuffer> workerEntities = SystemAPI.GetBuffer<LinkedEntityBuffer>(entity);

                StringBuilder description = new();

                for (int j = 0; j < workerEntities.Length; j++)
                {
                    Entity worker = workerEntities[j].entity;
                    Citizen c = SystemAPI.GetComponent<Citizen>(worker);
                    CitizenJob job = SystemAPI.GetComponent<CitizenJob>(worker);

                    description.AppendLine(c.name.ToString());
                    description.AppendLine(GenerateJobDescription(job));
                    description.AppendLine("-------------------");
                }

                entries.Add(new() { Title = string.Empty, Description = description.ToString() });

                data = new InfoPopupEntry()
                {
                    Title = $"Office : number of available jobs {office.nbOfAvailableJob}",
                    Entries = entries
                };
            }

            InfoPopup.currentData = data;
            PopupsManager.Instance.OpenPopup(PopupsManager.PopupType.Info);
        }

        private string GenerateCitizenDescription(Citizen citizen)
        {
            return "Age : " + citizen.age + "\n" +
                                "Gender : " + citizen.gender + "\n" +
                                "Current activity : " + citizen.activity + "\n" +
                                "Happiness : " + citizen.happiness;
        }

        private string GenerateJobDescription(CitizenJob job)
        {
            return "Hours : " + job.startHour + " to " + job.endHour + "\n" +
                                "Salary/day : " + job.salaryPerDay + "$\n";
        }
    }
}

