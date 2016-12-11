using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Tooltip("Array of big parent floor objects to enable/disable.")]
    public GameObject[] floors;

    #region Private Fields
    //floor the player will respawn at
    private int currentFloor = 0;
    //current checkpoint the player will respawn at
    Vector3 currentCheckpoint = Vector3.zero;
    //array of checkpoints
    Checkpoint[] checkpoints;
    //array of terminals
    Terminal[] terminals;
    //player object
    Player player;
    #endregion

    #region Properties
    // Location to respawn at... almost managed without an if statement
    public Vector3 RespawnLocation
    { get { return currentCheckpoint; } }
    #endregion

    #region Unity Defaults
    void Start () {
        //Disable all floors except for the first
        Invoke("DisableFloorsPastTutorial", 1);
        //set checkpoint initially
        currentCheckpoint = GameObject.Find("StartSpawnPoint").transform.position;
        //find checkpoints
        checkpoints = GameObject.FindObjectsOfType<Checkpoint>();
        //disable the canvas thingey
        GameObject.Find("CheckpointPopupCanvas").SetActive(false);
        //get the terminals
        terminals = GameObject.FindObjectsOfType<Terminal>();
        //get the player
        player = GameObject.Find("Player").GetComponent<Player>();
    }
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            //Application.LoadLevel(0);//is this seriously deprecated?
            SceneManager.LoadScene(0);//do I seriously need a using statement for this?
        }
    }
    #endregion

    #region Methods
    public void ActivateNextFloor(Vector3 checkpointPosition)
    {
        if(++currentFloor >= floors.Length)
        {
            SceneManager.LoadScene(0);
            return;
        }
        //save states
        foreach (Checkpoint c in checkpoints)
            c.SetSpriteOn(false);
        foreach (Terminal t in terminals)
            t.SaveState();
        //Increment the checkpoint
        currentCheckpoint = checkpointPosition;
        //Activate the next floor in the array
        floors[currentFloor].SetActive(true);
    }
    public void DeactivatePrevFloor()
    {
        //Deactivate the previous floor in the array
        floors[currentFloor - 1].SetActive(false);
    }
    void DisableFloorsPastTutorial()
    {
        //Deactivates all floors except for 0
        for (int i = 1; i < floors.Length; ++i)
            floors[i].SetActive(false);
    }
    public void SetCheckpoint(Checkpoint newCheckpoint)
    {
        //currentCheckpoint.ToggleSprite();
        foreach (Checkpoint c in checkpoints) 
            c.SetSpriteOn(false);
        foreach (Terminal t in terminals)
            t.SaveState();
        currentCheckpoint = newCheckpoint.transform.position;
        player.SaveState();
    }
    public void RestoreTerminals()
    {
        foreach (Terminal t in terminals)
            t.RestoreLastState();
    }
    #endregion
}
