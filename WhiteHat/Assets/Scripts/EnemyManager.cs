using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour {

    #region Public Fields
    /// <summary>
    /// Time it will take for cameras to trigger an alarm, in seconds*
    /// *"Seconds" only if the player is directly in front of the camera; at further distances it will take much longer.
    /// NOTE: Because of this, a very small value works best. I'm liking 0.2 from my testing.
    /// </summary>
    public float spotTimeToAlert = 0.2f;
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
    private enum AlertState
    {
        Patrol,
        Alarmed,
        Searching,
    }
    //Current alert state
    private AlertState alertState;
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
	
	}

    public void TriggerAlarm()
    {
        Debug.Log("CONTACT!");
        //Trigger the alarm state
        alertState = AlertState.Alarmed;
        //Update last known location
        lastKnownLoc = playerObj.transform.position;
    }
}
