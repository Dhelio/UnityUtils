using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.Core {


    /// <summary>
    /// Simple inheritable class to make anything a singleton quickly.
    /// </summary>
    public abstract class SingletonNetworkBehaviour<T> : NetworkBehaviour where T : MonoBehaviour {

        [SerializeField] private SingletonReplacementPolicy replacementPolicy;
        [SerializeField] private bool dontDestroyOnLoad = false;

        public static T Instance = null;

        protected virtual void Awake() {
            if (Instance == null)
                Instance = this as T;
            else if (Instance != this) {
                switch (replacementPolicy) {
                    case SingletonReplacementPolicy.DESTROY_THIS:
                        Destroy(this);
                        break;
                    case SingletonReplacementPolicy.DESTROY_OTHER:
                        Destroy(Instance);
                        Instance = this as T;
                        break;
                    case SingletonReplacementPolicy.DESTROY_THIS_GAMEOBJECT:
                        Destroy(this.gameObject);
                        break;
                    case SingletonReplacementPolicy.DESTROY_OTHER_GAMEOBJECT:
                        Destroy(Instance.gameObject);
                        Instance = this as T;
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