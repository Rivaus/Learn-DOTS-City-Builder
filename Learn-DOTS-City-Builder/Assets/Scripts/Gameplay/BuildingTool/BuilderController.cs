using quentin.tran.common;
using quentin.tran.models.grid;
using quentin.tran.simulation;
using quentin.tran.simulation.grid;
using quentin.tran.ui.audio;
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
        /// Queue of current create building commands.
        /// </summary>
        public Queue<CreateBuildingEntityCommand> createBuildingCommands = new();

        /// <summary>
        /// Queue of current delete building commands.
        /// </summary>
        public Queue<DeleteBuildEntityCommand> deleteBuildingCommands = new();

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
        public uint CurrentBuilding { get; set; }

        /// <summary>
        /// Current building category.
        /// </summary>
        public GridCellType CurrentBuildingCategory { get; set; }

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

        private Statistics statistics;

        private bool isBuilding = false;

        #endregion

        #region Methods

        private void Awake()
        {
            Debug.Assert(cam != null);
            Debug.Assert(cellGridSelectedFeedback != null);

            this.gridPlane = new Plane(Vector3.up, Vector3.zero);

            InputManager.OnClick += BuildAsync;
            InputManager.OnClickRelease += StopBuilding;

            Instance = this;
        }

        private void Start()
        {
            this.Mode = BuildingMode.None;

            this.roadBuilder = new();
            this.buildingBuilder = new();
            this.deleteBuilder = new DeleteBuilderController(this.roadBuilder);

            this.statistics = StatisticsManager.Instance.Statistics;
        }

        private void OnDestroy()
        {
            InputManager.OnClick -= BuildAsync;
            InputManager.OnClickRelease -= StopBuilding;
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
                statistics.HoveredCell = this.hoveredCell;

                if (this.hoveredCell.x < 0 || this.hoveredCell.y < 0 || this.hoveredCell.x > GridProperties.GRID_SIZE || this.hoveredCell.y > GridProperties.GRID_SIZE)
                {
                    this.cellGridSelectedFeedback.gameObject.SetActive(false);
                    return;
                }

                this.cellGridSelectedFeedback.gameObject.SetActive(true);
                this.cellGridSelectedFeedback.position = new Vector3(this.hoveredCell.x, 0, this.hoveredCell.y) * GridProperties.GRID_CELL_SIZE;
            }
            else
            {
                statistics.HoveredCell = int2.zero;
            }
        }

        private async void BuildAsync()
        {
            this.isBuilding = true;

            while (this.isBuilding)
            {
                await Awaitable.EndOfFrameAsync();

                Build();
            }
        }

        private void StopBuilding()
        {
            this.isBuilding = false;
        }

        private void Build()
        {
            if (this.Mode == BuildingMode.None || this.enabled == false)
                return;

            if (this.hoveredCell.x < 0 || this.hoveredCell.y < 0 || this.hoveredCell.x > GridProperties.GRID_SIZE || this.hoveredCell.y > GridProperties.GRID_SIZE)
                return;

            if ((this.Mode is not BuildingMode.Delete) && !GridManager.Instance.IsCellBuildable(this.hoveredCell.x, this.hoveredCell.y))
                return;

            IBuilderModule builder = null;

            switch (this.Mode)
            {
                case BuildingMode.Road:
                    builder = this.roadBuilder;
                    break;
                case BuildingMode.Building:
                    this.buildingBuilder.CurrentBuildingKey = this.CurrentBuilding;
                    this.buildingBuilder.BuildingType = this.CurrentBuildingCategory;
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
                Debug.LogError($"No builder found for {this.Mode}");
                return;
            }

            bool action = false;

            foreach (IBuildingEntityCommand buildCommand in builder.Handle(this.hoveredCell.x, this.hoveredCell.y))
            {
                switch (buildCommand)
                {
                    case CreateBuildingEntityCommand createCmd:
                        this.createBuildingCommands.Enqueue(createCmd);
                        action = true;
                        break;
                    case DeleteBuildEntityCommand deleteCmd:
                        this.deleteBuildingCommands.Enqueue(deleteCmd);
                        action = true;
                        break;
                }
            }

            if (action)
            {
                if (this.Mode == BuildingMode.Delete)
                    UISoundEffect.PlaySound(UISoundEffect.SoundEffect.DeleteBuilding);
                else
                    UISoundEffect.PlaySound(UISoundEffect.SoundEffect.CreateBuilding);
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

        public Ray RayFromCamera()
        {
            return this.cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        }

        #endregion

        public interface IBuilderModule
        {
            IEnumerable<IBuildingEntityCommand> Handle(int x, int y);
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
