using UnityEngine;
using System.Collections;

public class AbilityBattery : MonoBehaviour {

    RectTransform rectTransform;
    Player p;

    float initialHeight;

	// Use this for initialization
	void Start () {
        rectTransform = GetComponent<RectTransform>();

        initialHeight = rectTransform.rect.height;

        p = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {

        rectTransform.sizeDelta = new Vector2(rectTransform.rect.width,(p.percentActiveLeft/100)*initialHeight);
	}
}
