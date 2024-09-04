using quentin.tran.simulation;
using quentin.tran.simulation.system.citizen;
using quentin.tran.ui.customElements;
using quentin.tran.ui.manipulator;
using System;
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

            this.Q<Label>("header-label").AddManipulator(new DragAndDropManipulator(this));
            this.Q<Button>("close-button").clickable.clicked += () => this.Hide();

            // Create tabs
            List<TabButtonElement> tabs = new();

            this.citizenTabButton = this.Q<TabButtonElement>("citizen-tab-button");
            this.citizenTabButton.userData = new PopulationSection();
            this.citizenTabButton.onClick += () => DisplaySection(this.citizenTabButton);
            tabs.Add(this.citizenTabButton);

            this.jobTabButton = this.Q<TabButtonElement>("job-tab-button");
            this.jobTabButton.userData = new JobSection();
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
            {
                this.mainSection.Add(section.SectionElement);
                section.Update();
            }
        }

        #region Sections

        private interface IPopupSection
        {
            VisualElement SectionElement { get; }

            void Update();
        }

        private class PopulationSection : IPopupSection
        {
            public VisualElement SectionElement { get; private set; }

            private PieChartElement populationByAgeChart;

            private PieChartElement.PieChartCategory[] categories;

            private Statistics.CitizenStatistics previous;

            public PopulationSection()
            {
                this.categories = new PieChartElement.PieChartCategory[]
                {
                    new PieChartElement.PieChartCategory { title = "Babies (1 - 2 yo)", count = 0},
                    new PieChartElement.PieChartCategory { title = "Children (3 - 10 yo)", count = 2},
                    new PieChartElement.PieChartCategory { title = "Teenagers (11 - 17 yo)", count = 0},
                    new PieChartElement.PieChartCategory { title = $"Adults ({ApplyToJobSystem.MIN_AGE_TO_WORK} - {ApplyToJobSystem.MAX_AGE_TO_WORK} yo)", count = 5},
                    new PieChartElement.PieChartCategory { title = $"Retirees ({ApplyToJobSystem.MAX_AGE_TO_WORK + 1} -)", count = 0},
                };

                VisualElement section = new() { name = "population-section" };

                if (StatisticsManager.Instance is not null)
                    section.dataSource = StatisticsManager.Instance.Statistics;
                else
                    Debug.LogError("StatisticsManager must exist");

                // 1. Population by age
                Label populationByAgeHeader = new() { name = "population-section__by-age-header", text = "Population : Age distribution" };
                populationByAgeHeader.AddToClassList(SECTION_HEADER_CLASS);
                section.Add(populationByAgeHeader);

                this.populationByAgeChart = new() { name = "population-section__by-age-chart", BorderWidth = 20 };
                this.populationByAgeChart.style.alignSelf = Align.Center;
                this.populationByAgeChart.style.width = 350;
                this.populationByAgeChart.style.height = 150;
                this.populationByAgeChart.style.marginTop = 15;
                this.populationByAgeChart.style.marginBottom = 25;
                this.populationByAgeChart.SetCategories(this.categories);
                section.Add(this.populationByAgeChart);

                // 2. Houses
                Label housesHeader = new() { name = "population-section__houses", text = "House Building" };
                housesHeader.AddToClassList(SECTION_HEADER_CLASS);
                section.Add(housesHeader);

                TableEntryElement numberOfHouseBuildings = new() { Title = "House Buildings", Value = "0" };
                numberOfHouseBuildings.ValueLabel.SetBinding("text", new DataBinding()
                {
                    dataSourcePath = new Unity.Properties.PropertyPath(nameof(Statistics.NumberOfHouseBuildings)),
                    bindingMode = BindingMode.ToTarget
                });
                section.Add(numberOfHouseBuildings);

                TableEntryElement numberOfHouses = new() { Title = "Houses (house buildings can have several housing)", Value = "0" };
                numberOfHouses.ValueLabel.SetBinding("text", new DataBinding()
                {
                    dataSourcePath = new Unity.Properties.PropertyPath(nameof(Statistics.NumberOfHouses)),
                    bindingMode = BindingMode.ToTarget
                });
                section.Add(numberOfHouses);

                TableEntryElement emptyHomes = new() { Title = "Empty Homes", Value = "0" };
                emptyHomes.ValueLabel.SetBinding("text", new DataBinding()
                {
                    dataSourcePath = new Unity.Properties.PropertyPath(nameof(Statistics.NumberOfEmptyHouses)),
                    bindingMode = BindingMode.ToTarget
                });
                section.Add(emptyHomes);

                this.SectionElement = section;

                if (Application.isPlaying)
                    UpdateDataLoop();
            }

            /// <summary>
            /// A loop to resfresh <see cref="populationByAgeChart"/> if data has changed.
            /// </summary>
            private async void UpdateDataLoop()
            {
                while(true)
                {
                    await Awaitable.WaitForSecondsAsync(.5f);

                    Update();
                }
            }

            public void Update()
            {
                if (StatisticsManager.Instance is null)
                    return;

                try
                {
                    Statistics.CitizenStatistics stats = StatisticsManager.Instance.Statistics.citizenStatistics;

                    if (stats.Equals(previous)) 
                        return;

                    this.previous = stats.Copy();

                    this.categories[0].count = stats.NumberOfBabies;
                    this.categories[1].count = stats.NumberOfChildren;
                    this.categories[2].count = stats.NumberOfTeenagers;
                    this.categories[3].count = stats.NumberOfAdults;
                    this.categories[4].count = stats.NumberOfRetirees;

                    this.populationByAgeChart.SetCategories(this.categories);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private class JobSection : IPopupSection
        {
            public VisualElement SectionElement { get; private set; }

            public JobSection()
            {
                VisualElement section = new() { name = "job-section" };

                if (StatisticsManager.Instance is not null)
                    section.dataSource = StatisticsManager.Instance.Statistics;
                else
                    Debug.LogError("StatisticsManager must exist");

                Label officesHeader = new() { name = "population-job__buildings", text = "Job Buildings" };
                officesHeader.AddToClassList(SECTION_HEADER_CLASS);
                section.Add(officesHeader);

                TableEntryElement numberOfJobBuildings = new() { Title = "Buildings", Value = "0" };
                numberOfJobBuildings.ValueLabel.SetBinding("text", new DataBinding()
                {
                    dataSourcePath = new Unity.Properties.PropertyPath(nameof(Statistics.NumberOfJobBuildings)),
                    bindingMode = BindingMode.ToTarget
                });
                section.Add(numberOfJobBuildings);

                Label employmentHeader = new() { name = "population-job__employment", text = "Employment" };
                employmentHeader.AddToClassList(SECTION_HEADER_CLASS);
                section.Add(employmentHeader);

                TableEntryElement numberOfJobAvailable = new() { Title = "Number of available jobs", Value = "0" };
                numberOfJobAvailable.ValueLabel.SetBinding("text", new DataBinding()
                {
                    dataSourcePath = new Unity.Properties.PropertyPath(nameof(Statistics.NumberOfJobAvailable)),
                    bindingMode = BindingMode.ToTarget
                });
                section.Add(numberOfJobAvailable);

                TableEntryElement numberOfWorkers = new() { Title = "Workers", Value = "0" };
                numberOfWorkers.ValueLabel.SetBinding("text", new DataBinding()
                {
                    dataSourcePath = new Unity.Properties.PropertyPath(nameof(Statistics.NumberOfWorkers)),
                    bindingMode = BindingMode.ToTarget
                });
                section.Add(numberOfWorkers);

                TableEntryElement numberOfUnemployed = new() { Title = "Unemployed", Value = "0" };
                numberOfUnemployed.ValueLabel.SetBinding("text", new DataBinding()
                {
                    dataSourcePath = new Unity.Properties.PropertyPath(nameof(Statistics.NumberOfUnemployed)),
                    bindingMode = BindingMode.ToTarget
                });
                section.Add(numberOfUnemployed);

                this.SectionElement = section;
            }

            public void Update()
            {
            }
        }

        #endregion
    }
}
