using System.Threading.Tasks;
using Castrimaris.Core;
using Castrimaris.Core.Monitoring;
using Castrimaris.Network.Netcode;
using Castrimaris.ScriptableObjects;
using NetworkModes = Castrimaris.ScriptableObjects.InitializationParameters.NetworkModes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Castrimaris.Core.Utilities;
using System.Linq;
using UnityEngine.UIElements;



#if !UNITY_EDITOR && UNITY_ANDROID
    using UnityEngine.Android;
    using System.Collections.Generic;
#endif

#if UNITY_EDITOR
using ParrelSync;
#endif

namespace Castrimaris.Management {

    /// <summary>
    /// Handles the initialization process of the application
    /// </summary>
    public class InitializationManager : SingletonMonoBehaviour<InitializationManager> {

        #region PRIVATE VARIABLES

        private const string TAG = nameof(InitializationManager);

        [Header("References")]
        [SerializeField] private InitializationParameters parameters;

        private bool hasConcededPermissions = false;
        private bool isConnecting = false;
        private int checkedPermissionsCount = 0;

        #endregion

        #region PUBLIC METHODS

        public async Task Connect() {
            //Sanity Checks
            if (isConnecting)
                return;
            isConnecting = true;

            //Spawn Netcode's Systems.
            if (!parameters.SkipInstantiatingNetworkingSystemsPrefabs)
                await SpawnSystems(parameters.NetworkingSystemsPrefabs);

            //Conditional spawning of VR systems
            var isVR = Utilities.IsVrPlatform();
            if (isVR && !parameters.SkipInstantiatingVrSystemsPrefabs)
                await SpawnSystems(parameters.VRSystemsPrefabs); //Interactions systems are needed only if the current player is a VR one

            //Setup connection data
            SetupConnectionDefaultConfiguration();
            ParseOptionalCommandLineData();

            //Start network
            StartNetwork();

            //If it's server, start additional systems server-side
            await WaitForNetworkManager(); //We're gonna need the NetworkManager initialized for next steps.
            var isServer = NetworkManager.Singleton.IsServer;
            if (isServer && !parameters.SkipInstantiatingServerSystemsPrefabs)
                await SpawnNetworkSystems(parameters.ServerSystemsPrefabs);
        }
        #endregion

        #region UNITY OVERRIDES

        protected override void Awake() {
            base.Awake();

            if (parameters.NetworkingSystemsPrefabs.Length <= 0)
                Log.W($"Missing references for {nameof(parameters.NetworkingSystemsPrefabs)}! Please, be sure to add it in the Editor!", Use3DDebug: true);
        }

        private async void Start() {
            SetupLogLevel();
            await CheckAndroidPermissions();
            ForceClientModeOnAndroidPlatform();
            ForceClientModeOnStandalonePlatform();
            ForceServerModeOnLinuxPlatform();
            InitializeParrelSyncInstance();
            InitializeAdditionalSettings();
            await InitializeServer(); //Server gets initialized right away; clients use a lazy initialization to accomodate the introductory scene.
            LoadNextScene();
        }

        #endregion

        #region PRIVATE METHODS
        private void InitializeAdditionalSettings() {
            //TODO if platform == Oculus
            OVRPlugin.foveatedRenderingLevel = OVRPlugin.FoveatedRenderingLevel.HighTop;
        }

        private async Task SpawnSystems(GameObject[] Systems) {
            foreach (var system in Systems) {
                var gameObject = GameObject.Instantiate(system);
                gameObject.name = gameObject.name.Replace("(Clone)", "").Trim(); //We remove the (Clone) at the end of the name because it's ugly as all hell
            }
            await Task.CompletedTask;
        }

