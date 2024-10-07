using Unity.Entities;
using Unity.Rendering;

namespace quentin.tran.simulation.component.material
{
    [MaterialProperty("_Mode")]
    public struct SelectionMode : IComponentData
    {
        /// <summary>
        /// 0 = Default
        /// 1 = Error
        /// 2 = Selected
        /// </summary>
        public float mode;
    }
}