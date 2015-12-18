using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarVisionDroite : MonoBehaviour {
    public List<GameObject> carsSphere;

    private CarDriver driver;

    void Start()
    {
        driver = GetComponentInParent<CarDriver>();
    }

    void Update()
    {

        if(driver.ground && driver.ground.tag != "Cross" && driver.ground.tag != "CrossT")
        {
            carsSphere.Clear();
            GetComponent<SphereCollider>().enabled = false;
        }
        else
        {
            GetComponent<SphereCollider>().enabled = true;

        }


        //Debug.Log(driver.forceBrakeDroite + "   " + transform.parent.name);
        foreach (GameObject car in carsSphere)
        {
            float angle = GetTargetAngle(transform, car.transform) - 180;
            if(angle < -180.0f)
            {
                angle += 360;
            }

            //Debug.Log(angle + "   " + transform.parent.name + "    " +  driver.forceBrakeDroite );
            if((0.0f <= angle) && (angle <= 75.0f))
            {
                if(car.GetComponent<CarDriver>().speed > 1)
                {
                    driver.forceBrakeRight = true;
                    Debug.DrawLine(transform.position, car.transform.position, Color.red);

                }
                else
                {
                    driver.forceBrakeRight =false;
                    Debug.DrawLine(transform.position, car.transform.position, Color.green);

                }
            }
            else
            {
                driver.forceBrakeRight = false;
                Debug.DrawLine(transform.position, car.transform.position, Color.green);

            }
        }
        if (carsSphere.Count == 0)
        {
            driver.forceBrakeRight = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsCar(other.gameObject))
            carsSphere.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (IsCar(other.gameObject))
            carsSphere.Remove(other.gameObject);
    }

    bool IsCar(GameObject other)
    {
        return other.transform.parent.gameObject.name.Equals("Cars");
    }

    float GetTargetAngle(Transform _target, Transform _car)
    {
        var move = _target.position - _car.transform.position;
        var angle = -Mathf.Atan2(move.z, move.x) * 180 / Mathf.PI;
        angle -= gameObject.transform.rotation.eulerAngles.y - 90;
        angle = (360f + angle + 180f) % 360f - 180f;
        return angle;
    }
}