        private async Task SpawnNetworkSystems(GameObject[] Systems) {
            foreach (var system in Systems) {
                var tmp = GameObject.Instantiate(system);
                tmp.GetComponent<NetworkObject>().Spawn();
            }
            await Task.CompletedTask;
        }
        /// <summary>
        /// Initialize the network components if 
        /// </summary>
        private async Task InitializeServer() {
            switch (parameters.NetworkMode) {
                case NetworkModes.SERVER:
                case NetworkModes.SERVER_LOCAL:
                case NetworkModes.SERVER_LOOPBACK:
                case NetworkModes.HOST:
                    await Connect();
                    break;
            }
        }

        /// <summary>
        /// Starts the application either as Server, Client or Host based on the NetworkMode specified in the class properties.
        /// </summary>
        private void StartNetwork() {
            switch (parameters.NetworkMode) {
                case NetworkModes.SERVER_LOOPBACK:
                case NetworkModes.SERVER_LOCAL:
                case NetworkModes.SERVER:
                    Log.D("Starting as Server", Use3DDebug: true);
                    NetworkManager.Singleton.StartServer();
                    break;
                case NetworkModes.CLIENT_LOOPBACK:
                case NetworkModes.CLIENT_LOCAL:
                case NetworkModes.CLIENT:
                    Log.D("Starting as Client", Use3DDebug: true);
                    NetworkManager.Singleton.StartClient();
                    break;
                case NetworkModes.HOST:
                    Log.D("Starting as Host", Use3DDebug: true);
                    NetworkManager.Singleton.StartHost();
                    break;
                default:
                    Log.E("No such Network Mode!", Use3DDebug: true);
                    return;
            }
        }

        /// <summary>
        /// Parses optional commands if the application is executed in command line. Standard Unity parameters will be ignored.
        /// </summary>
        private void ParseOptionalCommandLineData() {
            if (CommandLineParser.ParseCommands(out var data)) {
                Log.D("Detected command line arguments. Applying values.", Use3DDebug: true);

                NetworkManager.Singleton.TryGetComponent<Castrimaris.Network.ITransport>(out var transport);

                if (transport == null) {
                    Log.W($"Tried to get ITransport from NetworkManager, but none is attached! Using Editor defined data for connection.", Use3DDebug: true);
                    return;
                }

                if (data.Address != null) {
                    transport.SetAddress(data.Address);
                }
                if (data.ServerListenAddress != null) {
                    transport.SetServerListenAddress(data.ServerListenAddress);
                }
                if (data.Port != null) {
                    transport.SetPort(data.Port.Value);
                }
            }
        }

        /// <summary>
        /// If the running platform is Android and not Editor, it will force CLIENT NetworkMode, unless overridden by forceModeHostQuest flag
        /// </summary>
        private void ForceClientModeOnAndroidPlatform() {
#if UNITY_ANDROID && !UNITY_EDITOR
            Log.D("Android platform detected, forcing client mode.");
            switch (parameters.NetworkMode) {
                case NetworkModes.SERVER:
                    parameters.ForceNetworkMode(NetworkModes.CLIENT);
                    break;
                case NetworkModes.HOST:
                case NetworkModes.SERVER_LOCAL:
                    parameters.ForceNetworkMode(NetworkModes.CLIENT_LOCAL);
                    break;
                case NetworkModes.SERVER_LOOPBACK:
                    parameters.ForceNetworkMode(NetworkModes.CLIENT_LOOPBACK);
                    break;
                case NetworkModes.CLIENT:
                case NetworkModes.CLIENT_LOCAL:
                case NetworkModes.CLIENT_LOOPBACK:
                    break;
                default:
                    throw new System.ArgumentException("No such network mode!");
            }
#endif
        }

