using UnityEngine;

namespace quentin.tran.ui.audio
{
    public class UISoundEffect : MonoBehaviour
    {
        private static UISoundEffect instance;

        public enum SoundEffect
        {
            CreateBuilding,
            DeleteBuilding
        }

        [System.Serializable]
        public class SoundEffectEntry
        {
            public AudioClip soundClip;

            public float volume;
        }

        [SerializeField]
        private AudioSource audioSource;

        [Space]
        [SerializeField]
        public SoundEffectEntry createBuildingSoundEffect;

        [SerializeField]
        public SoundEffectEntry deleteBuildingSoundEffect;

        private void Awake()
        {
            Debug.Assert(audioSource != null);
            Debug.Assert(createBuildingSoundEffect != null);
            Debug.Assert(deleteBuildingSoundEffect != null);

            instance = this;
        }

        private void OnDestroy()
        {
            instance = null;
        }

        public static void PlaySound(SoundEffect sound)
        {
            instance.audioSource.Stop();

            SoundEffectEntry soundEffect = null;

            switch (sound)
            {
                case SoundEffect.CreateBuilding:
                    soundEffect = instance.createBuildingSoundEffect;
                    break;
                case SoundEffect.DeleteBuilding:
                    soundEffect = instance.deleteBuildingSoundEffect;
                    break;
            }

            if (soundEffect is not null)
            {
                instance.audioSource.clip = soundEffect.soundClip;
                instance.audioSource.volume = soundEffect.volume;
                instance.audioSource.Play();
            }
        }
    }
}