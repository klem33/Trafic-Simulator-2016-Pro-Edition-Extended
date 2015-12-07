using UnityEngine;
using System.Collections;

public class CarEngine : MonoBehaviour
{
    public float coef = 100;
    public float move = 0;
    public float steerAngle = 0;
    public float steerAngleLimit = 45;

    void Update()
    {
        move = Mathf.Clamp(move, -1, 1);
        steerAngle = Mathf.Clamp(steerAngle, -45, 45);
        foreach (WheelCollider wheel in GetComponentsInChildren<WheelCollider>())
        {
            if (wheel.gameObject.name.Equals("DriveWheel"))
            {
                wheel.motorTorque = move > 0 ? move * coef : 0;
                wheel.brakeTorque = move < 0 ? -move * coef : 0;
                wheel.gameObject.transform.Rotate(0, -wheel.steerAngle, 0);
                wheel.steerAngle = steerAngle;
                wheel.gameObject.transform.Rotate(0, wheel.steerAngle, 0);
            }
        }
    }
}
