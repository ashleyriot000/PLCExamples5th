using UnityEngine;

public class CylinderController : MonoBehaviour
{
    public ConfigurableJoint joint;
    public Vector3 forwardPosition;
    public Vector3 backardPosition;

    //전진 함수
    public void ToForward(bool isOn)
    {
        if(isOn)
            joint.targetPosition = forwardPosition;
    }

    //후퇴 함수
    public void ToBackward(bool isOn)
    {
        if(isOn)
            joint.targetPosition = backardPosition;
    }
}
