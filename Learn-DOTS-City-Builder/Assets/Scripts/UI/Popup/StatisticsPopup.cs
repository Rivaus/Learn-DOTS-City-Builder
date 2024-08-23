using quentin.tran.ui.customElements;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui.popup
{
    [UxmlElement]
    public partial class StatisticsPopup : VisualElement
    {
        private TabButtonElement citizenTabButton, jobTabButton, educationTabButton, healthTabButton, entertainmentTabButton, budgetTabButton;

        public StatisticsPopup()
        {
            VisualTreeAsset template = Resources.Load("statistics-popup") as VisualTreeAsset;
            Debug.Assert(template is not null, "Popup template not found");

            this.Add(template.Instantiate());

            List<TabButtonElement> tabs = new();

            this.citizenTabButton = this.Q<TabButtonElement>("citizen-tab-button");
            tabs.Add(this.citizenTabButton);
            this.jobTabButton = this.Q<TabButtonElement>("job-tab-button");
            tabs.Add(this.jobTabButton);
            this.educationTabButton = this.Q<TabButtonElement>("education-tab-button");
            tabs.Add(this.educationTabButton);
            this.healthTabButton = this.Q<TabButtonElement>("health-tab-button");
            tabs.Add(this.healthTabButton);
            this.entertainmentTabButton = this.Q<TabButtonElement>("entertainment-tab-button");
            tabs.Add(this.entertainmentTabButton);
            this.budgetTabButton = this.Q<TabButtonElement>("budget-tab-button");
            tabs.Add(this.budgetTabButton);

            foreach(TabButtonElement tabButton in tabs)
            {
                tabButton.SelectClass = "popup__tab-button--selected";
                tabButton.OtherTabButtons.AddRange(tabs);
            }
        }
    }
}
