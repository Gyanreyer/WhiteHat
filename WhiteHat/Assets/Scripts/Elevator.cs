using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Elevator : MonoBehaviour {

    bool playerEntered = false;

    bool usedElevator;

    enum ElevatorState
    {
        levelEnd,
        riding,
        arriving,
        levelStart
    }

    ElevatorState state;

    public Vector3 nextPosition;

    Material elevatorFade;

    BoxCollider2D doorCollider;

    float arrivingTimer = .6f;

    private Text promptText;

	// Use this for initialization
	void Start () {
        elevatorFade = transform.FindChild("BlackFade").GetComponent<Renderer>().material;

        doorCollider = transform.FindChild("Door").GetComponent<BoxCollider2D>();

        promptText = GetComponentInChildren<Text>();

        state = ElevatorState.levelEnd;
	}
	
	// Update is called once per frame
	void Update () {

        if (playerEntered && Input.GetKeyDown(KeyCode.E) && state == ElevatorState.levelEnd)
        {
            GoToNextLevel();
        }

        if (state == ElevatorState.riding)
        {
            if (elevatorFade.color.a < 1)
            {
                Color fadeColor = elevatorFade.color;
                fadeColor.a += Time.deltaTime;
                elevatorFade.color = fadeColor;
            }
            else
            {
                GameObject playerGO = GameObject.Find("Player");

                Vector2 playerDiffFromElevator = playerGO.transform.position - transform.position;
                Vector2 cameraDiffFromPlayer = Camera.main.transform.position - playerGO.transform.position;

                state = ElevatorState.arriving;
                transform.position = new Vector3(nextPosition.x,nextPosition.y,transform.position.z);

                playerGO.transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z-2) + (Vector3)playerDiffFromElevator;
                Camera.main.transform.position = playerGO.transform.position + (Vector3)cameraDiffFromPlayer;
            }
        }
        else if (state == ElevatorState.arriving)
        {
            if (arrivingTimer > 0)
            {
                arrivingTimer -= Time.deltaTime;
            }
            else
            {
                if (elevatorFade.color.a > 0)
                {
                    Color fadeColor = elevatorFade.color;
                    fadeColor.a -= Time.deltaTime;
                    elevatorFade.color = fadeColor;
                }
                else
                {
                    transform.position += new Vector3(0,0,3);
                    GameObject.Find("Player").transform.position += new Vector3(0,0,3);

                    doorCollider.enabled = false;
                    state = ElevatorState.levelStart;

                    //disable previous floor
                    GameObject.Find("GameManager").GetComponent<GameManager>().DeactivatePrevFloor();
                }
            }
        }

	}

    void GoToNextLevel()
    {
        GameObject.Find("Player").transform.position -= new Vector3(0,0,3);
        transform.position -= new Vector3(0,0,3);

        //activate next floor and set checkpoint
        GameObject.Find("GameManager").GetComponent<GameManager>().ActivateNextFloor(nextPosition);

        usedElevator = true;

        doorCollider.enabled = true;

        state = ElevatorState.riding;

        promptText.color = Color.clear;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && state == ElevatorState.levelEnd)
        {
            playerEntered = true;
            //GoToNextLevel();

            promptText.color = new Color(.1f,.1f,.1f,1);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            playerEntered = false;

            promptText.color = Color.clear;
        }
    }
}
