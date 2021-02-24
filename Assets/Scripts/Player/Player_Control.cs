﻿using System.Collections;
using UnityEngine;

public class Player_Control : MonoBehaviour
{
    // Variables
    [Header("Components")]
    public CharacterController controller;
    public GameObject offsetPart;
    public GameObject model;
    public Camera playerCamera;

    [Header("Variables")]
    public float moveMulti;

    [Header("Scripts")]
    public Build buildScript;

    private const int lookOffset = 90;
    private bool placingObject = false;

    // Functions
    private void Update()
    {
        Move();
        Look();
        Interact();

        if (Input.GetKeyDown("q"))
        {
            buildScript.SetMenuActive();
        }
        if (Input.GetMouseButtonDown(0))
        {
            buildScript.TryTrigger();
        }
    }

    public void Move() // Get the xy input as a vector 2, and apply it with a speed multiplier over delta time.
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        controller.Move(move * moveMulti * Time.deltaTime);
        // Calculate FOV shadows
    }

    public void Look() // Find the angle between the cursor and the middle of the screen, and apply it to the offset part.
    {
        Vector2 mouse = Input.mousePosition;
        Vector2 middle = new Vector2(Screen.width, Screen.height) / 2;
        Vector2 offset = mouse - middle;

        float angle = -1 * (Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg + lookOffset);
        offsetPart.transform.rotation = Quaternion.Euler(0, angle, 0);
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
}
