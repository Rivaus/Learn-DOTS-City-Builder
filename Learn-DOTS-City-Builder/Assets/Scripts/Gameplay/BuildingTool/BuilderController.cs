using quentin.tran.common;
using quentin.tran.debug;
using quentin.tran.simulation.grid;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace quentin.tran.gameplay.buildingTool
{
    public class BuilderController : MonoBehaviour
    {
        public static BuilderController Instance { get; private set; }

        #region Fields

        [SerializeField]
        private Camera cam;

        [SerializeField]
        private Transform cellGridSelectedFeedback;

        #endregion

        #region Data Fields

        /// <summary>
        /// Plane representing the grid.
        /// </summary>
        private Plane gridPlane;

        /// <summary>
        /// Current cell hovered.
        /// </summary>
        private int2 hoveredCell;

        /// <summary>
        /// Queue of current building commands.
        /// </summary>
        public Queue<IBuildingCellCommand> commands = new();

        private BuildingMode mode;

        /// <summary>
        /// Current build mode (road, building, etc)
        /// </summary>
        public BuildingMode Mode
        {
            get => mode;
            set
            {
                this.mode = value;
                OnModeChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// When <see cref="mode"/> is set to <see cref="BuildingMode.Building"/>, this key is used to determine which building to spawn.
        /// </summary>
        public uint CurrentBuilding {  get; set; }

        /// <summary>
        /// Event raised when <see cref="Mode"/>changes.
        /// </summary>
        public event Action<BuildingMode> OnModeChanged;

        /// <summary>
        /// Controller to build roads.
        /// </summary>
        private RoadBuilderController roadBuilder = new();

        /// <summary>
        /// Controller to delete roads and buildings.
        /// </summary>
        private DeleteBuilderController deleteBuilder;

        /// <summary>
        /// Controller to build buildings.
        /// </summary>
        private BuildingBuilderController buildingBuilder;

        #endregion

        #region Methods

        private void Awake()
        {
            Debug.Assert(cam != null);
            Debug.Assert(cellGridSelectedFeedback != null);

            this.gridPlane = new Plane(Vector3.up, Vector3.zero);

            InputManager.OnClick += Build;

            Instance = this;
        }

        private void Start()
        {
            this.Mode = BuildingMode.None;

            this.roadBuilder = new();
            this.buildingBuilder = new();
            this.deleteBuilder = new DeleteBuilderController(this.roadBuilder);
        }

        private void OnDestroy()
        {
            InputManager.OnClick -= Build;
        }

        private void Update()
        {
            if (this.Mode == BuildingMode.None)
                return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (gridPlane.Raycast(ray, out float enter))
            {
                Vector3 point = ray.GetPoint(enter);
                this.hoveredCell = new int2((int)(point.x / GridProperties.GRID_CELL_SIZE), (int)(point.z / GridProperties.GRID_CELL_SIZE));

                if (this.hoveredCell.x < 0 || this.hoveredCell.y < 0 || this.hoveredCell.x > GridProperties.GRID_SIZE || this.hoveredCell.y > GridProperties.GRID_SIZE)
                {
                    this.cellGridSelectedFeedback.gameObject.SetActive(false);
                    return;
                }

                this.cellGridSelectedFeedback.gameObject.SetActive(true);
                this.cellGridSelectedFeedback.position = new Vector3(this.hoveredCell.x, 0, this.hoveredCell.y) * GridProperties.GRID_CELL_SIZE;          
            }
        }

        private void Build()
        {
            if (this.Mode == BuildingMode.None || this.enabled == false)
                return;

            if (this.hoveredCell.x < 0 || this.hoveredCell.y < 0 || this.hoveredCell.x > GridProperties.GRID_SIZE || this.hoveredCell.y > GridProperties.GRID_SIZE)
                return;

            IBuilderModule builder = null;

            switch (this.Mode)
            {
                case BuildingMode.Road:
                    builder = this.roadBuilder;
                    break;
                case BuildingMode.Building:
                    this.buildingBuilder.CurrentBuildingKey = this.CurrentBuilding;
                    builder = this.buildingBuilder;
                    break;
                case BuildingMode.Delete:
                    builder = this.deleteBuilder;
                    break;
                default:
                    break;
            }

            if (builder is null)
            {
                Debug.LogError($"No builder found for { this.Mode }");
                return;
            }

            foreach(IBuildingCellCommand buildCommand in builder.Handle(this.hoveredCell.x, this.hoveredCell.y))
            {
                this.commands.Enqueue(buildCommand);
            }           
        }

        public void Enable()
        {
            this.enabled = true;
        }

        public void Disable()
        {
            this.enabled = false;
        }

        #endregion

        public interface IBuilderModule
        {
            IEnumerable<IBuildingCellCommand> Handle(int x, int y);
        }

        public enum GridDirection
        {
            Top, Right, Bottom, Left
        }

        public enum BuildingMode
        {
            None, Road, Building, Delete
        }
    }
}
