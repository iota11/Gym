using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform follow_transform;
    public Transform m_transform;
    public Vector3 initial_offset;
    public float followRatio;
    void Start()
    {
        m_transform = GetComponent<Transform>();
        initial_offset = m_transform.position - follow_transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 curoffset = m_transform.position - follow_transform.position;
        float iniDis = initial_offset.magnitude;
        float curDis = curoffset.magnitude;
        float lerp = Mathf.Abs(curDis - iniDis) *2/ iniDis;
        lerp = Mathf.Clamp(lerp, 0,1);

        m_transform.position = Vector3.Lerp(m_transform.position, follow_transform.position+initial_offset, lerp);
    }
}
