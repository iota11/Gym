using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingShake : MonoBehaviour
{
    // Start is called before the first frame update
    public Shake body_shake;
    public float amplitude;
    public float phase;
    public Transform left_transform;
    public Transform right_transfrom;
    private float wing_state;
    //public Vector3 test = new Vector3(0,0,0);
    private Quaternion localStillRotation;
    private Vector3 localStillEulerAngle_L;
    private Vector3 localStillEulerAngle_R;

    public Vector3 dir = new Vector3(0,0,0);
    private Vector3 dir_L;
    private Vector3 dir_R;
    void Start()
    {
        //body_shake = 
        //localStillRotation = m_transform.localRotation;
        localStillEulerAngle_L = left_transform.localEulerAngles;
        localStillEulerAngle_R = right_transfrom.localEulerAngles;

        
    }

    // Update is called once per frame
    void Update()
    {
        dir_L = dir;
        dir_R = new Vector3(dir.x, -dir.y, dir.z);
        wing_state = body_shake.state + phase;

        float curPhase = Mathf.Sin(wing_state) * 10;
        //m_transform.localRotation = m_transform.localRotation + Quaternion.Euler(test);
        left_transform.localEulerAngles = localStillEulerAngle_L + dir_L * curPhase * amplitude;
        right_transfrom.localEulerAngles = localStillEulerAngle_R + dir_R * curPhase * amplitude;
        
    }
}
