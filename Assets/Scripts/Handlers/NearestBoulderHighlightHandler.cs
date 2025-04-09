using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Nano.Managers;
using Nano.Controllers;
using Nano.Obstacles;

namespace Nano.Handlers
{
    public class NearestBoulderHighlightHandler : MonoBehaviour
    {
        PlayerController playerController;
        PlayerStats stats;

        [SerializeField]
        List<GameObject> nearestBoulders;

        List<GrabPoint> jumpableRocks;

        readonly string boulderTag = "GrabPoint";

        bool isResting = false;

        private void Start()
        {
            EventManager.Instance.OnPlayerResting.AddListener(OnPlayerResting);
            EventManager.Instance.OnPlayerJump.AddListener(OnPlayerJump);
            EventManager.Instance.OnRockFalls.AddListener(OnRockFalls);
        }

        public void SetPlayer(PlayerController getPlayer, PlayerStats getStats)
        {
            playerController = getPlayer;
            stats = getStats;
        }

        private void ArrangeNearestBoulderList(GrabPoint current)
        {
            var boulders = GameObject.FindGameObjectsWithTag(boulderTag);

            nearestBoulders = new();

            var currentPosition = current.transform.position;
            var yDifference = currentPosition.y - stats.jumpSettings.maxLowerDistance;

            var nearests = boulders.Where(boulder => Vector2.Distance(currentPosition, boulder.transform.position) <= stats.jumpSettings.maxJumpDistance
            && boulder.transform.position.y > yDifference).ToList();

            //foreach(GameObject boulder in boulders)
            //{
            //    var boulderPosition = boulder.transform.position;

            //    if(Vector2.Distance(currentPosition, boulderPosition) <= stats.jumpSettings.maxJumpDistance &&
            //        Mathf.Abs(currentPosition.y - boulderPosition.y) <= stats.jumpSettings.maxLowerDistance)
            //    {
            //        nearestBoulders.Add(boulder.GetComponent<GrabPoint>());
            //    }
            //}

            nearestBoulders = nearests;
        }

        private void OnPlayerResting(float stamina, GrabPoint currentGrabPoint)
        {
            ArrangeNearestBoulderList(currentGrabPoint);

            if (nearestBoulders is null) throw new UnityException("nearestBoulders is null!");

            if (isResting) return;

            //if (jumpableRocks is null) jumpableRocks = new();
            //else jumpableRocks.Clear();

            foreach (GameObject boulderObj in nearestBoulders)
            {
                var boulder = boulderObj.GetComponent<GrabPoint>();

                if (boulder && boulder != currentGrabPoint)
                {
                    boulder.ShowHighlight();
                    //jumpableRocks.Add(boulder);
                }
            }

            //EventManager.Instance.OnJumpableRocks?.Invoke(jumpableRocks);
            isResting = true;
        }

        private void OnPlayerJump()
        {
            if (nearestBoulders is null) return;//throw new UnityException("nearestBoulders is null!");

            foreach (GameObject boulderObj in nearestBoulders)
            {
                GrabPoint boulder = null;
                if(boulderObj)
                    boulder = boulderObj.GetComponent<GrabPoint>();

                if (boulder != null)
                    boulder.StopHighlight();
            }
            isResting = false;
        }

        private void OnRockFalls(GrabPoint grabPoint)
        {
            if (nearestBoulders is null) throw new UnityException("nearestBoulders is null!");

            nearestBoulders.Remove(grabPoint.gameObject);
        }

    }
}