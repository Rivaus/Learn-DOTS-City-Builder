using quentin.tran.models.grid;
using UnityEngine;

namespace quentin.tran.ui
{
    [CreateAssetMenu(fileName = "New Building Item", menuName = "City Builder/Building Tool")]
    public class BuildingItemData : ScriptableObject
    {
        public Texture2D icon;

        public string title;

        public uint key;

        public GridCellType type;
    }
}