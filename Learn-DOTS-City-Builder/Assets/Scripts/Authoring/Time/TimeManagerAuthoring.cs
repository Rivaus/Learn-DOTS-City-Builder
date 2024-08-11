using System;
using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring
{
    public class TimeManagerAuthoring : MonoBehaviour
    {
        public float timeScale = 1.0f;

        private class Baker : Baker<TimeManagerAuthoring>
        {
            public override void Bake(TimeManagerAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, new TimeManager()
                {
                    dateTime = new DateTime(2024, 6, 1, 11, 0, 0),
                    timeScale = authoring.timeScale,
                });
            }
        }
    }

    /// <summary>
    /// Contains all data about current game time.
    /// </summary>
    public struct TimeManager : IComponentData
    {
        /// <summary>
        /// Current time.
        /// </summary>
        public DateTime dateTime;

        /// <summary>
        /// TimeScale equals to 0 means 1 second = 1 minute
        /// </summary>
        public float timeScale;
    }
}