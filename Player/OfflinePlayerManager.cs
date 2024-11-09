using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Utilities;
using Castrimaris.Player.Contracts;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Castrimaris.Player {

    public class OfflinePlayerManager : SingletonMonoBehaviour<OfflinePlayerManager>, IPlayer {

        [Header("References")]
        [SerializeField] private InterfaceReference<IPlayerController> playerController;
        [SerializeField] private InterfaceReference<IFader> fader;
        [SerializeField] private InterfaceReference<IPlayerAppearance> appearance;
        
        private PlayerInitializationHelper initializationHelper;

        public ulong Id => 0;

        public void FadeIn() => fader.Interface.FadeIn();

        public void FadeOut() => fader.Interface.FadeOut();

        public void Lock() {
            //Skip grounded check otherwise the controller would move the player up and down
            playerController.Interface.IsLockedToVehicle = true;

            if (Utilities.IsVrPlatform()) {
                //Hurricane VR spawns configurable joints for grabbing that behave strangely when moving up and down.
                //We're going to "lock" the joints to avoid problems
                var leftJoint = this.transform.RecursiveFind("LeftOffset").GetComponent<ConfigurableJoint>();
                var rightJoint = this.transform.RecursiveFind("RightOffset").GetComponent<ConfigurableJoint>();

                leftJoint.xMotion = ConfigurableJointMotion.Locked;
                leftJoint.yMotion = ConfigurableJointMotion.Locked;
                leftJoint.zMotion = ConfigurableJointMotion.Locked;
                leftJoint.angularXMotion = ConfigurableJointMotion.Locked;
                leftJoint.angularYMotion = ConfigurableJointMotion.Locked;
                leftJoint.angularZMotion = ConfigurableJointMotion.Locked;

                rightJoint.xMotion = ConfigurableJointMotion.Locked;
                rightJoint.yMotion = ConfigurableJointMotion.Locked;
                rightJoint.zMotion = ConfigurableJointMotion.Locked;
                rightJoint.angularXMotion = ConfigurableJointMotion.Locked;
                rightJoint.angularYMotion = ConfigurableJointMotion.Locked;
                rightJoint.angularZMotion = ConfigurableJointMotion.Locked;
            }
        }

        public void Unlock() {
            playerController.Interface.IsLockedToVehicle = false;

            if (Utilities.IsVrPlatform()) {
                var leftJoint = this.transform.RecursiveFind("LeftOffset").GetComponent<ConfigurableJoint>();
                var rightJoint = this.transform.RecursiveFind("RightOffset").GetComponent<ConfigurableJoint>();

                leftJoint.xMotion = ConfigurableJointMotion.Free;
                leftJoint.yMotion = ConfigurableJointMotion.Free;
                leftJoint.zMotion = ConfigurableJointMotion.Free;
                leftJoint.angularXMotion = ConfigurableJointMotion.Free;
                leftJoint.angularYMotion = ConfigurableJointMotion.Free;
                leftJoint.angularZMotion = ConfigurableJointMotion.Free;

                rightJoint.xMotion = ConfigurableJointMotion.Free;
                rightJoint.yMotion = ConfigurableJointMotion.Free;
                rightJoint.zMotion = ConfigurableJointMotion.Free;
                rightJoint.angularXMotion = ConfigurableJointMotion.Free;
                rightJoint.angularYMotion = ConfigurableJointMotion.Free;
                rightJoint.angularZMotion = ConfigurableJointMotion.Free;
            }
        }

        protected override void Awake() {
            base.Awake();

            if (playerController.Interface == null)
                throw new MissingReferenceException($"No reference set for {nameof(playerController)}! Please, set it in the Editor.");
            if (fader.Interface == null)
                throw new MissingReferenceException($"No reference set for {nameof(fader)}! Please, set it in the Editor.");
        }

        private void Start() {
            Initialize();
            fader.Interface.FadeOut();
            appearance.Interface.Load();
        }

        private void Initialize() {
            if (!this.gameObject.TryGetComponent<PlayerInitializationHelper>(out initializationHelper))
                return;

            var runtimePlatform = Utilities.GetRuntimePlatform();

            switch (runtimePlatform) {
                case RuntimePlatformTypes.PC:
                case RuntimePlatformTypes.ANDROID:
                    var ui = initializationHelper.MainUIAdditionalSystemPrefab;
                    ui = GameObject.Instantiate(ui);
                    ui.name = "User Controls";
                    break;

                case RuntimePlatformTypes.QUEST1:
                case RuntimePlatformTypes.QUEST2:
                case RuntimePlatformTypes.QUEST3:
                    var vrSystems = initializationHelper.AdditionalVRSystems;
                    foreach (var system in vrSystems)
                        GameObject.Instantiate(system);
                    break;
                default:
                    break;
            }

            //Setup the player head meshes on the "CameraIgnore" layer, so that the Player camera won't see them
            var tags = GetComponentsInChildren<Tags>();
            var cameraIgnoreTags = from tag in tags
                                   where tag.Has("CameraIgnore")
                                   select tag;
            foreach (var cameraIgnoreTag in cameraIgnoreTags) {
                cameraIgnoreTag.gameObject.layer = LayerMask.NameToLayer("CameraIgnore");
            }
        }

    }

}