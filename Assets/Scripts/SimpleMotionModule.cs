using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class SimpleMotionModule : MXObject
{
    public ServoAmp axis1;
    public ServoAmp axis2;
    public ServoAmp axis3;
    public PositioningManager manager;
    public float feedbackTime = 0.3f;

    public DeviceAddress plcReadyAddress = new DeviceAddress("PLC Ready");
    public DeviceAddress moduleReadyAddress = new DeviceAddress("Module Ready");
    public DeviceAddress servoAllOnAddress = new DeviceAddress("Servo All On");

    public DeviceAddress axis1JogForwardAddress = new DeviceAddress("1축 조그 정방향 신호");
    public DeviceAddress axis2JogForwardAddress = new DeviceAddress("2축 조그 정방향 신호");
    public DeviceAddress axis3JogForwardAddress = new DeviceAddress("3축 조그 정방향 신호");

    public DeviceAddress axis1JogReverseAddress = new DeviceAddress("1축 조그 역방향 신호");
    public DeviceAddress axis2JogReverseAddress = new DeviceAddress("2축 조그 역방향 신호");
    public DeviceAddress axis3JogReverseAddress = new DeviceAddress("3축 조그 역방향 신호");

    public DeviceAddress axis1PositioningOnAddress = new DeviceAddress("1축 위치결정 기동 신호");
    public DeviceAddress axis2PositioningOnAddress = new DeviceAddress("2축 위치결정 기동 신호");
    public DeviceAddress axis3PositioningOnAddress = new DeviceAddress("3축 위치결정 기동 신호");

    public DeviceAddress axis1PositionNumAddress = new DeviceAddress("1축 위치결정 번호");
    public DeviceAddress axis2PositionNumAddress = new DeviceAddress("2축 위치결정 번호");
    public DeviceAddress axis3PositionNumAddress = new DeviceAddress("3축 위치결정 번호");

    public DeviceAddress axis1StopAddress = new DeviceAddress("1축 정지 신호");
    public DeviceAddress axis2StopAddress = new DeviceAddress("2축 정지 신호");
    public DeviceAddress axis3StopAddress = new DeviceAddress("3축 정지 신호");

    public DeviceAddress axis1ReceivedAddress = new DeviceAddress("1축 기동완료 신호");
    public DeviceAddress axis2ReceivedAddress = new DeviceAddress("2축 기동완료 신호");
    public DeviceAddress axis3ReceivedAddress = new DeviceAddress("3축 기동완료 신호");

    public DeviceAddress axis1BusyAddress = new DeviceAddress("1축 BUSY 신호");
    public DeviceAddress axis2BusyAddress = new DeviceAddress("2축 BUSY 신호");
    public DeviceAddress axis3BusyAddress = new DeviceAddress("3축 BUSY 신호");

    public DeviceAddress axis1ErrorAddress = new DeviceAddress("1축 ERROR 신호");
    public DeviceAddress axis2ErrorAddress = new DeviceAddress("2축 ERROR 신호");
    public DeviceAddress axis3ErrorAddress = new DeviceAddress("3축 ERROR 신호");

    public DeviceAddress axis1CompletedAddress = new DeviceAddress("1축 위치결정 완료 신호");
    public DeviceAddress axis2CompletedAddress = new DeviceAddress("2축 위치결정 완료 신호");
    public DeviceAddress axis3CompletedAddress = new DeviceAddress("3축 위치결정 완료 신호");

    private bool _haveToExcuteAxis1;
    private bool _haveToExcuteAxis2;
    private bool _haveToExcuteAxis3;

    private int _axis1Positioning;
    private int _axis2Positioning;
    private int _axis3Positioning;

    private bool _receivedAxis1Positioning;
    private bool _receivedAxis2Positioning;
    private bool _receivedAxis3Positioning;

    private float _remainAxis1ReceiveTime;
    private float _remainAxis2ReceiveTime;
    private float _remainAxis3ReceiveTime;

    private bool _completedAxis1Positioning;
    private bool _completedAxis2Positioning;
    private bool _completedAxis3Positioning;

    private float _remainAxis1Complete;
    private float _remainAxis2Complete;
    private float _remainAxis3Complete;

    private bool axis1Busy;
    public bool Axis1Busy
    {
        get => axis1Busy;
        set
        {
            if (axis1Busy == value)
                return;

            axis1Busy = value;
            if (axis1BusyAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis1BusyAddress.address, (short)(value ? 1 : 0));
        }
    }

    private bool axis2Busy;
    public bool Axis2Busy
    {
        get => axis2Busy;
        set
        {
            if (axis2Busy == value)
                return;

            axis2Busy = value;
            if (axis2BusyAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis2BusyAddress.address, (short)(value ? 1 : 0));

        }
    }
    private bool axis3Busy;
    public bool Axis3Busy
    {
        get => axis3Busy;
        set
        {
            if (axis3Busy == value)
                return;

            axis3Busy = value;
            if (axis3BusyAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis3BusyAddress.address, (short)(value ? 1 : 0));

        }
    }

    private bool axis1Error;
    public bool Axis1Error
    {
        get => axis1Error;
        set
        {
            if (axis1Error == value) 
                return;

            axis1Error = value;
            if (axis1ErrorAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis1ErrorAddress.address, (short)(value ? 1 : 0));
        }
    }
    private bool axis2Error;
    public bool Axis2Error
    {
        get => axis2Error;
        set
        {
            if (axis2Error == value)
                return;

            axis2Error = value;
            if (axis2ErrorAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis2ErrorAddress.address, (short)(value ? 1 : 0));
        }
    }
    private bool axis3Error;
    public bool Axis3Error
    {
        get => axis3Error;
        set
        {
            if (axis3Error == value)
                return;

            axis3Error = value;
            if (axis3ErrorAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis3ErrorAddress.address, (short)(value ? 1 : 0));
        }
    }

    private void Start()
    {
        if (plcReadyAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(plcReadyAddress.address, PLCReady);
        if (servoAllOnAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(servoAllOnAddress.address, ServoAllOn);

        if (axis1JogForwardAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis1JogForwardAddress.address, Axis1JogForward);
        if (axis2JogForwardAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis2JogForwardAddress.address, Axis2JogForward);
        if (axis3JogForwardAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis3JogForwardAddress.address, Axis3JogForward);

        if (axis1JogReverseAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis1JogReverseAddress.address, Axis1JogReverse);
        if (axis2JogReverseAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis2JogReverseAddress.address, Axis2JogReverse);
        if (axis3JogReverseAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis3JogReverseAddress.address, Axis3JogReverse);

        if (axis1PositioningOnAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis1PositioningOnAddress.address, StartAxis1Positioning);
        if (axis2PositioningOnAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis2PositioningOnAddress.address, StartAxis2Positioning);
        if (axis3PositioningOnAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis3PositioningOnAddress.address, StartAxis3Positioning);
    }

    public void PLCReady(short data)
    {
        if (data == 1)
        {
            Debug.Log($"[{plcReadyAddress.address}] PLC Ready!!");
            if (moduleReadyAddress.useDevice)
            {
                MXRequester.Get.AddSetDeviceRequest(moduleReadyAddress.address, 1);
                Debug.Log($"[{moduleReadyAddress.address}] 심플모션 모듈 Ready!!");
            }
        }
        else
        {
            Debug.Log($"[{plcReadyAddress.address}] PLC not ready!!");
        }
    }

    public void ServoAllOn(short data)
    {
        Debug.Log($"[{servoAllOnAddress.address}] Servo On!! {(data == 1 ? "ON" : "OFF")}");
        axis1.ServoOn(data != 0);
        axis2.ServoOn(data != 0);
        axis3.ServoOn(data != 0);
    }

    public void Axis1JogForward(short data)
    {
        axis1.JogForward(data != 0);
    }
    public void Axis2JogForward(short data)
    {
        axis2.JogForward(data != 0);
    }
    public void Axis3JogForward(short data)
    {
        axis3.JogForward(data != 0);
    }

    public void Axis1JogReverse(short data)
    {
        axis1.JogForward(data != 0);
    }
    public void Axis2JogReverse(short data)
    {
        axis2.JogForward(data != 0);
    }
    public void Axis3JogReverse(short data)
    {
        axis3.JogForward(data != 0);
    }

    public void StartAxis1Positioning(short data)
    {
        if (data == 1)
        {
            _haveToExcuteAxis1 = true;
        }
    }

    public void StartAxis2Positioning(short data)
    {
        if (data == 1)
        {
            _haveToExcuteAxis2 = true;
        }
    }

    public void StartAxis3Positioning(short data)
    {
        if (data == 1)
        {
            _haveToExcuteAxis2 = true;
        }
    }

    public void SetAxis1Positioning(short data)
    {
        _axis1Positioning = data;
    }

    public void SetAxis2Positioning(short data)
    {
        _axis2Positioning = data;
    }

    public void SetAxis3Positioning(short data)
    {
        _axis3Positioning = data;
    }

    public void StopAxis1(short data)
    {
        axis1.IsStopped = data != 0;
    }
    public void StopAxis2(short data)
    {
        axis2.IsStopped = data != 0;
    }
    public void StopAxis3(short data)
    {
        axis3.IsStopped = data != 0;
    }

    public void OnCompletedAxis1Positioning()
    {
        _completedAxis1Positioning = true;
        _remainAxis1Complete = Time.time + feedbackTime;
        if (axis1CompletedAddress.useDevice)
            MXRequester.Get.AddSetDeviceRequest(axis1CompletedAddress.address, 1);
    }
    public void OnCompletedAxis2Positioning()
    {
        _completedAxis2Positioning = true;
        _remainAxis2Complete = Time.time + feedbackTime;
        if (axis2CompletedAddress.useDevice)
            MXRequester.Get.AddSetDeviceRequest(axis2CompletedAddress.address, 1);
    }
    public void OnCompletedAxis3Positioning()
    {
        _completedAxis3Positioning = true;
        _remainAxis3Complete = Time.time + feedbackTime;
        if (axis3CompletedAddress.useDevice)
            MXRequester.Get.AddSetDeviceRequest(axis3CompletedAddress.address, 1);
    }

    private void Update()
    {
        if(_haveToExcuteAxis1 && _axis1Positioning != 0)
        {
            if(!axis1.opr_Complete && _axis1Positioning == 9001)
            {
                axis1.Homing();
            }
            else if(axis1.opr_Complete && _axis1Positioning == 9002)
            {
                axis1.Homing();
            }
            else
            {
                axis1.Positioning(manager.positionList[_axis1Positioning].axis1);
            }

            _haveToExcuteAxis1 = false;
            _receivedAxis1Positioning = true;
            _remainAxis1ReceiveTime = Time.time + feedbackTime;
            if (axis1ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis1ReceivedAddress.address, 1);
        }
        if (_haveToExcuteAxis2 && _axis2Positioning != 0)
        {
            if (!axis2.opr_Complete && _axis2Positioning == 9001)
            {
                axis2.Homing();
            }
            else if (axis2.opr_Complete && _axis2Positioning == 9002)
            {
                axis2.Homing();
            }
            else
            {
                axis2.Positioning(manager.positionList[_axis2Positioning].axis2);
            }

            _haveToExcuteAxis2 = false;
            _receivedAxis2Positioning = true;
            _remainAxis2ReceiveTime = Time.time + feedbackTime;
            if (axis2ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis2ReceivedAddress.address, 1);
        }
        if (_haveToExcuteAxis3 && _axis3Positioning != 0)
        {
            if (!axis3.opr_Complete && _axis3Positioning == 9001)
            {
                axis3.Homing();
            }
            else if (axis3.opr_Complete && _axis3Positioning == 9002)
            {
                axis3.Homing();
            }
            else
            {
                axis3.Positioning(manager.positionList[_axis3Positioning].axis3);
            }

            _haveToExcuteAxis3 = false;
            _receivedAxis3Positioning = true;
            _remainAxis3ReceiveTime = Time.time + feedbackTime;
            if (axis3ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis3ReceivedAddress.address, 1);
        }

        if (_receivedAxis1Positioning && _remainAxis1ReceiveTime < Time.time)
        {
            _receivedAxis1Positioning = false;
            if(axis1ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis1ReceivedAddress.address, 0);
        }
        if (_receivedAxis2Positioning && _remainAxis2ReceiveTime < Time.time)
        {
            _receivedAxis2Positioning = false;
            if (axis2ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis2ReceivedAddress.address, 0);
        }
        if (_receivedAxis3Positioning && _remainAxis3ReceiveTime < Time.time)
        {
            _receivedAxis3Positioning = false;
            if (axis3ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis3ReceivedAddress.address, 0);
        }

        if (_completedAxis1Positioning && _remainAxis1Complete < Time.time)
        {
            if(axis1CompletedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis1CompletedAddress.address, 0);

            _completedAxis1Positioning = false;
        }
        if (_completedAxis2Positioning && _remainAxis2Complete < Time.time)
        {
            if (axis2CompletedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis2CompletedAddress.address, 0);

            _completedAxis2Positioning = false;
        }
        if (_completedAxis3Positioning && _remainAxis3Complete < Time.time)
        {
            if (axis3CompletedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis3CompletedAddress.address, 0);

            _completedAxis3Positioning = false;
        }
    }
}
