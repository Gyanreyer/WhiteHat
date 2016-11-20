
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TerminalPopup : MonoBehaviour
{
    public Sprite[] abilitySprites;
    public Sprite[] passiveSprites;

    private Image abilityIcon;
    private Text abilityText;

    private GameObject playerGO;
    private RectTransform imageTransform;

    // Use this for initialization
    void Awake()
    {
        playerGO = GameObject.FindGameObjectWithTag("Player");

        imageTransform = GetComponent<RectTransform>();

        abilityIcon = transform.FindChild("NewAbilityIcon").GetComponent<Image>();
        abilityText = transform.FindChild("AbilityText").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        transform.position = playerGO.transform.position - new Vector3(playerGO.transform.up.x * imageTransform.rect.width * .8f, playerGO.transform.up.y * imageTransform.rect.height * .8f, 0);
    }

    public void UpdateAbilityIcon(ActiveAbilities ability)
    {
        Sprite newSprite = new Sprite();
        string newText = "Take Ability\n";

        switch (ability)
        {
            case ActiveAbilities.invisible:
                newSprite = abilitySprites[0];
                newText += "(Invisibility)";
                break;

            case ActiveAbilities.dash:
                newSprite = abilitySprites[1];
                newText += "(Dash)";
                break;

            case ActiveAbilities.shoot:
                newSprite = abilitySprites[1];
                newText += "(Gun)";
                break;

        }

        abilityText.text = newText;

        abilityIcon.sprite = newSprite;
    }

    public void UpdateAbilityIcon(PassiveAbilities ability)
    {
        Sprite newSprite = new Sprite();
        string newText = "Modify Stat\n";

        switch(ability)
        {
            case PassiveAbilities.addSpeed:
                newSprite = passiveSprites[0];
                newText += "(Move faster)";
                break;

            case PassiveAbilities.addDetectionResisitance:
                newSprite = passiveSprites[1];
                newText += "(Harder to detect)";
                break;            
        }


        abilityText.text = newText;

        abilityIcon.sprite = newSprite;
    }
}