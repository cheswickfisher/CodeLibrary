using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Stats : MonoBehaviour
{
    [SerializeField]
    private float fallResistance;
    [Tooltip("reduces damage from impact forces")]
    [SerializeField][Min(0)]
    private float armor;
    [Tooltip("max 10,000hp")]
    [SerializeField][Min(1)]
    private float maxHealth;
    [Tooltip("percentage of max health recovered per second")]
    [SerializeField][Range(0, 1)]
    private float healthRegenRate;
    [SerializeField]
    private bool invulnerable;
    [SerializeField]
    private float health;

    bool canRegen;
    float regenStartInterval = 3.0f;
    float regenStartTime;

    DamageInfo damageInfo;

    protected virtual void Awake()
    {
        maxHealth = Mathf.Max(1f, Mathf.Min(10000f, maxHealth));
        health = maxHealth;
        regenStartTime = Time.time;
        damageInfo = new DamageInfo();
    }

    public void ApplyDamage(float impactForce, CollisionType collisionType)
    {
        if (!invulnerable)
        {
            damageInfo.Set(impactForce, armor, fallResistance, collisionType);
            float damage = DamageModel.CalculateImpactDamage(damageInfo);
            //Debug.Log("damage = " + damage);
            if(damage > 0) { regenStartTime = Time.time; }
            health = Mathf.Max(0, health - damage);
            AdjustHealthBar();
        }
    }

    /// <summary>
    /// used in functions that would be affected by damage. ie as multiplier in a movement function to cause the agent to move slower as more damage accumulates.
    /// </summary>
    /// <returns></returns>
    public float CurrentDamageFactor()
    {        
        return health / maxHealth;        
    }

    public void UpdateHealth()
    {
        if (health < maxHealth)
        {
            if (CanRegen())
            {
                health += healthRegenRate * maxHealth * Time.deltaTime;                
            }
        }

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        AdjustHealthBar();
    }

    private bool CanRegen()
    {
        if (Time.time - regenStartTime > regenStartInterval)
        {
            return true;
        }
        return false;
    }

    protected abstract void AdjustHealthBar();
}
