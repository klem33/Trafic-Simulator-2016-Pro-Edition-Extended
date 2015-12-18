using UnityEngine;
using System.Collections;

public class WheelAnimator : MonoBehaviour {
    public GameObject wheelVisual;

    void Update ()
    {
        WheelCollider collider = GetComponent<WheelCollider>();
        wheelVisual.transform.localEulerAngles = new Vector3(
            wheelVisual.transform.localEulerAngles.x,
            collider.steerAngle - wheelVisual.transform.localEulerAngles.z,
            wheelVisual.transform.localEulerAngles.z
        );
        wheelVisual.transform.Rotate(collider.rpm / 60 * 360 * Time.deltaTime, 0, 0);
    }
}
