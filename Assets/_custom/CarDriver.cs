using UnityEngine;
using System.Collections;

public class CarDriver : MonoBehaviour
{
    public float k = 1;
    public Vector3 destination = new Vector3(-60f, 0f, 143f);
    
    private NavMeshPath path;
    private int pathIndex;

    void Start()
    {
	}

    void Update()
    {
        try
        {
            UpdatePath();
            var angle = GetTargetAngle();
            var alpha = 0.05f;
            GetComponent<CarEngine>().steerAngle = alpha * angle + (1 - alpha) * GetComponent<CarEngine>().steerAngle;
            GetComponent<CarEngine>().move = k;
        }
        catch
        {
            GetComponent<CarEngine>().move = -1;
        }
    }

    void ComputePath()
    {
        path = new NavMeshPath();
        pathIndex = 0;
        NavMesh.CalculatePath(gameObject.transform.position, destination, NavMesh.AllAreas, path);
    }

    void UpdatePath()
    {
        ComputePath();
        while (Vector3.Distance(path.corners[pathIndex], gameObject.transform.position) < 5)
            ++pathIndex;
        Debug.DrawLine(gameObject.transform.position, path.corners[pathIndex]);
    }

    float GetTargetAngle()
    {
        var move = path.corners[pathIndex] - gameObject.transform.position;
        var angle = -Mathf.Atan2(move.z, move.x) * 180 / Mathf.PI;
        angle -= gameObject.transform.rotation.eulerAngles.y - 90;
        angle = (360f + angle + 180f) % 360f - 180f;
        return angle;
    }
}
