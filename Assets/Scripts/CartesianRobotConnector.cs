using System;
using UnityEngine;

public class CartesianRobotConnector : MXObject
{
    public ServoAmp axis1;
    public ServoAmp axis2;
    public ServoAmp axis3;
    public PositioningManager manager;
    public float feedbackTime = 0.3f;

    public DeviceAddress plcReadyAddress = new DeviceAddress("PLC Ready");
    public DeviceAddress moduleReadyAddress = new DeviceAddress("Module Ready");
    public DeviceAddress servoAllOnAddress = new DeviceAddress("Servo All On");

    public DeviceAddress axis1JogFWDAddress = new DeviceAddress("1축 조그 정방향 신호");
    public DeviceAddress axis2JogFWDAddress = new DeviceAddress("2축 조그 정방향 신호");
    public DeviceAddress axis3JogFWDAddress = new DeviceAddress("3축 조그 정방향 신호");

    public DeviceAddress axis1JogRVSAddress = new DeviceAddress("1축 조그 역방향 신호");
    public DeviceAddress axis2JogRVSAddress = new DeviceAddress("2축 조그 역방향 신호");
    public DeviceAddress axis3JogRVSAddress = new DeviceAddress("3축 조그 역방향 신호");
    
    public DeviceAddress axis1POSAddress = new DeviceAddress("1축 위치결정 기동 신호");
    public DeviceAddress axis2POSAddress = new DeviceAddress("2축 위치결정 기동 신호");
    public DeviceAddress axis3POSAddress = new DeviceAddress("3축 위치결정 기동 신호");

    public DeviceAddress axis1PosNUMAddress = new DeviceAddress("1축 위치결정 번호");
    public DeviceAddress axis2PosNUMAddress = new DeviceAddress("2축 위치결정 번호");
    public DeviceAddress axis3PosNUMAddress = new DeviceAddress("3축 위치결정 번호");

    public DeviceAddress axis1StopAddress = new DeviceAddress("1축 정지 신호");
    public DeviceAddress axis2StopAddress = new DeviceAddress("2축 정지 신호");
    public DeviceAddress axis3StopAddress = new DeviceAddress("3축 정지 신호");

    public DeviceAddress axis1RSTAddress = new DeviceAddress("1축 리셋 신호");
    public DeviceAddress axis2RSTAddress = new DeviceAddress("2축 리셋 신호");
    public DeviceAddress axis3RSTAddress = new DeviceAddress("3축 리셋 신호");
    
    public DeviceAddress axis1ReceivedAddress = new DeviceAddress("1축 기동완료 신호");
    public DeviceAddress axis2ReceivedAddress = new DeviceAddress("2축 기동완료 신호");
    public DeviceAddress axis3ReceivedAddress = new DeviceAddress("3축 기동완료 신호");

    public DeviceAddress axis1BusyAddress = new DeviceAddress("1축 BUSY 신호");
    public DeviceAddress axis2BusyAddress = new DeviceAddress("2축 BUSY 신호");
    public DeviceAddress axis3BusyAddress = new DeviceAddress("3축 BUSY 신호");

    public DeviceAddress axis1ErrorAddress = new DeviceAddress("1축 ERROR 신호");
    public DeviceAddress axis2ErrorAddress = new DeviceAddress("1축 ERROR 신호");
    public DeviceAddress axis3ErrorAddress = new DeviceAddress("1축 ERROR 신호");

    public DeviceAddress axis1CompletedAddress = new DeviceAddress("1축 위치결정 완료 신호");
    public DeviceAddress axis2CompletedAddress = new DeviceAddress("2축 위치결정 완료 신호");
    public DeviceAddress axis3CompletedAddress = new DeviceAddress("3축 위치결정 완료 신호");


    private bool haveToExcuteAxis1;
    private int axis1Positioning;
    private bool receivedAxis1Positioning;
    private bool completedAxis1Positioning;
    private float remainAxis1ReceivedTime;
    private float remainAxis1Completed;

    private bool haveToExcuteAxis2;
    private int axis2Positioning;
    private bool receivedAxis2Positioning;
    private bool completedAxis2Positioning;
    private float remainAxis2ReceivedTime;
    private float remainAxis2Completed;

    private bool haveToExcuteAxis3;
    private int axis3Positioning;
    private bool receivedAxis3Positioning;
    private bool completedAxis3Positioning;
    private float remainAxis3ReceivedTime;
    private float remainAxis3Completed;

