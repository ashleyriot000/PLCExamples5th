using UnityEngine;
using UnityEngine.UI;

public class InverterDisplayer : MonoBehaviour
{
    public InverterController controller;       //연결된 인버터
    public Text frequencyText;                  //현재 지령 주파수 표시

    //정방향 버튼 클릭했을 때 호출되어야 하는 콜백함수
    public void ClickForwardButton()
    {
        controller.IsOnSTF = !controller.IsOnSTF;
    }
    //역방향 버튼을 클릭했을 때 호출되어야 하는 콜백함수
    public void ClickBackwardButton()
    {
        controller.IsOnSTR = !controller.IsOnSTR;
    }
    //지령 주파수 상승 버튼을 클릭했을 때 호출되어야 하는 콜백함수
    public void IncreaseFrequency(float increase)
    {
        controller.ChangeFrequency(controller.targetHz + increase);
    }

    //지령 주파수 하강 버튼을 클릭했을 때 호출되어야 하는 콜백함수
    public void DecreaseFrequency(float decrease)
    {
        controller.ChangeFrequency(controller.targetHz - decrease);
    }
}
