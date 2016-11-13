using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TextureTilingController : MonoBehaviour {

    public Texture texture;
    public float textureToMeshY = 2f;//Contain texture to certain size

    Vector3 prevScale = Vector3.one;

    float prevTextureToMeshY = -1f;

	// Use this for initialization
	void Start () {
        prevScale = gameObject.transform.lossyScale;
        prevTextureToMeshY = textureToMeshY;

        UpdateTiling();
	}
	
	// Update is called once per frame
	void Update () {
	    if(transform.lossyScale != prevScale || !Mathf.Approximately(textureToMeshY, prevTextureToMeshY))
        {
            UpdateTiling();
        }

        prevScale = transform.lossyScale;
        prevTextureToMeshY = textureToMeshY;
	}

    [ContextMenu("UpdateTiling")]
    void UpdateTiling()
    {
        float planeSizeX = 10f;
        float planeSizeY = 10f;

        float textureToMeshX = ((float)texture.width / texture.height) * textureToMeshY;

        GetComponent<Renderer>().sharedMaterial.mainTextureScale = new Vector2(planeSizeX*transform.lossyScale.x/textureToMeshX,planeSizeY*transform.lossyScale.y/textureToMeshY);
    }
}
