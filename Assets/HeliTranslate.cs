using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

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
    public Vector3 cur_vel_hori = new Vector3(0, 0, 0);
    public float cur_speed_ver = 0.0f;
    public Vector3 cur_vel_ver = new Vector3(0,0,0);
    public float cur_rotateDeg = 0.0f;
    public float cur_rotateDeg2 = 0.0f;
    public float playSpeed = 0f;

    public float last_speed_hori = 0.0f;
    public float last_speed_ver = 0.0f;
    public float up_force = 10;
    public Vector3 accRotate;
    private Quaternion initial_body_rotation;
    private float layer_weight = 0.0f;
    private Quaternion idealRotation;
    private float des_angle;
    private float rotate_dive_x;
    private float rotate_dive_y;
    private float rotate_y_temp;
    void Start()
    {
        //anim.SetFloat("flySpeed", 10f);
        m_transform = GetComponent<Transform>();
        _fastNoise = new FastNoise();
        initial_body_rotation = body_transform.localRotation;
        idealRotation = initial_body_rotation;
        m_flyState = FlyState.Stay;
        m_accState = AccState.Stable;
    }
    void FixedUpdate()
    {
        CheckState();
        switch (m_flyState) {
            case FlyState.Stay:
                FlyNormal();
                break;
            case FlyState.Normal:
                FlyNormal();
                break;
            case FlyState.Drop:
                //FlyNormal();
                FlyDrop();
                break;
            case FlyState.Climb:
                FlyNormal();
                break;
        }
       

    }

    private void CheckState() {
        Vector3 dir = des_transfrom.position - m_transform.position;
        float distance = dir.magnitude;
        //Debug.Log("dir is " + dir);
        float angle = Mathf.Abs(Mathf.Acos(Vector3.Dot(dir.normalized, new Vector3(0, 1, 0))) * Mathf.Rad2Deg);
        //Debug.Log("angle is " + angle);
        if ((0f < angle)&& (angle <30f) && distance > 10) {
            m_flyState = FlyState.Climb;
        }else if((30f<angle) && (150> angle) && (distance > stopDis)) {
            m_flyState = FlyState.Normal;
        }else if((angle> 150f) &&(distance > 50f)) {
            m_flyState = FlyState.Drop;
        }else if(distance <= stopDis) {
            m_flyState = FlyState.Stay;
        } else {
            m_flyState = FlyState.Normal;
        }
        des_angle = angle;
    }

    private void Stay() {

    }
    private void FlyNormal() {
        anim.SetBool("is_diving", false);
        LocomoteHorizontal();
        LocomoteVertical(acc, move_speed_ver);
        Veer();
        //ApplyNoise();
        CalAndUpdateForce();
        ApplyAccToBody();
    }

    private void FlyDrop()
    {
        LocomoteHorizontal();
        LocomoteVertical(acc*2, move_speed_ver*10f);
        CalAndUpdateForce();
        ApplyAccToBody();
        anim.SetBool("is_diving", true);
        //Quaternion rotationDown = Quaternion.Euler(90f, 0f, 0f);
        //body_transform.localRotation = rotationDown;
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
        Quaternion detlaRotationX;
        if (m_flyState == FlyState.Drop) {
            rotate_dive_x = Mathf.Lerp(rotate_dive_x, des_angle, Time.deltaTime);
        } else {
            rotate_dive_x = Mathf.Lerp(rotate_dive_x, cur_rotateDeg * 0.7f, Time.deltaTime);
        }
        detlaRotationX = Quaternion.AngleAxis(Mathf.Clamp(rotate_dive_x, -90, 90), new Vector3(1, 0, 0));


        //veer acc rotation
        float rotateDeg2 = Mathf.Clamp(accRotate.magnitude*-7 * acc_sensitive, -65, 65);
        if (Vector3.Dot(accRotate, m_transform.right) < 0)
        {
            rotateDeg2 = -rotateDeg2;
        }
        if (Mathf.Abs(cur_rotateDeg2 - rotateDeg2) > 0.5f) {
            cur_rotateDeg2 = Mathf.Lerp(cur_rotateDeg2, rotateDeg2, 0.1f);
        }
        Quaternion detlaRotationZ = Quaternion.AngleAxis(cur_rotateDeg2, new Vector3(0, 0, 1));


        if (m_flyState == FlyState.Drop) {
            rotate_y_temp += - cur_speed_ver * Time.deltaTime * 7f;
            //rotate_y_temp = rotate_y_temp % 360;
            rotate_dive_y = Mathf.Lerp(rotate_dive_y, rotate_y_temp, Time.deltaTime);
        } else {
            rotate_y_temp = 0;
            rotate_dive_y = Mathf.Lerp(rotate_dive_y, 0, Time.deltaTime);
        }

        Quaternion detlaRotationY = Quaternion.AngleAxis(rotate_dive_y, new Vector3(0, 1, 0));
        body_transform.localRotation =  initial_body_rotation * detlaRotationY* detlaRotationZ * detlaRotationX;


        //apply playspeed
        float up_factor = Mathf.Clamp((up_force - 10f) / 50f , -0.2f, 1f);
        layer_weight =Mathf.Lerp(layer_weight,  Mathf.Clamp((15 - up_force) / 20f, 0f, 0.9f), 5*Time.deltaTime);
        playSpeed = Mathf.Clamp( Mathf.Abs(Mathf.Abs(cur_rotateDeg2)*0.6f + Mathf.Abs(cur_rotateDeg)) / 110f + 1f + up_factor, 0.7f, 3f);
        if(m_accState == AccState.SpeedDown) {
            playSpeed += 0.3f;
        }
        anim.SetFloat( "flySpeed", playSpeed);
        anim.SetLayerWeight(1, layer_weight-0.6f);
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

                    cur_speed_hori -= Mathf.Clamp(cur_speed_hori/2 , 2*acc, acc*5)* Time.deltaTime;
                    m_accState = AccState.SpeedDown;
                } else {
                    cur_speed_hori = 3f * Mathf.Clamp(distance.magnitude / (stopDis * 5), 0, 1);
                    m_accState = AccState.Stable;
                }
            } else {
                if (cur_speed_hori < move_speed_hori) { //acc to move speed max
                    cur_speed_hori += acc  * Time.deltaTime;
                    m_accState = AccState.SpeedUp;
                } else {
                    m_accState = AccState.Stable;
                }
            }
            cur_vel_hori = m_transform.forward * cur_speed_hori;
        } else {
            //cur_speed_hori = 0f; //
        }
        m_transform.position += cur_vel_hori * Time.deltaTime;

    }
    private void LocomoteVertical(float grav_acc, float speed_limit)
    {
        Vector3 distance = des_transfrom.position - m_transform.position;
        Vector3 distance2D = new Vector3(distance.x, 0, distance.z);
        float speed_ratio =2* distance.y / distance2D.magnitude;
        distance.x = 0;
        distance.z = 0;
        up_force =  10;

        if (Mathf.Abs(distance.y) > stopDis * 4f) //acc or maintain
        {
            if ((cur_speed_ver < speed_limit) ||(Vector3.Dot(distance.normalized, cur_vel_ver) < 0f)) 
            {
                m_accState = AccState.SpeedUp;
                cur_vel_ver += distance.normalized * grav_acc * Time.deltaTime;
                up_force = distance.normalized.y * grav_acc + 10;
            }else if(cur_speed_ver > speed_limit) {  //is overspeeding
                m_accState = AccState.SpeedDown;
                cur_vel_ver -= cur_vel_ver.normalized * Mathf.Clamp((cur_vel_ver.magnitude/speed_limit-1f)*30f, 0, 10f) * grav_acc * Time.deltaTime;
                up_force = 10f - cur_vel_ver.normalized.y * Mathf.Clamp((cur_vel_ver.magnitude / speed_limit - 1f) * 10f, 0, 4f) * grav_acc;
            }
        } else {
            if ((cur_speed_ver > 2f) ) {
                m_accState = AccState.SpeedDown;
                cur_vel_ver -= cur_vel_ver.normalized * Mathf.Clamp(cur_vel_ver.magnitude/10f, 0, 4f)* grav_acc * Time.deltaTime;
                up_force  = 10f -cur_vel_ver.normalized.y * Mathf.Clamp(cur_vel_ver.magnitude/10f, 0, 4f) * grav_acc;
            } else {
                m_accState = AccState.Stable;
                cur_vel_ver = cur_vel_ver * Mathf.Clamp(distance.magnitude / (stopDis * 2), 0, 1);
            }
        }
        cur_speed_ver = cur_vel_ver.magnitude;
        m_transform.position += cur_vel_ver * Time.deltaTime;

    }

    private void Veer()
    {
        Vector3 distance = des_transfrom.position - m_transform.position;
        distance.y = 0;
        if (distance.magnitude > stopDis*2)
        {
            Vector3 targetDir = des_transfrom.position - m_transform.position;
            targetDir.y = 0;
            float angle = Mathf.Acos(Vector3.Dot(targetDir.normalized, m_transform.forward));
            angle = Mathf.Rad2Deg * angle;
            Vector3 veerAxis = Vector3.Cross(targetDir.normalized, m_transform.forward);
            Vector3 newVelDir =Quaternion.AngleAxis( - angle * Time.deltaTime * rotate_speed, veerAxis) * m_transform.forward;
            accRotate = (newVelDir.normalized - m_transform.forward) * cur_speed_hori;
            if (Mathf.Abs(angle) > 0.1f) {
                m_transform.RotateAround(m_transform.position, veerAxis, -angle * Time.deltaTime * rotate_speed);
            }
        }
    }
}
