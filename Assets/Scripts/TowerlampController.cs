using UnityEngine;

public class TowerlampController : MonoBehaviour
{
    //타워 램프의 상태를 정의한 열거형
    public enum LampState
    {
        Green,
        Yellow,
        Red
    }

    public SignController red;
    public SignController yellow;
    public SignController green;
    
    public LampState currentState;      //현재 상태
    public float greenDuration = 30f;   //각 램프 상태의 유지시간
    public float yellowDuration = 5f;
    public float redDuration = 10f;
    public float blinkDuration = 0.5f;

    private float nextStateTime;
    private float nextBlinkTime;

    private void Start()
    {
        currentState = LampState.Green;
        nextStateTime = Time.time + greenDuration;
        green.IsOn = true;
    }


    void Update()
    {
        /*      1. 시작하면 처음에는 녹색만 켜짐.
         *      2. 30초 후에 녹색이 꺼지고, 노란불이 켜지기 시작.
         *      3. 5초동안 0.5초 켜지고, 0.5초 꺼지기 반복.
         *      4. 10초 동안 빨간 불만 켜짐.
         *      5. 1번으로 돌아감.
         */

        switch (currentState)
        {
            case LampState.Green:
                UpdateGreen();
                break;
            case LampState.Yellow:
                UpdateYellow();
                break;
            case LampState.Red:
                UpdateRed();
                break;
            default:
                break;
        }

    }

    private void UpdateRed()
    {
        if(nextStateTime < Time.time)
        {
            currentState = LampState.Green;
            green.IsOn = true;
            yellow.IsOn = false;
            red.IsOn = false;
            nextStateTime = Time.time + greenDuration;
            return;
        }
    }

    private void UpdateYellow()
    {
        if (nextStateTime < Time.time)
        {
            currentState = LampState.Red;
            red.IsOn = true;
            yellow.IsOn = false;
            green.IsOn = false;
            nextStateTime = Time.time + redDuration;
            return;
        }

        if (nextBlinkTime < Time.time)
        {
            nextBlinkTime = Time.time + blinkDuration;
            yellow.IsOn = !yellow.IsOn;
        }
    }

    private void UpdateGreen()
    {
        if (nextStateTime < Time.time)
        {
            currentState = LampState.Yellow;
            red.IsOn = false;
            yellow.IsOn = true;
            green.IsOn = false;
            nextBlinkTime = Time.time + blinkDuration;
            nextStateTime = Time.time + yellowDuration;
            return;
        }
    }
}
