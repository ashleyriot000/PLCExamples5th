using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Hierarchy;

public enum MCodeType
{
    WithMode,
    AfterMode
}


public class ServoConveyorConnector : MXObject
{
    public ServoConveyorController controller;
    public ServoConveyorConnector mainConnector;

    public DeviceAddress plcReadyAddress = new DeviceAddress("PLC Ready");
    public DeviceAddress servoAllOnAddress = new DeviceAddress("Servo All On");
    public DeviceAddress moduleReadyAddress = new DeviceAddress("Module Ready");

    public DeviceAddress positioningOnAddress = new DeviceAddress("위치결정 기동신호");
    public DeviceAddress positioningDataAddress = new DeviceAddress("위치결정 번호");
    public DeviceAddress stopAddress = new DeviceAddress("정지 신호");

    public DeviceAddress receivedAddress = new DeviceAddress("기동완료 신호");
    public DeviceAddress busyAddress = new DeviceAddress("BUSY 신호");
    public DeviceAddress completedAddress = new DeviceAddress("위치결정 완료 신호");
    public DeviceAddress mCodeOnAddress = new DeviceAddress("M코드 발동 신호");
    public DeviceAddress mCodeAddress = new DeviceAddress("M코드 저장 주소");
    public DeviceAddress mCodeReleaseAddress = new DeviceAddress("M코드 해제 신호");

    public float feedbackTime = 0.3f;
    public MCodeType mCodeType = MCodeType.WithMode;

    private bool receivedPositioning = false;
    private bool haveToExcute = false;
    private bool completedPositioning = false;
    private int currentPosition = 0;
    private float remainFeedbackTime;
    private float remainReceiveTime;


    private List<ServoConveyorConnector> subConnectors = new List<ServoConveyorConnector>();
    public void AddConnector(ServoConveyorConnector connector)
    {
        subConnectors.Add(connector);
    }


    private void Start()
    {
        if (plcReadyAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(plcReadyAddress.address, PLCReady);
        if (servoAllOnAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(servoAllOnAddress.address, ServoOn);
        if (positioningOnAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(positioningOnAddress.address, StartPositioning);
        if (positioningDataAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(positioningDataAddress.address, SetPositioning);
        if (stopAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(stopAddress.address, Stop);
        if (mCodeReleaseAddress.useDevice)
            MXRequester.Get.AddDeviceAddress(mCodeReleaseAddress.address, ReleaseMCode);


        if(mainConnector != null)
        {
            mainConnector.AddConnector(this);
        }
    }

    private void Update()
    {
        if(haveToExcute && currentPosition != 0)
        {
            int mCode = controller.StartPositioning(currentPosition);
            if (receivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(receivedAddress.address, 1);
            if (busyAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(busyAddress.address, 1);

            if(mCodeType == MCodeType.WithMode && mCode != 0)
            {
                if (mCodeOnAddress.useDevice)
                    MXRequester.Get.AddSetDeviceRequest(mCodeOnAddress.address, 1);
                if (mCodeAddress.useDevice)
                    MXRequester.Get.AddSetDeviceRequest(mCodeAddress.address, (short)mCode);
            }

            haveToExcute = false;
            receivedPositioning = true;
            remainReceiveTime = Time.time + feedbackTime;
        }

        if(receivedPositioning && remainReceiveTime < Time.time)
        {
            receivedPositioning = false;
            if (receivedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(receivedAddress.address, 0);
        }

        if(completedPositioning && remainFeedbackTime < Time.time)
        {
            completedPositioning = false;
            if (completedAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(completedAddress.address, 0);
        }
    }

    public void OnCompletedPositioning(int mCode)
    {
        if(completedAddress.useDevice)
            MXRequester.Get.AddSetDeviceRequest(completedAddress.address, 1);

        if (mCodeType == MCodeType.AfterMode && mCodeOnAddress.useDevice)
            MXRequester.Get.AddSetDeviceRequest(mCodeOnAddress.address, 1);
        
        if (mCodeType == MCodeType.AfterMode && mCodeAddress.useDevice)
            MXRequester.Get.AddSetDeviceRequest(mCodeAddress.address, (short)mCode);

        if (busyAddress.useDevice)
            MXRequester.Get.AddSetDeviceRequest(busyAddress.address, 0);

        completedPositioning = true;
        remainFeedbackTime = Time.time + feedbackTime;
    }

    private void ReleaseMCode(short data)
    {
        if(data == 1)
        {
            if (mCodeAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(mCodeAddress.address, 0);
            if (mCodeOnAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(mCodeOnAddress.address, 0);
            if (mCodeReleaseAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(mCodeReleaseAddress.address, 0);
        }
    }

    private void Stop(short data)
    {
        if(data == 1)
        {
            controller.Stop();
            if (busyAddress.useDevice)
                MXRequester.Get.AddSetDeviceRequest(busyAddress.address, 0);
        }
    }

    private void SetPositioning(short data)
    {
        currentPosition = data;
    }

    private void StartPositioning(short data)
    {
        haveToExcute = data != 0;
    }

    private void ServoOn(short data)
    {
        controller.ServoOn(data != 0);
        if(subConnectors.Count > 0)
        {
            for (int i = 0; i < subConnectors.Count; ++i)
            {
                subConnectors[i].ServoOn(data);
            }
        }
    }

    private void PLCReady(short data)
    {
        Debug.Log($"PLC {(data != 0 ? "" : "Not")} Ready!!!");
        if (moduleReadyAddress.useDevice)
            MXRequester.Get.AddSetDeviceRequest(moduleReadyAddress.address, data);

    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(mainConnector != null)
        {
            plcReadyAddress.useDevice = false;
            servoAllOnAddress.useDevice = false;
            moduleReadyAddress.useDevice = false;
        }
    }
#endif
}
