using quentin.tran.simulation.component;
using quentin.tran.simulation.component.material;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace quentin.tran.simulation.monobehavior
{
    public class SunRotation : MonoBehaviour
    {
        [SerializeField, Range(0f, .5f)]
        private float snowLevel;

        [SerializeField]
        private MeshRenderer groundRenderer;

        EntityQuery buildingsQuery;

        private void Start()
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            buildingsQuery = em.CreateEntityQuery(ComponentType.ReadWrite<SnowLevel>());
        }

        void Update()
        {
            DateTime time = TimeManagerMonoHandler.time.dateTime;

            float angle = -90 + (360 / 24) * (time.Hour + time.Minute / 60f);

            this.transform.localRotation = Quaternion.Euler(angle, -30, 0);

            using NativeArray<Entity> entities = this.buildingsQuery.ToEntityArray(Allocator.Temp);
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityCommandBuffer cmd = new(Allocator.Temp);

            foreach (Entity e in entities)
            {
                cmd.SetComponent(e, new SnowLevel() { level = snowLevel });
            }
            cmd.Playback(em);
            cmd.Dispose();

            groundRenderer.material.SetFloat("_SnowLevel", snowLevel);
        }
    }
}
