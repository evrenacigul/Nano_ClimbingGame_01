using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Nano.Utilities;
using Nano.Objects;
using Nano.Obstacles;

namespace Nano.Managers
{
    public class EventManager : SingletonMonoBehaviour<EventManager>
    {
        //Base Events
        public UnityEvent<GameStates> OnGameStateChanged;

        //Input Events
        public UnityEvent<Vector2> OnInputTouchStart;
        public UnityEvent<Vector2> OnInputTouchStationary;
        public UnityEvent<Vector2> OnInputTouchEnd;
        public UnityEvent<GrabPoint> OnGrabPointSelected;
        public UnityEvent<Vector3> OnFinishAreaTapped;

        //Player Events
        public UnityEvent OnPlayerJump;
        public UnityEvent OnPlayerFirstJump;
        public UnityEvent<GrabPoint> OnPlayerGrabbed;
        public UnityEvent OnPlayerCantJump;
        public UnityEvent OnPlayerDie;
        public UnityEvent<float, GrabPoint> OnPlayerResting;
        public UnityEvent<object[]> OnPlayerStaminaDepleted;
        public UnityEvent OnPlayerStaminaFull;
        public UnityEvent<Vector3> OnPlayerHitEnemy;
        public UnityEvent<Vector3> OnPlayerThrowGrenade;

        //Enemy Events
        public UnityEvent OnEnemySpawned;
        public UnityEvent<object[]> OnEnemyCatchPlayer;
        public UnityEvent OnEnemyReleasePlayer;
        public UnityEvent<object[]> OnEnemyDie;
        public UnityEvent<Vector3, float> OnEnemyHitPlayer;

        //UI Events
        public UnityEvent<float, float, float> OnStaminaUpdate;
        public UnityEvent<float> OnProgressUpdate;
        public UnityEvent OnStarAppears;
        public UnityEvent OnWinScreenAppears;
        public UnityEvent OnFailScreenAppears;

        //Obstacle Events
        public UnityEvent<GrabPoint> OnRockFalls;
        public UnityEvent<List<GrabPoint>> OnJumpableRocks;
        public UnityEvent<GrabPoint, Vector3> OnRockHitZombie;
        public UnityEvent<GrabPoint> OnShakyRock;
        public UnityEvent<GrabPoint> OnShakyRockFalls;
        public UnityEvent<Vector3> OnGrenadeExplosion;

        //Time Events
        public UnityEvent OnSlowMotionStart;
        public UnityEvent OnSlowMotionStop;

        //public SerializableDictionary<string, UnityEvent> CustomEvents;
    }
}