using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YATP
{
    public class SeekingBehaviour : MonoBehaviour
    {
        Hunter hunter;
        PlayerController player;

        private void Awake()
        {
            hunter = GetComponent<Hunter>();
        }

        // Start is called before the first frame update
        void Start()
        {
            player = PlayerController.Instance;
         
        }

        // Update is called once per frame
        void Update()
        {
            SeekPlayer();
        }

        void SeekPlayer()
        {
            if (player.State == PlayerState.Dead)
                return;

            // Get the player distance

            if (hunter.IsPlayerSpotted())
            {
                hunter.SetState(HunterState.Chasing);
                return;
            }

            // Try to get the older trail 
            Vector3 destination = Vector3.zero;
            GameObject olderTrail;
            bool destinationFound = false;
            if (PlayerTrailManager.Instance.TryGetOlderTrail(out olderTrail))
            {
                destination = olderTrail.transform.position;
                destinationFound = true;
            }
            else
            {
                // We are eventually in a building and the player is hiding somewhere, so there won't be new traces at all
                // We may try to get some waypoint
            }

            if (destinationFound)
                hunter.SetDestination(destination);
        }
    }

}
