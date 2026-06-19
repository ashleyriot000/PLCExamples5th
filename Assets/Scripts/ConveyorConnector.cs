using UnityEngine;

public class ConveyorConnector : MonoBehaviour
{
    public ConveyorController controller;
    public DeviceAddress forward = new DeviceAddress("Forward");
    public DeviceAddress reverse = new DeviceAddress("Reverse");

    private void Start()
    {
        controller = GetComponent<ConveyorController>();
        MXRequester.Get.AddDeviceAddress(forward.address, OnChangedForward);
        MXRequester.Get.AddDeviceAddress(reverse.address, OnChangedReverse);
    }


    public void OnChangedForward(short data)
    {
        controller.IsOnForward = data != 0;
    }

    public void OnChangedReverse(short data) 
    {
        controller.IsOnReverse = data != 0;
    }





















}
