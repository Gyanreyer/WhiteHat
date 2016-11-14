using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour {

    #region Public Fields
    [Tooltip("Time it will take for cameras to trigger an alarm, in seconds. 'Seconds' only if the player is directly in front of the camera; at further distances it will take much longer.")]
    public float spotTimeToAlert = 0.2f;
    [Tooltip("Duration of the alarmed phase once the alarm triggers, in seconds.")]
    public float alarmTime = 10f;
    [Tooltip("Duration of the search phase once the alarm triggers, in seconds.")]
    public float searchTime = 10f;
    [Tooltip("Constant added each frame while the player is in view of cameras.")]
    public float spotTimeAddedConstant = 0.004f;
    #endregion
    #region Private Fields
    //Reference to the player
    private GameObject playerObj;
    //Last known location of the player
    private Vector3 lastKnownLoc;
    //Arrays to hold enemies
    private GameObject[] cameras;
    private GameObject[] robots;
    //Array to hold navnodes
    //private GameObject[] navNodes;//aStar object holds these as "Verts"
    //Reference to the canvas
    private Text canvasText;
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
    //Holds a-star related methods
    AStarGraph aStar;
    #endregion
    #region Properties
    public AlertStates AlertState
    { get { return alertState; } }
    public float SpotTimeAddedConstant
    {get { return spotTimeAddedConstant; } }
    #endregion

    #region Unity Defaults
    void Start () {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        //get array of all navnodes
        //navNodes = GameObject.FindGameObjectsWithTag("NavNodes");
        //get array of all cameras
        cameras = GameObject.FindGameObjectsWithTag("CameraAI");
        //get array of all robots
        robots = GameObject.FindGameObjectsWithTag("RobotAI");
        //set their spot time
        foreach(GameObject c in cameras)
        {
            c.GetComponent<CameraAI>().TimeToAlert = spotTimeToAlert;
        }
        //get ref to canvas text object
        canvasText = GameObject.Find("Canvas").transform.GetChild(0).gameObject.GetComponent<Text>();
        aStar = new AStarGraph(); // I guess you don't need this because Unity automatically constructs objects?
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
    #endregion

    /// <summary>
    /// Manages the patrol state
    /// </summary>
    private void ManagePatrol()
    {
        //revert canvas text
        canvasText.text = "";
    }

    /// <summary>
    /// Manages the alarmed alert state
    /// </summary>
    private void ManageAlarmed()
    {
        //increase alarm cooldown timer
        currentTimeInAlarm += Time.deltaTime;
        //check if alert is over
        if (currentTimeInAlarm > alarmTime)
        {
            //reset current alarm timer
            currentTimeInAlarm = 0;
            //switch to searching state
            alertState = AlertStates.Searching;
        }
        //Update canvas text
        canvasText.text = "ALARMED\n" + (alarmTime - currentTimeInAlarm).ToString("F1");
    }

    /// <summary>
    /// Manages the searching alert state
    /// </summary>
    private void ManageSearching()
    {
        //increase search cooldown timer
        currentTimeInSearching += Time.deltaTime;
        //check if searching is over
        if (currentTimeInSearching > alarmTime)
        {
            //reset current search timer
            currentTimeInSearching = 0;
            //switch to patrol state
            alertState = AlertStates.Patrol;
        }
        //Update canvas text
        canvasText.text = "SEARCHING\n" + (searchTime - currentTimeInSearching).ToString("F1");
    }

    /// <summary>
    /// Sets the manager to the alerted state, resets the alert timer, and updates the last known player location
    /// </summary>
    public void TriggerAlarm()
    {
        //Trigger the alarm state
        alertState = AlertStates.Alarmed;
        //reset timer
        currentTimeInAlarm = 0;
        //Update last known location
        lastKnownLoc = playerObj.transform.position;
    }

    /// <summary>
    /// Uses A-Star methods to return a short path from start to end
    /// </summary>
    public GameObject[] GenerateAStarPath(NavNode start, NavNode end)
    {
        return aStar.AStar(start, end);
    }

    /// <summary>
    /// Returns the closest node to a given location.
    /// Super not efficient right now... will do quadtrees later if/when necessary
    /// </summary>
    public GameObject FindClosestNode(Vector3 loc)
    {
        float minDist = float.MaxValue;
        float tempDist = 0;
        int closestIndex = 0;
        for (int i = 0; i < aStar.Verts.Length; ++i) 
        {
            tempDist = Vector3.SqrMagnitude(aStar.Verts[i].transform.position - loc);
            if (tempDist < minDist)
            {
                minDist = tempDist;
                closestIndex = i;
            }
        }
        return aStar.Verts[closestIndex];
    }
}
