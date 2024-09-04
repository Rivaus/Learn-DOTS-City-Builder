using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace quentin.tran.ui.customElements
{
    [UxmlElement]
    public partial class TabButtonElement : VisualElement
    {
        public event Action onClick;

        private bool isSelected = false;

        public string SelectClass { get; set; }

        public List<TabButtonElement> OtherTabButtons { get; private set; } = new();

        public TabButtonElement()
        {
            this.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == (int)MouseButton.LeftMouse && e.clickCount == 1)
                {
                    Select();
                    e.StopPropagation();
                }
            });
        }

        private void Select()
        {
            if (isSelected)
                return;

            foreach (TabButtonElement tabButton in OtherTabButtons)
                tabButton.UnSelect();

            isSelected = true;
            this.AddToClassList(SelectClass);

            this.onClick?.Invoke();
        }

        private void UnSelect()
        {
            isSelected = false;

            this.RemoveFromClassList(SelectClass);
        }
    }
}