    private bool axis1Busy;
    public bool Axis1BUSY
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
    public bool Axis2BUSY
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
    public bool Axis3BUSY
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
    public bool Axis1ERROR
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
    public bool Axis2ERROR
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
    public bool Axis3ERROR
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

    public void OnCompletedAxis1Positioning()
    {
        completedAxis1Positioning = true;
        remainAxis1Completed = Time.time + feedbackTime;
        if (axis1CompletedAddress.useDevice)
            MXRequester.Get.AddSetDeviceRequest(axis1CompletedAddress.address, 1);
    }

    public void OnCompletedAxis2Positioning()
    {
        completedAxis2Positioning = true;
        remainAxis2Completed = Time.time + feedbackTime;
        if (axis2CompletedAddress.useDevice)
            MXRequester.Get.AddSetDeviceRequest(axis2CompletedAddress.address, 1);
    }

    public void OnCompletedAxis3Positioning()
    {
        completedAxis3Positioning = true;
        remainAxis3Completed = Time.time + feedbackTime;
        if (axis3CompletedAddress.useDevice)
            MXRequester.Get.AddSetDeviceRequest(axis3CompletedAddress.address, 1);
    }

    void Start()
    {
        if (plcReadyAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(plcReadyAddress.address, PLCReady);
        if (servoAllOnAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(servoAllOnAddress.address, ServoAllOn);

        if (axis1JogFWDAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis1JogFWDAddress.address, Axis1JogFWD);
        if (axis2JogFWDAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis2JogFWDAddress.address, Axis2JogFWD);
        if (axis3JogFWDAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis3JogFWDAddress.address, Axis3JogFWD);

        if (axis1JogRVSAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis1JogRVSAddress.address, Axis1JogRVS);
        if (axis2JogRVSAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis2JogRVSAddress.address, Axis2JogRVS);
        if (axis3JogRVSAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis3JogRVSAddress.address, Axis3JogRVS);

        if (axis1POSAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis1POSAddress.address, StartAxis1Positioning);
        if (axis2POSAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis2POSAddress.address, StartAxis2Positioning);
        if (axis3POSAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis3POSAddress.address, StartAxis3Positioning);

        if (axis1PosNUMAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis1PosNUMAddress.address, SetAxis1Positioning);
        if (axis2PosNUMAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis2PosNUMAddress.address, SetAxis2Positioning);
        if (axis3PosNUMAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis3PosNUMAddress.address, SetAxis3Positioning);

        if (axis1StopAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis1StopAddress.address, StopAxis1);
        if (axis2StopAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis2StopAddress.address, StopAxis2);
        if (axis3StopAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis3StopAddress.address, StopAxis3);

        if (axis1RSTAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis1RSTAddress.address, ResetAxis1);
        if (axis2RSTAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis2RSTAddress.address, ResetAxis2);
        if (axis3RSTAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(axis3RSTAddress.address, ResetAxis3);

    }

    private void ResetAxis3(short data)
    {
        if (data != 0)
            axis3.Reset();
    }
    private void ResetAxis2(short data)
    {
        if (data != 0)
            axis2.Reset();
    }
    private void ResetAxis1(short data)
    {
        if(data != 0)
            axis1.Reset();
    }
    private void StopAxis3(short data)
    {
        axis3.IsStopped = data != 0;
    }
    private void StopAxis2(short data)
    {
        axis2.IsStopped = data != 0;
    }
    private void StopAxis1(short data)
    {
        axis1.IsStopped = data != 0;
    }
    private void SetAxis3Positioning(short data)
    {
        axis3Positioning = data;
    }
    private void SetAxis2Positioning(short data)
    {
        axis2Positioning = data;
    }
    private void SetAxis1Positioning(short data)
    {
        axis1Positioning = data;
    }
    private void StartAxis3Positioning(short data)
    {
        if (data != 0)
        {
            haveToExcuteAxis3 = true;
        }
    }
    private void StartAxis2Positioning(short data)
    {
        if (data != 0)
        {
            haveToExcuteAxis2 = true;
        }
    }
    private void StartAxis1Positioning(short data)
    {
        if(data != 0)
        {
            haveToExcuteAxis1 = true;
        }
    }
    private void Axis3JogRVS(short data)
    {
        axis3.JogReverse(data != 0);
    }
    private void Axis2JogRVS(short data)
    {
        axis2.JogReverse(data != 0);
    }
    private void Axis1JogRVS(short data)
    {
        axis1.JogReverse(data != 0);
    }
    private void Axis3JogFWD(short data)
    {
        axis3.JogForward(data != 0);
    }
    private void Axis2JogFWD(short data)
    {
        axis2.JogForward(data != 0);
    }
    private void Axis1JogFWD(short data)
    {
        axis1.JogForward(data != 0);
    }
    private void ServoAllOn(short data)
    {
        axis1.ServoOn(data != 0);
        axis2.ServoOn(data != 0);
        axis3.ServoOn(data != 0);
    }
    private void PLCReady(short data)
    {
        if (moduleReadyAddress.useDevice)
        {
            MXRequester.Get.AddSetDeviceRequest(moduleReadyAddress.address, data);
            Debug.Log($"[{moduleReadyAddress.address}] Module {(data != 0 ? "" : "Not")} Ready!!!");
        }

        if(data != 0)
        {
            Debug.Log($"[{plcReadyAddress.address}] PLC Ready!!!");
        }
        else
        {
            Debug.Log($"[{plcReadyAddress.address}] PLC Not Ready!!!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(haveToExcuteAxis1 && axis1Positioning != 0)
        {
            //원점복귀 신호
            if(!axis1.opr_Complete && axis1Positioning == 9001)
            {
                axis1.Homing();
            }
            //고속 원점복귀 신호
            else if(axis1.opr_Complete && axis1Positioning == 9002)
            {
                axis1.Homing();
            }
            else
            {
                axis1.Positioning(manager.positionList[axis1Positioning - 1].axis1);
            }

            haveToExcuteAxis1 = false;
            receivedAxis1Positioning = true;
            remainAxis1ReceivedTime = Time.time + feedbackTime;
            if (axis1ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis1ReceivedAddress.address, 1);
        }

        if(receivedAxis1Positioning && remainAxis1ReceivedTime < Time.time)
        {
            receivedAxis1Positioning = false;
            if (axis1ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis1ReceivedAddress.address, 0);
        }

        if (haveToExcuteAxis2 && axis2Positioning != 0)
        {
            //원점복귀 신호
            if (!axis2.opr_Complete && axis2Positioning == 9001)
            {
                axis2.Homing();
            }
            //고속 원점복귀 신호
            else if (axis2.opr_Complete && axis2Positioning == 9002)
            {
                axis2.Homing();
            }
            else
            {
                axis2.Positioning(manager.positionList[axis2Positioning -1].axis2);
            }

            haveToExcuteAxis2 = false;
            receivedAxis2Positioning = true;
            remainAxis2ReceivedTime = Time.time + feedbackTime;
            if (axis2ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis2ReceivedAddress.address, 1);
        }

        if (receivedAxis2Positioning && remainAxis2ReceivedTime < Time.time)
        {
            receivedAxis2Positioning = false;
            if (axis2ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis2ReceivedAddress.address, 0);
        }

        if (haveToExcuteAxis3 && axis3Positioning != 0)
        {
            //원점복귀 신호
            if (!axis3.opr_Complete && axis3Positioning == 9001)
            {
                axis3.Homing();
            }
            //고속 원점복귀 신호
            else if (axis3.opr_Complete && axis3Positioning == 9002)
            {
                axis3.Homing();
            }
            else
            {
                axis3.Positioning(manager.positionList[axis3Positioning -1].axis3);
            }

            haveToExcuteAxis3 = false;
            receivedAxis3Positioning = true;
            remainAxis3ReceivedTime = Time.time + feedbackTime;
            if (axis3ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis3ReceivedAddress.address, 1);
        }

        if (receivedAxis3Positioning && remainAxis3ReceivedTime < Time.time)
        {
            receivedAxis3Positioning = false;
            if (axis3ReceivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis3ReceivedAddress.address, 0);
        }


        if(completedAxis1Positioning && remainAxis1Completed < Time.time)
        {
            if (axis1CompletedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis1CompletedAddress.address, 0);

            completedAxis1Positioning = false;
        }

        if (completedAxis2Positioning && remainAxis2Completed < Time.time)
        {
            if (axis2CompletedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis2CompletedAddress.address, 0);

            completedAxis2Positioning = false;
        }

        if (completedAxis3Positioning && remainAxis3Completed < Time.time)
        {
            if (axis3CompletedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(axis3CompletedAddress.address, 0);

            completedAxis3Positioning = false;
        }
    }
}
