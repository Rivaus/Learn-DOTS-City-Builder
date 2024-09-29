using UnityEngine;

namespace quentin.tran.gameplay.camera
{
    /// <summary>
    /// Controller to move and rotation camera.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Focus Point")]
        [SerializeField]
        private Transform focusPoint;

        [SerializeField]
        private Vector3 defaultOffset = Vector3.zero;

        [Header("Speed")]
        [SerializeField]
        private float moveSpeed = 15f;

        [SerializeField]
        private float mouseSensitivity = .35f;

        [SerializeField]
        private float scrollSensitivity = 10f;

        private void Start()
        {
            this.transform.position = this.focusPoint.transform.position + this.defaultOffset;
            this.transform.LookAt(this.focusPoint.position);
        }

        private void OnEnable()
        {
            InputManager.OnOrbitalRotate += RotateCamera;
            InputManager.OnZoom += Zoom;
        }

        private void OnDisable()
        {
            InputManager.OnOrbitalRotate -= RotateCamera;
            InputManager.OnZoom -= Zoom;
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            Vector3 offset = this.transform.position - this.focusPoint.position;
            float distanceFromFocus = offset.magnitude;

            Vector2 input = InputManager.GetCameraMovement() * Time.deltaTime * this.moveSpeed * distanceFromFocus * .15f;
            this.focusPoint.transform.position += this.transform.right * input.x + Quaternion.Euler(0, -90, 0) * this.transform.right * input.y;

            this.transform.position = this.focusPoint.position + offset;
        }

        private void RotateCamera(Vector2 orbitalInput)
        {
            this.transform.RotateAround(this.focusPoint.position, Vector3.up, this.mouseSensitivity * orbitalInput.x);
            transform.RotateAround(this.focusPoint.position, this.transform.right, -this.mouseSensitivity * orbitalInput.y);

            float angleToGround = Vector3.Angle(this.transform.forward, Quaternion.Euler(0, -90, 0) * this.transform.right);

            if (angleToGround < 10f)
                transform.RotateAround(this.focusPoint.position, this.transform.right, (10 - angleToGround));

            if (angleToGround > 90)
                transform.RotateAround(this.focusPoint.position, this.transform.right, (90 - angleToGround));

            Debug.DrawRay(this.transform.position, this.transform.forward * 10);
            Debug.DrawRay(this.transform.position, (Quaternion.Euler(0, -90, 0) * this.transform.right) * 10);
        }

        private void Zoom(float zoomInput)
        {
            Vector3 offset = this.transform.position - this.focusPoint.position;
            float distanceFromFocus = offset.magnitude;

            if (distanceFromFocus < 1 && zoomInput > 0)
                return;

            if (distanceFromFocus > 50 && zoomInput < 0)
                return;

            float maxZoom = Mathf.Abs(distanceFromFocus * 0.1f * zoomInput);

            float speedZoom = Mathf.Clamp(distanceFromFocus * zoomInput * scrollSensitivity, -maxZoom, maxZoom);
            Vector3 zoom = -offset.normalized * speedZoom;
            transform.position += zoom;
        }
    }
}

