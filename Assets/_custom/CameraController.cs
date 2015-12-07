using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public GameObject Car;

    private Vector3 offset;

    private Vector3 offsetRot;

    void Start()
    {
        offset = transform.position - Car.transform.position;
        offsetRot = transform.eulerAngles - Car.transform.eulerAngles;
    }

    void LateUpdate()
    {
        //transform.eulerAngles = Car.transform.eulerAngles + offsetRot;
        transform.position = Car.transform.position + offset;
    }
}
