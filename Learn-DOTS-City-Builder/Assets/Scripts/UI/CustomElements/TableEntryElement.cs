using UnityEngine.UIElements;

namespace quentin.tran.ui.customElements
{
    [UxmlElement]
    public partial class TableEntryElement : VisualElement
    {
        [UxmlAttribute]
        public string Title
        {
            get => this.TitleLabel.text;
            set
            {
                this.TitleLabel.text = value;
            }
        }

        [UxmlAttribute]
        public string Value
        {
            get => this.ValueLabel.text;
            set
            {
                this.ValueLabel.text = value;
            }
        }


        public Label TitleLabel { get; private set; }

        public Label ValueLabel { get; private set; }

        public TableEntryElement()
        {
            this.AddToClassList("table-entry");

            this.TitleLabel = new() { name = "table-entry__title-label" };
            this.TitleLabel.AddToClassList(this.TitleLabel.name);
            Add(this.TitleLabel);

            this.ValueLabel = new() { name = "table-entry__value-label" };
            this.ValueLabel.AddToClassList(this.ValueLabel.name);
            Add(this.ValueLabel);
        }
    }
}

