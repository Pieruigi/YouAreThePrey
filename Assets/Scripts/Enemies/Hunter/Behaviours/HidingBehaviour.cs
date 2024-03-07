using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using YATP;

/// <summary>
/// When the hunter is close to the player and the player is not moving away.
/// The hunter shot once to warn the player... the second shot the player is dead.
/// </summary>
public class HidingBehaviour : MonoBehaviour
{

    [SerializeField]
    float warningShotTime = 20f;

    [SerializeField]
    float deadlyShotTime = 40f;

    float timeElapsed = 0;
    bool warningShot = false;
    bool deadlyShot = false;

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
        timeElapsed += Time.deltaTime;

        if(timeElapsed > warningShotTime)
        {
            if(!warningShot)
            {
                warningShot = true;
                Debug.Log("WarnThePlayer");
            }
        }

        if(timeElapsed > deadlyShotTime)
        {
            if (!deadlyShot)
            {
                deadlyShot = true;
                player.Die();
                Debug.Log("KillThePlayer");
            }
        }
    }
}
