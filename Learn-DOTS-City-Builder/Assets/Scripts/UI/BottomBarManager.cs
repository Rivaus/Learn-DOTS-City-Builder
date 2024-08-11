
using quentin.tran.gameplay.buildingTool;
using quentin.tran.simulation;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui
{
    public class BottomBarManager
    {
        private BuildingModeButton viewModeButton, buildRoadButton, buildBuildingButton, destroyBuildingButton;

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
            this.destroyBuildingButton = root.Q<BuildingModeButton>("destroy-building-button");
            this.destroyBuildingButton.clickable.clicked += SeDestroyBuildingMode;

            this.timeLabel = root.Q<Label>("time-label");

            BuilderController.Instance.OnModeChanged += UpdateBuildingButtons;
            UpdateBuildingButtons(BuilderController.Instance.Mode);

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
            BuilderController.Instance.Mode = BuilderController.BuildingMode.Building;
        }

        private void SeDestroyBuildingMode()
        {
            BuilderController.Instance.Mode = BuilderController.BuildingMode.Delete;
        }

        private void UpdateBuildingButtons(BuilderController.BuildingMode mode)
        {
            this.viewModeButton.IsSelected = false;
            this.buildRoadButton.IsSelected = false;
            this.buildBuildingButton.IsSelected = false;
            this.destroyBuildingButton.IsSelected = false;

            switch (mode)
            {
                case BuilderController.BuildingMode.None:
                    this.viewModeButton.IsSelected = true;
                    break;
                case BuilderController.BuildingMode.Road:
                    this.buildRoadButton.IsSelected = true;
                    break;
                case BuilderController.BuildingMode.Building:
                    this.buildBuildingButton.IsSelected = true;
                    break;
                case BuilderController.BuildingMode.Delete:
                    this.destroyBuildingButton.IsSelected = true;
                    break;
                default:
                    break;
            }
        }

        public void Destroy()
        {
            this.running = false;

            BuilderController.Instance.OnModeChanged -= UpdateBuildingButtons;

            this.viewModeButton.clickable.clicked -= SetViewModeMode;
            this.buildRoadButton.clickable.clicked -= SetBuildRoadMode;
            this.buildBuildingButton.clickable.clicked -= SetBuildBuildingMode;
            this.destroyBuildingButton.clickable.clicked -= SeDestroyBuildingMode;
        }
    }
}

