using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Tooltip("Array of big parent floor objects to enable/disable.")]
    public GameObject[] floors;
    [Tooltip("Array of elevators to respawn at. Must be added in order.")]
    public GameObject[] elevators;

    #region Private Fields
    //floor the player will respawn at
    private int currentFloor = 0;
    #endregion

    #region Properties
    // Location to respawn at
    public Vector3 RespawnLocation
    {
        get { return elevators[currentFloor].transform.position; }
    }
    #endregion

    #region Unity Defaults
    void Start () {
        //Disable all floors except for the first
        for (int i = 1; i < floors.Length; ++i)
            floors[i].SetActive(false);
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
    public void ActivateNextFloor()
    {
        //Increment the checkpoint
        ++currentFloor;
        //Activate the next floor in the array
        floors[currentFloor].SetActive(true);
    }
    public void DeactivatePrevFloor()
    {
        //Deactivate the previous floor in the array
        floors[currentFloor - 1].SetActive(false);
    }
    #endregion
}
