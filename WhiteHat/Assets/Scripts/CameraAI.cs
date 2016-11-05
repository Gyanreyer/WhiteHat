using UnityEngine;
using System.Collections;

public class CameraAI : MonoBehaviour
{
    #region Public Fields
    [Tooltip("Minimum rotation - the camera won't go past this when tracking the player")]
    public float minRot = 0f;
    [Tooltip("Maximum rotation - the camera also won't go past this")]
    public float maxRot = 90f;
    [Tooltip("Speed that the camera will rotate")]
    public float rotationSpeed = 2f;
    [Tooltip("Time to wait at ends while in patrol phase, in seconds")]
    public float waitDuration = 1f;
    #endregion
    #region Private Fields
    //Enum of possible states
    private enum CameraStates
    {
        Patroling,
        Waiting,
        Following
    }
    //Current camera state
    private CameraStates camState = CameraStates.Waiting;
    //A reference to the player
    private GameObject playerObj;
    //Reference to the enemy manager
    private EnemyManager enemyMan;
    //Whether the camera is going towards its end rotation or not
    private bool rotatingRight = true;
    //The total rotation... perhaps a better way to do this but it's all I remember how to do
    private float totalRotation = 0f;
    //A vector from here to the player
    private Vector3 vecToPlayer;
    //Time spent waiting during wait state
    private float waitingTime = 0f;
    //Time to trigger an alarm, in seconds
    private float timeToAlert = 2f;
    //Time spent staring at the player
    private float spottingTime = 0f;
    //Renderer for the FOV mesh
    private Renderer render;

    private FieldOfView fov;//Field of view for camera, mainly handles visualation but also returns if player is in sight

    public GameObject bulletPrefab;
    public float fireRate;//Shots per second

    private float fireTime=0f;

    #endregion
    #region Properties
    public float TimeToAlert
    {
        set { timeToAlert = value; }
    }
    #endregion
    #region Unity Defaults
    void Start()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        enemyMan = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
        render = this.transform.GetChild(0).GetComponent<Renderer>();
        render.material.SetColor("_Color", new Color(221, 221, 221, 0.4f));

        fov = GetComponent<FieldOfView>();
    }

    void Update()
    {
        //set view mesh color based on alert state
        switch (enemyMan.AlertState)
        {
            case EnemyManager.AlertStates.Patrol:
                render.material.SetColor("_Color", new Color(221, 221, 221, 0.4f));
                break;
            case EnemyManager.AlertStates.Alarmed:
                render.material.SetColor("_Color", new Color(255, 0, 0, 0.4f));
                break;
            case EnemyManager.AlertStates.Searching:
                render.material.SetColor("_Color", new Color(255, 255, 0, 0.4f));
                break;
        }
        //Handles camera states
        switch (camState)
        {
            case CameraStates.Waiting:
                Wait();
                break;
            case CameraStates.Patroling:
                Patrol();
                break;
            case CameraStates.Following:
                Follow();
                ShootPlayer();
                break;
        }
        //Check for LoS
        if (fov.playerVisible)//(CanSeePlayer())
        {
            vecToPlayer = playerObj.transform.position - this.gameObject.transform.position;
            camState = CameraStates.Following;
        }
        else
        {
            //reset spotting time
            spottingTime = 0;
            if (camState == CameraStates.Following)
                camState = CameraStates.Patroling;
        }
    }
    #endregion
    #region Methods
    /// <summary>
    /// Follow the player; runs when in the following state
    /// </summary>
    private void Follow()
    {
        //Get forward dot for distance
        float forwardDot = Vector3.Dot(this.transform.up, vecToPlayer);
        //increase time staring at player, modified inversely by distance
        spottingTime += Time.deltaTime / forwardDot;
        //check to see if you should trigger an alarm
        if (spottingTime >= timeToAlert)
        {
            enemyMan.TriggerAlarm();
        }
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
        //Update mesh color based on alert state
        switch (enemyMan.AlertState)
        {
            case EnemyManager.AlertStates.Patrol:
                render.material.SetColor("_Color", new Color(255, 255, 0, 0.4f));
                break;
            case EnemyManager.AlertStates.Alarmed:
                render.material.SetColor("_Color", new Color(255, 0, 0, 0.4f));
                break;
            case EnemyManager.AlertStates.Searching:
                render.material.SetColor("_Color", new Color(255, 0, 0, 0.4f));
                break;
        }
    }
    /// <summary>
    /// Wait at one of two extremes; runs in waiting state
    /// </summary>
    private void Wait()
    {
        //wait a little longer
        waitingTime += Time.deltaTime;
        //check to see if we're done
        if (waitingTime >= waitDuration)
        {
            waitingTime = 0;
            camState = CameraStates.Patroling;
        }
    }
    /// <summary>
    /// Rotates back and forth; runs in patrol state
    /// </summary>
    private void Patrol()
    {
        //if we're rotating in the positive direction
        if (rotatingRight)
        {
            //increase rotation
            totalRotation += rotationSpeed;
            //check if you've hit bounds
            if (totalRotation > maxRot)
            {
                //clamp rotation
                totalRotation = maxRot;
                //swawp direction
                rotatingRight = false;
                //change state
                camState = CameraStates.Waiting;
            }
        }
        else//inverse of previous blocks
        {
            totalRotation -= rotationSpeed;
            if (totalRotation < minRot)
            {
                totalRotation = minRot;
                rotatingRight = true;
                camState = CameraStates.Waiting;
            }
        }
        //update actual rotation
        this.transform.eulerAngles = new Vector3(0, 0, totalRotation);
    }
    #endregion


    private void ShootPlayer()
    {
        if (fireTime < 1 / fireRate)
        {
            fireTime += Time.deltaTime;
            return;
        }

        fireTime = 0;
        GameObject bullet = (GameObject)Instantiate(bulletPrefab,transform.position,Quaternion.identity);
        bullet.GetComponent<Bullet>().setUp(transform.position,vecToPlayer);

    }
}
