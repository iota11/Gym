using System.Collections;
using System.Collections.Generic;
using UnityEngine;




    public class BodyControl : MonoBehaviour
    {
        public enum Leg_state
        {
            right,
            left
        }

        public float timeScale = 1.0f;
        private float startTimeDelta;
        Transform m_transform;

        public float speed = 3.0f;
        public float acc = 1.0f;
        private float speed_ori;
        public Transform right_aim;
        public Transform left_aim;

        public float T_stall = 0.1f;
        public float T_move = 0.4f;
        public float step = 0.4f;

        //public float stepHight = 0.3f;
        //private  float foot_speed = 3.0f;


        private Vector3 right_newPos;
        private Vector3 right_oldPos;
        private Vector3 right_curPos;

        private Vector3 left_newPos;
        private Vector3 left_oldPos;
        private Vector3 left_curPos;

        private float lerp_left = 1;
        private float lerp_right = 1;

        private float miniorRadius = 0.2f;

        public float noise_scale = 0.1f;
        public float noise_speed = 1f;


        private Vector3 movement;
        private float height;

        public float height_influence = 0f;
        public float foot_width = 0.4f;
        public float maxFootWidth = 1.0f;
        public Leg_state m_legState = Leg_state.left;
        // Start is called before the first frame update


        private Quaternion leftInitialRotation;
        private Quaternion rightInitialRotation;
        private Quaternion m_InitialRotation;
        void Start()
        {
            m_transform = GetComponent<Transform>();
            speed_ori = speed;


            right_curPos = right_aim.position;
            left_curPos = left_aim.position;

            right_newPos = right_curPos;
            left_newPos = left_curPos;

            right_oldPos = right_curPos;
            left_oldPos = left_curPos;

            height = m_transform.position.y;


            leftInitialRotation = left_aim.transform.localRotation;
            rightInitialRotation = right_aim.transform.localRotation;
            m_InitialRotation = m_transform.localRotation;

            startTimeDelta = Time.fixedDeltaTime;
        }

        // Update is called once per frame
        void Update()
        {

        }


        private void CheckToMove(Vector3 pos_left, Vector3 pos_center, Vector3 pos_right)
        {

        }


        bool IsInEllipse(Vector2 point, Vector2 leftDir, Vector2 rightDir, float minorRadius)
        {
            Vector2 center = leftDir / 2f + rightDir / 2f;
            Vector2 majorDir = (leftDir - rightDir).normalized;
            float a = Vector2.Distance(leftDir, rightDir) / 2f;
            float b = minorRadius>a? a: minorRadius;
            float c = Mathf.Sqrt(Mathf.Pow(a, 2f) - Mathf.Pow(b, 2f));
            Vector2 C1 = center + majorDir * c;
            Vector2 C2 = center - majorDir * c;
            return (Vector2.Distance(C1, point) + Vector2.Distance(C2, point)) <= 2 * a;
        }

        

        private void FixedUpdate()
        {

            //overall Transform
            Time.fixedDeltaTime = startTimeDelta * timeScale;
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");


            Vector3 noise = new Vector3(Mathf.PerlinNoise(Time.time* noise_speed, 0)-0.5f, 0, Mathf.PerlinNoise(0, Time.time* noise_speed) -0.5f);
            movement = new Vector3(x, 0, z) + noise * noise_scale;
            Vector3 movementWorld = m_transform.TransformDirection(movement);
            m_transform.Translate(movement * speed * Time.deltaTime);

            
            int layerMask = 1 << 8;
            layerMask = 1 << 6;


            right_aim.position = right_curPos;
            left_aim.position = left_curPos;



            //calculate next steps
            Vector2 pos_hip = new Vector2(m_transform.position.x, m_transform.position.z);
            Vector2 right_curPos2D = new Vector2(right_curPos.x, right_curPos.z);
            Vector2 left_curPos2D = new Vector2(left_curPos.x, left_curPos.z);
            Vector2 pos_center = right_curPos2D + left_curPos2D;
            Vector2 movement2D = new Vector2(movementWorld.x, movementWorld.z);
            Vector2 prdc_hip = pos_hip + movement2D * miniorRadius *speed;//predicts positio
            Vector2 m_forward = new Vector2(m_transform.forward.x, m_transform.forward.z);


            Vector2 ideal_left = left_curPos2D;
            Vector2 ideal_right = right_curPos2D;

            //calculate whether to move or not
            if ((lerp_left >=1) && (lerp_right >=1))//when both finished moving
            {
                float footDis = Vector2.Distance(left_curPos2D, right_curPos2D);

                if (!IsInEllipse(prdc_hip, left_curPos2D, right_curPos2D, miniorRadius) // if inbalanced
                   || footDis >= maxFootWidth) 
                {
                    if(footDis >= maxFootWidth)
                    {
                        if(Vector2.Distance(left_curPos2D, prdc_hip) > Vector2.Distance(right_curPos2D, prdc_hip))
                        {
                            m_legState = Leg_state.left;
                        }
                        else
                        {
                            m_legState = Leg_state.right;
                        }
                    
                    }

                    //calculate the prediction foot.
                    Vector2 foot_dir = new Vector2(-m_forward.y, m_forward.x);
                    ideal_left = prdc_hip + foot_dir * foot_width;
                    ideal_right = prdc_hip - foot_dir * foot_width;
                    if (m_legState == Leg_state.left) //to move left rig
                    {
                        left_newPos = new Vector3(ideal_left.x, 0f, ideal_left.y);
                        lerp_left = 0;
                        m_legState = Leg_state.right;
                    }
                    else { 
                        right_newPos = new Vector3(ideal_right.x, 0f, ideal_right.y);
                        lerp_right = 0;
                        m_legState = Leg_state.left;
                    }
                }
            }
   
            if (lerp_right < 1)
            {
                Vector3 footpos = Vector3.Lerp(right_oldPos, right_newPos, lerp_right);
                footpos.y += Mathf.Sin(lerp_right * Mathf.PI) * 0.3f;
                right_curPos = footpos;
                lerp_right += Time.deltaTime * 3;
            }
            else
            {
                right_oldPos = right_newPos;
            }


            if (lerp_left < 1)
            {
                Vector3 footpos = Vector3.Lerp(left_oldPos, left_newPos, lerp_left);
                footpos.y += Mathf.Sin(lerp_left * Mathf.PI) * 0.3f;
                left_curPos = footpos;
                lerp_left += Time.deltaTime * 0.3f;
            }
            else
            {
                left_oldPos = left_newPos;
                left_oldPos = left_newPos;
            }

        }

        private void LateUpdate()
        {
            float new_height = height - movement.magnitude * height_influence;
            m_transform.position = new Vector3(m_transform.position.x, new_height, m_transform.position.z);

            left_aim.localRotation = m_transform.localRotation * leftInitialRotation;
            right_aim.localRotation = m_transform.localRotation * rightInitialRotation;

        }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.9f);
        Gizmos.DrawCube(left_newPos, new Vector3(0.1f, 0.1f, 0.1f));
        Gizmos.DrawCube(right_newPos, new Vector3(0.1f, 0.1f, 0.1f));

        Gizmos.color = new Color(0.1f, 0.1f, 0, 0.5f);
        Gizmos.DrawCube(right_oldPos, new Vector3(0.1f, 0.1f, 0.1f));
        Gizmos.DrawCube(left_oldPos, new Vector3(0.1f, 0.1f, 0.1f));
        

    }



}




 
