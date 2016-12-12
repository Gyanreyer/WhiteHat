using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {

    public Sprite offSprite, onSprite;
    public float activateRange;
    private SpriteRenderer spriteRend;
    public GameObject popupCanvas;
    private Player player;
    private GameManager gameMan;
    private bool inRange = false;

    void Start ()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        spriteRend = this.GetComponent<SpriteRenderer>();
        this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-45, 45)));
        gameMan = GameObject.Find("GameManager").GetComponent<GameManager>();
	}
	
	void Update ()
    {
        Vector2 vecToPlayer = player.transform.position - this.transform.position;

        if (vecToPlayer.sqrMagnitude < activateRange * activateRange)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                gameMan.SetCheckpoint(this);
                this.SetSpriteOn(true);
            }
            if (!inRange)
            {
                inRange = true;
                popupCanvas.SetActive(true);
            }
        }
        else if (inRange)
        {
            popupCanvas.SetActive(false);
            inRange = false;
        }
	}

    public void SetSpriteOn(bool on)
    {
        if (on)
        {
            spriteRend.sprite = onSprite;
            this.transform.localScale = new Vector3(.025f, .025f, 1);
        }
        else
        {
            spriteRend.sprite = offSprite;
            this.transform.localScale = new Vector3(.25f, .25f, 1);
        }
    }
}