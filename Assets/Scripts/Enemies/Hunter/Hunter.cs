using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace YATP
{
    /// <summary>
    /// Seeking means the hunter is following player trails ( is over the shooting range ).
    /// Chasing means the hunter is looking at the player ( is in the shooting range ).
    /// 
    /// </summary>
    public enum HunterState { Seeking, Chasing, Attacking, CheckingTrail }

    public class Hunter : MonoBehaviour
    {
        /// <summary>
        /// Limit at which the hunter can see the player
        /// </summary>
        [SerializeField]
        float sightRange = 400;

        [SerializeField]
        float shootingRange = 200;

        NavMeshAgent agent;
        HunterState state = HunterState.Seeking;
        MonoBehaviour currentBehaviour;

        PlayerController player;
        float checkingTime;
        float checkingElapsed;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // Start is called before the first frame update
        void Start()
        {
            player = PlayerController.Instance;
            SetState((int)HunterState.Seeking); // Should start with none
        }

        private void Update()
        {
            UpdateState();
        }

        void UpdateState()
        {
            switch (state)
            {
                case HunterState.Seeking:
                    SeekThePlayer();
                    break;

                case HunterState.CheckingTrail:
                    CheckTrail();
                    break;
            }
        }

        void SeekThePlayer()
        {
            if (player.State == PlayerState.Dead)
                return;

            // Get the player distance
            
            if(IsPlayerSpotted())
            {
                SetState(HunterState.Chasing);
                return;
            }

            // Try to get the older trail 
            Vector3 destination = Vector3.zero;
            GameObject olderTrail;
            bool destinationFound = false;
            if(PlayerTrailManager.Instance.TryGetOlderTrail(out olderTrail))
            {
                destination = olderTrail.transform.position;
                destinationFound = true;
            }
            else
            {
                // We are eventually in a building and the player is hiding somewhere, so there won't be new traces at all
                // We may try to get some waypoint
            }

            if(destinationFound) 
                agent.SetDestination(destination);
        }

        void CheckTrail()
        {
            if (IsPlayerSpotted())
            {
                SetState(HunterState.Chasing);
                return;
            }

            checkingElapsed += Time.deltaTime;
            if(checkingElapsed > checkingTime)
            {
                SetState(HunterState.Seeking);
            }

        }

        bool IsPlayerSpotted()
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);


            return false;
        }

        public void SetState(HunterState state)
        {
            Debug.Log($"Setting new state:{state}");

            if (this.state == state)
                return;

            // Disable the current behaviour
            //if (currentBehaviour)
            //    currentBehaviour.enabled = false;

            this.state = state;
            switch(state)
            {
                case HunterState.Seeking:
                    //currentBehaviour = seekingBehaviour;
                    break;
                case HunterState.CheckingTrail: 
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                break;
            }

            //if(currentBehaviour)
            //    currentBehaviour.enabled = true;
        }

        public void SetCheckintTime(float value)
        {
            checkingTime = value;
            checkingElapsed = 0;
        }


    }

}
