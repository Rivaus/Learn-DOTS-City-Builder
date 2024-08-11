using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace quentin.tran.authoring.citizen
{
    public class CitizenAuthoring : MonoBehaviour
    {
        private class Baker : Baker<CitizenAuthoring>
        {
            public override void Bake(CitizenAuthoring authoring)
            {
                Entity e = GetEntity(TransformUsageFlags.Dynamic);
            }
        }
    }

    public struct Citizen : IComponentData
    {
        public int age;

        public CitizenGender gender;

        public FixedString32Bytes name;

        public Entity house;

        /// <summary>
        /// In range 0:100
        /// </summary>
        public float happiness;
    }

    public enum CitizenGender
    {
        Male, Female
    }
}

