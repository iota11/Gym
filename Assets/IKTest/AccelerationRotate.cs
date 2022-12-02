using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationRotate : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform m_transform;

    public Queue<Vector3> pos_list, vel_list, acc_list;
    public Vector3 ori_pos;
    public Vector3 avg_acc;
    public Vector3 ori_rotation;
    public int queueLength = 3;
    public float sensitive = 1f;
    public bool active;
    private Quaternion m_InitialRotation;
    void Start()
    {
        m_transform = GetComponent<Transform>();
        ori_rotation = m_transform.localEulerAngles;
        m_InitialRotation = m_transform.localRotation;
        ori_pos = m_transform.position;
        pos_list = new Queue<Vector3>();
        vel_list = new Queue<Vector3>();
        acc_list = new Queue<Vector3>();

        for (int i=0; i<queueLength; i++)
        {
            pos_list.Enqueue(m_transform.position);
        }
        for(int i = 0; i<queueLength-1; i++)
        {
            vel_list.Enqueue(new Vector3(0, 0, 0));
        }
        for(int i=0; i< queueLength-2; i++)
        {
            acc_list.Enqueue(new Vector3(0, 0, 0));
        }


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 pos_new = m_transform.position;
        pos_list.Enqueue(pos_new);
        pos_list.Dequeue();

        Vector3[] pos_array = pos_list.ToArray();
        
        Vector3 vel_new = (pos_array[1] - pos_array[0])/Time.deltaTime;
        //Vector3 v1 = (pos_array[2] - pos_array[1])/Time.deltaTime;
        vel_list.Enqueue(vel_new);
        vel_list.Dequeue();

        Vector3[] vel_array = vel_list.ToArray();
        Vector3 acc_new = (vel_array[1] - vel_array[0]) / Time.deltaTime;
        acc_list.Enqueue(acc_new);
        acc_list.Dequeue();

        vel_array = vel_list.ToArray();
        Vector3 avg = new Vector3();
        for (int i= 0; i<vel_array.Length; i++)
        {
            avg += vel_array[i];
        }
        avg /= vel_array.Length;

        avg_acc = avg;
        //UpdateQueue();
        




    }
    void UpdateQueue()
    {
        Vector3 pos_new = m_transform.position;
        pos_list.Enqueue(pos_new);
        pos_list.Dequeue();
    }


    private void LateUpdate()
    {

        Vector3 acc_rotation = Vector3.Cross(avg_acc, new Vector3(0, 1, 0)).normalized * avg_acc.magnitude;

        //acc_rotation = new Vector3(acc_rotation.x, -acc_rotation.z, acc_rotation.y);
        //m_transform.eulerAngles = m_transform.eulerAngles - acc_rotation * sensitive;
        Quaternion cur_rotation = Quaternion.Euler(acc_rotation*sensitive);
        if (active)
        {
            cur_rotation = Quaternion.Inverse(cur_rotation);
        }
        m_transform.localRotation = cur_rotation * m_InitialRotation;
    }
    void ApplyRotation()
    {
        
    }
}
