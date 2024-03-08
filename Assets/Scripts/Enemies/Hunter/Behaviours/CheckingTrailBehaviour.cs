using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using YATP;

/// <summary>
/// When the hunter is close to the player and the player is not moving away.
/// The hunter shot once to warn the player... the second shot the player is dead.
/// </summary>
public class CheckingTrailBehaviour : MonoBehaviour
{

   
    Hunter hunter;
    PlayerController player;

    float checkingTime = 0;
    float checkingElapsed = 0;

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
        CheckTrail();       
    }

    private void OnEnable()
    {
        hunter.StopAgent();
    }

    void CheckTrail()
    {
        if (hunter.IsPlayerSpotted())
        {
            hunter.SetState(HunterState.Chasing);
            return;
        }

        checkingElapsed += Time.deltaTime;
        if (checkingElapsed > checkingTime)
        {
            hunter.SetState(HunterState.Seeking);
        }

    }

    public void SetCheckingTime(float value)
    {
        checkingTime = value;
        checkingElapsed = 0;
    }

}
