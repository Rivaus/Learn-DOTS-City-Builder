
using quentin.tran.authoring;
using quentin.tran.gameplay;
using quentin.tran.gameplay.buildingTool;
using quentin.tran.simulation;
using quentin.tran.ui.popup;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;
using static quentin.tran.simulation.Statistics;
using static quentin.tran.ui.GameplayUIManager;

namespace quentin.tran.ui
{
    public class BottomBarManager
    {
        private List<CategoryItemData> houseItems = null;
        private List<CategoryItemData> officeItems = null;

        private BuildingModeButton viewModeButton, buildRoadButton, buildBuildingButton, buildOfficeButton, destroyBuildingButton;

        private Label timeLabel;

        private bool running = true;

        private List<Button> timeScaleButtons = new();

        private BottomBarManagerSubMenu subMenu;

        public BottomBarManager(VisualElement root, List<CategoryItemData> houseItems, List<CategoryItemData> officeItems)
        {
            this.houseItems = houseItems;
            this.officeItems = officeItems;

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

            var timeScale0 = root.Q<Button>("time-scale-x0");
            this.timeScaleButtons.Add(timeScale0);
            timeScale0.clickable.clicked += () => SetTimeScale(0, timeScale0);

            var timeScale1 = root.Q<Button>("time-scale-x1");
            this.timeScaleButtons.Add(timeScale1);
            timeScale1.clickable.clicked += () => SetTimeScale(1, timeScale1);

            var timeScale10 = root.Q<Button>("time-scale-x10");
            this.timeScaleButtons.Add(timeScale10);
            timeScale10.clickable.clicked += () => SetTimeScale(10, timeScale10);

            var timeScale30 = root.Q<Button>("time-scale-x30");
            this.timeScaleButtons.Add(timeScale30);
            timeScale30.clickable.clicked += () => SetTimeScale(30, timeScale30);


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

            subMenu = new(root.Q<VisualElement>("submenu"));

            Update();

            InputManager.OnViewMode += SetViewModeMode;
            InputManager.OnHouseMode += SetBuildBuildingMode;
            InputManager.OnRoadMode += SetBuildRoadMode;
            InputManager.OnJobMode += SetOfficeBuildingMode;
            InputManager.OnDeleteMode += SetDestroyBuildingMode;
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
            this.subMenu.Hide();
        }

        private void SetBuildRoadMode()
        {
            BuilderController.Instance.Mode = BuilderController.BuildingMode.Road;
            this.subMenu.Hide();
        }

        private void SetBuildBuildingMode()
        {
            this.subMenu.Show(this.houseItems);
        }

        private void SetOfficeBuildingMode()
        {
            this.subMenu.Show(this.officeItems);
        }

        private void SetDestroyBuildingMode()
        {
            BuilderController.Instance.Mode = BuilderController.BuildingMode.Delete;
            this.subMenu.Hide();
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

        private void SetTimeScale(int timeScale, Button selectedButton)
        {
            foreach (Button btn in this.timeScaleButtons)
                btn.RemoveFromClassList("time-scale-button--selected");

            selectedButton.AddToClassList("time-scale-button--selected");

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

            InputManager.OnViewMode -= SetViewModeMode;
            InputManager.OnHouseMode -= SetBuildBuildingMode;
            InputManager.OnRoadMode -= SetBuildRoadMode;
            InputManager.OnJobMode -= SetOfficeBuildingMode;
            InputManager.OnDeleteMode -= SetDestroyBuildingMode;
        }
    }
}