        private void ForceClientModeOnStandalonePlatform() {
#if !UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX)
            Log.D($"Standalone build detected, forcing client mode");
            switch (parameters.NetworkMode) {
                case NetworkModes.SERVER:
                    parameters.ForceNetworkMode(NetworkModes.CLIENT);
                    break;
                case NetworkModes.HOST:
                case NetworkModes.SERVER_LOCAL:
                    parameters.ForceNetworkMode(NetworkModes.CLIENT_LOCAL);
                    break;
                case NetworkModes.SERVER_LOOPBACK:
                    parameters.ForceNetworkMode(NetworkModes.CLIENT_LOOPBACK);
                    break;
                case NetworkModes.CLIENT:
                case NetworkModes.CLIENT_LOCAL:
                case NetworkModes.CLIENT_LOOPBACK:
                    break;
                default:
                    throw new System.ArgumentException("No such network mode!");
            }
#endif
        }

        /// <summary>
        /// When using ParrelSync, this will setup the <see cref="NetworkModes">Network Mode</see> based on the arguments passed in it.
        /// Arguments can be client, client local, server, server local. Any not known argument will default to client local.
        /// </summary>
        private void InitializeParrelSyncInstance() {
#if UNITY_EDITOR
            if (!ClonesManager.IsClone()) {
                Log.D($"This is NOT a clone instance.");
                return;
            }

            Log.I($"Parrel Sync instance detected. Setting up connection mode...");

            var argument = ClonesManager.GetArgument().ToLower();
            var arguments = argument.Split(' ').Distinct().ToHashSet();

            //Client
            if (arguments.Count == 1 && arguments.Contains("client"))
                parameters.ForceNetworkMode(NetworkModes.CLIENT);

            //Server
            else if (arguments.Count == 1 && arguments.Contains("server"))
                parameters.ForceNetworkMode(NetworkModes.SERVER);

            //Local Client
            else if (arguments.Contains("local") && arguments.Contains("client"))
                parameters.ForceNetworkMode(NetworkModes.CLIENT_LOCAL);

            //Local Server
            else if (arguments.Contains("local") && arguments.Contains("server"))
                parameters.ForceNetworkMode(NetworkModes.SERVER_LOCAL);

            //Loopback Client
            else if (arguments.Contains("loopback") && arguments.Contains("client"))
                parameters.ForceNetworkMode(NetworkModes.CLIENT_LOOPBACK);

            //Loopback Server
            else if (arguments.Contains("loopback") && arguments.Contains("server"))
                parameters.ForceNetworkMode(NetworkModes.SERVER_LOOPBACK);

            //Default behaviour
            else {
                Log.E($"Fatal error while trying to setup Parrel Sync clone instace: no known argument found. Applying default mode CLIENT_LOCAL");
                parameters.ForceNetworkMode(NetworkModes.CLIENT_LOCAL);
            }
#endif
        }

        /// <summary>
        /// If the running platform is Linux and not editor, it will force SERVER NetworkMode
        /// </summary>
        private void ForceServerModeOnLinuxPlatform() {
#if UNITY_SERVER && !UNITY_EDITOR
            Log.D("Linux platform detected, forcing server mode.", Use3DDebug: true);
            parameters.ForceNetworkMode(NetworkModes.SERVER);
#endif
        }

        /// <summary>
        /// Callback for the permissions of android, to check if all permissions were requested.
        /// </summary>
        /// <param name="PermissionName"></param>
        private void PermissionResponseCallback(string PermissionName) {
            checkedPermissionsCount++;
            hasConcededPermissions = (checkedPermissionsCount >= parameters.AndroidPermissions.Count) ? true : false;
        }

        /// <summary>
        /// Android only function to request permissions at runtime
        /// </summary>
        /// <returns></returns>
        private async Task CheckAndroidPermissions() {
#if UNITY_ANDROID && !UNITY_EDITOR
            //Setup callbacks
            PermissionCallbacks permissionCallbacks = new PermissionCallbacks();
                permissionCallbacks.PermissionGranted += PermissionResponseCallback;
                permissionCallbacks.PermissionDenied += PermissionResponseCallback;
                permissionCallbacks.PermissionDeniedAndDontAskAgain += PermissionResponseCallback;

                //Enumerate required permissions
                List<string> requiredPermissions = new List<string>();
                foreach (string permission in parameters.AndroidPermissions) {
                    if (!Permission.HasUserAuthorizedPermission(permission)) {
                        requiredPermissions.Add(permission);
                    }
                }

                //Request permissions, if necessary
                if (requiredPermissions.Count > 0) {
                    Permission.RequestUserPermissions(requiredPermissions.ToArray(), permissionCallbacks);
                } else {
                    hasConcededPermissions = true;
                }

                //Wait for permissions concession before moving on
                if (!hasConcededPermissions) {
                    await Task.Delay(1000);
                }
#elif UNITY_EDITOR
            await Task.CompletedTask;
#endif
        }

