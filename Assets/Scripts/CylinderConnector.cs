using UnityEngine;

public class CylinderConnector : MonoBehaviour
{
    public CylinderController controller;
    public DeviceAddress forward;
    public DeviceAddress backward;

    private void Start()
    {
        controller = GetComponent<CylinderController>();
        MXRequester.Get.AddDeviceAddress(forward.address, OnChangedForward);
        MXRequester.Get.AddDeviceAddress(backward.address, OnChangedBackward);
    }

    public void OnChangedForward(short data)
    {
        controller.ToForward(data != 0);
    }
    public void OnChangedBackward(short data)
    {
        controller.ToBackward(data != 0);
    }
}
