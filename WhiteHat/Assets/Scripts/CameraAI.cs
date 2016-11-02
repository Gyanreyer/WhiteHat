using UnityEngine;
using System.Collections;

public class CameraAI : MonoBehaviour
{
    #region Public Fields
    /// <summary>
    /// Rotation at the end of animation, 90 is only default
    /// </summary>
    public float endRot = 90f;
    /// <summary>
    /// Minimum rotation - the camera won't go past this when tracking the player
    /// </summary>
    public float minRot = 0f;
    /// <summary>
    /// Maximum rotation - the camera also won't go past this
    /// </summary>
    public float maxRot = 90f;
    /// <summary>
    /// The width of the sight cone in degrees
    /// </summary>
    public float sightWidthAngle = 45f;
    /// <summary>
    /// The length of the sight cone
    /// </summary>
    public float sightRange = 10f;
    /// <summary>
    /// Speed that the camera will rotate
    /// </summary>
    public float rotationSpeed = 2f;
    /// <summary>
    /// Time to wait at ends while in patrol phase, in seconds
    /// </summary>
    public float waitDuration = 1f;
    #endregion
    #region Private Fields
    //Enum of possible states
    private enum CameraState
    {
        Patroling,
        Waiting,
        Following
    }
    //Current camera state
    private CameraState camState = CameraState.Waiting;
    //A reference to the player
    private GameObject playerObj;
    //Whether the camera is going towards its end rotation or not
    private bool rotatingRight = true;
    //The total rotation... perhaps a better way to do this but it's all I remember how to do
    private float totalRotation = 0f;
    //A vector from here to the player
    private Vector3 vecToPlayer;
    //Time spent waiting during wait state
    private float waitingTime = 0f;
    //
    private Renderer render;//DEBUG//
    #endregion
    #region Unity Defaults
    void Start()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        render = this.GetComponent<Renderer>();//DEBUG//
        render.material.shader = Shader.Find("Specular");//DEBUG//
    }

    void Update()
    {
        //Handles camera states
        switch (camState)
        {
            case CameraState.Waiting:
                Wait();
                break;
            case CameraState.Patroling:
                Patrol();
                break;
            case CameraState.Following:
                Follow();
                break;
        }
        //Check for LoS
        if (CanSeePlayer())
        {
            camState = CameraState.Following;
            render.material.SetColor("_SpecColor", Color.red);//DEBUG//
        }
        else
        {
            if (camState == CameraState.Following)
                camState = CameraState.Patroling;
            render.material.SetColor("_SpecColor", Color.gray);//DEBUG//
        }
    }
    #endregion
    #region Methods
    /// <summary>
    /// Checks to see if the camera can see the player given its current position and rotation
    /// </summary>
    private bool CanSeePlayer()
    {
        //get vec between this and player
        vecToPlayer = playerObj.GetComponent<Transform>().position - this.gameObject.transform.position;
        //use dot product to project forward vector onto that - see if player is in front
        float forwardDot = Vector3.Dot(this.transform.up, vecToPlayer);
        //instead of doing a bunch of ifs and then returning true or false, just put 'em all in a single return statement
        return (forwardDot > 0 && forwardDot < sightRange && Vector3.Angle(vecToPlayer, this.transform.up) <= sightWidthAngle);
    }
    /// <summary>
    /// Follow the player; runs when in the following state
    /// </summary>
    private void Follow()
    {
        //check to see if we should rotate based on angle to player
        if (Vector3.Angle(vecToPlayer, this.transform.up) > rotationSpeed)
        {
            //use dot produce to see if it's to the left or right
            float rightDot = Vector3.Dot(this.transform.right, vecToPlayer);
            //rotate depending on dot result
            totalRotation -= rotationSpeed * rightDot / Mathf.Abs(rightDot);
            //clamp rotation
            if (totalRotation > maxRot)
                totalRotation = maxRot;
            else if (totalRotation < minRot)
                totalRotation = minRot;
            //update rotation
            this.transform.eulerAngles = new Vector3(0, 0, totalRotation);
        }
        //this.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.transform.up, vecToPlayer, rotationSpeed * Time.deltaTime, 0.0f));
    }
    /// <summary>
    /// Wait at one of two extremes; runs in waiting state
    /// </summary>
    private void Wait()
    {
        waitingTime += Time.deltaTime;
        if (waitingTime >= waitDuration)
        {
            waitingTime = 0;
            camState = CameraState.Patroling;
        }
    }
    /// <summary>
    /// Rotates back and forth; runs in patrol state
    /// </summary>
    private void Patrol()
    {
        if (rotatingRight)
        {
            totalRotation += rotationSpeed;
            if (totalRotation > maxRot)
            {
                totalRotation = maxRot;
                rotatingRight = false;
                camState = CameraState.Waiting;
            }
        }
        else
        {
            totalRotation -= rotationSpeed;
            if (totalRotation < minRot)
            {
                totalRotation = minRot;
                rotatingRight = true;
                camState = CameraState.Waiting;
            }
        }
        this.transform.eulerAngles = new Vector3(0, 0, totalRotation);
    }
    #endregion
}
