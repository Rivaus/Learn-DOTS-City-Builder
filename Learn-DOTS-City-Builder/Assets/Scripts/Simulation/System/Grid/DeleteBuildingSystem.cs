using quentin.tran.gameplay.buildingTool;
using quentin.tran.simulation.component;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace quentin.tran.simulation.system.grid
{
    /// <summary>
    /// System which handles <see cref="IBuildingEntityCommand"/> commands : delete entities.
    /// </summary>
    [UpdateBefore(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(CreateBuildingSystem))]
    partial struct DeleteBuildingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GridCellComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer entityCmdBuffer = new EntityCommandBuffer(Allocator.Temp);

            Queue<DeleteBuildEntityCommand> commands = BuilderController.Instance.deleteBuildingCommands;

            while (commands.Count > 0)
            {
                Delete(commands.Dequeue(), ref state, ref entityCmdBuffer);
            }

            entityCmdBuffer.Playback(state.EntityManager);
            entityCmdBuffer.Dispose();
        }

        [BurstCompile]
        private void Delete(DeleteBuildEntityCommand deleteCmd, ref SystemState state, ref EntityCommandBuffer cmd)
        {
            // Delete specific data
            switch (deleteCmd.buildingType)
            {
                case models.grid.GridCellType.House:

                    DeleteHouse(ref state, deleteCmd.index, ref cmd);

                    break;
                case models.grid.GridCellType.Office:

                    DeleteOffice(ref state, deleteCmd.index, ref cmd);

                    break;
                case models.grid.GridCellType.School:
                    break;
                default:
                    break;
            }

            // Delete global entity
            foreach ((var gridCell, var e) in SystemAPI.Query<RefRO<GridCellComponent>>().WithEntityAccess())
            {
                if (gridCell.ValueRO.index.Equals(deleteCmd.index))
                {
                    cmd.DestroyEntity(e);
                    return;
                }
            }
        }

        [BurstCompile]
        private void DeleteHouse(ref SystemState _, int2 index, ref EntityCommandBuffer cmd)
        {
            foreach ((RefRO<GridCellComponent> cell, DynamicBuffer<LinkedEntityBuffer> houses) in SystemAPI.Query<RefRO<GridCellComponent>, DynamicBuffer<LinkedEntityBuffer>>().WithAll<HouseBuilding>())
            {
                if (!cell.ValueRO.index.Equals(index))
                    continue;

                for (int i = 0; i < houses.Length; i++)
                {
                    // Delete house inhabitants
                    DynamicBuffer<LinkedEntityBuffer> inhabitants = SystemAPI.GetBuffer<LinkedEntityBuffer>(houses[i].entity);

                    for (int j = 0; j < inhabitants.Length; j++)
                    {
                        Entity inhabitant = inhabitants[j].entity;

                        // Remove every worker from their jobs
                        if (SystemAPI.HasComponent<CitizenJob>(inhabitant))
                        {
                            foreach ((RefRW<OfficeBuilding> office, DynamicBuffer<LinkedEntityBuffer> workers) in SystemAPI.Query<RefRW<OfficeBuilding>, DynamicBuffer<LinkedEntityBuffer>>())
                            {
                                int workerFound = -1;

                                for (int k = 0; k < workers.Length; k++)
                                {
                                    if (workers[k].entity == inhabitant)
                                    {
                                        office.ValueRW.nbOfAvailableJob = math.clamp(office.ValueRO.nbOfAvailableJob + 1, 0, office.ValueRO.nbJobs); // Free a job

                                        workerFound = k;
                                        break;
                                    }
                                }

                                if (workerFound >= 0)
                                {
                                    workers.RemoveAt(workerFound);
                                    break;
                                }
                            }
                        }

                        // Remove every child from school and student from university
                        UnityEngine.Debug.Log("TODO Remove from school");

                        // Destroy inhabitant
                        cmd.DestroyEntity(inhabitant);
                    }

                    // Delete houses
                    cmd.DestroyEntity(houses[i].entity);
                }
            }
        }

        [BurstCompile]
        private void DeleteOffice(ref SystemState _, int2 index, ref EntityCommandBuffer cmd)
        {
            // 1. Delete building
            // 2. Remove job for every citizen who were working here
            foreach ((RefRO<GridCellComponent> cell, DynamicBuffer<LinkedEntityBuffer> workers) in SystemAPI.Query<RefRO<GridCellComponent>, DynamicBuffer<LinkedEntityBuffer>>().WithAll<OfficeBuilding>())
            {
                if (!cell.ValueRO.index.Equals(index))
                    continue;

                for (int i = 0; i < workers.Length; i++)
                {
                    cmd.RemoveComponent<CitizenJob>(workers[i].entity);
                }
            }
        }
    }
}
