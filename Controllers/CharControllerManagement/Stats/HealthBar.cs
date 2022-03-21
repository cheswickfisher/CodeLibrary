using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    Camera mainCamera;
    MeshRenderer meshRenderer;
    MaterialPropertyBlock matBlock;

    public MeshRenderer _MeshRenderer { get => meshRenderer; }

    private void Awake()
    {
        mainCamera = Camera.main;
        meshRenderer = GetComponent<MeshRenderer>();
        matBlock = new MaterialPropertyBlock();        
    }

    public void AlignCamera()
    {
        if (mainCamera != null)
        {
            var camXform = mainCamera.transform;
            var forward = transform.position - camXform.position;
            forward.Normalize();
            var up = Vector3.Cross(forward, camXform.right);
            transform.rotation = Quaternion.LookRotation(forward, up);
        }
    }

    public void UpdateParams(float currentDamageFactor)
    {
        meshRenderer.GetPropertyBlock(matBlock);
        matBlock.SetFloat("_Fill", currentDamageFactor);
        meshRenderer.SetPropertyBlock(matBlock);
    }
}
