using UnityEngine.Events;
using UnityEngine;

public class InverterConnector : MXObject
{
    public Rigidbody shaft;
    public ConfigurableJoint joint;

    [Header("필수 출력 디바이스")]
    public DeviceAddress STF_Address = new DeviceAddress("정회전 신호");
    public DeviceAddress STR_Address = new DeviceAddress("역회전 신호");
    public DeviceAddress MRS_Address = new DeviceAddress("비상정지 신호");
    public DeviceAddress RST_Address = new DeviceAddress("리셋 신호");

    [Header("필수 입력 디바이스")]
    public DeviceAddress RUN_Address = new DeviceAddress("운행중 피드백 신호");
    public DeviceAddress SU_Address = new DeviceAddress("목표Hz 도달 신호");
    public DeviceAddress ALERT_Address = new DeviceAddress("고장 신호");

    [Header("모터 기본 성능")]
    [Delayed] public int poleCount = 4;         //극수
    [Delayed] public float maxFrequency = 60f;  //최대 주파수
    [Delayed] public float maxRPM = 1800f;      //정격 회전수
    [Delayed] public float accelTime = 1.0f;    //가속시간 (0 -> Max까지 도달하는데 걸리는 시간)
    [Delayed] public float decelTime = 1.0f;    //감속시간 (Max -> 0까지 도달하는데 걸리는 시간)

    [Header("다단 속도 설정")]
    public bool useStep = false;                //다단 속도제어. true로 변경하면 인버터 직접 제어가 우선 순위에서 밀림.

    public DeviceAddress RL_Address = new DeviceAddress("저단 속도 명령 신호"); //저단
    public DeviceAddress RM_Address = new DeviceAddress("중단 속도 명령 신호"); //중단
    public DeviceAddress RH_Address = new DeviceAddress("고단 속도 명령 신호"); //고단


    public float[] stepFrequencies = new float[8]
    {
        0f,
        10f,
        30f,
        0f,
        60f,
        0f,
        0f,
        0f
    };

    [Header("아날로그 입력 설정")]
    public bool useAnalogInput = false;     //아날로그 신호로 제어 기능 ON/OFF
    public DeviceAddress analogAddress = new DeviceAddress("아날로그 명령 신호");
    public int analogMaxResolution = 4000; //분해능(미쯔비시 : 4000, LS : 16000)

    [Header("모니터링")]
    [Delayed, Range(0f, 240f), SerializeField]
    private float targetHz = 0f;            //지령 주파수
    private int analogInputValue = 0;       //아날로그 지령 값
    [SerializeField] private float currentHz = 0f;      //현재 주파수
    [SerializeField] private float currentRPM = 0f;     //현재 RPM


    //PLC에 피드백 데이터
    private bool isRun = false;                         
    private bool emergencyStop = false;
    private bool isAlert = false;
    private bool isOnForward = false;
    private bool isOnReverse = false;
    private bool isOnLow = false;
    private bool isOnMiddle = false;
    private bool isOnHigh = false;
    private bool reachTargetHz = false;

    //상태 변화에 대한 델리게이트
    public UnityEvent<bool> onChangedRun;               //운전상태 변화
    public UnityEvent<bool> onChangedEMS;               //긴급정지 상태 변화
    public UnityEvent<bool> onChangedAlert;             //고장 상태 변화
    public UnityEvent<bool> onChangedForward;           //정회전 상태 변화
    public UnityEvent<bool> onChangedReverse;           //역회전 상태 변화
    public UnityEvent<bool> onChangedRL;                //저단 신호 변화
    public UnityEvent<bool> onChangedRM;                //중단 신호 변화
    public UnityEvent<bool> onChangedRH;                //고단 신호 변화
    public UnityEvent<bool> onReachTargetHz;            //목표 주파수에 도달 여부 
    public UnityEvent<int> onChangedAnalog;            //아날로그 신호 변화
    public UnityEvent<float> onChangedTargetHz;          //목표 주파수 변화
    public UnityEvent<float> onChangedCurrentHz;         //현재 주파수 변화
    public UnityEvent<float> onChangedCurrentRPM;        //현재 RPM 변화

    public float GetCurrentHz => currentHz;             //현재 주파수값 가져오기
    public float GetCurrentRPM => currentRPM;           //현재 RPM값 가져오기
        
    //모터의 운전 상태 변화에 따른 프로퍼티
    public bool IsRun
    {
        get => isRun;

        private set
        {
            if (isRun == value)
                return;

            isRun = value;
            onChangedRun?.Invoke(value);

            if (!RUN_Address.useDevice)
                return;

            if(string.IsNullOrEmpty(RUN_Address.address))
            {
                Debug.LogWarning("입력 디바이스 주소가 비어있어 신호를 보낼 수 없습니다. 주소를 채워주세요");
                return;
            }

            //PLC에 변경된 상태 신호를 보내는 함수
            MXRequester.Get.AddSetDeviceRequest(RUN_Address.address, (short)(value ? 1 : 0));
        }
    }

