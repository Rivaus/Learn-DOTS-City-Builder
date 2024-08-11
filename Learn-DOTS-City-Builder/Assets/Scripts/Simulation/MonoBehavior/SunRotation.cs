using quentin.tran.authoring;
using UnityEngine;

namespace quentin.tran.simulation.monobehavior
{
    public class SunRotation : MonoBehaviour
    {
        void Update()
        {
            TimeManager time = TimeManagerMonoHandler.time;

            float angle = -90 + (360 / 24) * (time.dateTime.Hour + time.dateTime.Minute / 60f);

            this.transform.localRotation = Quaternion.Euler(angle, -30, 0);
        }
    }
}
