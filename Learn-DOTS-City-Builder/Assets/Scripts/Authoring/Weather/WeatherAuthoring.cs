using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.weather
{
    public class WeatherAuthoring : MonoBehaviour
    {
        public Material material;

        private class Baker : Baker<WeatherAuthoring>
        {
            public override void Bake(WeatherAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, new Weather()
                {
                    daysOfRain = 0
                });
            }
        }
    }

    public struct Weather : IComponentData
    {
        public float daysOfRain;

        public float snowLevel;
    }
}
