using System;
using UnityEngine;
using UnityEngine.Events;

//해당 스크립트가 반드시 필요한 컴포넌트를 세팅하는 속성
[RequireComponent(typeof(ConfigurableJoint))]
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
    public float gearRatio = 1.0f;              //연결된 기어의 감속비
    public float ballscrewLead = 10f;           //서보 모터 1회전당 전진 길이

    [Header("[3] 속도 파라미터")]
    [Tooltip("위치 결정 운전시 최대 속도")]
    public float maxSpeed = 500f;               //위치 결정시 최대 속도
    [Tooltip("수동 JOG운전시 속도")]
    public float jogSpeed = 100f;               //수동 운전시 스피드

    public int defaultHomingDirection = 1;      //원점 복귀 기본 방향
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
    public UnityEvent<float> onChangedPos;          //위치결정값이 변경되면 변경된 값을 받을 콜백함수를 담는 델리게이트
    public UnityEvent<bool> onChangedReady;         //준비 상태값이 변경되면 그 값을 받을 콜백함수를 담는 델리게이트
    public UnityEvent<bool> onChangedError;         //에러 상태값이 변경되면 그 값을 받을 콜백함수를 담는 델리게이트
    public UnityEvent<bool> onChangedBusy;          //명령 수행 상태값이 변경되면 그 값을 받을 함수들을 담는 델리게이트

    //현재 펄스값을 확인 혹은 변경하는 프로퍼티
    public int GetCurrentPulse
    {
        get => currentPulse;
        private set
        {
            if (currentPulse == value)
                return;

            currentPulse = value;
            onChangedPulse?.Invoke(value);
        }
    }
    //현재 위치결정값을 확인 혹은 변경하는 프로퍼티
    public float GetCurrentUnit
    {
        get => currentPos_Unit;
        private set
        {
            if (currentPulse == value)
                return;

            currentPos_Unit = value;
            onChangedPos?.Invoke(value);
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

    public bool isReady = false;                    //명령을 받을 수 있는 상태 여부
    public bool isBusy = false;                     //명령 수행중
    public bool inPosition = false;                 //명령 수행 완료 여부
    public bool isError = false;                    //에러 경고
    public bool opr_Complete = false;               //원점 찾은 상태 여부


    //각 상태에 대한 프로퍼티
    public bool IsReady
    {
        get => isReady;

        set
        {
            if (isReady == value)
                return;

            isReady = value;
            onChangedReady?.Invoke(value);
        }
    }

    public bool IsError
    {
        get => isError;

        set
        {
            if (isError == value)
                return;

            isError = value;
            onChangedError?.Invoke(value);
        }
    }

    public bool IsBusy
    {
        get => isBusy;

        set
        {
            if (isBusy == value)
                return;

            isBusy = value;
            onChangedBusy?.Invoke(value);
        }
    }

    public bool IsJogging
    {
        get => cmd_JogFoward || cmd_JogReverse;
    }

    //받은 지령 명령을 저장하는 내부 변수
    private bool cmd_ServoOn = false;           //서보 모터에 전원을 켜라는 명령 저장
    private bool cmd_StartPos = false;          //지정 위치로 이동하라는 명령 저장
    private bool cmd_StartOPR = false;          //원점 복귀 명령 저장
    private bool cmd_Prev_StartOPR = false;     //이전 원점 복귀 명령 저장
    private bool cmd_JogFoward = false;         //수동 조그 전진 명령 저장
    private bool cmd_JogReverse = false;        //수동 조그 후퇴 명령 저장
    private int cmd_TargetPulse = 0;            //목표 위치 펄스값


    private ConfigurableJoint joint;            //실제 이동을 제어하는 컴포넌트
    private Rigidbody rb;                       //중력과 마찰값을 설정할 수 잇는 컴포넌트
    private float currentVelocity_Unit = 0f;
    private float internalTarget_Unit = 0f;

    [SerializeField] private AmpState currentState = AmpState.Idle;
    private bool homingHitDog = false;
    private int homingDir = -1;
    private float homeOffset_Unit = 0f;


    private void Awake()
    {
        joint = GetComponent<ConfigurableJoint>();
        rb = GetComponent<Rigidbody>();
        rb.automaticCenterOfMass = false;
        rb.automaticInertiaTensor = false;
        rb.linearDamping = 35f;


        //시작 위치를 계산
        float startPos = (ampType == AmpType.Linear) ?
            -transform.localPosition.x * 1000.0f : transform.localRotation.eulerAngles.x;

        currentPulse = PhysToPulse(startPos);
        internalTarget_Unit = PulseToUnit(currentPulse);
        homeOffset_Unit = 0f;
    }

    //모터의 힘(Torque)를 조절하는 함수
    private void SetJointForce(float force)
    {
        if(force > 0.1f)
        {
            rb.linearDamping = 0f;
            rb.useGravity = false;
        }
        else
        {
            rb.linearDamping = 35f;
            rb.useGravity = true;
        }


        JointDrive drive = new JointDrive
        {
            positionSpring = force,
            positionDamper = 100f,
            maximumForce = float.MaxValue
        };


        if (ampType == AmpType.Linear)
        {
            joint.xDrive = drive;
        }
        else
        {
            joint.angularXDrive = drive;
        }
    }

    private void ApplyPhysics(float value)
    {
        //작동 방식이 선형일 경우
        if (ampType == AmpType.Linear)
        {
            float mmValue = value;
            if (controlUnit == ControlUnit.pulse)
            {
                mmValue = (value / motorResolution) * gearRatio * ballscrewLead;
            }

            //조인트에 목표 위치값을 적용.
            joint.targetPosition = new Vector3(mmValue / 1000.0f, 0f, 0f);
        }
        else    //작동 방식이 회전일 경우                
        {
            //회전 각 대입
            float degValue = value;
            if(controlUnit == ControlUnit.pulse)
            {
                degValue = (value / motorResolution) * gearRatio * 360.0f;
            }

            //조인트에 목표 회전값을 적용.
            joint.targetRotation = Quaternion.Euler(degValue, 0f, 0f);
        }
    }

    //원점 복귀 명령 함수
    private void SetupHoming()
    {
        //원점 복귀하기 위해서 세팅되어야 하는 값들을 설정.
        currentState = AmpState.Homing_Search;
        homingDir = defaultHomingDirection;
        opr_Complete = false;
        inPosition = false;
        homingHitDog = false;
        IsBusy = true;
    }

    //원점 복귀 완료 함수
    private void CompletedHoming()
    {
        //현재 속도를 0으로 내리고 위치 고정
        currentVelocity_Unit = 0f;

        //현재 위치를 기억해서 원점이 여기부터 원점이라는 걸 저장
        homeOffset_Unit = internalTarget_Unit;

        //내외부에 원점이 확인됐음을 알림.
        opr_Complete = true;
        cmd_StartOPR = false;
        currentState = AmpState.Idle;
        IsBusy = false;
    }

    //외부 제어 함수
    //서보 모터의 전원을 켜고 끄는 함수
    public void ServoOn(bool isOn)
    {
        if(cmd_ServoOn = isOn)
        {   
            //시작 위치를 계산
            float startPos = (ampType == AmpType.Linear) ?
                -transform.localPosition.x * 1000.0f : transform.localRotation.eulerAngles.x;

            currentPulse = PhysToPulse(startPos);
            internalTarget_Unit = PulseToUnit(currentPulse);
            homeOffset_Unit = internalTarget_Unit;

            ApplyPhysics(internalTarget_Unit);
        }
        else
        {
            IsReady = false;
            IsBusy = false;
            inPosition = false;
            IsError = false;
            opr_Complete = false;
            cmd_StartPos = false;
            cmd_StartOPR = false;
            cmd_Prev_StartOPR = false;
            cmd_JogFoward = false;
            cmd_JogReverse = false;
            cmd_TargetPulse = 0;
        }
    }
    //해당 위치로 이동 함수
    public void Positioning(int pulse)
    {
        if (!IsReady || IsBusy)
            return;

        cmd_TargetPulse = pulse;
        cmd_StartPos = true;
        IsBusy = true;
    }
    //수동 전진 이동 함수
    public void JogForward(bool isOn)
    {
        if (!IsReady || IsBusy)
            return;

        cmd_JogFoward = isOn;
    }
    //수동 후퇴 이동 함수
    public void JogReverse(bool isOn)
    {
        if (!IsReady || IsBusy)
            return;

        cmd_JogReverse = isOn;
    }

    //원점 복귀 함수
    public void Homing()
    {
        if (!IsReady || IsBusy)
            return;

        cmd_StartOPR = true;
    }

    //에러 리셋 함수
    public void Reset()
    {
        if(IsError)
        {
            IsError = false;
            IsBusy = false;
            //다시 대기 모드로 전환.
            currentState = AmpState.Idle;

            cmd_StartOPR = false;
            cmd_JogFoward = false;
            cmd_JogReverse = false;
            cmd_StartPos = false;
            cmd_TargetPulse = 0;
        }
    }

    private void FixedUpdate()
    {
        if (joint == null)
            return;

        if(!cmd_ServoOn && !IsError)
        {
            IsReady = false;
            currentPos_Unit = 0f;

            //모터에 토크를 제거함.
            SetJointForce(0);

            return;
        }

        IsReady = true;
        //모터에 토크를 적용함.
        SetJointForce(100000f);

        bool isRisingEdge_OPR = (cmd_StartOPR && !cmd_Prev_StartOPR);
        cmd_Prev_StartOPR = cmd_StartOPR;

        float targetVelocity = 0f;

        switch (currentState)
        {
            case AmpState.Idle:
                if(cmd_JogFoward || cmd_JogReverse)
                {
                    currentState = AmpState.Jogging;
                }
                else if(cmd_StartPos)
                {
                    currentState = AmpState.Positioning;
                }
                else if(isRisingEdge_OPR)
                {
                    if(opr_Complete)
                    {
                        //이미 원점 찾은 상태고, 원점 위치로 강제 이동
                        Positioning(0);
                    }
                    else
                    {
                        SetupHoming();
                    }
                }
                break;
            case AmpState.Jogging:
                if(cmd_JogFoward)
                {
                    targetVelocity = jogSpeed;
                }
                else if(cmd_JogReverse)
                {
                    targetVelocity = -jogSpeed;
                }
                else
                {
                    currentState = AmpState.Idle;
                }
                break;
            case AmpState.Positioning:
                //펄스 명령을 단위(mm/degree)로 변환
                float targetPos = PulseToUnit(cmd_TargetPulse);
                //남은 거리 계산
                float distance = targetPos - internalTarget_Unit;
                //도착 여부 판점
                if(Mathf.Abs(distance) <= inPosWidth)
                {
                    targetVelocity = 0f;

                    //강제로 위치와 속도를 고정(떨림 현상 방지)
                    currentVelocity_Unit = 0f;
                    internalTarget_Unit = targetPos;
                    ApplyPhysics(internalTarget_Unit);

                    IsBusy = false;
                    inPosition = true;
                    cmd_StartPos = false;
                    currentState = AmpState.Idle;
                }
                else
                {
                    inPosition = false;
                    if(Mathf.Abs(distance) < 2.0f)
                    {
                        targetVelocity = distance > 1f ?
                            Mathf.Sign(distance) * 10f : Mathf.Sign(distance);
                    }
                    else
                    {
                        //제동 거리 예측
                        float stopDist = (Mathf.Abs(currentVelocity_Unit) * accelTime * 0.001f);
                        //제동 거리보다 남은 거리가 짧으면 속도 대폭 감소
                        if(Mathf.Abs(distance) < stopDist)
                        {
                            targetVelocity = distance * 2.0f;
                        }
                        else
                        {
                            targetVelocity = Mathf.Sign(distance) * maxSpeed;
                        }
                    }
                }
                break;
            case AmpState.Homing_Search:
                //원점을 모르는 상태 -> 찾아다녀야 함.
                targetVelocity = homingHighSpeed * homingDir;       //설정된 방향으로 원점복귀 고속상태로 이동

                //근점 도그가 감지되면
                if(isOnProximityDOG)
                {
                    currentState = AmpState.Homing_Creep;           //저속모드로 변환해 원점을 정확하게 찾는다.
                    homingHitDog = true;
                }
                else if((homingDir == -1 && isOnLimitSensorNegative) || (homingDir == 1) && isOnLimitSensorPositive)
                {
                    currentVelocity_Unit = 0f;
                    homingDir = -homingDir;
                    currentState = AmpState.Homing_Retry;
                }
                break;
            case AmpState.Homing_Retry:
                targetVelocity = homingHighSpeed * homingDir;
                if(isOnProximityDOG)
                {
                    currentState = AmpState.Homing_Creep;
                    homingHitDog = true;
                }
                break;
            case AmpState.Homing_Creep:
                targetVelocity = homingCreepSpeed * defaultHomingDirection; 

                if(!isOnProximityDOG && homingHitDog)
                {
                    CompletedHoming();
                    targetVelocity = 0f;
                }
                break;
            case AmpState.Error:
                targetVelocity = 0f;
                break;
        }


        bool isHoming =
            (currentState == AmpState.Homing_Search) ||
            (currentState == AmpState.Homing_Retry) ||
            (currentState == AmpState.Homing_Creep);

        //정방향으로 이동중인데 상한 리미트 센서가 감지되면
        if(isOnLimitSensorPositive && targetVelocity > 0f)
        {
            //정지시키고
            targetVelocity = 0f;
            //원점 복귀중이 아니면
            if(!isHoming)
            {
                IsError = true;
                IsBusy = false;
                currentState = AmpState.Error;
            }
        }
        //역방향으로 이동중인데 하한 리미트 센서에 감지되면
        if (isOnLimitSensorNegative && targetVelocity < 0f)
        {
            //정지시키고
            targetVelocity = 0f;
            //원점 복귀중이 아니면
            if (!isHoming)
            {
                IsError = true;
                IsBusy = false;
                currentState = AmpState.Error;
            }
        }

        //가감속 적용하기
        //공식 : 현재 속도 => 이전 속도 + (가속도 * 시간)
        float referenceSpeed = (currentState == AmpState.Jogging) ? jogSpeed : maxSpeed;
        float accelRate = referenceSpeed / (accelTime * 0.001f); //초당 속도 변화량
        //현재 초당 이동 속도
        currentVelocity_Unit = Mathf.MoveTowards(currentVelocity_Unit, targetVelocity, accelRate * Time.fixedDeltaTime);

        //위치 적분
        internalTarget_Unit += currentVelocity_Unit * Time.fixedDeltaTime;

        //실제 유니티상의 물리적인 위치로 보내기
        ApplyPhysics(internalTarget_Unit);

        GetCurrentUnit = internalTarget_Unit - homeOffset_Unit;

        //현재 물리 위치를 펄스로 변환해서 모니터링해서 확인할 수 있도록 값 적용.
        GetCurrentPulse = PhysToPulse(internalTarget_Unit);
    }



    //펄스 -> 단위(mm/degree) 변환
    public float PulseToUnit(int currentPulse)
    {
        //입력받은 펄스값 / 모터 1회전당 펄스값 => 실제 회전 수
        float revs = (float)currentPulse / motorResolution;
        //서보 모터의 회전수 * 기어 감속비 => 샤프트 회전수
        float shaftRevs = revs * gearRatio;
        float unitValue = 0f;

        if(controlUnit == ControlUnit.mm)
        {
            unitValue = shaftRevs * ballscrewLead;
        }
        else if(controlUnit == ControlUnit.degree)
        {
            unitValue = shaftRevs * 360f;
        }
        else
        {
            unitValue = currentPulse;
        }

        return unitValue + homeOffset_Unit;
    }

    //단위(mm/degree => pulse 변환)
    public int PhysToPulse(float physicalVelocity)
    {
        //현재 이동값 - 원점 보정값 => 원점 기분 위치
        float relativePos = physicalVelocity - homeOffset_Unit;

        //각 제어방식에 맞게 샤프트의 회전수 구하고
        float shaftRevs = (controlUnit == ControlUnit.mm) ? relativePos / ballscrewLead : relativePos / 360f;
        //샤프트 회전수 / 기어비 => 서보모터가 회전해야할 실제 회전수
        float motorRevs = shaftRevs / gearRatio;
        //모터 회전수 * 모터의 분해능 => 실제로 이동한 펄스값
        return (int)(motorRevs * motorResolution);
    }
}
