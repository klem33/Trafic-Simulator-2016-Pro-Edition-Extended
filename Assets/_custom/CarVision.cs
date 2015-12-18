using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarVision : MonoBehaviour {
    public List<GameObject> cars;
    private CarDriver driver;

    void Start()
    {
        driver = GetComponentInParent<CarDriver>();
    }

    void Update()
    {
        UpdateColliderProps();
        driver.forceBrake = false;
        foreach (var car in cars)
        {
            float dist = Vector3.Distance(transform.position, car.transform.position);
            BoxCollider collider = GetComponent<BoxCollider>();

            if (dist > 0.6f * collider.size.z)
            {   
                if(car.GetComponent<CarDriver>().speed < driver.speed)
                {
                    driver.forceBrake = true;
                }

                //Debug.DrawLine(transform.position, car.transform.position, Color.green);
            }
            else
            {
                if (car.GetComponent<CarDriver>().speed > 0.1f)
                {
                    driver.forceBrake = true;
                }
                //Debug.DrawLine(transform.position, car.transform.position, Color.red);
            }
        }


        if( cars.Count == 0)
        {
            driver.forceBrake = false;
        }
    }

    void UpdateColliderProps()
    {
        // Size proportional to the speed and to the driver k setting
        
        BoxCollider collider = this.GetComponent<BoxCollider>();
        var size = collider.size;
        var center = collider.center;
        size.z = driver.speed*driver.speed + 10;
        center.z = (size.z / 2.0f);
        collider.size = size;
        collider.center = center;
        // Same rotation as the wheels
        transform.rotation = transform.parent.rotation;
        transform.Rotate(Vector3.up, (GetComponentInParent<CarDriver>().steeringAngle) /2.0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsCar(other.gameObject))
            cars.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (IsCar(other.gameObject))
            cars.Remove(other.gameObject);
    }

    bool IsCar(GameObject other)
    {
        return other.transform.parent.gameObject.name.Equals("Cars");
    }
}
