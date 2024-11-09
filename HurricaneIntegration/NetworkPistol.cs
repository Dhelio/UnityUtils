using HurricaneVR.Framework.Weapons;
using HurricaneVR.Framework.Weapons.Guns;
using Unity.Netcode;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    /// <summary>
    /// Makes the default Hurricane pistol Networked. Callbacks needs to be set in the relative <see cref="HVRPistol"/> callbacks.
    /// </summary>
    [RequireComponent(typeof(HVRPistol))]
    [RequireComponent(typeof(HVRGunSounds))]
    public class NetworkPistol : NetworkBehaviour {

        [Header("References")]
        [SerializeField] private GameObject hitMarker;

        private HVRPistol pistol;
        private HVRGunSounds sounds;

        public void PlaySound() => PlaySoundServerRpc(NetworkObject.OwnerClientId);

        public void Hit(GunHitArgs gunHitArgs) { if (pistol.Ammo.CurrentCount > 0) HitServerRpc(gunHitArgs.HitPoint); }

        private void Awake() {
            pistol = GetComponent<HVRPistol>();
            sounds = GetComponent<HVRGunSounds>();
        }

        [ServerRpc]
        private void HitServerRpc(Vector3 position) {
            var marker = GameObject.Instantiate(hitMarker);
            marker.transform.position = position;

            marker.GetComponent<NetworkObject>().Spawn(true);
        }

        [ServerRpc]
        private void PlaySoundServerRpc(ulong shootingClient) {
            InternalPlaySound(shootingClient);
            PlaySoundClientRpc(shootingClient);
        }

        [ClientRpc] private void PlaySoundClientRpc(ulong shootingClient) => InternalPlaySound(shootingClient);

        private void InternalPlaySound(ulong shootingClient) {
            if (NetworkManager.Singleton.LocalClientId == shootingClient)
                return;

            if (pistol.Ammo.CurrentCount <= 0) 
                sounds.PlayOutOfAmmo();
            else
                sounds.PlayGunFire();
        }

    }

}