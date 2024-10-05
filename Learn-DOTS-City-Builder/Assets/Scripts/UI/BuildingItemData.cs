using quentin.tran.models.grid;
using System.Collections.Generic;
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

    [System.Serializable]
    public class CategoryItemData
    {
        public string label;

        public List<BuildingItemData> items = new();
    }
}