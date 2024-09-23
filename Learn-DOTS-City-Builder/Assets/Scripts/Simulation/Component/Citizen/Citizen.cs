using Unity.Collections;
using Unity.Entities;

namespace quentin.tran.simulation.component
{
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

        public CitizenActivity activity;
    }

    public struct CitizenBaby : IComponentData { }

    public struct CitizenChild : IComponentData { }

    public struct CitizenTeenager : IComponentData { }

    public struct CitizenAdult : IComponentData { }

    public struct CitizenSenior : IComponentData { }

    public enum CitizenGender
    {
        Male, Female
    }

    public enum CitizenActivity
    {
        AtHome, AtOffice, AtSchool, AtEntertainment
    }
}