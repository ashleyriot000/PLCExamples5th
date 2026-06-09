using UnityEngine;

public class CylinderController : MonoBehaviour
{
    public enum InterlockType   //인터락 우선순위 
    {
        FirstInputPriority,     //선입력 우선
        LateInputPriority,      //후입력 우선
        MutualResetCircuit      //양쪽 신호 무시
    }
    public ConfigurableJoint joint;
    public InterlockType priority;
    public Vector3 forwardPosition;
    public Vector3 backwardPosition;

    private bool isOnForward;
    private bool isOnBackward;
    //전진 함수
    public void ToForward(bool isOn)
    {
        if(isOnForward == false && isOn)
        {
            joint.targetPosition = priority switch
            {
                InterlockType.FirstInputPriority => isOnBackward ? backwardPosition : forwardPosition,
                InterlockType.LateInputPriority => forwardPosition,
                InterlockType.MutualResetCircuit => isOnBackward ? backwardPosition : forwardPosition,
                _ => Vector3.zero
            };
        }

        isOnForward = isOn;
    }

    //후퇴 함수
    public void ToBackward(bool isOn)
    {
        if (isOnBackward == false && isOn)
        {
            joint.targetPosition = priority switch
            {
                InterlockType.FirstInputPriority => isOnForward ? forwardPosition : backwardPosition,
                InterlockType.LateInputPriority => backwardPosition,
                InterlockType.MutualResetCircuit => isOnForward ? forwardPosition : backwardPosition,
                _ => Vector3.zero
            };
        }

        isOnBackward = isOn;
    }
}
