// Author: John Cabusora (J.C.)
using System;
using UnityEngine;

/*
 * Camera movement using InputManager's axes to provide potential support for controllers.
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

    private Vector2 minMouseBoundaries;
    private Vector2 maxMouseBoundaries;

    /*
     * Initialize values
     */
    private void Start()
    {
        sensitivity *= 10;
        zoomSensitivity *= 100;

        minMouseBoundaries = new Vector2(pixelBoundaries.x / Screen.width, pixelBoundaries.y / Screen.height);
        maxMouseBoundaries = new Vector2((Screen.width - pixelBoundaries.x) / Screen.width, (Screen.height + pixelBoundaries.y) / Screen.height);
    }

    private void Update()
    {
        PanCamera();
        ZoomCamera();
    }

    /*
     * Move camera based on either WASD/Arrow keys, or by moving the cursor to the edges or outside of the game screen.
     */
    private void PanCamera()
    {
        float xMovement = 0;
        float yMovement = 0;

        // Convert mouse position to viewport position
        Vector3 viewportPoint = mainCamera.ScreenToViewportPoint(Input.mousePosition);

        xMovement = Input.GetAxisRaw("Horizontal");
        yMovement = Input.GetAxisRaw("Vertical");

        // Far left of the viewport starts at (0, 0) so we have to subtract by one so that the camera goes left
        if (viewportPoint.x <= minMouseBoundaries.x) xMovement = -1 + viewportPoint.x;
        else if (viewportPoint.x >= maxMouseBoundaries.x) xMovement = viewportPoint.x;

        // Far bottom of the viewport starts at (0, 0) so we have to subtract by one so that the camera goes down
        if (viewportPoint.y <= minMouseBoundaries.y) yMovement = -1 + viewportPoint.y;
        else if (viewportPoint.y >= maxMouseBoundaries.y) yMovement = viewportPoint.y;

        mainCamera.transform.position += new Vector3(xMovement * sensitivity * Time.deltaTime, yMovement * sensitivity * Time.deltaTime, 0);
    }

    private void ZoomCamera()
    {
        if (mainCamera.orthographicSize >= zoomBoundaries.x && mainCamera.orthographicSize <= zoomBoundaries.y)
            mainCamera.orthographicSize -= Input.GetAxisRaw("Mouse ScrollWheel") * zoomSensitivity * Time.deltaTime;

        // Makes sure camera does not get stuck to a number below the zoom boundaries, otherwise players can't zoom anymore
        if (mainCamera.orthographicSize <= zoomBoundaries.x) mainCamera.orthographicSize = zoomBoundaries.x;
        else if (mainCamera.orthographicSize >= zoomBoundaries.y) mainCamera.orthographicSize = zoomBoundaries.y;
    }
}
