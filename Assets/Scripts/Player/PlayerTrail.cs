using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;


namespace YATP
{
    public class PlayerTrail : MonoBehaviour
    {
        //[SerializeField]
        float delay;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Init(float delay)
        {
            this.delay = delay;
        }

        private void OnTriggerEnter(Collider other)
        {
            
            if (other.CompareTag("Hunter"))
            {
                // Remove all previous trails if any
                PlayerTrailManager.Instance.RemoveAllPresiousTrails(gameObject);
                // Remove this trail
                PlayerTrailManager.Instance.RemoveTrail(gameObject);

                if(delay > 0)
                {
                    Hunter hunter = other.GetComponent<Hunter>();
                    hunter.SetCheckingTrailTime(delay);
                    hunter.SetState(HunterState.CheckingTrail);
                    
                }
            }
        }

    }

}
