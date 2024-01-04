using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;

public class ProjectileTarget : MonoBehaviour
{
    //  Settings include resistance, recovery rate, (optional: expressions list, index of hit, index of defeated, defeated threshold, recovery time,) head X, head Y, head Z, body X, body Y, body Z parameters?
    //	Stores and updates total damage imparted by projectiles, updates parameters and expressions based on damage done and reduces impact over time by recovery rates 
    [SerializeField][Tooltip("The target model")] public CubismModel target;
    [SerializeField][Tooltip("The target artmesh (found in \"Drawables\")")] public CubismDrawable targetMesh;
    [SerializeField][Tooltip("Compared against weight & velocity of projectile for how much impact it creates")] private int resistance = 1000;
    [SerializeField][Tooltip("Seconds to fully recover from hits when at max damage taken")] private float recoveryTime = 2.0f;
    [SerializeField][Tooltip("Amount from 0-1 of bounce from exceeding max hit parameter amount")] private float schizoLevel = 0.25f;
    [SerializeField][Tooltip("The parameter for model head x rotation (Live2d default ParamAngleX)")] private CubismParameter modelHeadX;
    [SerializeField][Tooltip("The parameter for model head y rotation (Live2d default ParamAngleY)")] private CubismParameter modelHeadY;
    [SerializeField][Tooltip("The parameter for model head z rotation (Live2d default ParamAngleZ)")] private CubismParameter modelHeadZ;
    [SerializeField][Tooltip("The parameter for model body x rotation (Live2d default ParamBodyAngleX)")] private CubismParameter modelBodyX;
    [SerializeField][Tooltip("The parameter for model body y rotation (Live2d default ParamBodyAngleY)")] private CubismParameter modelBodyY;
    [SerializeField][Tooltip("The parameter for model body z rotation (Live2d default ParamBodyAngleZ)")] private CubismParameter modelBodyZ;

    private float damage = 0;
    private Vector3 peristentDamageVector;
    private Vector3 temporaryDamageVector;
    private int targetTopIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        peristentDamageVector = Vector3.zero;
        temporaryDamageVector = Vector3.zero;
        if (targetMesh == null || target == null)
        {
            Debug.LogError("Target null.");
            return;
        }
        targetTopIndex = GetHighestYIndex(targetMesh.VertexPositions); // Calculate the highest point of the target mesh, stolen from tutel on head, probably should be midpoint idk
    }

    // Update is called once per frame
    void Update()
    {
        damage = Math.Max(damage - Time.deltaTime / recoveryTime, 0f);
        peristentDamageVector = peristentDamageVector.normalized * damage;
        temporaryDamageVector = temporaryDamageVector * Math.Max(1f - Time.deltaTime * 10, 0.5f);
    }

    private void LateUpdate()
    {
        if(peristentDamageVector.magnitude + temporaryDamageVector.magnitude > 0.02f)
        {
            blendParameter(modelHeadX, Vector3.right);
            blendParameter(modelHeadY, Vector3.up);
            blendParameter(modelHeadZ, Vector3.forward);
            blendParameter(modelBodyX, Vector3.right);
            blendParameter(modelBodyY, Vector3.up);
            blendParameter(modelBodyZ, Vector3.forward);
        }
    } 

    private void blendParameter(CubismParameter param, Vector3 paramPositiveDirection)
    {
        if(param == null)
            return;
        float paramRange = param.MaximumValue - param.MinimumValue;
        float paramCenter = (param.MaximumValue + param.MinimumValue) / 2;
        // THIS IS WRONG and doesn't work right if the vector3 x,y,z component that matches paramPositiveDirection is negative because a magnitude is always positive.
        // I was trying to do something clever/lazy and it was actually dumb.
        // TODO: Fix this with something like:
        // Vector3 damageVector = Vector3.Scale((peristentDamageVector + temporaryDamageVector), paramPositiveDirection).magnitude;
        // float newParamValue = param.Value + paramRange * (damageVector.x + damageVector.y + damageVector.z) + paramCenter;
        float newParamValue = param.Value + paramRange * Vector3.Scale((peristentDamageVector + temporaryDamageVector), paramPositiveDirection).magnitude + paramCenter;
        if(newParamValue + param.Value > param.MaximumValue || newParamValue + param.Value < param.MinimumValue)
            newParamValue = paramCenter + (newParamValue + param.Value) % ((paramRange / 2) * schizoLevel);
        param.BlendToValue(CubismParameterBlendMode.Additive, newParamValue);
    }

    public Vector3 GetTargetPosition()
    {
        if (targetMesh == null || target == null) return Vector3.zero;
        // return Vector3.Scale(target.transform.position, target.transform.lossyScale);
        return targetMesh.transform.position + Vector3.Scale(targetMesh.VertexPositions[targetTopIndex], target.transform.localScale);
    }

    public void AddDamage(int weight, Vector3 impactVector)
    {
        impactVector.z = impactVector.x + (impactVector.x > 0 ? Math.Abs(impactVector.y) : 0 - Math.Abs(impactVector.y));
        float impactDamage = impactVector.magnitude * (float)weight / (float)resistance;
        temporaryDamageVector = (impactVector * impactDamage).normalized; // This squares velocity since it is stored in vector, which is desired, but still normalized so it's not crazy
        peristentDamageVector += impactVector.normalized * impactDamage; // This one does not square velocity
        Debug.Log($"peristentDamageVector: {peristentDamageVector}");
    }

    private int GetHighestYIndex(Vector3[] array)
    {
        int highestIndex = 0;
        float highestY = array[0].y;

        for (int i = 1; i < array.Length; i++)
        {
            if (array[i].y > highestY)
            {
                highestY = array[i].y;
                highestIndex = i;
            }
        }

        return highestIndex;
    }
}
