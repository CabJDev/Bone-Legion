// Author: John Cabusora (J.C.)
using System;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Camera movement using the new Input System.
 */
public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private float sensitivity;
    [SerializeField]
    private float zoomSensitivity;
    [SerializeField]
    private Vector2 pixelBoundaries;
    [SerializeField]
    private Vector2 zoomBoundaries;

    private Vector2 maxMouseBoundaries;

    private InputAction pointer;
    private InputAction keyboardMove;
    private InputAction zoom;

    /*
     * Initialize values
     */
    private void Start()
    {
        pointer = InputSystem.actions.FindAction("Pointer");
        keyboardMove = InputSystem.actions.FindAction("KeyboardMove");
        zoom = InputSystem.actions.FindAction("Zoom");

        sensitivity *= 10;
        zoomSensitivity *= 100;
        maxMouseBoundaries = new Vector2(Screen.width - pixelBoundaries.x, Screen.height - pixelBoundaries.y);
    }

    private void Update()
    {
        if (Application.isFocused)
        {
            PanCamera();
            ZoomCamera();
        }
    }

    /*
     * Move camera based on either WASD/Arrow keys, or by moving the cursor to the edges or outside of the game screen.
     */
    private void PanCamera()
    {
        Vector2 values = keyboardMove.ReadValue<Vector2>();

        Vector2 pointerPos = pointer.ReadValue<Vector2>();

        if (pointerPos.x <= pixelBoundaries.x) values.x = -1;
        else if (pointerPos.x >= maxMouseBoundaries.x) values.x = 1;

        if (pointerPos.y <= pixelBoundaries.y) values.y = -1;
        else if (pointerPos.y >= maxMouseBoundaries.y) values.y = 1;

        mainCamera.transform.position += new Vector3(values.x * sensitivity * Time.unscaledDeltaTime, values.y * sensitivity * Time.unscaledDeltaTime, 0);
    }

    private void ZoomCamera()
    {
        if (mainCamera.orthographicSize >= zoomBoundaries.x && mainCamera.orthographicSize <= zoomBoundaries.y)
            mainCamera.orthographicSize -= zoom.ReadValue<float>() * zoomSensitivity * Time.unscaledDeltaTime;

        // Makes sure camera does not get stuck to a number below the zoom boundaries, otherwise players can't zoom anymore
        if (mainCamera.orthographicSize <= zoomBoundaries.x) mainCamera.orthographicSize = zoomBoundaries.x;
        else if (mainCamera.orthographicSize >= zoomBoundaries.y) mainCamera.orthographicSize = zoomBoundaries.y;
    }
}
