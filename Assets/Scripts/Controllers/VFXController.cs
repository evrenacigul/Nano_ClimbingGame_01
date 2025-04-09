using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Nano.Managers;
using Nano.Obstacles;

namespace Nano.Controllers
{
    public class VFXController : MonoBehaviour
    {
        [SerializeField]
        VolumeProfile volumeProfile;

        [Header("VFX List")]
        [SerializeField]
        Object OutOfStamina;

        [SerializeField]
        Object JumpableRocks;

        [SerializeField]
        Object PlayerKickZombie;

        [SerializeField]
        Object ZombieHitPlayer;

        [SerializeField]
        Object RockHitsZombie;

        [SerializeField]
        Object GrenadeExplosion;

        Dictionary<string, List<GameObject>> instVfxObjects;

        Vignette vignette;

        float vignetteDefaultIntensity;

        private void Start()
        {
            if(volumeProfile is null)
                volumeProfile = GetComponent<Volume>()?.profile;

            if (volumeProfile is null) throw new System.NullReferenceException(nameof(VolumeProfile));

            if (!volumeProfile.TryGet(out vignette)) throw new System.NullReferenceException(nameof(vignette));

            vignetteDefaultIntensity = vignette.intensity.value;

            instVfxObjects = new();

            EventManager.Instance.OnEnemyCatchPlayer.AddListener(OnEnemyCatchPlayer);
            EventManager.Instance.OnEnemyReleasePlayer.AddListener(OnEnemyReleasePlayer);
            EventManager.Instance.OnPlayerStaminaDepleted.AddListener(OnPlayerStaminaDepleted);
            EventManager.Instance.OnPlayerStaminaFull.AddListener(OnPlayerStaminaFull);
            EventManager.Instance.OnJumpableRocks.AddListener(OnJumpableRocks);
            EventManager.Instance.OnPlayerJump.AddListener(OnPlayerJump);
            EventManager.Instance.OnPlayerHitEnemy.AddListener(OnPlayerHitEnemy);
            EventManager.Instance.OnEnemyHitPlayer.AddListener(OnEnemyHitPlayer);
            EventManager.Instance.OnRockHitZombie.AddListener(OnRockHitZombie);
            EventManager.Instance.OnGrenadeExplosion.AddListener(OnGrenadeExplosion);

            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStates state)
        {
            switch(state)
            {
                case GameStates.Load:
                    foreach(List<GameObject> list in instVfxObjects.Values)
                    {
                        if (list.Count > 0)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                Destroy(list[i]);
                            }
                        }
                    }
                    OnEnemyReleasePlayer();

                    break;
            }
        }

        private void OnEnemyCatchPlayer(object [] args)
        {
            vignette.active = true;
            vignette.color.value = Color.red;

            LeanTween.value(gameObject, vignetteDefaultIntensity, 0.55f, 0.2f).setOnUpdate((float val) => 
            {
                vignette.intensity.value = val;
            }).setLoopPingPong().setLoopClamp();
        }

        private void OnEnemyReleasePlayer()
        {
            LeanTween.cancel(gameObject);

            vignette.intensity.value = vignetteDefaultIntensity;
            vignette.active = false;

            DestroyVFX("PlayerKickZombie");
            DestroyVFX("ZombieHitPlayer");
        }

        private void OnPlayerStaminaDepleted(object [] args)
        {
            var positionTr = (Transform)args[0];
            CreateVFX("OutOfStamina", OutOfStamina, positionTr.position, positionTr);

            vignette.active = true;
            vignette.color.value = Color.black;
            LeanTween.value(gameObject, vignetteDefaultIntensity, 0.55f, 0.2f).setOnUpdate((float val) =>
            {
                vignette.intensity.value = val;
            }).setLoopPingPong().setLoopClamp();
        }

        private void OnPlayerStaminaFull()
        {
            LeanTween.cancel(gameObject);

            vignette.intensity.value = vignetteDefaultIntensity;
            vignette.active = false;

            DestroyVFX("OutOfStamina");
        }

        private void OnJumpableRocks(List<GrabPoint> grabPoints)
        {
            foreach(GrabPoint grabPoint in grabPoints)
            {
                var pos = grabPoint.transform.position;
                pos.z -= 1f;
                CreateVFX("JumpableRocks", JumpableRocks, pos, grabPoint.transform);
            }
        }

        private void OnPlayerJump()
        {
            DestroyVFX("JumpableRocks");
        }

        private void OnPlayerHitEnemy(Vector3 pos)
        {
            CreateVFX("PlayerKickZombie", PlayerKickZombie, pos);
        }

        private void OnEnemyHitPlayer(Vector3 pos, float _)
        {
            CreateVFX("ZombieHitPlayer", ZombieHitPlayer, pos);
        }

        private void OnRockHitZombie(GrabPoint grabPoint, Vector3 hitPoint)
        {
            var pos = grabPoint.transform.position;

            CreateVFX("RockHitsZombie", RockHitsZombie, hitPoint, grabPoint.transform);
        }

        private void OnGrenadeExplosion(Vector3 blowPoint)
        {
            CreateVFX("GrenadeExplosion", GrenadeExplosion, blowPoint);
        }

        private void OnApplicationQuit()
        {
            vignette.active = false;
            vignette.intensity.value = vignetteDefaultIntensity;
        }

        private void CreateVFX(string vfxName, Object obj, Vector3 position, Transform parent = null)
        {
            var vfxObj = Instantiate(obj, position, Quaternion.identity) as GameObject;
            if (parent != null)
                vfxObj.transform.SetParent(parent);

            if (instVfxObjects.ContainsKey(vfxName))
            {
                instVfxObjects[vfxName].Add(vfxObj);
            }
            else
            {
                List<GameObject> temp = new();
                temp.Add(vfxObj);
                instVfxObjects.Add(vfxName, temp);
            }
        }

        private void DestroyVFX(string vfxName)
        {
            if (!instVfxObjects.ContainsKey(vfxName)) return;

            var objList = instVfxObjects[vfxName];
            for(int i = 0; i < objList.Count; i++)
            {
                if(objList[i])
                    Destroy(objList[i]);
            }

            objList.Clear();
        }
    }
}