using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Apple;
using UnityEngine.Events;

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
        public UnityAction OnStartChasing;
        public UnityAction OnStartSeeking;

        public static Hunter Instance { get; private set; }

        /// <summary>
        /// Limit at which the hunter can see the player
        /// </summary>
        [SerializeField]
        float sightRange = 400;

        [SerializeField]
        float shootingRange = 200;

        [SerializeField]
        Transform eyes;

        NavMeshAgent agent;
        HunterState state = HunterState.Seeking;
        MonoBehaviour currentBehaviour;

        PlayerController player;
        float checkingTime;
        float checkingElapsed;

        private void Awake()
        {
            if(!Instance)
            {
                Instance = this;
                agent = GetComponent<NavMeshAgent>();
            }
            else
            {
                Destroy(gameObject);
            }
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
                    SeekPlayer();
                    break;

                case HunterState.CheckingTrail:
                    CheckTrail();
                    break;
                case HunterState.Chasing:
                    ChasePlayer();
                    break;
            }
        }

        void ChasePlayer()
        {
            if(player.State == PlayerState.Dead) 
                return;

            if (!IsPlayerSpotted())
            {
                SetState(HunterState.Seeking);
                return;
            }

            agent.SetDestination(player.transform.position);
        }

        void SeekPlayer()
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
            Vector3 dir = player.transform.position - transform.position;

            // Player is too far
            if (dir.magnitude > sightRange)
                return false;

            float angle = Vector3.Angle(transform.forward, Vector3.ProjectOnPlane(dir, Vector3.up));
            Debug.Log($"Hit - angle:{angle}");
            // Not in the sight angle
            if (angle > 70)
                return false;

            RaycastHit hit;
            int mask = LayerMask.GetMask(new string[] { "TrailTrigger" });
            if(Physics.Raycast(eyes.position, player.Camera.transform.position - eyes.position, out hit, sightRange, ~mask))
            {
                Debug.Log($"Hit - {hit.collider.name}");
                if (hit.collider.CompareTag("Player"))
                    return true;
            }

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
                    OnStartSeeking?.Invoke();
                    break;
                case HunterState.CheckingTrail: 
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                break;
                case HunterState.Chasing:
                    agent.ResetPath();
                    OnStartChasing?.Invoke();
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