        /// <summary>
        /// Wait for the NetworkManager singleton to be initialized
        /// </summary>
        private static async Task WaitForNetworkManager() {
            while (NetworkManager.Singleton == null)
                await Task.Delay(100);
        }

        /// <summary>
        /// Handles scene loading.
        /// </summary>
        private void LoadNextScene() {
            switch (parameters.NetworkMode) {
                case NetworkModes.SERVER_LOOPBACK:
                case NetworkModes.SERVER_LOCAL:
                case NetworkModes.SERVER:
                case NetworkModes.HOST:
                    if (parameters.ServerTargetSceneName == null || parameters.ServerTargetSceneName == string.Empty) {
                        Log.E($"Skipping scene loading for this server because scene name was not defined!", Use3DDebug: true);
                    } else {
                        NetworkManager.Singleton.SceneManager.LoadScene(parameters.ServerTargetSceneName, LoadSceneMode.Single);
                    }
                    break;
                case NetworkModes.CLIENT:
                case NetworkModes.CLIENT_LOOPBACK:
                case NetworkModes.CLIENT_LOCAL:
                    if (parameters.AutoConnectClient) {
                        Connect();
                        return;
                    }
                    if (parameters.ClientTargetSceneName == null || parameters.ClientTargetSceneName == string.Empty) {
                        Log.W($"Skipping scene loading for this client because scene name was not defined.", Use3DDebug: true);
                    } else {
                        SceneManager.LoadScene(parameters.ClientTargetSceneName);
                    }
                    break;
                default:
                    Log.E($"Error: no such case for {nameof(NetworkModes)}!", Use3DDebug: true);
                    break;
            }
        }

        /// <summary>
        /// Setups default connection parameters to connect either to the remote AWS machine or 
        /// </summary>
        private void SetupConnectionDefaultConfiguration() {
            NetworkManager.Singleton.TryGetComponent<Castrimaris.Network.ITransport>(out var transport);

            if (transport == null) {
                Log.W("Tried to get ITransport from NetworkManager, but none is attached! Using Editor defined data for connection configuration.", Use3DDebug: true);
                return;
            }

            switch (parameters.NetworkMode) {
                case NetworkModes.CLIENT:
                case NetworkModes.SERVER:
                    Log.D("Using remote server configuration.", Use3DDebug: true);
                    transport.SetConnectionData(parameters.AwsServerAddress, parameters.AwsServerPort, "0.0.0.0");
                    break;
                case NetworkModes.CLIENT_LOCAL:
                case NetworkModes.SERVER_LOCAL:
                case NetworkModes.HOST:
                    Log.D("Using local server configuration.", Use3DDebug: true);
                    transport.SetConnectionData(parameters.LocalServerAddress, parameters.LocalServerPort, "0.0.0.0");
                    break;
                case NetworkModes.SERVER_LOOPBACK:
                case NetworkModes.CLIENT_LOOPBACK:
                    Log.D("Using loopback server configuration", Use3DDebug: true);
                    transport.SetConnectionData(parameters.LocalHostServerAddress, parameters.LocalHostServerPort, "0.0.0.0");
                    break;
                default:
                    break;
            }

        }

        private void SetupLogLevel() {
            Log.LogLevel = parameters.LogLevel;
        }

        #endregion

    }
}
