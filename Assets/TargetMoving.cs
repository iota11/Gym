using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMoving : MonoBehaviour
{
    // Start is called before the first frame update
    private Transform m_transform;
    private FastNoise _fastNoise;
    public float radius;
    public Transform player_Transform;
    public float rotationSpeed;
    public float amplitude = 1f;
    public float height = 5f;
    void Start()
    {
        m_transform = GetComponent<Transform>();
        _fastNoise = new FastNoise();
    }

    // Update is called once per frame
    void Update()
    {
        

        float noise = _fastNoise.GetSimplex(Time.time, Time.time * Time.time, Time.time + Time.time) + 1;
        Vector3 noisePos = new Vector3(Mathf.Cos(noise * Mathf.PI),
                                    Mathf.Sin(noise * Mathf.PI),
                                    Mathf.Sin(noise * Mathf.PI) * Mathf.Cos(noise * Mathf.PI));


        m_transform.position = player_Transform.position + new Vector3(Mathf.Sin(Time.time * rotationSpeed) * radius, height, Mathf.Cos(Time.time * rotationSpeed) * radius) + noisePos* amplitude;
    }
}
