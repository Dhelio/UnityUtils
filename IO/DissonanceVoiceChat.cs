using Dissonance;
using Castrimaris.IO.Contracts;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO {

    //TODO
    public class DissonanceVoiceChat : NetworkBehaviour, IDissonancePlayer, IVoiceComms {

        #region Private Variables
        [Header("References")]
        [SerializeField] private Transform targetTransform = null;

        [Header("Events")]
        [SerializeField] private UnityEvent onSpeaking = new UnityEvent();

        [Header("Debug")]
        [SerializeField, Attributes.ReadOnly] private NetworkObject rootNetworkObject;
        [SerializeField, Attributes.ReadOnly] private DissonanceComms comms;
        [SerializeField, Attributes.ReadOnly] private bool isInitialized = false;
        [SerializeField, Attributes.ReadOnly] private bool isTracking;
        [SerializeField, Attributes.ReadOnly] private NetworkVariable<FixedString128Bytes> playerId = new NetworkVariable<FixedString128Bytes>(new FixedString128Bytes(""), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        //[SerializeField, Attributes.ReadOnly] private string cachedPlayerId = "";
        [SerializeField, Attributes.ReadOnly] private bool isLocalPlayer = false;

        #endregion

        #region Properties

        public string PlayerId => playerId.Value.ToString();

        public Vector3 Position => (targetTransform != null) ? targetTransform.position : Vector3.one * float.PositiveInfinity;

        public Quaternion Rotation => (targetTransform != null) ? targetTransform.rotation : Quaternion.identity;

        public NetworkPlayerType Type => (isLocalPlayer) ? NetworkPlayerType.Local : NetworkPlayerType.Remote;

        public bool IsTracking => isTracking;

        public bool IsInitialized => isInitialized;

        public UnityEvent OnSpeakingEvent => onSpeaking;

        #endregion

        #region Public Methods

        public void Destroy() {
            if (isTracking) {
                comms.StopTracking(this);
                isTracking = false;
            }
            playerId.OnValueChanged -= UpdateCachedPlayerName;
        }

        public async Task Initialize() {
            Core.Monitoring.Log.D($"name of the base root object: {this.transform.root.name}");

            isLocalPlayer = NetworkManager.Singleton.LocalClientId == OwnerClientId;

            rootNetworkObject = this.transform.root.GetComponent<NetworkObject>();

            //Find Dissonance comms
            comms = FindObjectOfType<DissonanceComms>();
            if (comms == null)
                throw new MissingReferenceException($"Fatal error while trying to retrieve a reference for {nameof(DissonanceComms)}: no such reference found!");

            //Ensure that there is a valid Transform
            if (targetTransform == null) {
                Core.Monitoring.Log.E($"No reference found for {nameof(targetTransform)}. Using local transform as bakcup...");
                targetTransform = this.transform;
            }

            //Wait for dissonanceComms to be initialized
            while (comms.LocalPlayerName == null)
                await Task.Delay(100);

            //Update player id
            if (isLocalPlayer) {
                playerId.Value = comms.LocalPlayerName;
            }
        }

        public Task Login() {
            Core.Monitoring.Log.D($"Starting monitoring for gameobject with name: {PlayerId}");
            comms.TrackPlayerPosition(this);
            isTracking = true;

            return Task.CompletedTask;
        }

        public Task Join(string ChannelName, ChannelTypes ChannelType) {
            throw new System.NotImplementedException();
        }

        public Task Leave(string ChannelName, ChannelTypes ChannelType) {
            throw new System.NotImplementedException();
        }

        public Task Logout() {
            throw new System.NotImplementedException();
        }

        public Task Restart() {
            throw new System.NotImplementedException();
        }

        #endregion

        private void Awake() {
            playerId.OnValueChanged += UpdateCachedPlayerName;
        }

        private void StartTracking() {
            if (isTracking)
                return;

            if (comms == null)
                return;

            comms.TrackPlayerPosition(this);
            isTracking = true;
        }

        private void StopTracking() {
            if (!isTracking)
                return;

            if (comms == null)
                return;

            comms.StopTracking(this);
            isTracking = false;
        }

        private void UpdateCachedPlayerName(FixedString128Bytes previousName, FixedString128Bytes newName) {
            //cachedPlayerId = newName.ToString();

            if (isTracking) {
                StopTracking();
                StartTracking();
            }
        }
    }
}
