using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum FishState
{
    FindDestination = 0,
    Cruise = 1,
    Circle =2
}

public class FishController : MonoBehaviour
{
    
    private Transform m_transform;
    public FishState m_state;
    public Transform des_transfrom;
    public Transform head_tranform;
    public float rotate_speed = 5f;
    public float move_speed = 10f;
    public float shake_frequency = 1f;
    public float shake_amplitude = 0.0f;
    public float speed_factor = 1.0f;
    public float shake_factor = 1.0f;
    private bool ready_move;
    public bool isFlee;
    [SerializeField] GameObject crystal;
    void Start()
    {
        m_transform = GetComponent<Transform>();
        m_state = FishState.Cruise;
        ready_move = false;

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 direction = Vector3.Normalize(des_transfrom.position - m_transform.position);
        if (Mathf.Abs(Vector3.Dot(direction, m_transform.forward)) > 0.8) ready_move = true;
       
        RotateToDes(direction);
        
        if(ready_move)
        {
            MoveToDes(des_transfrom.position - direction*0.3f);
        }
        Shakehead();
        
    }

    void RotateToDes(Vector3 dir)
    {
        Quaternion rotation = Quaternion.LookRotation(dir);
        m_transform.rotation = Quaternion.Lerp(m_transform.rotation, rotation, rotate_speed);
    }

    public void MoveToDes(Vector3 pos) 
    {
        //float speed_factor = 1.0f;
        float distance = Vector3.Distance(m_transform.position , pos);

        if (distance > 5)
        {
            speed_factor = 1.0f;
            shake_factor = 2.0f;
        }
        else
        {
            speed_factor = (distance / 5f) * 0.9f + 0.1f;//ease out
            shake_factor = 0.2f;
        }

        m_transform.position = Vector3.MoveTowards(m_transform.position, pos, move_speed * speed_factor * Time.deltaTime);
    }



    void Shakehead()
    {
        //head_tranform.Rotate(0,move_speed * Mathf.Sin(Time.time)/10,  0, Space.Self);
        head_tranform.Rotate(0, 0, Mathf.Sin(Time.deltaTime*shake_frequency) *shake_amplitude, Space.Self);
    }
    public void SetDesLoc(Vector3 loc)
    {
        des_transfrom.position = loc;
    }

    public Vector3 GetDesLoc(Vector3 loc)
    {
        return des_transfrom.position;
    }

}
