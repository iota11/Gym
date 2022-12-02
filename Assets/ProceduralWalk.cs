using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;


public enum BodyState
{
    move,
    stall
}
public class ProceduralWalk : MonoBehaviour
{
    public List<ProceduralLeg> legsList;
    public float period =1.2f;
    private float stall;
    public float airTime = 0.03f;
    public float stableRadius = 0.2f;
    private int legCount;
    public Transform m_transform;
    public float noise_scale = 0f;
    public float noise_speed = 1f;
    public float speed;
    public float sessionTime = 0.0f;
    public Vector3 movement;
    public Vector3 movementWorld;
    public BodyState m_state = BodyState.stall;
    public int triggerLegIndex = 0;
    public float initialCenterDis = 0f;
    public float stableRotationY = 0.0f;
    public Vector3 testMovement;
    public float rotateSpeed = 1f;
    public float  minSpeed = 1.0f;
    public float maxSpeed = 10.0f;
    public float minMoveHeight = 1.0f;
    public float maxMoveHeight = 4.0f;
    void Start()
    {
        m_transform = this.GetComponent<Transform>();
        legCount = legsList.Count;
        stall = period / (float)legCount;
        initialCenterDis = GetCenterDis();
        stableRotationY = m_transform.eulerAngles.y;

    }

    private void Update()
    {
        UpdateParam();

    }


    private void UpdateParam()
    {
        float minStride = 10000f;
        for(int i = 0; i< legCount; i++)
        {
            if (legsList[i].legStride < minStride)
            {
                minStride = legsList[i].legStride;
            }
        }
        float T1min = minStride / speed;
        period = airTime + legCount * T1min;
        float curSpeed = movementWorld.magnitude * speed;
        curSpeed = Mathf.Clamp(curSpeed, minSpeed, maxSpeed);
        float speedLerp = (speed - minSpeed) / (maxSpeed - minSpeed);
        float moveMax = period - T1min;
        float moveMin = period / 2f;
        float legMoveTime = Mathf.Lerp( moveMin, moveMax, speedLerp);
        float legMoveHight = Mathf.Lerp(minMoveHeight, maxMoveHeight, speedLerp);
        airTime = Mathf.Lerp(0.03f, 0.1f, speedLerp);
        foreach(ProceduralLeg leg in legsList)
        {
            leg.moveTime = legMoveTime;
            leg.heightScale = legMoveHight;
        }
    }
    private float GetCenterDis()
    {
        Vector3 center = GetCenter();
        float totalDis = 0.0f;
        for(int i=0; i< legCount; i++)
        {
            totalDis += Vector3.Distance(center, legsList[i].curPos);
        }
        return totalDis;
    }

    private Vector3 GetCenter()
    {
        Vector3 center = new Vector3(0f, 0f, 0f);
        foreach (ProceduralLeg leg in legsList)
        {
            center += leg.newPos;
        }
        center /= legsList.Count;
        return center;
    }

    private bool CheckToMove(Vector3 bodyPredict)
    {
        Vector3 center = GetCenter();

        //check each legs to move:
        bool haveLegToMove = false;
        for (int i = 0; i < legCount; i++)
        {
            Vector3 ideaLocation = legsList[i].IdealLocation(bodyPredict);
            float distance = Vector3.Distance(ideaLocation, legsList[i].newPos);
            if (distance > stableRadius)
            {
                haveLegToMove = true;
            }
        }
        return haveLegToMove;
    }


    private int CheckBestLeg(Vector3 predictPos)
    {
        int index = 0;
        float maxDis = 0;
        for(int i = 0;  i < legCount; i++)
        {
            float distance = Vector3.Distance(legsList[i].curPos, predictPos);
           if(distance > maxDis)
            {
                maxDis = distance;
                index = i;
            }
        }
        return index;
    }
    private void FixedUpdate()
    {
        //update 
        stall = period / (float)legCount;
        //self movement
        movement = Locomote();
        m_transform.Translate(movement * speed * Time.deltaTime);
        movementWorld = m_transform.TransformDirection(movement);
        float predicTime = legsList[0].moveTime + (period - legsList[0].moveTime) / 2f;
        Vector3 bodyPredict = m_transform.position + movementWorld * speed * predicTime;

        if(m_state == BodyState.stall)
        {
            if (CheckToMove(bodyPredict)) {
                m_state = BodyState.move;
                sessionTime = 0.0f;
                triggerLegIndex = CheckBestLeg(bodyPredict);
            }
        }


        if (m_state == BodyState.move)
        {
            sessionTime += Time.fixedDeltaTime;
            ProceduralLeg idealLeg = legsList[triggerLegIndex];
            if ((!idealLeg.istriggered) &&(idealLeg.isStall())){
                Vector3 idealPlace = idealLeg.IdealLocation(bodyPredict);
                idealLeg.setDestination(idealPlace);
             }

            //trigger next one is finished
            if((sessionTime >= stall) || (idealLeg.isStall()))
            {
                triggerLegIndex = (triggerLegIndex + 1) % legCount;
                sessionTime = 0.0f;
            }
        }

        if (!CheckToMove(bodyPredict))
        {
            m_state = BodyState.stall;
            sessionTime = 0;
        }



        for (int i = 0; i < legCount; i++)
        {
            if (true)
            {
                legsList[i].MoveLeg(Time.fixedDeltaTime);
                legsList[i].CheckTimer(period);
            }
        }
    }

    private Vector3 Locomote()
    {

        //control rotation
        if (Input.GetKey(KeyCode.E))
        {
            m_transform.Rotate(m_transform.up, Time.deltaTime*10*rotateSpeed);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            m_transform.Rotate(m_transform.up, -Time.deltaTime*10 * rotateSpeed);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            speed +=Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.C))
        {
            speed -= Time.deltaTime;
        }
        speed = Mathf.Clamp(speed, 1f, 9f);
        //control movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 noise = new Vector3(Mathf.PerlinNoise(Time.time * noise_speed, 0) - 0.5f, 0, Mathf.PerlinNoise(0, Time.time * noise_speed) - 0.5f);
       return  new Vector3(x, 0, z) + noise * noise_scale + testMovement;
    }
}
