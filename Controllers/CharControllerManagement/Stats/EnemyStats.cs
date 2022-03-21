using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : Stats
{
    [SerializeField] HealthBar healthBarPrefab;
    HealthBar healthBarClone;
    [SerializeField]
    float healthBarHeight;

    private float checkDistance = 20.0f;

    protected override void Awake()
    {
        GenerateHealthBar();
        base.Awake();
    }

    protected override void AdjustHealthBar()
    {
        if (CheckDistanceToPlayer())
        {
            healthBarClone.AlignCamera();
        }
        if (CurrentDamageFactor() != 1)
        {
            healthBarClone.UpdateParams(CurrentDamageFactor());
        }
    }

    private bool CheckDistanceToPlayer()
    {
        float distance = (GameController.playerTransform.position - transform.position).sqrMagnitude;
        if(distance < checkDistance * checkDistance)
        {
            if (!healthBarClone._MeshRenderer.enabled) { healthBarClone._MeshRenderer.enabled = true; }
            return true;
        }
        if (healthBarClone._MeshRenderer.enabled) { healthBarClone._MeshRenderer.enabled = false; }
        return false;
    }

    private void GenerateHealthBar()
    {
        healthBarClone = Instantiate(healthBarPrefab);
        healthBarClone.transform.SetParent(transform);
        healthBarClone.transform.localPosition = new Vector3(0, healthBarHeight, 0);
    }
}
