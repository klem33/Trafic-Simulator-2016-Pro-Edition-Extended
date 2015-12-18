using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Computes the targets that the car must follow.
/// The first target is computed by the constructor.
/// The following ones are computed by UpdateTarget.
/// This base class provides some helper functions.
/// </summary>
[Serializable]
public class TargetFinder
{
    public Transform target;
    public float maxSpeed;

    protected CarDriver driver;
    protected GameObject car, ground;

    /// <summary>
    /// Compute the first target inside your constructor.
    /// </summary>
    /// <param name="driver"></param>
    public TargetFinder(CarDriver driver)
    {
        this.driver = driver;
        car = driver.gameObject;
        ground = driver.ground;
    }

    /// <summary>
    /// Compute the next targets by overriding this method.
    /// </summary>
    public virtual void UpdateTarget()
    {
    }

    /// <summary>
    /// Saves the closest point as current target.
    /// </summary>
    /// <param name="targets">Array of points</param>
    /// <returns>Index of the closest point</returns>
    public int ChooseMinDistanceTarget(Transform[] targets)
    {
        Transform bestTarget = null;
        float bestDistance = Mathf.Infinity;
        int bestI = -1;
        
        for (int i = 0; i < targets.Length; ++i)
        {
            if (!targets[i])
                continue;
            float distance = Vector3.Distance(car.transform.position, targets[i].position);
            if (distance < bestDistance)
            {
                bestTarget = targets[i];
                bestDistance = distance;
                bestI = i;
            }
        }

        target = bestTarget;
        return bestI;
    }

    /// <summary>
    /// Saves the furthest point as current target.
    /// </summary>
    /// <param name="targets">Array of points</param>
    /// <returns>Index of the furthest point</returns>
    public int ChooseMaxDistanceTarget(Transform[] targets)
    {
        Transform bestTarget = null;
        float bestDistance = -1;
        int bestI = -1;
        
        for (int i = 0; i < targets.Length; ++i)
        {
            if (!targets[i])
                continue;
            float distance = Vector3.Distance(car.transform.position, targets[i].position);
            if (distance > bestDistance)
            {
                bestTarget = targets[i];
                bestDistance = distance;
                bestI = i;
            }
        }

        target = bestTarget;
        return bestI;
    }

    /// <summary>
    /// Creates an array containing all the direct children of an object.
    /// </summary>
    /// <param name="transform">Parent to explore</param>
    /// <returns>Array of children</returns>
    public Transform[] GetChildrenOf(Transform transform)
    {
        var children = new Transform[transform.childCount];
        for (int i = 0; i < children.Length; ++i)
            children[i] = transform.GetChild(i);
        return children;
    }
}

/// <summary>
/// Computes the path for straight lines.
/// </summary>
public class LaneTargetFinder : TargetFinder
{
    /// <summary>
    /// Directs the car towards the other end of the road.
    /// </summary>
    /// <param name="driver"></param>
    public LaneTargetFinder(CarDriver driver) : base(driver)
    {
        maxSpeed = 12;
        ChooseMaxDistanceTarget(GetChildrenOf(ground.transform));
    }

    /// <summary>
    /// Slows the car down when reaching the next intersection.
    /// </summary>
    public override void UpdateTarget()
    {
        if (Vector3.Distance(car.transform.position, target.position) < 15)
            maxSpeed = 3.8f;
    }
}

/// <summary>
/// Chooses a path at 3 and 4-way intersections.
/// </summary>
public class CrossTargetFinder : TargetFinder
{
    Transform entrances, turns, exits;
    int i;

    /// <summary>
    /// Picks one of the possible paths (excluding the current one).
    /// </summary>
    /// <param name="driver"></param>
    public CrossTargetFinder(CarDriver driver) : base(driver)
    {
        maxSpeed = 3f;

        entrances = ground.transform.GetChild(0);
        turns = ground.transform.GetChild(1);
        exits = ground.transform.GetChild(2);

        Transform[] entranceArray = GetChildrenOf(entrances);
        int entranceIndex = ChooseMinDistanceTarget(entranceArray);
        i = UnityEngine.Random.Range(0, entranceArray.Length - 1);
        if (i >= entranceIndex)
            ++i;
    }

    /// <summary>
    /// Guides the car between the first, second and third point of the path.
    /// </summary>
    public override void UpdateTarget()
    {
        if (Vector3.Distance(car.transform.position, target.position) > 0.5)
            return;
        if (target.IsChildOf(entrances))
            target = turns.GetChild(i);
        else if (target.IsChildOf(turns))
            target = exits.GetChild(i);
    }
}

/// <summary>
/// Computes the path for the car to follow turns.
/// </summary>
public class BendTargetFinder : TargetFinder
{
    int side, index, step;

