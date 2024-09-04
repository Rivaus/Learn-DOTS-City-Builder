using System;
using UnityEngine;

namespace quentin.tran.simulation.monobehavior
{
    public class SunRotation : MonoBehaviour
    {
        void Update()
        {
            DateTime time = TimeManagerMonoHandler.time.dateTime;

            float angle = -90 + (360 / 24) * (time.Hour + time.Minute / 60f);

            this.transform.localRotation = Quaternion.Euler(angle, -30, 0);
        }
    }
}
