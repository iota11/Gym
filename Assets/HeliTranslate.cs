using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;



public class HeliTranslate : MonoBehaviour
{
    public enum FlyState
    {
        Normal,
        Climb,
        Stay,
        Drop
    };
    public enum AccState {
        SpeedUp,
        SpeedDown,
        Stable,
    }
    private Transform m_transform;
    public Transform body_transform;
    public Transform des_transfrom;
    public float stopDis = 1.0f;
    public float rotate_speed = 5f;
    public float move_speed_hori = 10f;
    public float move_speed_ver = 10f;
    public float acc = 5.0f;
    public float acc_sensitive = 1.0f;
    public Vector3 acc_measure = new Vector3(0,0,0);
    public float noiseScale = 0.0f;
    public Animator anim;
    public FlyState m_flyState;
    public AccState m_accState;

    //
    private FastNoise _fastNoise;
    private float collectedTime;
    public float cur_speed_hori = 0.0f;
    public Vector3 cur_vel_hori;
    public float cur_speed_ver = 0.0f;
    public Vector3 cur_vel_ver;
    public float cur_rotateDeg = 0.0f;
    public float cur_rotateDeg2 = 0.0f;
    public float playSpeed = 0f;

    public float last_speed_hori = 0.0f;
    public float last_speed_ver = 0.0f;
    public Vector3 accRotate;
    private Quaternion initial_body_rotation;

    void Start()
    {
        //anim.SetFloat("flySpeed", 10f);
        m_transform = GetComponent<Transform>();
        _fastNoise = new FastNoise();
        initial_body_rotation = body_transform.localRotation;
        m_flyState = FlyState.Stay;
        m_accState = AccState.Stable;
    }
    void FixedUpdate()
    {
        LocomoteHorizontal();
        LocomoteVertical();
        Veer();
        //ApplyNoise();

        CalAndUpdateForce();
        ApplyAccToBody();

    }
   
    private void ApplyAccToBody()
    {
        
        Vector3 acc2D = new Vector3(acc_measure.x, 0f, acc_measure.z);
        Vector3 accWind = cur_speed_hori * 0.02f *m_transform.forward;
        Vector3 acc_locomote = accWind + acc2D;

        //locomotion acc rotation
        float rotateDeg = Mathf.Clamp(acc_locomote.magnitude * acc_sensitive, -45, 45);
        if(Vector3.Dot(acc2D, m_transform.forward) < 0)
        {
            rotateDeg = -rotateDeg; 
        }
        
        if (Mathf.Abs(cur_rotateDeg - rotateDeg) > 0.5f)
        {
            cur_rotateDeg = Mathf.Lerp(cur_rotateDeg, rotateDeg, 0.2f);
        }
        //cur_rotateDeg = rotateDeg;
        Quaternion detlaRotationX = Quaternion.AngleAxis(cur_rotateDeg, new Vector3(1,0,0));


        //veer acc rotation
        float rotateDeg2 = Mathf.Clamp(accRotate.magnitude*-5 * acc_sensitive, -65, 65);
        if (Vector3.Dot(accRotate, m_transform.right) < 0)
        {
            rotateDeg2 = -rotateDeg2;
        }
        Quaternion detlaRotationZ = Quaternion.AngleAxis(cur_rotateDeg2, new Vector3(0, 0, 1));
        if (Mathf.Abs(cur_rotateDeg2 - rotateDeg2) > 0.5f)
        {
            cur_rotateDeg2 = Mathf.Lerp(cur_rotateDeg2, rotateDeg2, 0.1f);
        }
        body_transform.localRotation =  initial_body_rotation * detlaRotationZ * detlaRotationX;

        //apply playspeed
        playSpeed = Mathf.Clamp( Mathf.Abs(Mathf.Abs(cur_rotateDeg2)*0.6f + Mathf.Abs(cur_rotateDeg)) / 80f + 1f, 0f, 10f);
        if(m_accState == AccState.SpeedDown) {
            playSpeed += 0.3f;
        }
        anim.SetFloat( "flySpeed", playSpeed);
    }

