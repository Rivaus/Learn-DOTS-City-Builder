
using quentin.tran.authoring;
using quentin.tran.common;
using quentin.tran.gameplay.buildingTool;
using quentin.tran.simulation;
using quentin.tran.ui.popup;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;
using static quentin.tran.simulation.Statistics;

namespace quentin.tran.ui
{
    public class BottomBarManager
    {
        private BuildingModeButton viewModeButton, buildRoadButton, buildBuildingButton, buildOfficeButton, destroyBuildingButton;

        private Label timeLabel;

        private bool running = true;

        public BottomBarManager(VisualElement root)
        {
            this.viewModeButton = root.Q<BuildingModeButton>("view-mode-button");
            this.viewModeButton.clickable.clicked += SetViewModeMode;
            this.buildRoadButton = root.Q<BuildingModeButton>("build-road-button");
            this.buildRoadButton.clickable.clicked += SetBuildRoadMode;
            this.buildBuildingButton = root.Q<BuildingModeButton>("build-building-button");
            this.buildBuildingButton.clickable.clicked += SetBuildBuildingMode;
            this.buildOfficeButton = root.Q<BuildingModeButton>("build-office-button");
            this.buildOfficeButton.clickable.clicked += SetOfficeBuildingMode;
            this.destroyBuildingButton = root.Q<BuildingModeButton>("destroy-building-button");
            this.destroyBuildingButton.clickable.clicked += SetDestroyBuildingMode;
            var statisticsButton = root.Q<BuildingModeButton>("statistics-button");
            statisticsButton.clickable.clicked += () => PopupsManager.Instance.OpenPopup(PopupsManager.PopupType.Statistics);

            this.timeLabel = root.Q<Label>("time-label");
            root.Q<Button>("time-scale-x0").clickable.clicked += () => SetTimeScale(0);
            root.Q<Button>("time-scale-x1").clickable.clicked += () => SetTimeScale(1);
            root.Q<Button>("time-scale-x10").clickable.clicked += () => SetTimeScale(10);
            root.Q<Button>("time-scale-x50").clickable.clicked += () => SetTimeScale(50);

            BuilderController.Instance.OnModeChanged += UpdateBuildingButtons;
            UpdateBuildingButtons(BuilderController.Instance.Mode);

            Label citizensCount = root.Q<Label>("citizens-count");
            citizensCount.dataSource = StatisticsManager.Instance.Statistics.citizenStatistics;
            citizensCount.SetBinding("text", new DataBinding()
            {
                dataSourcePath = new Unity.Properties.PropertyPath(nameof(CitizenStatistics.NumberOfCitizens)),
                bindingMode = BindingMode.ToTarget
            });

            Label hoveredCell = root.Q<Label>("hovered-cell");
            hoveredCell.dataSource = StatisticsManager.Instance.Statistics;
            hoveredCell.SetBinding("text", new DataBinding()
            {
                dataSourcePath = new Unity.Properties.PropertyPath(nameof(Statistics.HoveredCell)),
                bindingMode = BindingMode.ToTarget
            });

            Update();
        }

        private async void Update()
        {
            while (this.running)
            {
                await Awaitable.WaitForSecondsAsync(.2f);

                this.timeLabel.text = TimeManagerMonoHandler.time.dateTime.ToString("dd MMMM yyyy HH:mm");
            }
        }

        private void SetViewModeMode()
        {
            BuilderController.Instance.Mode = BuilderController.BuildingMode.None;
        }

        private void SetBuildRoadMode()
        {
            BuilderController.Instance.Mode = BuilderController.BuildingMode.Road;
        }

        private void SetBuildBuildingMode()
        {
            BuilderController.Instance.CurrentBuilding = GridCellKeys.SIMPLE_HOUSE_01;
            BuilderController.Instance.CurrentBuildingCategory = models.grid.GridCellType.House;
            BuilderController.Instance.Mode = BuilderController.BuildingMode.Building;
        }

        private void SetOfficeBuildingMode()
        {
            BuilderController.Instance.CurrentBuilding = GridCellKeys.SIMPLE_JOB_OFFICE_01;
            BuilderController.Instance.CurrentBuildingCategory = models.grid.GridCellType.Office;
            BuilderController.Instance.Mode = BuilderController.BuildingMode.Building;
        }

        private void SetDestroyBuildingMode()
        {
            BuilderController.Instance.Mode = BuilderController.BuildingMode.Delete;
        }

        private void UpdateBuildingButtons(BuilderController.BuildingMode mode)
        {
            this.viewModeButton.IsSelected = false;
            this.buildRoadButton.IsSelected = false;
            this.buildBuildingButton.IsSelected = false;
            this.buildOfficeButton.IsSelected = false;
            this.destroyBuildingButton.IsSelected = false;

            uint key = BuilderController.Instance.CurrentBuilding;

            switch (mode)
            {
                case BuilderController.BuildingMode.None:
                    this.viewModeButton.IsSelected = true;
                    break;
                case BuilderController.BuildingMode.Road:
                    this.buildRoadButton.IsSelected = true;
                    break;
                case BuilderController.BuildingMode.Building:
                    if (key >= 5000 && key < 10000)
                        this.buildBuildingButton.IsSelected = true;
                    else if (key >= 10000 && key < 15000)
                        this.buildOfficeButton.IsSelected = true;
                    break;
                case BuilderController.BuildingMode.Delete:
                    this.destroyBuildingButton.IsSelected = true;
                    break;
                default:
                    break;
            }
        }

        private void SetTimeScale(int timeScale)
        {
            using EntityQuery timeManagerQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(TimeManager));
            timeManagerQuery.GetSingletonRW<TimeManager>().ValueRW.timeScale = timeScale;
        }

        public void Destroy()
        {
            this.running = false;

            BuilderController.Instance.OnModeChanged -= UpdateBuildingButtons;

            this.viewModeButton.clickable.clicked -= SetViewModeMode;
            this.buildRoadButton.clickable.clicked -= SetBuildRoadMode;
            this.buildOfficeButton.clickable.clicked -= SetOfficeBuildingMode;
            this.buildBuildingButton.clickable.clicked -= SetBuildBuildingMode;
            this.destroyBuildingButton.clickable.clicked -= SetDestroyBuildingMode;
        }
    }
}

