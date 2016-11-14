using UnityEngine;
using System.Collections;

public class TerminalPopup : MonoBehaviour {

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

	}

    public void UpdatePosition()
    {
        transform.position = playerGO.transform.position - new Vector3(playerGO.transform.up.x * imageTransform.rect.width * .8f, playerGO.transform.up.y * imageTransform.rect.height * .8f, 0);
    }
}
