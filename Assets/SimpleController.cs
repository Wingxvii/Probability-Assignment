using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleController : MonoBehaviour
{
    CharacterController controller;
    Vector3 dir;

    public float gravity = 10.0f;
    public float speed = 5.0f;

    public float horizontalSpeed = 1.0f;
    public float verticalSpeed = 1.0f;
    float xRotation = 0.0f;
    float yRotation = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.visible = false;

    }

    void Update()
    {
        //rotation
        float mouseX = Input.GetAxis("Mouse X") * horizontalSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSpeed;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        this.transform.eulerAngles = new Vector3(xRotation, yRotation, 0.0f);

        //movement
        Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        dir = transform.TransformDirection(dir);

        dir *= speed;
        dir.y = -gravity;

        controller.Move(dir * Time.deltaTime);
    }

}
