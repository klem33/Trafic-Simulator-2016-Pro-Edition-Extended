using UnityEngine;
using System.Collections;

public class CarEngine : MonoBehaviour
{
    public float coef = 100;
    public float move = 0;
    public float steer = 0;

    void Start()
    {
        GetComponent<NavMeshAgent>().SetDestination(new Vector3(90, 0, 30));
    }

    void Update()
    {
        move = Mathf.Clamp(move, -1, 1);
        steer = Mathf.Clamp(steer, -1, 1);
        foreach (WheelCollider wheel in GetComponentsInChildren<WheelCollider>())
        {
            if (wheel.gameObject.name.Equals("FrontWheel"))
            {
                wheel.motorTorque = move > 0 ? move * coef : 0;
                wheel.brakeTorque = move < 0 ? -move * coef : 0;
                wheel.gameObject.GetComponent<Transform>().Rotate(0, -wheel.steerAngle, 0);
                wheel.steerAngle = steer * 45;
                wheel.gameObject.GetComponent<Transform>().Rotate(0, wheel.steerAngle, 0);
            }
        }
    }
}
