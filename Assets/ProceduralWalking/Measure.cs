using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class Measure : MonoBehaviour
{
    public Transform cursor1;
    public Transform cursor2;
    public float length;
    public float lengthXY;
    public float lengthXZ;

    private void Start()
    {
        cursor1 = GetComponent<Transform>();

    }
    private void Update()
    {
        length = Vector3.Distance(cursor1.position, cursor2.position);
        lengthXY = Vector2.Distance(new Vector2(cursor1.position.x, cursor1.position.y), new Vector2(cursor2.position.x, cursor2.position.y));
        lengthXZ = Vector2.Distance(new Vector2(cursor1.position.x, cursor1.position.z), new Vector2(cursor2.position.x, cursor2.position.z));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(cursor1.position, cursor2.position);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(cursor1.position, 0.1f);
        Gizmos.DrawSphere(cursor2  .position, 0.1f);

    }
}
