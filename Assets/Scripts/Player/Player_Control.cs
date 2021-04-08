using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player_Control : MonoBehaviour
{
    // Variables
    [Header("Components")]
    private CharacterController controller;
    private GameObject offsetPart;
    private Source chanellingPart;
    private Camera playerCamera;

    [Header("Control")]
    public float moveMultiplier;
    public float offsetMultiplier;
    public Vector2 cameraLookSpeedLimits;
    public float cameraLookAcceleration;

    public Vector2 cameraZoomLimits;
    public float detailThreshold;
    public float cameraZoomDuration;
    public float scrollMultiplier;

    public float heightConstraint;
    public Vector3 resetHeight 
    { 
        get
        {
            Vector3 current = transform.position;
            current.y = heightConstraint;
            return current;
        } 
    }

    [Header("Interactions")]
    public float collisionForceMultiplier;

    [Header("Scripts")]
    public Build buildScript;

    // Calculations
    private const int lookOffset = 90;
    private float cameraLookSpeed;
    public float zoomTarget { get; private set; }
    public float zoomLevel { get
        {
            float range = cameraZoomLimits.y - cameraZoomLimits.x;
            float target = playerCamera.transform.position.y - cameraZoomLimits.x;
            return target / range;
        } 
    }
    private IEnumerator zoomSmooth;

    private Vector3 oldMouse;

    // Functions
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        offsetPart = transform.Find("Offset Part").gameObject;
        chanellingPart = transform.Find("Chanelling Part").GetComponent<Source>();
        playerCamera = Camera.main;
        zoomTarget = playerCamera.transform.position.y;
    }

    private void Update()
    {
        Move();
        Look();
        Zoom();

        if (transform.position.y != heightConstraint)
        {
            transform.position = resetHeight;
        }
        if (Input.GetKeyDown("q"))
        {
            buildScript.ToggleState();
        }
        if (Input.GetMouseButtonDown(0))
        {
            buildScript.Trigger();
        }
        if (Input.GetKeyDown("e"))
        {
            StartCoroutine(Use());
        }
    }

    public void Move() // Get the xy input as a vector 2, and apply it with a speed multiplier over delta time.
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        controller.Move(move * moveMultiplier * Time.deltaTime);
        // Calculate FOV shadows
    }

    public void Look() // Offset the camera based on mouse position, and rotate the character model.
    {
        // Get mouse input
        oldMouse = Input.mousePosition;
        Vector2 mouse = Input.mousePosition;
        Vector2 middle = new Vector2(Screen.width, Screen.height) / 2;
        Vector2 cameraOffset = mouse - middle;

        // Find camera movement
        float zoomMultiplier = Mathf.Clamp(zoomLevel, 0.1f, 1);
        Vector2 cameraTarget = new Vector3(
            cameraOffset.x * offsetMultiplier * zoomMultiplier,
            cameraOffset.y * offsetMultiplier * zoomMultiplier
            );
        Vector2 position2D = new Vector2(playerCamera.transform.localPosition.x, playerCamera.transform.localPosition.z);
        Vector2 positionOffset = position2D - cameraTarget;

        if (positionOffset.magnitude == 0) // If at position, reset speed.
        { cameraLookSpeed = cameraLookSpeedLimits.x; }
        else // Apply movement
        {
            Vector2 positionStep = -positionOffset.normalized * Mathf.Clamp(cameraLookSpeed, 0, positionOffset.magnitude);
            
            cameraLookSpeed = Mathf.Clamp(cameraLookSpeed + cameraLookAcceleration, cameraLookSpeedLimits.x, cameraLookSpeedLimits.y);
            playerCamera.transform.Translate(positionStep);
        }

        // Rotate Model
        float angle = -1 * (Mathf.Atan2(cameraOffset.y, cameraOffset.x) * Mathf.Rad2Deg + lookOffset);
        offsetPart.transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    public void Zoom() // Control camera height with the scroll wheel
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            float delta = -Input.mouseScrollDelta.y * scrollMultiplier;
            Transform camera = playerCamera.transform;
            zoomTarget = Mathf.Clamp(zoomTarget + delta, cameraZoomLimits.x, cameraZoomLimits.y);

            if (zoomSmooth != null)
            { StopCoroutine(zoomSmooth); }
            zoomSmooth = ZoomTowards(camera, cameraZoomDuration);
            StartCoroutine(zoomSmooth);
        }
    }
    
    public IEnumerator Use()
    {
        chanellingPart.arrayList.ForEach(item => item.inputs++);
        yield return new WaitUntil(() => Input.GetKeyUp("e"));
        chanellingPart.arrayList.ForEach(item => item.inputs--);
    }

    public IEnumerator ZoomTowards(Transform camera, float duration) // Smooth changes in the camera's height
    {
        float time = 0;
        while (camera.localPosition.y != zoomTarget)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            float step = Mathf.Lerp(camera.localPosition.y, zoomTarget, progress);
            camera.localPosition = new Vector3(camera.localPosition.x, step, camera.localPosition.z);

            yield return new WaitForSeconds(0.01f);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 force = hit.moveDirection * hit.moveLength * collisionForceMultiplier;
        if (hit.rigidbody != null)
        {
            hit.rigidbody.AddForce(force);
        }
    }
}
