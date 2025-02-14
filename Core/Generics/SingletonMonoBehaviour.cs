using UnityEngine;

namespace Castrimaris.Core {

    public enum SingletonReplacementPolicy { DESTROY_THIS, DESTROY_OTHER, DESTROY_THIS_GAMEOBJECT, DESTROY_OTHER_GAMEOBJECT, DO_NOTHING }

    /// <summary>
    /// Simple inheritable class to make anything a singleton quickly.
    /// </summary>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {

        [SerializeField] private SingletonReplacementPolicy replacementPolicy;
        [SerializeField] private bool dontDestroyOnLoad;
        
        private static T instance = null;

        /// <summary>
        /// The static instance of this singleton pattern. Same as calling Singleton.
        /// </summary>
        public static T Instance { get { return instance; } }

        /// <summary>
        /// The static instance of this singleton pattern. Same as calling Instance.
        /// </summary>
        public static T Singleton => Instance;

        /// <summary>
        /// Wheter this singleton has been instanced or not.
        /// </summary>
        public static bool IsInstanced { get { return instance != null; } }

        protected virtual void Awake() {
            if (Instance == null) { 
                instance = this as T;
            } else if (Instance != this) {
                switch (replacementPolicy) {
                    case SingletonReplacementPolicy.DESTROY_THIS:
                        Destroy(this);
                        break;
                    case SingletonReplacementPolicy.DESTROY_OTHER:
                        Destroy(instance);
                        instance = this as T;
                        break;
                    case SingletonReplacementPolicy.DESTROY_THIS_GAMEOBJECT:
                        Destroy(this.gameObject);
                        break;
                    case SingletonReplacementPolicy.DESTROY_OTHER_GAMEOBJECT:
                        Destroy(instance.gameObject);
                        instance = this as T;
                        break;
                    case SingletonReplacementPolicy.DO_NOTHING:
                        break;
                    default:
                        Debug.LogError($"FATAL ERROR: no such singleton replacement policy!");
                        break;
                }
            }

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(this);
        }
    }
}