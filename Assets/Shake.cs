using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator arm_animator;
    public Transform m_transform;
    public float amplitude = 3f;
    public float amplitude2 = 3f;
    public float freq = 2f;
    public float phase = 0f;
    public float state = 0f;
    private Vector3 localStillPos;
    private FastNoise _fastNoise;


    public Transform wing_L_transform;
    public Transform wing_R_transform;
    void Start()
    {
        m_transform = GetComponent<Transform>();
        localStillPos = m_transform.localPosition;
        _fastNoise = new FastNoise();
    }

    // Update is called once per frame
    void Update()
    {

        float noise = _fastNoise.GetNoise(Time.time, m_transform.position.x)+1;
        //float noise = _fastNoise.GetSimplex(Time.time, Time.time, Time.time) + 1;
        Debug.Log(noise);
        Vector3 noiseDirection = new Vector3(Mathf.Cos(noise * Mathf.PI), 
                                        Mathf.Sin(noise * Mathf.PI), 
                                        Mathf.Cos(noise * Mathf.PI));
        state = freq * Time.time + phase;
        
        float state_pos = amplitude * Mathf.Sin(freq * Time.time + phase) / 100f;
        m_transform.localPosition = localStillPos + new Vector3(0, 0, state_pos) + noiseDirection*amplitude2/100f;
       


    }
}
