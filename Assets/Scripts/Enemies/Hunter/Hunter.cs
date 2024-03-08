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
    public enum HunterState { None, Seeking, Chasing, Attacking, CheckingTrail }

    public class Hunter : MonoBehaviour
    {
        public UnityAction OnStartChasing;
        public UnityAction OnStartSeeking;

        public static Hunter Instance { get; private set; }

        [SerializeField]
        SeekingBehaviour seekingBehaviour;

        [SerializeField]
        CheckingTrailBehaviour checkingTrailBehaviour;

        [SerializeField]
        ChasingBehaviour chasingBehaviour;

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
        HunterState state = HunterState.None;
        MonoBehaviour currentBehaviour;

        PlayerController player;
        
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
            DisableBehaviourAll();
            SetState(HunterState.Seeking); // Should start with none
        }

        private void Update()
        {
        }

        void DisableBehaviourAll() 
        {
            seekingBehaviour.enabled = false;
            checkingTrailBehaviour.enabled = false; 
        }

      
        public bool IsPlayerSpotted()
        {
            Vector3 dir = player.transform.position - transform.position;

            // Player is too far
            if (dir.magnitude > sightRange)
                return false;

            float angle = Vector3.Angle(transform.forward, Vector3.ProjectOnPlane(dir, Vector3.up));
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
            if (currentBehaviour)
                currentBehaviour.enabled = false;

            this.state = state;
            switch(state)
            {
                case HunterState.Seeking:
                    currentBehaviour = seekingBehaviour;
                    OnStartSeeking?.Invoke();
                    break;
                case HunterState.CheckingTrail:
                    currentBehaviour = checkingTrailBehaviour;
                    break;
                case HunterState.Chasing:
                    currentBehaviour = chasingBehaviour;
                    OnStartChasing?.Invoke();
                    break;
            }

            if(currentBehaviour)
                currentBehaviour.enabled = true;
        }

        public void SetCheckingTrailTime(float value)
        {
            checkingTrailBehaviour.SetCheckingTime(value);
        }

        public void SetDestination(Vector3 destination)
        {
            agent.SetDestination(destination);
        }

        public void StopAgent()
        {
            if(!agent) return;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

}
