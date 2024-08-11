using quentin.tran.authoring;
using quentin.tran.gameplay;
using Unity.Entities;

namespace quentin.tran.simulation
{
    public class TimeManagerMonoHandler: ISingleton<TimeManagerMonoHandler>, IUpdatable
    {
        public static TimeManagerMonoHandler Instance { get; private set; }

        public static TimeManager time;

        private EntityQuery timeManagerEntityQuery;

        public TimeManagerMonoHandler()
        {
            this.timeManagerEntityQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(TimeManager));
        }

        public void Clear()
        {
            this.timeManagerEntityQuery.Dispose();
        }

        public void Update(float dt)
        {
            time = this.timeManagerEntityQuery.GetSingleton<TimeManager>();
        }
    }
}
