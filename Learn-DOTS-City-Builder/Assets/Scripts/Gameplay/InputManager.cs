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

        public static Vector2 GetCameraOrbitalMovement() => Instance.controls.CameraMap.OrbitalView.ReadValue<Vector2>();

        public static float GetCameraZoom() => Instance.controls.CameraMap.Zoom.ReadValue<float>();

        public static event Action OnClick, OnClickRelease;

        public InputManager()
        {
            Instance = this;

            this.controls = new();
            this.controls.Enable();

            this.controls.CameraMap.Click.performed += Click;
            this.controls.CameraMap.Click.canceled += ClickRelease;
        }

        private void Click(InputAction.CallbackContext context) => OnClick?.Invoke();

        private void ClickRelease(InputAction.CallbackContext context) => OnClickRelease?.Invoke();

        public void Clear()
        {
            Instance = null;

            this.controls.CameraMap.Click.performed -= Click;
        }
    }
}

