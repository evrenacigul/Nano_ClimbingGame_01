using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Utilities;
using Nano.Objects;
using Nano.Obstacles;

namespace Nano.Managers
{
    public class SoundManager : MonoBehaviour
    {
        [Header("------Music Settings------")]
        [SerializeField]
        List<AudioClip> backgroundMusicList;

        [SerializeField]
        AudioSource musicPlayer;

        [SerializeField]
        bool playRandomMusic = false;

        [Header("------SFX Settings------")]
        [SerializeField]
        List<AudioClip> JumpToRock;

        [SerializeField]
        List<AudioClip> PlayerJump;

        [SerializeField]
        List<AudioClip> PlayerKicks;

        [SerializeField]
        List<AudioClip> PlayerFalls;

        [SerializeField]
        List<AudioClip> ShakyRock;

        [SerializeField]
        List<AudioClip> ZombieCatchPlayer;

        [SerializeField]
        List<AudioClip> ZombieDeath;

        [SerializeField]
        List<AudioClip> GrenadeExplosion;

        [SerializeField]
        List<AudioClip> FailScreenAppears;

        [SerializeField]
        List<AudioClip> WinScreenAppears;

        [SerializeField]
        List<AudioClip> StarAppears;

        [SerializeField]
        int sfxChannelCount = 4;

        List<AudioSource> channels;

        void Start()
        {
            if(backgroundMusicList != null && backgroundMusicList.Count > 0)
            {
                musicPlayer.loop = true;

                if (playRandomMusic)
                {
                    var selectRandom = Random.Range(0, backgroundMusicList.Count);
                    musicPlayer.clip = backgroundMusicList[selectRandom];
                }
                else
                {
                    musicPlayer.clip = backgroundMusicList[0];
                }

                musicPlayer.Play();
            }

            channels = new();

            for(int i = 0; i < sfxChannelCount; i++)
            {
                GameObject newObj = new();
                newObj.transform.SetParent(transform);
                newObj.name = "SFX Channel " + i.ToString();
                var source = newObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                channels.Add(source);
            }

            EventManager.Instance.OnPlayerGrabbed.AddListener(OnPlayerGrabbed);
            EventManager.Instance.OnPlayerJump.AddListener(OnPlayerJump);
            EventManager.Instance.OnPlayerFirstJump.AddListener(OnPlayerFirstJump);
            EventManager.Instance.OnPlayerHitEnemy.AddListener(OnPlayerHitEnemy);
            EventManager.Instance.OnPlayerDie.AddListener(OnPlayerDie);

            EventManager.Instance.OnShakyRock.AddListener(OnShakyRock);

            EventManager.Instance.OnEnemyCatchPlayer.AddListener(OnEnemyCatchPlayer);
            EventManager.Instance.OnEnemyDie.AddListener(OnEnemyDie);

            EventManager.Instance.OnGrenadeExplosion.AddListener(OnGrenadeExplosion);

            EventManager.Instance.OnFailScreenAppears.AddListener(OnFailScreenAppears);
            EventManager.Instance.OnWinScreenAppears.AddListener(OnWinScreenAppears);
            EventManager.Instance.OnStarAppears.AddListener(OnStarAppears);
        }

        private void OnPlayerGrabbed(GrabPoint _)
        {
            PlayRandomAudio(JumpToRock);
        }

        private void OnPlayerFirstJump()
        {
            OnPlayerJump();
        }

        private void OnPlayerJump()
        {
            PlayRandomAudio(PlayerJump);
        }

        private void OnPlayerHitEnemy(Vector3 _)
        {
            PlayRandomAudio(PlayerKicks);
        }

        private void OnPlayerDie()
        {
            PlayRandomAudio(PlayerFalls);
        }

        private void OnShakyRock(GrabPoint grabPoint)
        {
            PlayRandomAudio(ShakyRock);
        }

        private void OnEnemyCatchPlayer(object [] args)
        {
            PlayRandomAudio(ZombieCatchPlayer);
        }

        private void OnEnemyDie(object[] args)
        {
            PlayRandomAudio(ZombieDeath);
        }

        private void OnGrenadeExplosion(Vector3 _)
        {
            PlayRandomAudio(GrenadeExplosion);
        }

        private void OnFailScreenAppears()
        {
            PlayRandomAudio(FailScreenAppears);
        }

        private void OnWinScreenAppears()
        {
            PlayRandomAudio(WinScreenAppears);
        }

        private void OnStarAppears()
        {
            PlayRandomAudio(StarAppears);
        }

        private void PlayRandomAudio(List<AudioClip> audioClips)
        {
            var selectRandom = SelectRandom(audioClips);

            if(selectRandom != null)
                PlaySound(selectRandom);
        }

        private void PlaySound(AudioClip clip)
        {
            var source = GetIdleSource();
            if (source is null || clip is null) return;

            source.PlayOneShot(clip);
        }

        private AudioSource GetIdleSource()
        {
            foreach(AudioSource source in channels)
            {
                if (!source.isPlaying) return source;
            }

            return null;
        }

        private AudioClip SelectRandom(List<AudioClip> audioClips)
        {
            if (audioClips is null || audioClips.Count == 0) return null;

            if (audioClips.Count > 1)
                return audioClips[Random.Range(0, audioClips.Count)];
            else
                return audioClips[0];
        }
    }
}