using System.Collections;
using UnityEngine;

public class Player_Control : MonoBehaviour
{
    // Variables
    [Header("Components")]
    public CharacterController controller;
    public GameObject offsetPart;
    public GameObject model;
    public Camera playerCamera;

    [Header("Control")]
    public float moveMultiplier;
    public float offsetMultiplier;
    public Vector2 cameraLookSpeedLimits;
    public float cameraLookAcceleration;

    public Vector2 cameraZoomLimits;
    public float cameraZoomDuration;
    public float scrollMultiplier;

    [Header("Interactions")]
    public float collisionForceMultiplier;

    [Header("Scripts")]
    public Build buildScript;

    private const int lookOffset = 90;
    private float cameraLookSpeed;
    private float zoomTarget;
    private IEnumerator zoomSmooth;
    private float zoomDependantOffsetMultiplier
    {
        get
        {
            float current = zoomTarget - cameraZoomLimits.x;
            float max = cameraZoomLimits.y - cameraZoomLimits.x;
            return Mathf.Clamp(current / max, 0.1f, 1);
        }
    }
    private Vector3 oldMouse;
    // Functions
    private void Start()
    {
        zoomTarget = playerCamera.transform.position.y;
    }

    private void Update()
    {
        Move();
        Interact();

        if (Input.mousePosition != oldMouse)
        {
            oldMouse = Input.mousePosition;
            Look();
        }
        if (Input.mouseScrollDelta.y != 0)
        {
            Zoom(-Input.mouseScrollDelta.y * scrollMultiplier);
        }
        if (Input.GetKeyDown("q"))
        {
            buildScript.ToggleState();
        }
        if (Input.GetMouseButtonDown(0))
        {
            buildScript.Trigger();
        }
    }

    public void Move() // Get the xy input as a vector 2, and apply it with a speed multiplier over delta time.
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        controller.Move(move * moveMultiplier * Time.deltaTime);
        // Calculate FOV shadows
    }

    public void Look() // Find the angle between the cursor and the middle of the screen, and apply it to the offset part. Also offset the camera based on mouse position
    {
        Vector2 mouse = Input.mousePosition;
        Vector2 middle = new Vector2(Screen.width, Screen.height) / 2;
        Vector2 offset = mouse - middle;

        Transform camera = playerCamera.transform;
        Vector3 cameraTarget = new Vector3(
            offset.x * (offsetMultiplier * zoomDependantOffsetMultiplier),
            playerCamera.transform.localPosition.y,
            offset.y * (offsetMultiplier * zoomDependantOffsetMultiplier));
        Vector3 cameraStep = Vector3.MoveTowards(camera.localPosition, cameraTarget, cameraLookSpeed);

        cameraLookSpeed = Mathf.Clamp(cameraLookSpeed + cameraLookAcceleration, cameraLookSpeedLimits.x, cameraLookSpeedLimits.y);
        camera.localPosition = cameraStep;
        if (cameraStep == cameraTarget)
        { cameraLookSpeed = cameraLookSpeedLimits.x; }

        float angle = -1 * (Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg + lookOffset);
        offsetPart.transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    public void Zoom(float delta)
    {
        Transform camera = playerCamera.transform;
        zoomTarget = Mathf.Clamp(zoomTarget + delta, cameraZoomLimits.x, cameraZoomLimits.y);

        if (zoomSmooth != null)
        { StopCoroutine(zoomSmooth); }
        zoomSmooth = ZoomTowards(camera, cameraZoomDuration);
        StartCoroutine(zoomSmooth);
    }

    public IEnumerator ZoomTowards(Transform camera, float duration)
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

    public void Interact()
    {
        /* Check if interact item within range
         * choose closest item
         * display label
         * Check if interact key is pressed
         * trigger item
         */
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 force = hit.moveDirection * hit.moveLength * collisionForceMultiplier;
        if (hit.rigidbody != null)
        {
            hit.rigidbody.AddForce(force);
        }
    }

    public RaycastHit? getMouseSphereCast()
    {
        float rayOffset = 1.05f;
        Vector3 mouseOffset = new Vector3((Screen.width - Screen.width * rayOffset) / 2, (Screen.height - Screen.height * rayOffset) / 2);
        Vector3 mouse = (Input.mousePosition * rayOffset) + mouseOffset;
        Ray ray = playerCamera.ScreenPointToRay(mouse);

        if (Physics.SphereCast(ray, 1, out RaycastHit hitInfo))
        {
            return hitInfo;
        }
        else
        {
            return null;
        }
    }

    public RaycastHit? getMouseRaycast()
    {
        float rayOffset = 1.05f;
        Vector3 mouseOffset = new Vector3((Screen.width - Screen.width * rayOffset) / 2, (Screen.height - Screen.height * rayOffset) / 2);
        Vector3 mouse = (Input.mousePosition * rayOffset) + mouseOffset;
        Ray ray = playerCamera.ScreenPointToRay(mouse);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            return hitInfo;
        }
        else
        {
            return null;
        }
    }
}
