using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//based off CatlikeCoding(Jasper Flick) Camera tutorial
public class CameraFollow : MonoBehaviour
{

    public Transform target;
    public Transform playerInputSpace = default;
    public float rotationSpeed = 100.0f;
    public float followSpeed = 10.0f;
    public float alignDelay = 5.0f;
    public float alignSmoothRange = 45f;
    public bool flipY = true;
    public bool flipX = true;
    public float distance = 5.0f;
    float xRot;
    float yRot;
    float lastManualRotationTime;
    Vector3 prevPos;
    Vector3 curPos;
    public float focusRadius = 1f;
    public float focusCentering = .5f;
    Vector3 focusPoint, prevFocusPoint;
    Vector2 orbitAngles = new Vector2(45f, 0f);
    public Vector3 mousePosition;
    public Vector2 screenCenter;
    public Vector3 centeredMousePos;
    public float leftBound;
    public float rightBound;
    public float topBound;
    public float bottomBound;
    bool moveCamera;
    Camera cam;
    public GameObject waterView;
    public bool underwater;
    public BoxCollider waterBoundingVolume;
    private void Awake()
    {
        focusPoint = target.position;
        transform.localRotation = Quaternion.Euler(orbitAngles);
        cam = GetComponent<Camera>();
        waterView.SetActive(false);
    }
    private void Start()
    {
        Cursor.visible = false;

    }
    private void LateUpdate()
    {
        bool prevUnderwater = underwater;
        underwater = CheckInsideWaterBoundingVolume(transform.position);
        if (underwater)
        {
            Underwater();
        }
        if(prevUnderwater == true && underwater == false)
        {
            waterView.SetActive(false);
        }

    }
    void FixedUpdate()
    {

        mousePosition = Input.mousePosition;
        screenCenter = new Vector2(Screen.width * .5f, Screen.height * .5f);

        centeredMousePos = new Vector3(mousePosition.x - screenCenter.x, mousePosition.y - screenCenter.y, mousePosition.z);
        leftBound = -screenCenter.x ;
        rightBound = screenCenter.x;
        topBound = screenCenter.y;
        bottomBound = -screenCenter.y;

        const float error = 5f;

       // if(Input.GetMouseButton(1) || Input.GetMouseButton(0)) { moveCamera = true; }
       
       // else { moveCamera = false; }

        UpdateFocusPoint();
        Quaternion lookRotation;
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            lookRotation = Quaternion.Euler(orbitAngles);
        }    
		else 
        {
			lookRotation = transform.localRotation;
		}
        distance -= Input.GetAxis("Mouse ScrollWheel") * 10.0f;     
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
        transform.localPosition = focusPoint - lookDirection * distance;
    }

    void UpdateFocusPoint()
    {
        prevFocusPoint = focusPoint;
        Vector3 targetPoint = target.position;
        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
                if (distance > focusRadius)
                {
                    t = Mathf.Min(t, focusRadius / distance);
                }
                focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
            }
        }
        else
        {
            focusPoint = targetPoint;
        }
    }

    bool ManualRotation()
    {
        Vector2 input = new Vector2(
            flipY ? -Input.GetAxis("Mouse Y") : Input.GetAxis("Mouse Y"),
            flipX ? -Input.GetAxis("Mouse X"): Input.GetAxis("Mouse X")
        );

        const float e = 0.001f;
        if ((input.x < -e || input.x > e || input.y < -e || input.y > e) /*&& moveCamera*/)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }

    bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        {
            return false;
        }
        Vector2 movement = new Vector2(
            focusPoint.x - prevFocusPoint.x,
            focusPoint.z - prevFocusPoint.z
        );
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.000001f)
        {
            return false;
        }
        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < alignSmoothRange)
        {
            rotationChange *= deltaAbs / alignSmoothRange;
        }
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        return true;
    }

    void ConstrainAngles()
    {
        if (orbitAngles.x < 0f)
        {
            orbitAngles.x += 360f;
        }
        else if (orbitAngles.x >= 360f)
        {
            orbitAngles.x -= 360f;
        }

        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }

    static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }

    private void Underwater()
    {
        waterView.gameObject.SetActive(true);

        float x = waterView.gameObject.transform.localScale.x;
        float y = waterView.gameObject.transform.localScale.y;

        float newX = y * cam.aspect;
        waterView.gameObject.transform.localScale = new Vector3(newX, y, 1);

        waterView.gameObject.transform.forward = cam.transform.forward;
        waterView.gameObject.transform.position = cam.transform.position + cam.transform.forward * (cam.nearClipPlane + .01f);
    }

    private bool CheckInsideWaterBoundingVolume(Vector3 point)
    {
        return waterBoundingVolume.bounds.Contains(point);
    }

}
