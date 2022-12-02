using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;




public class ProceduralBiped : MonoBehaviour
{
   
    private Transform m_transform;
    public float speed = 3.0f;
    public Transform left_aim;
    public Transform right_aim;
    public Transform hips;

    public float T_stall = 0.1f;
    public float T_move = 0.4f;
    public float step = 0.4f;

    private float miniorRadius = 0.2f;

    public float noise_scale = 0.1f;
    public float noise_speed = 1f;
    public float footWidth = 0.4f;

    private Vector3 movement;
    private Vector3 movementWorld;
    private float height;

    public float height_influence = 0f;
    public float foot_width = 0.4f;
    public float maxFootWidth = 1.0f;

    private Quaternion leftInitialRotation;
    private Quaternion rightInitialRotation;
    private Quaternion m_InitialRotation;

    private BipedLeg leftLeg;
    private BipedLeg rightLeg;

    public float legLength = 0.7f;
    void Start()
    {
        m_transform = GetComponent<Transform>();
        /*
        right_curPos = right_aim.position;
        left_curPos = left_aim.position;

        right_newPos = right_curPos;
        left_newPos = left_curPos;

        right_oldPos = right_curPos;
        left_oldPos = left_curPos;
        */
        leftLeg = new BipedLeg(left_aim, 0.3f, legLength) ;
        rightLeg = new BipedLeg(right_aim, 0.3f, legLength);
        height = m_transform.position.y;


        leftInitialRotation = left_aim.transform.localRotation;
        rightInitialRotation = right_aim.transform.localRotation;
        m_InitialRotation = m_transform.localRotation;

    }

    void Update()
    {
        //overall Transform
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");


        Vector3 noise = new Vector3(Mathf.PerlinNoise(Time.time * noise_speed, 0) - 0.5f, 0, Mathf.PerlinNoise(0, Time.time * noise_speed) - 0.5f);
        movement = new Vector3(x, 0, z) + noise * noise_scale;
        movementWorld = m_transform.TransformDirection(movement);
        m_transform.Translate(movement * speed * Time.deltaTime);

        LegMovement();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(leftLeg.newPos, 0.1f);
        Gizmos.DrawSphere(rightLeg.newPos, 0.1f);
    }

    private void LegMovement()
    {
        //First check whether we need move or not
        Vector3 footDir = new Vector3(m_transform.forward.z, 0, m_transform.forward.x);
        if (ToMove())
        {
            if (leftLeg.isStall() && rightLeg.isStall())
            {
                //set leftLegFirst;
                     leftLeg.setDestination(m_transform.position + movementWorld * 2* T_move - footDir* footWidth, 2*T_move);
            }
            else if (leftLeg.isStall() && !rightLeg.isStall())
            {
                //check if leg meet condition to move
                if(Vector3.Distance(leftLeg.curPos, hips.position) > legLength)
                {
                    //set leftLeg
                    leftLeg.setDestination(m_transform.position + movementWorld * 2 * T_move - footDir * footWidth, 2 * T_move);
                }
            }
            else if (!leftLeg.isStall() && rightLeg.isStall()){
                //check if right meet condition to move
                if (Vector3.Distance(rightLeg.curPos, hips.position) > legLength)
                {
                    //set right
                    rightLeg.setDestination(m_transform.position + movementWorld * 2 * T_move + footDir * footWidth, 2 * T_move);
                }

            }
            else
            {
                // do nothing
            }
        }
        
        //move leftleg
        leftLeg.MoveLeg(Time.fixedDeltaTime/(2f*T_move));
        //move rightleg
        rightLeg.MoveLeg(Time.fixedDeltaTime/(2f*T_move));

        

    }



    private bool IsBalanced(Vector3 leftPos, Vector3 rightPos, Vector3 bodyPoint, float radius)
    {
        Vector2 CG = new Vector2((leftPos.x + rightPos.x) / 2f, (leftPos.z + rightPos.z) / 2f);
        return Vector2.Distance(CG, new Vector2(bodyPoint.x, bodyPoint.z)) <= radius;
    }

    private bool ToMove()
    {
         if(!IsBalanced(leftLeg.newPos, rightLeg.newPos, m_transform.position+movement*2*T_move, 0.2f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public class BipedLeg
{
    public Transform m_transform;
    public float legLength;
    public bool isLeft;
    public LegState m_state = LegState.stall;
    public float lerp = 0.0f;
    public Vector3 curPos;
    public Vector3 oldPos;
    public Vector3 newPos;
    public float heightScale;
    public float moveSpeed;
    //constructor
    public BipedLeg(Transform legTransform, float footHeightScale, float newlegLength)
    {
        m_transform = legTransform;
        legLength = newlegLength;
        curPos = m_transform.position;
        oldPos = m_transform.position;
        newPos = m_transform.position;
        heightScale = footHeightScale;
        m_state = LegState.stall;
    }

    public bool isStall()
    {
        return m_state == LegState.stall;
    }

    public void MoveLeg(float deltaTime)
    {
        if ((lerp < 1) && m_state == LegState.move)
        {
            Vector3 footpos = Vector3.Lerp(oldPos, newPos, lerp);
            footpos.y += footHeight(lerp) * heightScale;
            curPos = footpos;
            m_transform.position = curPos;
            lerp += deltaTime;
        }
        if (lerp >= 1.0f) //reset trigger
        {
            oldPos = newPos;
            m_state = LegState.stall;
        }
    }

    public void setDestination(Vector3 newDes, float moveTime)
    {
        newPos = newDes;
        m_state = LegState.move;
        lerp = 0.0f;
        float distance = Vector3.Distance(curPos, newDes);
        moveSpeed = distance / moveTime;
    }

    private float footHeight(float x)
    {
        x = Mathf.Clamp(x, 0f, 1f);
        return Mathf.Pow((1 - x), 3) * x;
    }

    private float footHeightCircle(float x)
    {
        return Mathf.Sin(lerp * Mathf.PI) * 0.3f;
    }


}