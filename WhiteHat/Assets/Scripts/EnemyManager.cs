using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour {

    #region Public Fields
    [Tooltip("Time it will take for cameras to trigger an alarm, in seconds. 'Seconds' only if the player is directly in front of the camera; at further distances it will take much longer.")]
    public float spotTimeToAlert = 0.2f;
    [Tooltip("Duration of the alarmed phase once the alarm triggers, in seconds.")]
    public float alarmTime = 10f;
    [Tooltip("Duration of the search phase once the alarm triggers, in seconds.")]
    public float searchTime = 10f;
    #endregion
    #region Private Fields
    //Reference to the player
    private GameObject playerObj;
    //Last known location of the player
    private Vector3 lastKnownLoc;
    //Arrays to hold enemies
    private GameObject[] cameras;
    private GameObject[] robots;
    //States for alert phase
    public enum AlertStates
    {
        Patrol,
        Alarmed,
        Searching,
    }
    //Current alert state
    private AlertStates alertState;
    //Current alarm time
    private float currentTimeInAlarm;
    //Current search time
    private float currentTimeInSearching;
    #endregion
    #region Properties
    public AlertStates AlertState
    { get { return alertState; } }
    #endregion

    // Use this for initialization
    void Start () {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        //get array of all cameras
        cameras = GameObject.FindGameObjectsWithTag("CameraAI");
        //get array of all robots
        //robots = GameObject.FindGameObjectsWithTag("RobotAI");
        //set their spot time
        foreach(GameObject c in cameras)
        {
            c.GetComponent<CameraAI>().TimeToAlert = spotTimeToAlert;
        }
	}
	
	// Update is called once per frame
	void Update () {
        switch (alertState)
        {
            case AlertStates.Patrol:
                ManagePatrol();
                break;
            case AlertStates.Alarmed:
                ManageAlarmed();
                break;
            case AlertStates.Searching:
                ManageSearching();
                break;
        }
	}

    private void ManagePatrol()
    {

    }

    private void ManageAlarmed()
    {
        //increase alarm cooldown timer
        currentTimeInAlarm += Time.deltaTime;
        //Debug.Log("alarm time: " + currentTimeInAlarm);//DEBUG//
        //check if alert is over
        if (currentTimeInAlarm > alarmTime)
        {
            //reset current alarm timer
            currentTimeInAlarm = 0;
            //switch to searching state
            alertState = AlertStates.Searching;
            //
        }
    }

    private void ManageSearching()
    {
        //increase search cooldown timer
        currentTimeInSearching += Time.deltaTime;
        //Debug.Log("searching time: " + currentTimeInSearching);//DEBUG//
        //check if searching is over
        if (currentTimeInSearching > alarmTime)
        {
            //reset current search timer
            currentTimeInSearching = 0;
            //switch to patrol state
            alertState = AlertStates.Patrol;
            //
        }
    }

    public void TriggerAlarm()
    {
        //Debug.Log("CONTACT!");//DEBUG//
        //Trigger the alarm state
        alertState = AlertStates.Alarmed;
        //Update last known location
        lastKnownLoc = playerObj.transform.position;
        //reset 
    }
}
