using UnityEngine;

public class ACMotorController : MonoBehaviour
{
    public HingeJoint shaft;        //회전축을 회전시키기 위해 필요.
    public Rigidbody rb;            //회전 저항 수정하기 위해 필요.

    public Vector3 torqueAxis = Vector3.forward;    //회전축 방향 설정값
    public float targetVelocity = 60f;              //초당 목표 회전속도
    public float torque = 1f;                       //토크값
    public float damping = 0.01f;                   //회전 저항값

    private bool isOnForward = false;               //정방향 전류On
    private bool isOnBackward = false;              //역방향 전류On

    private void Awake()
    {
        shaft = GetComponent<HingeJoint>();
        if(shaft != null)
        {
            shaft.motor = new JointMotor()
            {
                targetVelocity = this.targetVelocity,
                force = this.torque,
                freeSpin = false
            };
            shaft.axis = torqueAxis;
        }

        rb = GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.maxAngularVelocity = 6000f;
            rb.angularDamping = damping;
        }
    }

    public bool IsOnForward
    {
        get => isOnForward;

        set
        {
            if (isOnForward == value)
                return;

            if(isOnForward = value)
            {
                isOnBackward = false;
            }

            if(isOnForward)
            {
                shaft.axis = torqueAxis;
                shaft.useMotor = true;
            }
            else if(isOnForward == isOnBackward)
            {
                shaft.useMotor = false;
            }
        }
    }

    public bool IsOnBackward
    {
        get => isOnBackward;

        set
        {
            if (isOnBackward == value)
                return;

            if (isOnBackward = value)
            {
                isOnForward = false;
            }


            if (isOnBackward)
            {
                shaft.axis = torqueAxis * -1f;
                shaft.useMotor = true;
            }
            else if (isOnForward == isOnBackward)
            {
                shaft.useMotor = false;
            }
        }
    }

    
}
