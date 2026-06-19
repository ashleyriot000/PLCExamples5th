using UnityEngine;

public class LampConnector : MonoBehaviour
{
    public SignController controller;
    public DeviceAddress device;

    private void Start()
    {
        controller = GetComponent<SignController>();
        MXRequester.Get.AddDeviceAddress(device.address, OnChangedLamp);
    }

    public void OnChangedLamp(short data)
    {
        controller.IsOn = data != 0;
    }
}
