using UnityEngine;
using System.Collections;

public class TutorialTerminal : MonoBehaviour {

    public float useRadius = 3f;

    private Player player;

    public GameObject terminalPopup;

    private bool inRange;

	// Use this for initialization
	void Start () {
	    player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        terminalPopup.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 vecToPlayer = player.transform.position - transform.position;

        if (vecToPlayer.magnitude < useRadius)
        {
            if(!inRange)
            {
                terminalPopup.SetActive(true);
                terminalPopup.GetComponent<TutorialTerminalPopup>().UpdatePosition();
                inRange = true;
            }          
        }
        else if(inRange)
        {
            inRange = false;
            terminalPopup.SetActive(false);
        }
    }
}
