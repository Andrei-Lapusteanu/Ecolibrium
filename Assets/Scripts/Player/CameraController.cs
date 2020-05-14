using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float MIN_CAM_HEIGHT = 5;
    private const float MAX_CAM_HEIGHT = 30;
    private const int MIN_ZOOM_LEVEL = 1;
    private const int MAX_ZOOM_LEVEL = 6;


    private const int MOUSE_INPUT_LCLICK = 0;
    private const int MOUSE_INPUT_RCLICK = 1;
    private const int MOUSE_INPUT_MCLICK = 2;

    private float zoomLevel = MIN_ZOOM_LEVEL;
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private float mouseRotateSpeed = 7.0f;
    private float mousePanSensXY = 5.0f;

    public float mouseDragSpeed = 2.0f;
    private Vector3 mouseDragOrigin;

    private Camera camera;
    private Rigidbody rigidbody;

    private float camLinearSpeedXZ = 45.0f;
    private float camLinearSpeedY = 25.0f;
    private float camDecelerationFactor = 0.925f;
    private float camAngularSpeed = 0.3f;
    private float camMaxMovementSpeed = 100.0f;
    private float camRotateSpeed = 5.0f;
    private Vector3 maxCameraSpeedVec;

    void Start()
    {
        camera = GetComponent<Camera>();
        rigidbody = GetComponent<Rigidbody>();

        pitch = 15.0f;
        maxCameraSpeedVec = new Vector3(camMaxMovementSpeed, 0, camMaxMovementSpeed);
        camera.transform.Rotate(camera.transform.right, pitch);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Calculate vectors parallel to groudnd level
        Vector3 camForwardVector = new Vector3(rigidbody.transform.forward.x, 0, rigidbody.transform.forward.z);
        Vector3 camRightVector = new Vector3(rigidbody.transform.right.x, 0, rigidbody.transform.right.z);

        // Speed up time
        if (Input.GetKey(KeyCode.KeypadPlus))
            if (Time.timeScale < 10)
                Time.timeScale += 0.1f;

        // Slow down time
        if (Input.GetKey(KeyCode.KeypadMinus))
            if (Time.timeScale > 1.0f)
            {
                Time.timeScale -= 0.1f;

                if (Time.timeScale < 1.0f)
                    Time.timeScale = 1.0f;
            }

        // Move camera forward
        if (Input.GetKey(KeyCode.W))
            if (Mathf.Abs(Vector3.Magnitude(rigidbody.velocity)) < Vector3.Magnitude(maxCameraSpeedVec))
                rigidbody.AddForce(camForwardVector * camLinearSpeedXZ * transform.position.y / 4.0f, ForceMode.Acceleration);

        // Move camera backward
        if (Input.GetKey(KeyCode.S))
            if (Mathf.Abs(Vector3.Magnitude(rigidbody.velocity)) < Vector3.Magnitude(maxCameraSpeedVec))
                rigidbody.AddForce(camForwardVector * camLinearSpeedXZ * transform.position.y / 4.0f * -1, ForceMode.Acceleration);

        // Move camera left
        if (Input.GetKey(KeyCode.A))
            if (Mathf.Abs(Vector3.Magnitude(rigidbody.velocity)) < Vector3.Magnitude(maxCameraSpeedVec))
                rigidbody.AddForce(camRightVector * camLinearSpeedXZ * transform.position.y / 4.0f * -1, ForceMode.Acceleration);

        // Move camera right
        if (Input.GetKey(KeyCode.D))
            if (Mathf.Abs(Vector3.Magnitude(rigidbody.velocity)) < Vector3.Magnitude(maxCameraSpeedVec))
                rigidbody.AddForce(camRightVector * camLinearSpeedXZ * transform.position.y / 4.0f, ForceMode.Acceleration);

        // Move camera CCW
        if (Input.GetKey(KeyCode.Q))
            rigidbody.AddTorque(Vector3.up * camRotateSpeed * -1, ForceMode.Acceleration);

        // Move camera CW
        if (Input.GetKey(KeyCode.E))
            rigidbody.AddTorque(Vector3.up * camRotateSpeed, ForceMode.Acceleration);

        // Zoom in (scroll up)
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (transform.position.y > MIN_CAM_HEIGHT)
            {
                rigidbody.AddForce(Vector3.down * camLinearSpeedY, ForceMode.Impulse);
                //transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.down, 100f * Time.deltaTime);
                //rigidbody.position = Vector3.MoveTowards(rigidbody.position, rigidbody.position + Vector3.down, 5.0f * Time.deltaTime);
                //StartCoroutine(CameraSmoothZooming(Vector3.down, 1.0f));
                rigidbody.AddTorque(rigidbody.transform.right * camAngularSpeed * -1, ForceMode.Impulse);
                //StartCoroutine(CameraSmoothZooming(Vector3.down, 1.0f));
                zoomLevel -= .5f;
            }
        }
        // Zoom out (scroll down)
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)  // Scroll down (zoom-out)
        {
            if (transform.position.y < MAX_CAM_HEIGHT)
            {
                rigidbody.AddForce(Vector3.up * camLinearSpeedY, ForceMode.Impulse);
                //rigidbody.MovePosition(rigidbody.position + Vector3.up);
                rigidbody.AddTorque(rigidbody.transform.right * camAngularSpeed, ForceMode.Impulse);
                zoomLevel += .5f;
            }
        }

        // (OLD) Hold right mouse button and move mouse on X axis to rotate camera
        // Left click to raycast from camera to scene
        else if (Input.GetMouseButtonDown(MOUSE_INPUT_LCLICK))
        {
            //OLD rigidbody.AddTorque(Vector3.up * Input.GetAxis("Mouse X") * mouseRotateSpeed, ForceMode.Acceleration);

            // Send ray by converting mouse pos on sccren to world coords
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Hide all panels
            StatusPanelController.HideAll();

            // If something with collider had been hit
            if (Physics.Raycast(ray, out hit))
            {
                // Get tag dependent appropriate script and call method
                switch (hit.transform.tag)
                {
                    case "Wolf":
                        hit.transform.GetComponent<Wolf>().ActivatePanel();
                        break;

                    case "Rabbit":
                        hit.transform.GetComponent<Rabbit>().ActivatePanel();
                        break;

                    // Disable all panels
                    default:
                        StatusPanelController.HideAll();
                        break;
                }
            }
        }

        // Hold right mouse button and move mouse on XY to pan camera
        if (Input.GetMouseButton(MOUSE_INPUT_RCLICK))
        {
            rigidbody.AddForce(camRightVector * camLinearSpeedXZ * Input.GetAxis("Mouse X") * mousePanSensXY * -1f * 0.6f);
            rigidbody.AddForce(camForwardVector * camLinearSpeedXZ * Input.GetAxis("Mouse Y") * mousePanSensXY * -1f);
            //mouseDragOrigin = Input.mousePosition;
            //Vector3 pos = camera.ScreenToViewportPoint(Input.mousePosition - mouseDragOrigin);
            //Vector3 move = new Vector3(Input.GetAxis("Mouse X") * mouseDragSpeed, 0, Input.GetAxis("Mouse Y") * mouseDragSpeed);
            //transform.Translate(move, Space.World);
        }

        if (rigidbody.position.y > MAX_CAM_HEIGHT)
        {
            rigidbody.position = new Vector3(rigidbody.position.x, MAX_CAM_HEIGHT, rigidbody.position.z);
            transform.eulerAngles = new Vector3(47.0f, transform.eulerAngles.y, transform.eulerAngles.z);
        }

        if (rigidbody.position.y < MIN_CAM_HEIGHT)
        {
            rigidbody.position = new Vector3(rigidbody.position.x, MIN_CAM_HEIGHT, rigidbody.position.z);
            transform.eulerAngles = new Vector3(30.0f, transform.eulerAngles.y, transform.eulerAngles.z);
        }

        // Decelerate camera motion
        rigidbody.velocity *= camDecelerationFactor;
        rigidbody.angularVelocity *= camDecelerationFactor;

        // Block z-axis rotation
        //rigidbody.rotation = new Quaternion(rigidbody.rotation.x, rigidbody.rotation.y, 0.0f, 0.0f);
        rigidbody.transform.eulerAngles = new Vector3(rigidbody.transform.eulerAngles.x, rigidbody.transform.eulerAngles.y, 0.0f);

        // Limit camera movement
        if (Mathf.Abs(Mathf.Clamp(rigidbody.position.x, -WorldLimits.WORLD_LIMIT_X, WorldLimits.WORLD_LIMIT_X)) >= 150)
            rigidbody.position = new Vector3(Mathf.Clamp(rigidbody.position.x, -WorldLimits.WORLD_LIMIT_X, WorldLimits.WORLD_LIMIT_X), rigidbody.position.y, rigidbody.position.z);
        if (Mathf.Abs(Mathf.Clamp(rigidbody.position.z, -WorldLimits.WORLD_LIMIT_Z, WorldLimits.WORLD_LIMIT_Z)) >= 150)
            rigidbody.position = new Vector3(rigidbody.position.x, rigidbody.position.y, Mathf.Clamp(rigidbody.position.z, -WorldLimits.WORLD_LIMIT_Z, WorldLimits.WORLD_LIMIT_Z));
    }

    IEnumerator CameraSmoothZooming(Vector3 dir, float speed)
    {
        float startTime = Time.time;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + Vector3.down;

        while (endPos.y < transform.position.y)
        {
            //float move = Mathf.Lerp(0, 0.05f, (Time.time - startTime) / speed * 10);
            //float move = Mathf.SmoothDamp(0, 0.5f, ref speed, 0.2f, 1.0f);
            //transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.down, 1f * Time.deltaTime);
            transform.Rotate(Vector3.right, Mathf.Lerp(0, 2, 50f * Time.deltaTime));
            //transform.position += dir * move;

            yield return null;
        }
    }
}
