using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui
{
    [UxmlElement]
    public partial class BuildingModeButton : Button
    {
        private Color color = Color.white;

        [UxmlAttribute]
        public Color Color
        {
            get => this.color;
            set
            {
                this.color = value;

                if (!this.IsSelected)
                    this.style.borderBottomColor = this.style.borderTopColor = this.style.borderRightColor = this.style.borderLeftColor = this.Color;
            }
        }

        private Color selectedColor = Color.white;

        [UxmlAttribute]
        public Color SelectedColor
        {
            get => this.selectedColor;
            set
            {
                this.selectedColor = value;

                if (this.IsSelected)
                    this.style.backgroundColor = this.SelectedColor;
            }
        }

        private VectorImage icon;

        [UxmlAttribute]
        public VectorImage Icon
        {
            get => this.icon;

            set
            {
                this.icon = value;
                this.iconElement.style.backgroundImage = Background.FromVectorImage(value);
            }
        }

        private bool isSelected = false;

        [UxmlAttribute]
        public bool IsSelected
        {
            get => this.isSelected;

            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;

                    if (this.isSelected)
                    {
                        this.style.backgroundColor = this.SelectedColor;
                    }
                    else
                    {
                        this.style.backgroundColor = new Color(0, 0, 0, 0);
                    }
                }
            }
        }

        private VisualElement iconElement;

        public BuildingModeButton()
        {
            this.AddToClassList("gameplay-building-button");
            this.AddToClassList("circle-icon-button");

            this.iconElement = new VisualElement();
            Add(this.iconElement);

            this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            this.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnMouseEnter(MouseEnterEvent e)
        {
            if (!this.IsSelected)
            {
                this.iconElement.style.unityBackgroundImageTintColor = this.Color;
            }
            else
            {
                this.iconElement.style.unityBackgroundImageTintColor = Color.white;
                this.style.borderBottomColor = this.style.borderTopColor = this.style.borderRightColor = this.style.borderLeftColor = Color.white;
            }
        }

        private void OnMouseLeave(MouseLeaveEvent e)
        {
            if (!this.IsSelected)
            {
                this.iconElement.style.unityBackgroundImageTintColor = StyleKeyword.Null;
            }

            this.style.borderBottomColor = this.style.borderTopColor = this.style.borderRightColor = this.style.borderLeftColor = this.Color;
            this.iconElement.style.unityBackgroundImageTintColor = StyleKeyword.Null;
        }
    }
}