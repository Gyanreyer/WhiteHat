using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ActiveAbilities
{
    none,
    invisible,
    dash,
    shoot
}

public enum PassiveAbilities
{
    addSpeed,
    addDetectionResisitance
}


public class Player : MonoBehaviour {

    enum PlayerState
    {
        idle,
        running,
        dead
    }

    public float moveSpeed = 5;

    [Tooltip("How far the player will be able to scroll the camera.")]
    public float maxCameraPanDist = 5f;

    private Camera mainCamera;
    private Rigidbody2D rigidBody;

    private GameObject legs,body;

    private Vector2 velocity;

    private float lerpTime = 0;

    private PlayerState state;

    public Sprite deathSprite,bodySprite;

    public GameObject deathPartSys;

    public ActiveAbilities activeAbility;
    public PassiveAbilities passiveAbility;

    public float percentActiveLeft = 0;//100 is full
    private bool isAbilityActive = false;
    private float activeBarDecreaseAmt = 0;

    private SpriteRenderer bodySpriteRenderer, legsSpriteRenderer;
    private TrailRenderer dashTrail;

    public GameObject bullet;

	// Use this for initialization
	void Start () {
        mainCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody2D>();

        legs = GameObject.Find("Legs");
        body = GameObject.Find("Body");

        bodySpriteRenderer = body.GetComponent<SpriteRenderer>();
        legsSpriteRenderer = legs.GetComponent<SpriteRenderer>();

        dashTrail = legs.GetComponent<TrailRenderer>();
        dashTrail.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {

        //If alive, get movement
        if (state != PlayerState.dead)
        {
            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? .6f: 1);

            //Move the player with WASD, sets velocity to apply to rigidbody in FixedUpdate
            velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized * speed;

            legs.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90);
            //legs.transform.position = transform.position + new Vector3(0, 0, 1);

            //Rotate to face mouse
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

            transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg - 90);
        }

        if (velocity == Vector2.zero)
        {
            lerpTime += Time.deltaTime;

            if (state == PlayerState.running)
            {
                state = PlayerState.idle;
                UpdateAnimationState();
            }
        }
        else
        {
            lerpTime = .1f;

            if (state == PlayerState.idle)
            {
                state = PlayerState.running;
                UpdateAnimationState();
            }
        }

        if(Input.GetKeyDown(KeyCode.Q))
        {
            /*if (isAbilityActive)
                DeactivateAbility();

            else*/
            if(!isAbilityActive)
                UseAbility();
        }

        UpdateAbility();

        //update/pan camera
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, transform.position + new Vector3(Map(Input.mousePosition.x, 0, Screen.width, -maxCameraPanDist, maxCameraPanDist), Map(Input.mousePosition.y, 0, Screen.height, -maxCameraPanDist, maxCameraPanDist), -10), lerpTime);
    }

    //Updates physics
    void FixedUpdate()
    {
        rigidBody.velocity = velocity;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        //If got hit by a bullet, die
        if (other.gameObject.tag == "Bullet" && state != PlayerState.dead)
        {
            Vector2 bulletDir = other.gameObject.GetComponent<Bullet>().direction;

            transform.eulerAngles = new Vector3(0,0,Mathf.Atan2(bulletDir.y,bulletDir.x)*Mathf.Rad2Deg-90);

            state = PlayerState.dead;
            UpdateAnimationState();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Floppy")
        {
            GameObject.Find("WinText").GetComponent<Text>().text = "You win!";
            Invoke("BackToMenu", 2);

        }
    }

    private void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    //Get an ability from a terminal
    public void AddActiveAbility(ActiveAbilities actAb, float durationOrNumUses)//useAmt is either number of uses or num uses
    {
        activeAbility = actAb;

        percentActiveLeft = 100f;//Reset percent of active you can use

        activeBarDecreaseAmt = 100/durationOrNumUses;

        Debug.Log(activeAbility);

        GameObject.Find("qKeyPrompt").GetComponent<Image>().color = new Color(1,1,1,1);
    }

    public void AddPassiveAbility(PassiveAbilities pasAb)
    {
        switch(pasAb)
        {
            case PassiveAbilities.addSpeed:
                moveSpeed *= 1.2f;
                break;
            case PassiveAbilities.addDetectionResisitance:
                GameObject.Find("EnemyManager").GetComponent<EnemyManager>().spotTimeToAlert *= 1.1f;
                break;
        }

        Debug.Log(pasAb);
    }

    //Recharge ability bar to full
    public void RechargeActiveAbility()
    {
        percentActiveLeft = 100f;
    }

    //Use ability
    public void UseAbility()
    {
        if (activeAbility == ActiveAbilities.none || percentActiveLeft <= 0) return;//Don't do anything if don't have an ability or it's used up  

        GameObject.Find("qKeyPrompt").GetComponent<Image>().color = new Color(1, 1, 1, 0);

        switch (activeAbility)
        {
            case ActiveAbilities.invisible:
                isAbilityActive = true;

                gameObject.layer = 0;//Take off player layer so enemies can't see you

                legsSpriteRenderer.color = new Color(1,1,1,.5f);
                bodySpriteRenderer.color = new Color(1, 1, 1, .5f);//Make sprites semi-transparent

                break;

            case ActiveAbilities.dash:
                isAbilityActive = true;

                moveSpeed *= 2;
                dashTrail.enabled = true;
                break;
            case ActiveAbilities.shoot:
                percentActiveLeft -= activeBarDecreaseAmt;

                GameObject firedBullet = (GameObject)Instantiate(bullet,transform.position,Quaternion.identity);

                firedBullet.GetComponent<Bullet>().setUp(transform.up);

                break;
        }
    }

    private void DeactivateAbility()
    {
        isAbilityActive = false;//Deactive ability

        switch (activeAbility)
        {
            case ActiveAbilities.invisible:

                gameObject.layer = 8;

                legsSpriteRenderer.color = new Color(1, 1, 1, 1);
                bodySpriteRenderer.color = new Color(1, 1, 1, 1);

                
                break;

            case ActiveAbilities.dash:
                moveSpeed /= 2;
                dashTrail.enabled = false;
                break;
        }
    }

    private void UpdateAbility()
    {
        if (!isAbilityActive || activeAbility == ActiveAbilities.none) return;

        percentActiveLeft -= activeBarDecreaseAmt * Time.deltaTime;//Decrease timer

        //If used up, set active to false
        if(percentActiveLeft <= 0)
            DeactivateAbility();
    }

    /// <summary>
    /// Remapping function. Maybe move to a static helper methods class? Not sure if we'll need that though.
    /// </summary>
    private float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
    { return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget; }


    //Update animation/visual state of player based on their current state, always call when changing player state - should probably just make a method that changes the state with a parameter for desired state, on todo list
    void UpdateAnimationState()
    {
        if (state == PlayerState.idle)
        {
            legs.GetComponent<Animator>().Play("idle");
        }
        else if (state == PlayerState.running)
        {
            legs.GetComponent<Animator>().Play("run");
        }
        else if (state == PlayerState.dead)
        {
            legs.SetActive(false);//Disable legs so they don't display anymore
            body.GetComponent<SpriteRenderer>().sprite = deathSprite;//Set sprite to reflect death

            rigidBody.isKinematic = true;//Disable physics and stop the player's movement
            velocity = Vector2.zero;

            gameObject.layer = 0;//Change layer so camera won't keep shooting

            GameObject newPartSys = (GameObject)Instantiate(deathPartSys, transform.position, Quaternion.identity);//Spawn new instance of death part sys

            newPartSys.transform.eulerAngles = new Vector3(-this.transform.eulerAngles.z - 90, 0, 0);//Set rotation of death part sys to what it needs to be

            Invoke("Respawn", 2);
        }
    }

    void Respawn()
    {    
        ResetPlayer();
        this.transform.position = GameObject.Find("GameManager").GetComponent<GameManager>().RespawnLocation + new Vector3(0, 0, -2);
        GameObject.Find("EnemyManager").GetComponent<EnemyManager>().ResetAlarm();
    }

    void ResetPlayer()
    {
        body.GetComponent<SpriteRenderer>().sprite = bodySprite;

        state = PlayerState.idle;
        UpdateAnimationState();

        activeAbility = ActiveAbilities.none;

        rigidBody.isKinematic = false;

        gameObject.layer = 8;

        legs.SetActive(true);
    }
}
