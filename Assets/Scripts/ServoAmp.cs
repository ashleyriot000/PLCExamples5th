using UnityEngine;
using UnityEngine.Events;

public class ServoAmp : MonoBehaviour
{
    //서보 앰프의 구동 방식 열거형
    public enum AmpType
    {
        Linear,
        Rotary
    }

    //컨트롤 유닛 타입
    public enum ControlUnit
    {
        mm,
        degree,
        pulse
    }

    //앰프의 상태
    public enum AmpState
    {
        Idle,
        Jogging,
        Positioning,
        Homing_Search,
        Homing_Retry,
        Homing_Creep,
        Error
    }

    [Header("[1] 기구 및 단위 설정")]
    public AmpType ampType = AmpType.Linear;
    public ControlUnit controlUnit = ControlUnit.mm;

    [Header("[2] 기구 파라미터")]
    public float motorResolution = 131072f;     //서보모터의 분해능
    public float gearRatic = 1.0f;              //연결된 기어의 감속비
    public float ballscrewLead = 10f;           //서보 모터 1회전당 전진 길이

    [Header("[3] 속도 파라미터")]
    [Tooltip("위치 결정 운전시 최대 속도")]
    public float maxSpeed = 500f;               //위치 결정시 최대 속도
    [Tooltip("수동 JOG운전시 속도")]
    public float jogSpeed = 100f;               //수동 운전시 스피드
    [Tooltip("원점 도그를 찾으거 갈 때 속도(고속)")]
    public float homingHighSpeed = 200f;
    [Tooltip("도그 감지 후 정밀하게 이동할 때 속도(저속)")]
    public float homingCreepSpeed = 20.0f;

    [Header("[4] 가감속 시간")]
    public float accelTime = 100f;
    public float inPosWidth = 0.1f;                 //타겟 포지션의 도착 오차범위

    [Header("모니터링")]
    public float currentPos_Unit;
    public int currentPulse;

    public UnityEvent<int> onChangedPulse;          //펄스값이 변경되면 변경된 값을 받을 콜백함수를 담는 델리게이트
    public UnityEvent<bool> onChangedReady;         //준비 상태값이 변경되면 그 값을 받을 콜백함수를 담는 델리게이트
    public UnityEvent<bool> onChangedError;         //에러 상태값이 변경되면 그 값을 받을 콜백함수를 담는 델리게이트
    public UnityEvent<bool> onChangedBusy;          //명령 수행 상태값이 변경되면 그 값을 받을 함수들을 담는 델리게이트

    //현재 펄스값을 확인 혹은 변경하는 프로퍼티
    public int GetCurrentPulse
    {
        get => currentPulse;
        set
        {
            if (currentPulse == value)
                return;

            currentPulse = value;
            onChangedPulse?.Invoke(value);
        }
    }

    [Header("센서 입력")]
    public bool isOnLimitSensorPositive = false;         //상한 리미트 센서에 감지여부
    public bool isOnLimitSensorNegative = false;         //하한 리미트 센서에 감지여부
    public bool isOnProximityDOG = false;               //근점 도그 센서에 감지 여부

    public bool IsOnLSP
    {
        get => isOnLimitSensorPositive;
        set => isOnLimitSensorPositive = value;
    }

    public bool IsOnLSN
    {
        get => isOnLimitSensorNegative;
        set => isOnLimitSensorNegative = value;
    }

    public bool IsOnPDOG
    {
        get => isOnProximityDOG;
        set => isOnProximityDOG = value;
    }

}
