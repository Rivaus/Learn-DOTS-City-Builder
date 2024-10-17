using quentin.tran.authoring.weather;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace quentin.tran.simulation.monobehavior
{
    public class SunRotation : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer groundRenderer;

        void Update()
        {
            DateTime time = TimeManagerMonoHandler.time.dateTime;

            float angle = -90 + (360 / 24) * (time.Hour + time.Minute / 60f);

            this.transform.localRotation = Quaternion.Euler(angle, -30, 0);
        }

        [ContextMenu("Snow for 3 days")]
        private void Snow()
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = manager.CreateEntityQuery(typeof(Weather));

            if (query.TryGetSingletonRW<Weather>(out RefRW<Weather> weather))
            {
                weather.ValueRW.daysOfRain = 3;
            }
        }
    }
}
