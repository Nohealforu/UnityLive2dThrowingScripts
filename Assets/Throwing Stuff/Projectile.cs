using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //  Settings include weight, velocity, velocity variance, spread, rotation, rotation variance, 3d rotation (boolean), bounce velocity, bounce spread, bounce gravity, lifespan after bounce, impact sound (list of sounds?)
	//  Once activated by spawn point, calculates velocity and begins flying each frame, estimates eta based on distance and initial velocity
	//  Flies until reaching eta, initiates bounce based on settings and transfers momentum based on weight, velocity, and vector to model controller
    //  Possibly implement gravity of initial projectile, but will require aiming adjustments to make that work and limits to make sure target is always reachable.
    [SerializeField][Tooltip("The weight determines impact of projectile")] private int weight = 5;
    [SerializeField][Tooltip("Launch velocity of projectile (units per second)")] private float velocity = 20f;
    [SerializeField][Tooltip("Launch velocity variance of projectile")] private float velocityVariance = 8f;
    [SerializeField][Tooltip("Projectile spread (units radius at target position)")] private float spread = 0.5f;
    [SerializeField][Tooltip("Projectile rotation speed")] private float rotationSpeed = 100f;
    [SerializeField][Tooltip("Projectile rotation variance")] private float rotationVariance = 0.3f;
    [SerializeField][Tooltip("Enable to allow Z rotation")] private bool rotation3d = false;
    [SerializeField][Tooltip("Bounce velocity after target hit")] private float bounceVelocity = 6f;
    [SerializeField][Tooltip("Bounce velocity variance")] private float bounceVelocityVariance = 2f;
    [SerializeField][Tooltip("Bounce spread angle after target hit (% from reverse)")] private float bounceSpreadAngle = 0.50f;
    [SerializeField][Tooltip("Bounce gravity after target hit")] private float bounceGravity = 5f;
    [SerializeField][Tooltip("Projectile lifespan after bounce")] private float bounceLifespan = 2f;

    // todo: add sound effect, audioclip or something? idk how Unity sound works and have to figure out how to not break it with 100s of projectiles

    private Vector3 currentVelocity;
    private Vector3 currentRotationVector;
    private bool bouncing = false;
    private float bounceDuration;
    private float eta;
    private float flightTime = 0f;
    private ProjectileTarget target;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(target == null)
            return;
        float deltaTimeCapped = Math.Min(Time.deltaTime, 0.2f);
        float rotationAmount = deltaTimeCapped * rotationSpeed;
        transform.rotation *= Quaternion.AngleAxis(rotationAmount, currentRotationVector);
        transform.position += deltaTimeCapped * currentVelocity;
        flightTime += deltaTimeCapped;
        if(!bouncing && flightTime > eta)
        {
            bouncing = true;
            target.AddDamage(weight, currentVelocity);
            Vector3 bounceVector = Vector3.Lerp(currentVelocity.normalized * -1f, new Vector3(UnityEngine.Random.Range(-1f,1f), UnityEngine.Random.Range(-1f,1f), 0).normalized, UnityEngine.Random.Range(bounceSpreadAngle / 2, bounceSpreadAngle)); 
            currentVelocity = bounceVector.normalized * (bounceVelocity + UnityEngine.Random.Range(-bounceVelocityVariance, bounceVelocityVariance));
            currentRotationVector = (rotation3d ? UnityEngine.Random.onUnitSphere : new Vector3(0, 0, UnityEngine.Random.Range(-1f, 1f)));
            bounceDuration = 0f;
        }
        if(bouncing)
        {
            bounceDuration += deltaTimeCapped;
            currentVelocity.y -= bounceGravity * deltaTimeCapped;
            if(bounceDuration > bounceLifespan)
            {
                Destroy(this);
            }
            
            else if (bounceDuration > bounceLifespan / 2)
            {
                Color oldCol = GetComponent<Renderer>().material.color;
                Color newCol = new Color(oldCol.r, oldCol.g, oldCol.b, (bounceLifespan - bounceDuration) * 2 / bounceLifespan);
                GetComponent<Renderer>().material.color = newCol;
            }
        }
    }

    public void launchProjectile(ProjectileTarget launchTarget)
    {
        if(launchTarget == null)
        {
            Destroy(this);
        }
        else
        {
            target = launchTarget;
            Vector3 targetVector = target.GetTargetPosition() - transform.position + new Vector3(UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(-spread, spread), 0);
            currentVelocity = targetVector.normalized * (velocity + UnityEngine.Random.Range(-velocityVariance, velocityVariance));
            currentRotationVector = (rotation3d ? UnityEngine.Random.onUnitSphere : new Vector3(0, 0, UnityEngine.Random.Range(-1f, 1f)));
            currentRotationVector *= UnityEngine.Random.Range(1 - rotationVariance, 1 + rotationVariance);
            eta = targetVector.magnitude / currentVelocity.magnitude;
        }
    }

    public void OnDestroy()
    {
        if(TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
            Destroy(meshFilter.mesh);
        Destroy(gameObject);
    }
}
