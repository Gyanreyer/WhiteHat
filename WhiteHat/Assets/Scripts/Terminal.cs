using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Terminal : MonoBehaviour
{

    ActiveAbilities thisActive;//Ability that this terminal gives
    PassiveAbilities thisPassive;

    //Each float corresponds with ability at that index
    private float[] ActiveAbilitiesUsesOrDuration = {
        0,//None
        3,//Invisible, lasts 3 seconds
        3//,//Dash, lasts 3 seconds
        //1//Shoot, 1 bullet
    };

    private bool abilityIsActive;

    private Animator spriteAnimator;

    Player player;

    public float useRadius = 1f;

    public GameObject terminalPopup;
    private bool inRange;

    // Use this for initialization
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        spriteAnimator = GetComponent<Animator>();

        //Determine whether ability is active or passive, for now don't worry about it
        abilityIsActive = Random.value <= .45f;
        //abilityIsActive = true;

        if (abilityIsActive)
        {
            //thisActive = ActiveAbilities.shoot;
            thisActive = (ActiveAbilities)(int)Random.Range(1, ActiveAbilitiesUsesOrDuration.Length);//Pick a random ability from array
            spriteAnimator.Play("blue");
        }
        else
        {
            thisPassive = (PassiveAbilities)(int)Random.Range(0, 2);
            spriteAnimator.Play("orange");
        }

        terminalPopup = GameObject.Find("TerminalPopup");

    }

    void Start()
    {
        terminalPopup.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 vecToPlayer = player.transform.position - transform.position;

        if (vecToPlayer.magnitude < useRadius)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                //Give ability to player
                if (abilityIsActive)
                    player.AddActiveAbility(thisActive, ActiveAbilitiesUsesOrDuration[(int)thisActive]);
                else
                    player.AddPassiveAbility(thisPassive);

                Disable();
            }
            else if (Input.GetKeyDown(KeyCode.R) && player.activeAbility != ActiveAbilities.none)
            {
                player.RechargeActiveAbility();

                Disable();
            }

            if (!inRange)
            {
                terminalPopup.SetActive(true);
                terminalPopup.GetComponent<TerminalPopup>().UpdatePosition();
                if(abilityIsActive)
                    terminalPopup.GetComponent<TerminalPopup>().UpdateAbilityIcon(thisActive);
                else
                    terminalPopup.GetComponent<TerminalPopup>().UpdateAbilityIcon(thisPassive);
                inRange = true;
            }
        }
        else
        {
            if (inRange)
            {
                terminalPopup.SetActive(false);
                inRange = false;
            }
        }


    }


    void Disable()
    {
        //Change this terminal's sprite to indicate it's off
        spriteAnimator.Play("off");

        terminalPopup.SetActive(false);

        Destroy(this);//Can't interact with this anymore ever because just deleting this script
    }

    //Called when object was disabled in scene and was then re-enabled.  Need to start up proper animations accordingly
    void OnEnable()
    {
        if (abilityIsActive)
        {
            spriteAnimator.Play("blue");
        }
        else
        {
            spriteAnimator.Play("orange");
        }
    }
}