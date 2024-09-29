using quentin.tran.ui.customElements;
using quentin.tran.ui.manipulator;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui.popup
{
    [UxmlElement]
    public partial class InfoPopup : VisualElement
    {
        private VisualElement mainSection;

        private Label headerEntity;

        public static InfoPopupEntry currentData;

        public InfoPopup()
        {
            VisualTreeAsset template = Resources.Load("info-popup") as VisualTreeAsset;
            Debug.Assert(template is not null, "Popup template not found");

            this.Add(template.Instantiate());

            this.mainSection = this.Query<VisualElement>("main-section");
            Debug.Assert(mainSection is not null);

            this.Q<Label>("header-label").AddManipulator(new DragAndDropManipulator(this));
            this.Q<Button>("close-button").clickable.clicked += () => this.Hide();
            this.headerEntity = this.Q<Label>("header-entity");
        }

        public void Update()
        {
            this.mainSection.Clear();

            if (currentData == null)
                return;

            this.headerEntity.text = currentData.Title;

            foreach(InfoPopupSubEntry subEntry in currentData.Entries)
            {
                this.mainSection.Add(GenerateElement(subEntry));
            }
        }

        private VisualElement GenerateElement(InfoPopupSubEntry subEntry)
        {
            VisualElement elt = new();

            if (!string.IsNullOrWhiteSpace(subEntry.Title))
            {
                Label title = new Label() { text = subEntry.Title };
                elt.Add(title);
            }

            if (!string.IsNullOrWhiteSpace(subEntry.Description))
            {
                Label description = new Label() { text = subEntry.Description };
                elt.Add(description);
            }

            foreach (InfoPopupSubEntry e in subEntry.Entries)
            {
                elt.Add(GenerateElement(e));
            }

            return elt;
        }
    }

    public class InfoPopupEntry
    {
        public string Title { get; set; }

        public List<InfoPopupSubEntry> Entries { get; set; } = new();
    }

    public class InfoPopupSubEntry
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public List<InfoPopupSubEntry> Entries { get; set; } = new();
    }
}