    //긴급 정지 상태 변화에 프로퍼티
    public bool EStop
    {
        get => emergencyStop;

        private set
        {
            if(emergencyStop == value)
                return;

            emergencyStop = value;
            onChangedEMS?.Invoke(value);
        }
    }
    
    //모터 고장 상태에 따른 프로퍼티
    public bool IsAlert
    {
        get => isAlert;
        private set
        {
            if(isAlert == value)
                return;

            isAlert = value;
            onChangedAlert?.Invoke(value);

            if (!ALERT_Address.useDevice)
                return;

            if (string.IsNullOrEmpty(ALERT_Address.address))
            {
                Debug.LogWarning("입력 디바이스 주소가 비어있어 신호를 보낼 수 없습니다. 주소를 채워주세요");
                return;
            }

            //PLC에 변경된 상태 신호를 보내는 함수
            MXRequester.Get.AddSetDeviceRequest(ALERT_Address.address, (short)(value ? 1 : 0));
        }
    }

    //목표 속도 도달 여부에 따른 프로퍼티
    public bool ReachTargetHz
    {
        get => reachTargetHz;
        private set
        {
            if (reachTargetHz == value)
                return;

            reachTargetHz = value;
            onReachTargetHz?.Invoke(value);

            if (!SU_Address.useDevice)
                return;

            if (string.IsNullOrEmpty(SU_Address.address))
            {
                Debug.LogWarning("입력 디바이스 주소가 비어있어 신호를 보낼 수 없습니다. 주소를 채워주세요");
                return;
            }

            //PLC에 변경된 상태 신호를 보내는 함수
            MXRequester.Get.AddSetDeviceRequest(SU_Address.address, (short)(value ? 1 : 0));
        }
    }

    //정방향 회전 변화에 따른 프로퍼티
    public bool STF
    {
        get => isOnForward;
        set
        {
            if(isOnForward == value) 
                return;

            //정회전 상태 갱신후에 봤더니 정회전 중이면
            if(isOnForward = value)
            {
                //역방향 상태를 Off시킴.
                onChangedReverse?.Invoke(isOnReverse = false);
            }

            onChangedForward?.Invoke(value);
            onChangedTargetHz?.Invoke(CalculateTargetHz());
        }
    }

    //역방향 회전 변화에 따른 프로퍼티
    public bool STR
    {
        get => isOnReverse;
        set
        {
            if (isOnReverse == value)
                return;

            if(isOnReverse = value)
            {
                onChangedForward?.Invoke(isOnForward = false);
            }

            onChangedReverse?.Invoke(value);
            onChangedTargetHz?.Invoke(CalculateTargetHz());
        }
    }

    //저속 회전 상태 변화에 따른 프로퍼티
    public bool RL
    {
        get => isOnLow;

        set
        {
            if (!useStep)
                return;

            if (isOnLow == value)
                return;

            isOnLow = value;
            onChangedRL?.Invoke(value);
            onChangedTargetHz?.Invoke(CalculateTargetHz());
        }
    }

    public bool RM
    {
        get => isOnMiddle;

        set
        {
            if (!useStep)
                return;

            if (isOnMiddle == value)
                return;

            isOnMiddle = value;
            onChangedRM?.Invoke(value);
            onChangedTargetHz?.Invoke(CalculateTargetHz());
        }
    }

    public bool RH
    {
        get => isOnHigh;

        set
        {
            if (!useStep)
                return;

            if (isOnHigh == value)
                return;

            isOnHigh = value;
            onChangedRH?.Invoke(value);
            onChangedTargetHz?.Invoke(CalculateTargetHz());
        }
    }

    //아날로그 입력값의 변화에 따른 프로퍼티
    public int AnalogInput
    {
        get => analogInputValue;

        set
        {
            if (!useAnalogInput)
                return;

            if (analogInputValue == value)
                return;

            analogInputValue = value;
            if(analogInputValue > analogMaxResolution)
            {
                IsAlert = true;
                return;
            }

            if(analogInputValue < 0)
            {
                IsAlert = true;
                return;
            }

            onChangedAnalog?.Invoke(value);
            //아날로그 값을 주파수 형태로 변환한 값
            targetHz = (float)value / analogMaxResolution * maxFrequency;
            onChangedTargetHz?.Invoke(targetHz);
        }
    }


    //현재 주파수에 대한 프로퍼티
    public float CurrentHz
    {
        get => Mathf.Abs(currentHz);

        private set
        {
            currentHz = value;
            onChangedCurrentHz?.Invoke(Mathf.Abs(value));
        }
    }

    //현재 RPM에 대한 프로퍼티
    public float CurrentRPM
    {
        get => Mathf.Abs(currentRPM);

        private set
        {
            currentRPM = value;
            onChangedCurrentRPM?.Invoke(Mathf.Abs(value));
        }
    }

    //PLC로부터 받는 콜백함수
    public void ReceiveSTFSignal(short readValue)
    {
        STF = readValue != 0;
    }