    /// <summary>
    /// Finds the correct lane and direction.
    /// </summary>
    /// <param name="driver"></param>
    public BendTargetFinder(CarDriver driver) : base(driver)
    {
        maxSpeed = 11;
        
        var junctions = new Transform[4];
        int i;
        for (i = 0; i < junctions.Length; ++i)
            junctions[i] = ground.transform.GetChild(i / 2)
                .GetChild(i % 2 == 0 ? 0 : ground.transform.GetChild(i / 2).childCount - 1);
        i = ChooseMinDistanceTarget(junctions);
        junctions[i] = null; // The closest point is on the wrong side of the road.
        i = ChooseMinDistanceTarget(junctions);
        side = i / 2;
        index = i % 2 == 0 ? 0 : ground.transform.GetChild(i / 2).childCount - 1;
        step = i % 2 == 0 ? 1 : -1;
    }

    /// <summary>
    /// Guides the car along the selected succession of points.
    /// </summary>
    public override void UpdateTarget()
    {
        if (Vector3.Distance(car.transform.position, target.position) < 0.5)
        {
            index += step;
            if (index >= 0 && index < ground.transform.GetChild(side).childCount)
                target = ground.transform.GetChild(side).GetChild(index);
        }
    }
}

/// <summary>
/// Makes the car follow the path computed by subclasses of TargetFinder.
/// It chooses the correct subclass depending on the road's tag.
/// </summary>
public class CarDriver : MonoBehaviour
{

    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxBrakeTorque; // Car's brakeTorque value

    public float maxSteeringAngle; // maximum steer angle the wheel can have
    public float steeringAngleLowpass;
    public float steeringAngle;

    public bool forceBrakeRight;
    public bool forceBrake;

    // Output:

    public GameObject ground;
    public TargetFinder targetFinder;
    
    public float speed
    {
        get
        {
            return GetComponent<Rigidbody>().velocity.magnitude;
        }
    }

    public void FixedUpdate()
    {
        UpdateDriveWheels();
    }

    /// <summary>
    /// Updates the state of the drive wheels (torque, brake, steering).
    /// </summary>
    public void UpdateDriveWheels()
    {
        UpdateTarget();
        FollowTarget();
    }

    /// <summary>
    /// Updates the target to follow depending on the road.
    /// </summary>
    public void UpdateTarget()
    {
        GameObject lastGround = ground;
        ground = FindGround();
        if (ground)
        {
            bool groundChanged = ground != lastGround;

            if (groundChanged)
                targetFinder = CreateTargetFinder();
            else if (targetFinder != null)
                targetFinder.UpdateTarget();
        }
    }

    /// <summary>
    /// Makes the drive wheels move towards the target.
    /// </summary>
    public void FollowTarget()
    {
        var target = targetFinder != null ? targetFinder.target : null;
        if (target)
        {
            Debug.DrawLine(transform.position, target.position, Color.blue);

            float maxSpeed = !forceBrakeRight && !forceBrake ? targetFinder.maxSpeed : -1;

            float motorTorque = speed <= maxSpeed ? maxMotorTorque : 0;
            float brakeTorque = speed > maxSpeed ? maxBrakeTorque : 0;

            float maxAngle = maxSteeringAngle * (1 - (speed / 450.0f));
            steeringAngle = Mathf.Clamp(GetTargetAngle(target), -maxAngle, maxAngle);

            steeringAngle = steeringAngleLowpass * steeringAngle
                + (1 - steeringAngleLowpass) * GetComponentInChildren<DriveWheel>().GetComponent<WheelCollider>().steerAngle;

            foreach (var driveWheel in GetComponentsInChildren<DriveWheel>())
            {
                WheelCollider collider = driveWheel.GetComponent<WheelCollider>();
                collider.motorTorque = motorTorque;
                collider.brakeTorque = brakeTorque;
                collider.steerAngle = steeringAngle;
            }
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.up);
        }
    }

    /// <summary>
    /// Instanciates the right subclass of TargetFinder depending on the road's tag.
    /// </summary>
    /// <returns>A new instance of the appropriate TargetFinder subclass.</returns>
    public TargetFinder CreateTargetFinder()
    {
        switch (ground.tag)
        {
            case "Lane":
                return new LaneTargetFinder(this);
            case "Cross":
            case "CrossT":
                return new CrossTargetFinder(this);
            case "Bend":
                return new BendTargetFinder(this);
            default:
                return null;
        }
    }

    /// <summary>
    /// Retrieves the ground currently under the car.
    /// </summary>
    /// <returns>The game object representing the road.</returns>
    private GameObject FindGround()
    {
        WheelHit hit;
        foreach (var wheel in GetComponentsInChildren<DriveWheel>())
            if (wheel.GetComponent<WheelCollider>().GetGroundHit(out hit))
                return hit.collider.gameObject;
        return null;
    }

    /// <summary>
    /// Computes the angle to apply on the wheels to make them face the target.
    /// </summary>
    /// <param name="target">Point to rotate towards.</param>
    /// <returns>Angle to set on the wheels.</returns>
    float GetTargetAngle(Transform target)
    {
        var move = target.position - transform.position;
        var angle = -Mathf.Atan2(move.z, move.x) * 180 / Mathf.PI;
        angle -= gameObject.transform.rotation.eulerAngles.y - 90;
        angle = (360f + angle + 180f) % 360f - 180f;
        return angle;
    }
}
