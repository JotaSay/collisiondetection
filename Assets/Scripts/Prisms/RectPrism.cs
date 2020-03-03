using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RectPrism : Prism
{

    void Start()
    {
        points = new Vector3[] { new Vector3(0.5f, 0, 0.5f), new Vector3(0.5f, 0, -0.5f), new Vector3(-0.5f, 0, -0.5f), new Vector3(-0.5f, 0, 0.5f) };
        midY = 0;
        height = 1;
    }
    
    void Update()
    {
        var yMin = midY - height / 2 * transform.localScale.y;
        var yMax = midY + height / 2 * transform.localScale.y;
        foreach (var pt in points.Select(p => new Vector3(p.x * transform.localScale.x, 0, p.z * transform.localScale.z)))
        {
            var point = transform.position + Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * pt;
            Debug.DrawLine(point + Vector3.up * yMin, point + Vector3.up * yMax, Color.red);
        }
    }
}
