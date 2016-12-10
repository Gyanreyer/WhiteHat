using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {

    public Sprite offSprite, onSprite;
    SpriteRenderer spriteRend;
    //CheckpointPopup checkpointCanvas;
    GameObject player;
    public float activateRange;

	void Start ()
    {
        player = GameObject.Find("Player");
        spriteRend = this.GetComponent<SpriteRenderer>();
        this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-45, 45)));
        //checkpointCanvas = GameObject.Find("");
	}
	
	void Update ()
    {
        //Only bother with any of this if you aren't already active
        if(spriteRend.sprite==offSprite)
        {
            Vector2 vecToPlayer = player.transform.position - this.transform.position;
            if (vecToPlayer.sqrMagnitude < activateRange * activateRange)
            {
                //toggle the canvas here!!!
                if (Input.GetKeyDown(KeyCode.E))
                {
                    ToggleSprite();
                }
            }
        }
	}

    public void ToggleSprite()
    {
        if (spriteRend.sprite == offSprite)
            spriteRend.sprite = onSprite;
        else
            spriteRend.sprite = onSprite;
    }
}