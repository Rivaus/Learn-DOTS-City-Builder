using quentin.tran.gameplay.buildingTool;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace quentin.tran.ui
{
    public class BottomBarManagerSubMenu
    {
        private VisualElement header;

        private ScrollView scrollView;

        private VisualElement root;

        private CategoryItemData cuurentCategory;

        public BottomBarManagerSubMenu(VisualElement root)
        {
            root.Q<Button>("sub-menu__close").clickable.clicked += Hide;
            this.root = root;

            this.scrollView = root.Q<ScrollView>("sub-menu__scrollview");
            this.scrollView.Clear();

            this.header = root.Q<VisualElement>("sub-menu__header");
            this.header.Clear();

            Hide();
        }

        public void Hide() => this.root.Hide();

        public void Show(IEnumerable<CategoryItemData> categories)
        {
            this.root.Show();
            this.scrollView.Clear();
            this.header.Clear();

            VisualElement elt;

            for (int i = 0; i < categories.Count(); i++)
            {
                elt = GenerateCategoryElement(categories.ElementAt(i));
                if (i == 0)
                    SelectCategory(elt);

                this.header.Add(elt);
            }
        }

        private VisualElement GenerateCategoryElement(CategoryItemData category)
        {
            Button categoryButton = new() { name = "category-button", text = category.label };
            categoryButton.AddToClassList("sub-menu__header__item");
            categoryButton.clickable.clicked += () => SelectCategory(categoryButton);
            categoryButton.userData = category;

            return categoryButton;
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

        private void SelectCategory(VisualElement selected)
        {
            CategoryItemData category = selected.userData as CategoryItemData;

            if (this.cuurentCategory == category)
                return;

            this.cuurentCategory = category;

            foreach (VisualElement item in this.header.Children())
                item.RemoveFromClassList("sub-menu__header__item--selected");

            selected.AddToClassList("sub-menu__header__item--selected");

            this.scrollView.Clear();

            VisualElement elt;

            for (int i = 0; i < category.items.Count(); i++)
            {
                elt = GenerateItemElement(category.items.ElementAt(i));
                if (i == 0)
                    Select(elt);

                this.scrollView.Add(elt);
            }
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