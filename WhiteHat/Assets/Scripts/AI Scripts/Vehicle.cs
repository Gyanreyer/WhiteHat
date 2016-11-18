using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class Vehicle : MonoBehaviour
{
    public float maxSpeed = 6.0f;
    public float maxForce = 12.0f;
    public float mass = 1.0f;
    public float radius = 1.0f;
    public float tooCloseDist = 10f;
    public float wallFollowDist = 5f;
    public float wallFollowNormRange = 10f;
    public float wallFollowWeight = 2f;
    public LayerMask wallLayer;

    #region Fields
    protected Vector2 acceleration;
    protected Vector2 velocity;
    protected Vector2 desired;
    protected Vector2 force;
    #endregion

    #region Properties
    public Vector2 Velocity
    {
        get { return velocity; }
    }
    #endregion

    #region Unity Defaults
    virtual public void Start()
    {
        acceleration = Vector2.zero;
        velocity = transform.forward;
    }
    virtual public void Update()
    {
        //calc steering forces
        CalcSteeringForces();
        //add acc to vel
        velocity += acceleration * Time.deltaTime;
        //limit vel
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        //Rotate (-90 because the view has to match the direction of the robot)
        this.transform.eulerAngles = new Vector3(0, 0, (Mathf.Rad2Deg * Mathf.Atan2(this.velocity.y, this.velocity.x)) - 90);
        //move character
        this.transform.position += new Vector3(velocity.x * Time.deltaTime, velocity.y * Time.deltaTime, 0);
        //reset acc
        acceleration = Vector2.zero;
    }
    #endregion

    #region Methods
    abstract protected void CalcSteeringForces();
    protected void ApplyForce(Vector2 f)
    {
        acceleration += f / mass;
    }
    protected Vector2 Seek(Vector2 targetLoc)
    {
        desired = targetLoc - new Vector2(this.transform.position.x, this.transform.position.y);
        desired = desired.normalized * maxSpeed;
        desired -= velocity;
        return desired;
    }
    protected Vector2 WallFollow()
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(this.transform.position, this.transform.up, wallFollowDist, wallLayer);
        if (hitInfo)
        {
            Debug.DrawLine(this.transform.position, hitInfo.point, Color.green);
            //Debug.DrawRay(new Vector2(hitInfo.transform.position.x, hitInfo.transform.position.y), new Vector2(hitInfo.transform.position.x, hitInfo.transform.position.y) + hitInfo.normal * wallFollowNormRange);
            return (new Vector2(hitInfo.point.x, hitInfo.point.y) + hitInfo.normal * wallFollowNormRange) * wallFollowWeight;
        }
        return Vector2.zero;
    }
    #endregion
}
