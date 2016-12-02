using UnityEngine;
using System.Collections;

public class TutorialTerminalPopup : MonoBehaviour {

    private GameObject playerGO;
    private RectTransform imageTransform;

    // Use this for initialization
    void Awake () {
        playerGO = GameObject.FindGameObjectWithTag("Player");
        imageTransform = GetComponent<RectTransform>();
    }
	
	// Update is called once per frame
	void Update () {
        UpdatePosition();

        //Debug.Log(playerGO.transform.position);
    }

    public void UpdatePosition()
    {
        transform.position = playerGO.transform.position - new Vector3(playerGO.transform.up.x * imageTransform.rect.width * .7f, playerGO.transform.up.y * imageTransform.rect.height * .7f, 0);
    }
}