    private void ApplyNoise()
    {
        collectedTime += Time.deltaTime;
        Vector3 noiseMotion =  Vector3.zero;
        if (collectedTime > 1.0f)
        {
            collectedTime = 0.0f;
            float noise = _fastNoise.GetSimplex(Time.time, Time.time * Time.time, Time.time + Time.time) + 1;
            noiseMotion = noiseScale* new Vector3(Mathf.Cos(noise * Mathf.PI),
                                        Mathf.Sin(noise * Mathf.PI),
                                        Mathf.Sin(noise * Mathf.PI) * Mathf.Cos(noise * Mathf.PI));
        }
    }

    private void CalAndUpdateForce()
    {
        acc_measure = Vector3.zero;
        acc_measure = (cur_speed_hori - last_speed_hori) /Time.deltaTime * m_transform.forward;
        //acc_measure += (cur_speed_ver - last_speed_ver) * m_transform.up/Time.deltaTime;
        last_speed_hori = cur_speed_hori;
        last_speed_ver = cur_speed_ver;
    }
    private void LocomoteHorizontal()
    {
        Vector3 distance = des_transfrom.position - m_transform.position;
        distance.y = 0;
        if (distance.magnitude > stopDis)
        {
            if (distance.magnitude < stopDis * 10f) {
                if (cur_speed_hori > 3f) {    //slow down to 3f
                    cur_speed_hori -= acc *3* Time.deltaTime;
                    m_accState = AccState.SpeedDown;
                    m_transform.position += new Vector3(0, 1, 0) * 4f * Time.deltaTime;
                    Debug.Log("brace");
                } else {
                    m_accState = AccState.Stable;
                }
            } else {
                if (cur_speed_hori < move_speed_hori) { //acc to move speed max
                    cur_speed_hori += acc  * Time.deltaTime;
                    m_accState = AccState.SpeedUp;
                    m_transform.position += new Vector3(0,1,0) *-10f * Time.deltaTime;
                } else {
                    m_accState = AccState.Stable;
                }
            }
            float speedFactor = Mathf.Clamp( distance.magnitude / (stopDis*5), 0, 1);
            cur_speed_hori = cur_speed_hori * speedFactor;
            cur_vel_hori = m_transform.forward * cur_speed_hori;
            m_transform.position += cur_vel_hori * Time.deltaTime;
        } else {
            //cur_speed_hori = 0f; //
        }
    }
    private bool LocomoteVertical()
    {
        Vector3 distance = des_transfrom.position - m_transform.position;
        Vector3 distance2D = new Vector3(distance.x, 0, distance.z);
        float speed_ratio =2* distance.y / distance2D.magnitude;

        distance.x = 0;
        distance.z = 0;
        if (Mathf.Abs(distance.y) > 1f)
        {

            if ((cur_speed_ver < move_speed_ver)&&(distance.y > 0f))
            {
                cur_speed_ver += acc * Time.deltaTime;
                //force += acc * m_transform.up;
            }
            if (distance.y < 0f)
            {
                cur_speed_ver += 10.0f * Time.deltaTime;
            }
            float speedFactor = Mathf.Clamp(distance.magnitude / (stopDis * 5), 0.0f, 1);
            
            m_transform.position += distance.normalized * cur_speed_ver* speedFactor * Time.deltaTime;
            return true;
        }
        return false;
    }

    private void Veer()
    {
        Vector3 distance = des_transfrom.position - m_transform.position;
        distance.y = 0;
        if (distance.magnitude > stopDis)
        {
            Vector3 targetDir = (des_transfrom.position - m_transform.position).normalized;
            targetDir.y = 0;
            float angle = Mathf.Acos(Vector3.Dot(targetDir, m_transform.forward));
            angle = Mathf.Rad2Deg * angle;
            Vector3 veerAxis = Vector3.Cross(targetDir, m_transform.forward);
            Vector3 newVelDir =Quaternion.AngleAxis( - angle * Time.deltaTime * rotate_speed, veerAxis) * m_transform.forward;
            accRotate = (newVelDir.normalized - m_transform.forward) * cur_speed_hori;
            if (Mathf.Abs(angle) > 0.1f) {
                m_transform.RotateAround(m_transform.position, veerAxis, -angle * Time.deltaTime * rotate_speed);
            }
        }
    }
}
