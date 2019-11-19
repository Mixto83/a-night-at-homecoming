using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField] private float movementSpeed;
    [SerializeField] private float sensitivity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var x = Input.GetAxisRaw("Horizontal") * movementSpeed;
        var y = Input.GetAxisRaw("Vertical") * movementSpeed;

        var scroll = Input.GetAxis("Mouse ScrollWheel") * sensitivity;

        Vector3 movement = new Vector3(x, y, 0);

        transform.Translate(movement * this.GetComponent<Camera>().orthographicSize * 0.1f);

        if (this.GetComponent<Camera>().orthographicSize < 5f)
        {
            this.GetComponent<Camera>().orthographicSize = 5f;
        } else if (this.GetComponent<Camera>().orthographicSize > 30f)
        {
            this.GetComponent<Camera>().orthographicSize = 30f;
        }
        else
        {
            this.GetComponent<Camera>().orthographicSize -= scroll;
        }

    }
}
