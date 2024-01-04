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
    [SerializeField][Tooltip("Compared against weight & velocity of projectile for how much impact it creates")] private int resistance = 10000;
    [SerializeField][Tooltip("Seconds to fully recover from hits when at max damage taken")] private float recoveryTime = 2.0f;
    [SerializeField][Tooltip("Amount from 0-1 of bounce from exceeding max hit parameter amount")] private float schizoLevel = 0.25f;
    [SerializeField][Tooltip("Multiplier to reduce temp damage impact")] private float tempDamageDampen = 3f;
    [SerializeField][Tooltip("Multiplier to increase temp damage recovery rate")] private float tempDamageRecovery = 10f;
    [SerializeField][Tooltip("The parameters you want adjusted by x vector (Live2d default ParamAngleX, ParamBodyAngleX)")] private CubismParameter[] modelXParameters;
    [SerializeField][Tooltip("The parameters you want adjusted by y vector (Live2d default ParamAngleY, ParamBodyAngleY)")] private CubismParameter[] modelYParameters;
    [SerializeField][Tooltip("The parameters you want adjusted by z vector (Live2d default ParamAngleZ, ParamBodyAngleZ)")] private CubismParameter[] modelZParameters;
    [SerializeField][Tooltip("The parameters for expressions (cry, mad, etc.) you want increased by damage total (brought towards max)")] private CubismParameter[] modelExpressionParameters;
    [SerializeField][Tooltip("The parameters for expressions (cry, mad, etc.) you want decreased by damage total (brought towards 0 from max)")] private CubismParameter[] modelExpressionZeroParameters;

    private float damage = 0;
    private Vector3 peristentDamageVector;
    private Vector3 temporaryDamageVector;
    private int targetTopIndex = 0;
    private int targetBottomIndex = 0;

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
        GetYIndexBounds(targetMesh.VertexPositions, out targetTopIndex, out targetBottomIndex);
    }

    // Update is called once per frame
    void Update()
    {
        damage = Math.Max(damage - Time.deltaTime / recoveryTime * (damage > 1 ? damage : 1), 0f);
        peristentDamageVector = peristentDamageVector.normalized * damage;
        temporaryDamageVector = temporaryDamageVector * Math.Max(1f - Time.deltaTime * tempDamageRecovery, 0.5f);
    }

    private void LateUpdate()
    {
        if(peristentDamageVector.magnitude + temporaryDamageVector.magnitude > 0f)
        {
            foreach(CubismParameter param in modelXParameters)
                blendParameter(param, Vector3.right);
            foreach(CubismParameter param in modelYParameters)
                blendParameter(param, Vector3.up);
            foreach(CubismParameter param in modelZParameters)
                blendParameter(param, Vector3.forward);
        }
        if(damage > 0f)
        {
            foreach(CubismParameter param in modelExpressionParameters)
                param.BlendToValue(CubismParameterBlendMode.Additive, damage * param.MaximumValue);
            foreach(CubismParameter param in modelExpressionZeroParameters)
                param.BlendToValue(CubismParameterBlendMode.Additive, -damage * param.MaximumValue);
        }
    } 

    private void blendParameter(CubismParameter param, Vector3 paramPositiveDirection)
    {
        if(param == null)
            return;
        float paramRange = param.MaximumValue - param.MinimumValue;
        float paramCenter = (param.MaximumValue + param.MinimumValue) / 2;
        Vector3 damageVector = Vector3.Scale((peristentDamageVector + temporaryDamageVector), paramPositiveDirection);
        float newParamValue = param.Value + paramRange * (damageVector.x + damageVector.y + damageVector.z) + paramCenter;
        // use some math to bounce back from edge when parameter exceeds value.
        if(newParamValue + param.Value > param.MaximumValue)
            newParamValue = param.MaximumValue - (float)Math.Abs(Math.Cos((newParamValue + param.Value) * Math.PI / paramRange)) * paramRange * schizoLevel;
        else if(newParamValue + param.Value < param.MinimumValue)
            newParamValue = param.MinimumValue + (float)Math.Abs(Math.Cos((newParamValue + param.Value) * Math.PI / paramRange)) * paramRange * schizoLevel;
        param.BlendToValue(CubismParameterBlendMode.Additive, newParamValue);
    }

    public Vector3 GetTargetPosition()
    {
        if (targetMesh == null || target == null) return Vector3.zero;
        return targetMesh.transform.position + Vector3.Scale(Vector3.Lerp(targetMesh.VertexPositions[targetTopIndex], targetMesh.VertexPositions[targetBottomIndex], 0.5f), target.transform.localScale);
    }

    public void AddDamage(int weight, Vector3 impactVector)
    {
        impactVector.z = impactVector.x + (impactVector.x > 0 ? Math.Abs(impactVector.y) : 0 - Math.Abs(impactVector.y));
        float impactDamage = impactVector.magnitude * (float)weight / (float)resistance;
        temporaryDamageVector = (impactVector * impactDamage).normalized / tempDamageDampen; // This squares velocity since it is stored in vector, which is desired, but still normalized so it's not crazy
        peristentDamageVector += impactVector.normalized * impactDamage; // This one does not square velocity
        damage += impactDamage;
    }

    private void GetYIndexBounds(Vector3[] array, out int targetTopIndex, out int targetBottomIndex)
    {
        float highestY = array[0].y;
        float lowestY = array[0].y;
        targetTopIndex = 0;
        targetBottomIndex = 0;

        for (int i = 1; i < array.Length; i++)
        {
            if (array[i].y > highestY)
            {
                highestY = array[i].y;
                targetTopIndex = i;
            }
            else if(array[i].y < lowestY)
            {
                lowestY = array[i].y;
                targetBottomIndex = i;
            }
        }

        return;
    }
}
