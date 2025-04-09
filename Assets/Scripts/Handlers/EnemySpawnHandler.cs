using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Managers;
using Nano.Controllers;

namespace Nano.Handlers
{
    public class EnemySpawnHandler : MonoBehaviour
    {
        [SerializeField]
        bool spawnInfinite = false;

        [SerializeField]
        float initialDelayToStart = 2f;

        [SerializeField]
        float delayBetweenSpawns = 5f;

        [SerializeField]
        int maxSpawnCount = 5;

        [SerializeField]
        Object enemyPrefab;

        Transform enemiesParent;

        PlayerController playerController;

        Camera camera;

        int spawnCount = 0;

        float yOffsetBetweenCam;
        float zOffsetWall;

        private void Start()
        {
            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
            camera = Camera.main;

            yOffsetBetweenCam = camera.transform.localPosition.y - transform.localPosition.y;
        }

        private void OnGameStateChanged(GameStates state)
        {
            switch(state)
            {
                case GameStates.Play:
                    spawnCount = 0;
                    enemiesParent = GameObject.Find("Enemies").transform;
                    CalculateWallDistance();
                    StartCoroutine(Spawner());
                    break;
                case GameStates.Win:
                case GameStates.Fail:
                    spawnCount = maxSpawnCount;
                    StopCoroutine(Spawner());
                    break;
            }
        }

        private void CalculateWallDistance()
        {
            var pos = transform.position;
            Ray ray = new Ray(pos, transform.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    zOffsetWall = hit.distance;
                    //pos.z += hit.distance;
                    //transform.position = pos;
                }
            }
        }

        IEnumerator Spawner()
        {
            yield return new WaitForSeconds(initialDelayToStart);

            playerController = GameObject.Find("Player").GetComponent<PlayerController>();

            while ((spawnCount < maxSpawnCount || spawnInfinite) && GameManager.Instance.GetGameState == GameStates.Play)
            {
                if (!GameManager.Instance.IsMaxEnemy)
                {
                    var enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, enemiesParent) as GameObject;
                    var enemyPos = enemy.transform.position;
                    enemyPos.z += zOffsetWall;
                    enemy.transform.position = enemyPos;

                    EventManager.Instance.OnEnemySpawned?.Invoke();
                    spawnCount++;
                }

                var pos = transform.localPosition;
                pos.y = playerController.transform.localPosition.y - 1f;//camera.transform.localPosition.y;
                transform.localPosition = pos;

                yield return new WaitForSeconds(delayBetweenSpawns);

                if (GameManager.Instance.GetGameState != GameStates.Play)
                    yield break;
            }

        }
    }
}