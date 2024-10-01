using quentin.tran.gameplay.buildingTool;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace quentin.tran.ui
{
    public class BottomBarManagerSubMenu
    {
        private ScrollView scrollView;

        private VisualElement root;

        public BottomBarManagerSubMenu(VisualElement root)
        {
            root.Q<Button>("sub-menu__close").clickable.clicked += Hide;
            this.root = root;

            this.scrollView = root.Q<ScrollView>("sub-menu__scrollview");
            this.scrollView.Clear();

            Hide();
        }

        public void Hide() => this.root.Hide();

        public void Show(IEnumerable<BuildingItemData> items)
        {
            this.root.Show();
            this.scrollView.Clear();

            VisualElement elt;

            for (int i = 0; i < items.Count(); i++)
            {
                elt = GenerateItemElement(items.ElementAt(i));
                if (i == 0)
                    Select(elt);

                this.scrollView.Add(elt);
            }
        }

        private VisualElement GenerateItemElement(BuildingItemData item)
        {
            VisualElement container = new() { name = "building-item", userData = item };
            container.AddToClassList(container.name);

            VisualElement icon = new() { name = "building-item__icon" };
            icon.style.backgroundImage = Background.FromTexture2D(item.icon);
            icon.AddToClassList(icon.name);
            container.Add(icon);

            Label label = new() { name = "building-item__label", text = item.title };
            label.AddToClassList(label.name);
            container.Add(label);

            container.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == (int)MouseButton.LeftMouse)
                {
                    Select(container);
                    e.StopPropagation();
                }
            });

            return container;
        }

        private void Select(VisualElement selected)
        {
            foreach (VisualElement item in this.scrollView.Children())
            {
                item.RemoveFromClassList("building-item--selected");
            }

            selected.AddToClassList("building-item--selected");

            BuilderController.Instance.CurrentBuilding = (selected.userData as BuildingItemData).key;
            BuilderController.Instance.CurrentBuildingCategory = (selected.userData as BuildingItemData).type;
            BuilderController.Instance.Mode = BuilderController.BuildingMode.Building;
        }
    }
}