using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YATP
{
    public class PlayerTrailManager : MonoBehaviour
    {
        public static PlayerTrailManager Instance { get; private set; }

        [SerializeField]
        float trailDistance = 10;

        [SerializeField]
        float trailTime = 1;

        [SerializeField]
        GameObject trailPrefab;

        PlayerController player;
        float elapsedTime = 0;

        List<GameObject> trails = new List<GameObject>();
        bool noTrails = false;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
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
            Hunter.Instance.OnStartChasing += () => { noTrails = true; ClearTrailAll(); };
            Hunter.Instance.OnStartSeeking += () => { noTrails = false; };
        }

        // Update is called once per frame
        void Update()
        {
            if (player.State != PlayerState.Normal)
                return;

            if (noTrails)
                return;

            elapsedTime += Time.deltaTime;
            if(elapsedTime > trailTime)
            {
                elapsedTime = 0;

                // If the trail list is empty add a new trail
                bool addTrail = false;
                if(trails.Count == 0) 
                {
                    addTrail = true; 
                }
                else
                {
                    GameObject lastTrail = trails.Last();
                    // We don't want to add a new trail too close to the last one
                    if(Vector3.Distance(player.transform.position, lastTrail.transform.position) > trailDistance) 
                        addTrail = true;    
                }

                if(addTrail)
                {
                    AddTrail();
                }
                
                
            }
        }

        void AddTrail()
        {
            GameObject trail = Instantiate(trailPrefab, player.transform.position, player.transform.rotation);
            trail.GetComponent<PlayerTrail>().Init(4f);
            trails.Add(trail);
        }

        void ClearTrailAll()
        {
            for (int i = 0; i < trails.Count; i++)
            {
                Destroy(trails[i]);
            }

            trails.Clear();
        }

        public void RemoveAllPresiousTrails(GameObject trail)
        {
            List<GameObject> toRemove = new List<GameObject>();
            foreach(GameObject t in trails)
            {
                if (t != trail)
                    toRemove.Add(t);
                else
                    break;
            }
                        
            trails.RemoveAll(t=>toRemove.Contains(t));

            int count = toRemove.Count;
            for (int i = 0; i < count; i++)
            {
                Destroy(toRemove[i]);
            }
        }

        public void RemoveTrail(GameObject trail)
        {
            trails.Remove(trail);
            Destroy(trail);
        }

        public bool TryGetOlderTrail(out GameObject trail)
        {
            trail = null;
            if(trails.Count > 0)
                trail = trails[0];

            return trail != null;
        }
    }

}
