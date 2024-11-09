using Castrimaris.Attributes;
using Castrimaris.Core.Monitoring;
using System.Collections.Generic;
using UnityEngine;

namespace Castrimaris.ScriptableObjects {

    [CreateAssetMenu(fileName = "Initialization Parameters", menuName = "Castrimaris/ScriptableObjects/Initialization Parameters")]
    public class InitializationParameters : ScriptableObject {
        #region ENUMs
        public enum NetworkModes {
            SERVER,
            CLIENT,
            HOST,
            SERVER_LOCAL,
            CLIENT_LOCAL,
            SERVER_LOOPBACK,
            CLIENT_LOOPBACK
        }

        #endregion

        #region PARAMETERS

        [Header("Parameters")]
        [Tooltip("The Network configuration of the machine running the app.\n" +
            "SERVER and SERVER_LOCAL run the application as server mode but with different configurations for the server (as in: listening address, IP, etc)\n" +
            "CLIENT and CLIENT_LOCAL do the same thing but for the client mode.\n" +
            "HOST is for testing only, as it won't be used on Production.")]
        [SerializeField] private NetworkModes networkMode = NetworkModes.SERVER;
        [Tooltip("Connects the clients to the server as soon as the application starts. Useful for debugging purposes.")]
        [SerializeField] private bool forceClientConnection = false;

        [Header("AWS Connection Parameters")]
        [Tooltip("Server address used in the SERVER and CLIENT NetworkMode")]
        [ReadOnly, SerializeField] private string awsServerAddress = "15.160.214.24";
        [Tooltip("Server port used in the SERVER and CLIENT NetworkMode")]
        [ReadOnly, SerializeField] private ushort awsServerPort = 8200;

        [Header("Local Connection Parameters")]
        [Tooltip("Server address used in the SERVER_LOCAL and CLIENT_LOCAL NetworkMode")]
        [SerializeField] private string localServerAddress = "95.241.153.137";
        [Tooltip("Server port used in the SERVER_LOCAL and CLIENT_LOCAL NetworkMode")]
        [SerializeField] private ushort localServerPort = 7777;

        [Header("LocalHost Connection Parameters")]
        [Tooltip("Server address used in the SERVER_LOCALHOST and CLIENT_LOCALHOST NetworkMode (loopback address)")]
        [ReadOnly, SerializeField] private string localHostServerAddress = "127.0.0.1";
        [Tooltip("Server port used in the SERVER_LOCALHOST and CLIENT_LOCALHOST NetworkMode")]
        [ReadOnly, SerializeField] private ushort localHostServerPort = 7777;

        [Header("Log Parameters")]
        [Tooltip("Sets the log level of Monitoring.Log")]
        [SerializeField] private Core.Monitoring.LogLevel logLevel = Core.Monitoring.LogLevel.DEBUG;
        [Header("Constants")]
        [Tooltip("Required permissions on Android, to use devices such as microphones.")]
        [ReadOnly, SerializeField]
        private List<string> androidPermissions = new List<string>() {
            "android.permission.RECORD_AUDIO",
            "android.permission.BLUETOOTH_CONNECT",
            "android.permission.READ_EXTERNAL_STORAGE",
            "android.permission.WRITE_EXTERNAL_STORAGE"
            };

        [Header("Prefab References")]
        [Tooltip("Debugging flag for skipping the instantiating of the systems defined below")]
        [SerializeField] private bool skipInstantiatingNetworkingSystemsPrefabs = false;
        [Tooltip("Prefabs containing necessary components for Networking behaviours")]
        [SerializeField] private GameObject[] networkingSystemsPrefabs = null;

        [Tooltip("Debugging flag for skipping the instantiating of the systems defined below")]
        [SerializeField] private bool skipInstantiatingVrSystemsPrefabs = false;
        [Tooltip("Prefabs containing necessary components for working with interaction systems, such as Hurricane, AutoHand, etc.")]
        [SerializeField] private GameObject[] vrSystemsPrefabs = null;

        [Tooltip("Debugging flag for skipping the instantiating of the systems defined below")]
        [SerializeField] private bool skipInstantiatingServerSystemsPrefabs = false;
        [Tooltip("Prefabs containing necessary components for systems that need to be handled server side")]
        [SerializeField] private GameObject[] serverSystemsPrefabs = null;

        [Header("Hidden Parameters")] //HACK: it is necessary to have serialized variables, otherwise popup won't save scene names!
        [ReadOnly, SerializeField, HideInInspector] private string clientTargetSceneName = string.Empty;
        [ReadOnly, SerializeField, HideInInspector] private string serverTargetSceneName = string.Empty;

        #endregion

        #region PROPERTIES

        public NetworkModes NetworkMode =>  networkMode;
        public bool AutoConnectClient => forceClientConnection;
        public string AwsServerAddress => awsServerAddress; 
        public ushort AwsServerPort => awsServerPort; 
        public string LocalServerAddress => localServerAddress;
        public ushort LocalServerPort => localServerPort;
        public string LocalHostServerAddress => localHostServerAddress;
        public ushort LocalHostServerPort => localHostServerPort;
        public Core.Monitoring.LogLevel LogLevel => logLevel;
        public List<string> AndroidPermissions => androidPermissions;
        public GameObject[] NetworkingSystemsPrefabs => networkingSystemsPrefabs;
        public GameObject[] VRSystemsPrefabs =>  vrSystemsPrefabs;
        public GameObject[] ServerSystemsPrefabs => serverSystemsPrefabs;
        public string ClientTargetSceneName => clientTargetSceneName;
        public string ServerTargetSceneName => serverTargetSceneName;
        public bool SkipInstantiatingNetworkingSystemsPrefabs => skipInstantiatingNetworkingSystemsPrefabs;
        public bool SkipInstantiatingVrSystemsPrefabs => skipInstantiatingVrSystemsPrefabs ;
        public bool SkipInstantiatingServerSystemsPrefabs => skipInstantiatingServerSystemsPrefabs ;
        #endregion

        #region PUBLIC METHODS

        public void ForceNetworkMode(NetworkModes networkMode) {
            Log.D($"Forcing {nameof(networkMode)} {networkMode}");
            this.networkMode = networkMode;
        }

        #endregion

    }

}
