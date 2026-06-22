using UnityEngine;
using UnityEngine.Events;

public class RollerConveyorConnector : MXObject
{
    public HingeJoint[] rollers;        //돌리고 싶은 롤러만큼 연결

    public DeviceAddress forwardAddress = new DeviceAddress("롤러 정회전");
    public DeviceAddress reverseAddress = new DeviceAddress("롤러 역회전");

    public int maxRPM = 60; //정격 회전수(분당 회전수)
    [Min(0.1f)]
    public float accelTime = 1f;        //가감속 시간

    private float maxVelocity;      //최대 속도
    private float targetVelocity;   //목표 속도
    private float currentVelocity;  //현재 속도


    private bool isOnForward;
    public bool IsOnForward
    {
        get => isOnForward;
        private set
        {
            if (isOnForward == value)
                return;

            isOnForward = value;
            onChangedForward?.Invoke(value);
            CalculateTargetSpeed(isOnForward, isOnReverse);
        }
    }
    public UnityEvent<bool> onChangedForward;

    private bool isOnReverse;
    public bool IsOnReverse
    {
        get => isOnReverse;
        private set
        {
            if (isOnReverse == value)
                return;

            isOnReverse = value;
            onChangedReverse?.Invoke(value);
            CalculateTargetSpeed(isOnForward, isOnReverse);
        }
    }
    public UnityEvent<bool> onChangedReverse;

    private void Start()
    {
        if(forwardAddress.useDevice && !string.IsNullOrEmpty(forwardAddress.address))
            MXRequester.Get.AddDeviceAddress(forwardAddress.address, OnChangedForward);

        if (reverseAddress.useDevice && !string.IsNullOrEmpty(reverseAddress.address))
            MXRequester.Get.AddDeviceAddress(reverseAddress.address, OnChangedReverse);
    }

    private void FixedUpdate()
    {
        //현재 초당 회전각 속도 구하기.
        currentVelocity = Mathf.MoveTowards(currentVelocity, targetVelocity,
            (maxVelocity / accelTime) * Time.fixedDeltaTime);

        //모든 롤러에 동일한 각속도 적용.
        foreach(var roller in rollers)
        {
            var motor = roller.motor;
            motor.targetVelocity = currentVelocity;
            roller.motor = motor;
        }
    }

    private void CalculateTargetSpeed(bool isOnForward, bool isOnReverse)
    {
        //둘다 동일한 신호가 들어올 경우
        if(isOnForward == isOnReverse)
        {
            targetVelocity = 0f;
            return;
        }

        if(isOnForward)
        {
            targetVelocity = maxVelocity;
            return;
        }

        targetVelocity = -maxVelocity;
    }

    public void OnChangedForward(short data)
    {
        IsOnForward = data != 0;
    }

    public void OnChangedReverse(short data)
    {
        IsOnReverse = data != 0;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        //초당 최대 각속도를 구함
        maxVelocity = maxRPM * 6f;
    }
#endif
}
