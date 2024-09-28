using quentin.tran.simulation;
using quentin.tran.simulation.grid;
using System.Collections.Generic;
using UnityEngine;

namespace quentin.tran.gameplay
{
    public class ManagersInitializer : MonoBehaviour
    {
        private HashSet<ISingleton> managers = new();

        void Awake()
        {
            Application.runInBackground = true;

            this.managers.Add(new InputManager());
            this.managers.Add(new GridManager());
            this.managers.Add(new StatisticsManager());
            this.managers.Add(new RoadGridManager());
            this.managers.Add(new TimeManagerMonoHandler());
        }

        private void Update()
        {
            foreach (var manager in this.managers)
            {
                if (manager is IUpdatable updatable)
                    updatable.Update(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            foreach (var manager in this.managers)
            {
                manager.Clear();
            }
        }
    }

    public interface ISingleton
    {
        void Clear();
    }

    public interface ISingleton<T> : ISingleton
    {
        static T Instance { get; }
    }

    public interface IUpdatable
    {
        void Update(float dt);
    }
}

