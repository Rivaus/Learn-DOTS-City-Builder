using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace quentin.tran.gameplay
{
    /// <summary>
    /// Manages all inputs
    /// </summary>
    public class InputManager : ISingleton<InputManager>
    {
        public static InputManager Instance { get; private set; }

        private InputControls controls;

        public static Vector2 GetCameraMovement() => Instance.controls.CameraMap.Movement.ReadValue<Vector2>();

        public static float GetCameraZoom() => Instance.controls.CameraMap.Zoom.ReadValue<float>();

        public static event Action OnClick, OnClickRelease, OnRoadMode, OnHouseMode, OnDeleteMode, OnJobMode, OnViewMode;

        public static event Action<Vector2> OnOrbitalRotate;

        public static event Action<float> OnZoom;

        public InputManager()
        {
            Instance = this;

            this.controls = new();
            this.controls.Enable();

            this.controls.CameraMap.Click.performed += Click;
            this.controls.CameraMap.Click.canceled += ClickRelease;
            this.controls.CameraMap.OrbitalView.performed += OrbitalRotate;
            this.controls.CameraMap.Zoom.performed += Zoom;

            this.controls.BuildingMode.ViewMode.performed += ViewMode;
            this.controls.BuildingMode.CreateRoad.performed += RoadMode;
            this.controls.BuildingMode.CreateHouse.performed += HouseMode;
            this.controls.BuildingMode.CreateJob.performed += JobMode;
            this.controls.BuildingMode.DeleteBuilding.performed += DeleteMode;
        }

        private void Click(InputAction.CallbackContext context) => OnClick?.Invoke();

        private void ClickRelease(InputAction.CallbackContext context) => OnClickRelease?.Invoke();

        private void OrbitalRotate(InputAction.CallbackContext context) => OnOrbitalRotate?.Invoke(context.ReadValue<Vector2>());

        private void Zoom(InputAction.CallbackContext context) => OnZoom?.Invoke(context.ReadValue<float>());

        private void ViewMode(InputAction.CallbackContext context) => OnViewMode?.Invoke();

        private void RoadMode(InputAction.CallbackContext context) => OnRoadMode?.Invoke();

        private void HouseMode(InputAction.CallbackContext context) => OnHouseMode?.Invoke();

        private void JobMode(InputAction.CallbackContext context) => OnJobMode?.Invoke();

        private void DeleteMode(InputAction.CallbackContext context) => OnDeleteMode?.Invoke();

        public void Clear()
        {
            Instance = null;

            this.controls.CameraMap.Click.performed -= Click;
            this.controls.CameraMap.Click.canceled -= ClickRelease;
            this.controls.CameraMap.OrbitalView.performed -= OrbitalRotate;
            this.controls.CameraMap.Zoom.performed -= Zoom;

            this.controls.BuildingMode.ViewMode.performed -= ViewMode;
            this.controls.BuildingMode.CreateRoad.performed -= RoadMode;
            this.controls.BuildingMode.CreateHouse.performed -= HouseMode;
            this.controls.BuildingMode.CreateJob.performed -= JobMode;
            this.controls.BuildingMode.DeleteBuilding.performed -= DeleteMode;

            this.controls.Dispose();
        }
    }
}

