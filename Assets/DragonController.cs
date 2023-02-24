using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    private Transform m_transform;
    public Transform des_transfrom;
    public Transform head_tranform;
    public float rotate_speed = 5f;
    public float move_speed = 10f;
    private FastNoise _fastNoise;
    //about body movement
    public Transform body_transform;
    public Vector3 lastFrameNoisePos;
    public float noiseThreshold = 0.5f;
    public float body_amplitude = 10.0f;
    public float period = 0.0f;
    private float collectedTime = 0.0f;
    public Vector3 noisePos;
    public float lerpSpeed = 1.0f;




    public float speed_factor = 1.0f;

    private bool ready_move;
    public bool isFlee;
    [SerializeField] GameObject crystal;
    void Start()
    {
        m_transform = GetComponent<Transform>();
        _fastNoise = new FastNoise();
        ready_move = false;
        lastFrameNoisePos = new Vector3(0,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 direction = Vector3.Normalize(des_transfrom.position - m_transform.position);
        if (Mathf.Abs(Vector3.Dot(direction, m_transform.forward)) > 0.8) ready_move = true;
        RotateToDes(direction);
        if (ready_move)
        {
            MoveToDes(des_transfrom.position - direction * 0.3f);
        }

        BodyLocalShake();

    }


    void BodyLocalShake()
    {
        collectedTime += Time.deltaTime;
        if(collectedTime > period)
        {
            collectedTime = 0.0f;
            //update noise Pos;
            float noise = _fastNoise.GetSimplex(Time.time, Time.time * Time.time, Time.time + Time.time) + 1;
            noisePos = new Vector3(Mathf.Cos(noise * Mathf.PI),
                                        Mathf.Sin(noise * Mathf.PI),
                                        Mathf.Sin(noise * Mathf.PI) * Mathf.Cos(noise * Mathf.PI));

        }


        float speed = Mathf.Sqrt(1 - (collectedTime / period) * (collectedTime / period))* lerpSpeed;

        body_transform.localPosition = Vector3.Lerp(body_transform.localPosition, noisePos *body_amplitude, Time.deltaTime*speed);
    }



    void RotateToDes(Vector3 dir)
    {
        Quaternion rotation = Quaternion.LookRotation(dir);
        m_transform.rotation = Quaternion.Lerp(m_transform.rotation, rotation, rotate_speed);
    }

    public void MoveToDes(Vector3 pos)
    {
        //float speed_factor = 1.0f;
        float distance = Vector3.Distance(m_transform.position, pos);

        if (distance > 5)
        {
            speed_factor = 1.0f;
        }
        else
        {
            speed_factor = (distance / 5f) * 0.9f + 0.1f;//ease out
        }

        m_transform.position = Vector3.MoveTowards(m_transform.position, pos, move_speed * speed_factor * Time.deltaTime);
    }
}
