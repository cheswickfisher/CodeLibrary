using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DamageInfo
{
    float rawForce;
    CollisionType collisionType;
    float armor;
    float fallResistance;

    public float RawForce { get => rawForce;  }
    public CollisionType CollisionType { get => collisionType; }
    public float Armor { get => armor;  }
    public float FallResistance { get => fallResistance; }

    public void Set(float rawForce, float armor, float fallResistance, CollisionType collisionType) 
    {
        this.rawForce = rawForce;
        this.armor = armor;
        this.fallResistance = fallResistance;
        this.collisionType = collisionType;
    } 
}

public enum CollisionType
{
    Ground,
    InterAgent
}

public static class DamageModel 
{
    const float baseDamageReduction = 0f;
    const float baseFallDamageReduction = 270f;
    
    public static float CalculateImpactDamage(DamageInfo damageInfo)
    {        
        switch (damageInfo.CollisionType)
        {
            case CollisionType.Ground:
                //Debug.Log("rawForce = " + damageInfo.RawForce);
                return Mathf.Max(damageInfo.RawForce - baseFallDamageReduction, 0) * .5f;
            case CollisionType.InterAgent:
                float damage = Mathf.Max(damageInfo.RawForce - damageInfo.Armor - baseDamageReduction, 0);
                //Debug.Log("damage = " + damage + " , rawForce = " + damageInfo.RawForce);
                return Mathf.Max(damageInfo.RawForce  - damageInfo.Armor - baseDamageReduction, 0) ;
            default:
                return 0;
        }
    } 

}
