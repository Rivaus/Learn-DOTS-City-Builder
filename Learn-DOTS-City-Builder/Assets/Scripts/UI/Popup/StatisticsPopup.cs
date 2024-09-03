using quentin.tran.simulation.system.citizen;
using quentin.tran.ui.customElements;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui.popup
{
    [UxmlElement]
    public partial class StatisticsPopup : VisualElement
    {
        private const string SECTION_HEADER_CLASS = "popup__section__header";

        private TabButtonElement citizenTabButton, jobTabButton, educationTabButton, healthTabButton, entertainmentTabButton, budgetTabButton;

        private VisualElement mainSection;

        public StatisticsPopup()
        {
            VisualTreeAsset template = Resources.Load("statistics-popup") as VisualTreeAsset;
            Debug.Assert(template is not null, "Popup template not found");

            this.Add(template.Instantiate());

            this.mainSection = this.Query<VisualElement>("main-section");
            Debug.Assert(mainSection is not null);

            this.Q<Button>("close-button").clickable.clicked += () => this.Hide();

            // Create tabs
            List<TabButtonElement> tabs = new();

            this.citizenTabButton = this.Q<TabButtonElement>("citizen-tab-button");
            this.citizenTabButton.userData = new PopulationSection();
            this.citizenTabButton.onClick += () => DisplaySection(this.citizenTabButton);
            tabs.Add(this.citizenTabButton);

            this.jobTabButton = this.Q<TabButtonElement>("job-tab-button");
            this.jobTabButton.userData = null;
            this.jobTabButton.onClick += () => DisplaySection(this.jobTabButton);
            tabs.Add(this.jobTabButton);

            this.educationTabButton = this.Q<TabButtonElement>("education-tab-button");
            this.educationTabButton.userData = null;
            this.educationTabButton.onClick += () => DisplaySection(this.educationTabButton);
            tabs.Add(this.educationTabButton);

            this.healthTabButton = this.Q<TabButtonElement>("health-tab-button");
            this.healthTabButton.userData = null;
            this.healthTabButton.onClick += () => DisplaySection(this.healthTabButton);
            tabs.Add(this.healthTabButton);

            this.entertainmentTabButton = this.Q<TabButtonElement>("entertainment-tab-button");
            this.entertainmentTabButton.userData = null;
            this.entertainmentTabButton.onClick += () => DisplaySection(this.entertainmentTabButton);
            tabs.Add(this.entertainmentTabButton);

            this.budgetTabButton = this.Q<TabButtonElement>("budget-tab-button");
            this.budgetTabButton.userData = null;
            this.budgetTabButton.onClick += () => DisplaySection(this.budgetTabButton);
            tabs.Add(this.budgetTabButton);


            foreach(TabButtonElement tabButton in tabs)
            {
                tabButton.SelectClass = "popup__tab-button--selected";
                tabButton.OtherTabButtons.AddRange(tabs);
            }

            DisplaySection(this.citizenTabButton);
        }

        private void DisplaySection(TabButtonElement tabButton)
        {
            this.mainSection.Clear();

            if (tabButton.userData is IPopupSection section)
                this.mainSection.Add(section.SectionElement);
        }

        #region Sections

        private interface IPopupSection
        {
            VisualElement SectionElement { get; }
        }

        private class PopulationSection : IPopupSection
        {
            public VisualElement SectionElement { get; private set; }

            PieChartElement.PieChartCategory[] categories;

            public PopulationSection()
            {
                this.categories = new PieChartElement.PieChartCategory[]
                {
                    new PieChartElement.PieChartCategory { title = "Babies (1 - 2 yo)", count = 1},
                    new PieChartElement.PieChartCategory { title = "Children (3 - 10 yo)", count = 2},
                    new PieChartElement.PieChartCategory { title = "Teenagers (11 - 17 yo)", count = 3},
                    new PieChartElement.PieChartCategory { title = $"Adults ({ApplyToJobSystem.MIN_AGE_TO_WORK} - {ApplyToJobSystem.MAX_AGE_TO_WORK} yo)", count = 4},
                    new PieChartElement.PieChartCategory { title = $"Retired ({ApplyToJobSystem.MAX_AGE_TO_WORK + 1} -)", count = 5},
                };

                VisualElement section = new() { name = "population-section" };

                Label populationByAgeHeader = new() { name = "population-section__by-age-header", text = "Population : Age distribution" };
                populationByAgeHeader.AddToClassList(SECTION_HEADER_CLASS);
                section.Add(populationByAgeHeader);

                PieChartElement populationByAgeChart = new() { name = "population-section__by-age-chart", BorderWidth = 20 };
                populationByAgeChart.style.alignSelf = Align.Center;
                populationByAgeChart.style.width = 350;
                populationByAgeChart.style.height = 150;
                populationByAgeChart.SetCategories(this.categories);
                section.Add(populationByAgeChart);

                this.SectionElement = section;
            }
        }

        #endregion
    }
}