    public void ReceiveSTRSignal(short readValue)
    {
        STR = readValue != 0;
    }

    public void ReceiveRHSignal(short readValue)
    {
        RH = readValue != 0;
    }
    public void ReceiveRMSignal(short readValue)
    {
        RM = readValue != 0;
    }
    public void ReceiveRLSignal(short readValue)
    {
        RL = readValue != 0;
    }

    public void ReceiveEMSSignal(short readValue)
    {
        EStop = readValue != 0;
    }
        
    public void ReceiveResetSignal(short readValue)
    {
        if(readValue != 0)
        {
            IsAlert = false;
        }
    }

    public void ReceiveAnalogSignal(short readValue)
    {
        AnalogInput = readValue;
    }

    private void Awake()
    {
        //회전시킬 샤프트가 비어있으면
        if (shaft == null)
            shaft = GetComponent<Rigidbody>();

        if(shaft != null)
        {
            shaft.automaticCenterOfMass = false;
            shaft.automaticCenterOfMass = false;
            shaft.useGravity = false;
        }

        if(joint == null)
        {
            joint = GetComponent<ConfigurableJoint>();
        }

        if(joint != null)
        {
            //x,y,z축의 이동 금지
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            //x, y축 회전 금지
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            //z축만 회전
            joint.angularZMotion = ConfigurableJointMotion.Free;
        }
    }

    private void Start()
    {
        if (STF_Address.useDevice && !string.IsNullOrEmpty(STF_Address.address))
            MXRequester.Get.AddDeviceAddress(STF_Address.address, ReceiveSTFSignal);
        if (STR_Address.useDevice && !string.IsNullOrEmpty(STR_Address.address))
            MXRequester.Get.AddDeviceAddress(STR_Address.address, ReceiveSTRSignal);
        if (MRS_Address.useDevice && !string.IsNullOrEmpty(MRS_Address.address))
            MXRequester.Get.AddDeviceAddress(MRS_Address.address, ReceiveEMSSignal);
        if (RST_Address.useDevice && !string.IsNullOrEmpty(RST_Address.address))
            MXRequester.Get.AddDeviceAddress(RST_Address.address, ReceiveResetSignal);

        if (useStep)
        {
            if (RH_Address.useDevice && !string.IsNullOrEmpty(RH_Address.address))
                MXRequester.Get.AddDeviceAddress(RH_Address.address, ReceiveRHSignal);
            if (RM_Address.useDevice && !string.IsNullOrEmpty(RM_Address.address))
                MXRequester.Get.AddDeviceAddress(RM_Address.address, ReceiveRMSignal);
            if (RL_Address.useDevice && !string.IsNullOrEmpty(RL_Address.address))
                MXRequester.Get.AddDeviceAddress(RL_Address.address, ReceiveRLSignal);
        }

        if(useAnalogInput)
        {
            if (analogAddress.useDevice && !string.IsNullOrEmpty(analogAddress.address))
                MXRequester.Get.AddDeviceAddress(analogAddress.address, ReceiveAnalogSignal);
        }
    }

    private float GetFinalTargetHz()
    {
        if(EStop || IsAlert)
        {
            IsRun = false;
            return 0f;
        }

        if(!STF && !STR)
        {
            IsRun = false;
            return 0f;
        }

        IsRun = true;
        float direction = STF ? 1f : -1f;

        //다단 속도 확인
        int hzStep = 0;
        if (RL) hzStep += 1;
        if (RM) hzStep += 2;
        if(RH) hzStep += 4;

        if(hzStep > 0)
        {
            return direction * stepFrequencies[hzStep];
        }

        return direction * targetHz;
    }

    private float CalculateCurrentSpeed(float targetHz)
    {
        float rampRate = maxFrequency / (Mathf.Abs(targetHz) > 0.0001f ? accelTime : decelTime);
        CurrentHz = Mathf.MoveTowards(currentHz, targetHz, rampRate * Time.fixedDeltaTime);
        if(Mathf.Abs(targetHz - currentHz) < 0.0001f)
        {
            if (IsRun)
                ReachTargetHz = true;
            else
                ReachTargetHz = false;
        }
        else
            ReachTargetHz = false;

        CurrentRPM = (currentHz / maxFrequency) * maxRPM;

        return currentRPM * 0.10472f;
    }

    private float CalculateTargetHz()
    {
        if (!STF && !STR)
            return 0f;

        //다단 속도 확인
        int hzStep = 0;
        if (RL) hzStep += 1;
        if (RM) hzStep += 2;
        if (RH) hzStep += 4;

        if (hzStep > 0)
        {
            return stepFrequencies[hzStep];
        }

        return targetHz;
    }


    private void FixedUpdate()
    {
        shaft.angularVelocity = -transform.forward * CalculateCurrentSpeed(GetFinalTargetHz());
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        maxRPM = 120f * maxFrequency / poleCount;
    }

#endif
}

