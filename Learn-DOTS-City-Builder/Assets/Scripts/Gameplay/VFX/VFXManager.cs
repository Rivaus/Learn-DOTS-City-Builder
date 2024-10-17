using UnityEngine;

namespace quentin.tran.gameplay
{
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        public GameObject rainVFX;

        private void Awake()
        {
            if (Instance != null)
                throw new System.Exception("Only one VFXManager authorized");

            Instance = this;
            Debug.Assert(rainVFX != null);
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
