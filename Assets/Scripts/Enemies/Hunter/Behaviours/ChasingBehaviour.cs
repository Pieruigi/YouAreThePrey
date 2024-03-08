using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YATP
{
    public class ChasingBehaviour : MonoBehaviour
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
            ChasePlayer();
        }

        void ChasePlayer()
        {
            if (player.State == PlayerState.Dead)
                return;

            if (!hunter.IsPlayerSpotted())
            {
                hunter.SetState(HunterState.Seeking);
                return;
            }



            hunter.SetDestination(player.transform.position);
        }

    }

}
