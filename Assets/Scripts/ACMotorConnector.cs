using UnityEngine;

public class ACMotorConnector : MonoBehaviour
{
    public ACMotorController controller;
    public DeviceAddress forward = new DeviceAddress("Forward");
    public DeviceAddress reverse = new DeviceAddress("Reverse");

    private void Start()
    {
        controller = GetComponent<ACMotorController>();
        MXRequester.Get.AddDeviceAddress(forward.address, OnChangedForward);
        MXRequester.Get.AddDeviceAddress(reverse.address, OnChangedBackward);
    }

    public void OnChangedForward(short data)
    {
        controller.IsOnForward = data != 0;
    }
    public void OnChangedBackward(short data)
    {
        controller.IsOnBackward = data != 0;
    }
}
