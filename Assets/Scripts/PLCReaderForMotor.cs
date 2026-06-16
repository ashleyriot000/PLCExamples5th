using UnityEngine;
using UnityEngine.Events;
using ActUtlType64Lib;

public class PLCReaderForMotor : MonoBehaviour
{
    private bool isOnForward;
    public bool IsOnForward
    {
        get => isOnForward;
        private set
        {
            if (isOnForward == value)
                return;

            isOnForward = value;
            onChangedForward?.Invoke(value);
        }
    }
    public UnityEvent<bool> onChangedForward;


    private bool isOnReverse;
    public bool IsOnReverse
    {
        get => isOnReverse;
        private set
        {
            if (isOnReverse == value)
                return;

            isOnReverse = value;
            onChangedReverse?.Invoke(value);
        }
    }
    public UnityEvent<bool> onChangedReverse;

    private ActUtlType64 reader = null;



    void Start()
    {
        reader = new ActUtlType64();
        reader.ActLogicalStationNumber = 1;
        if(reader.Open() == 0)
        {
            Debug.Log("PLC와의 성공적으로 연결되었습니다.");
        }
        else
        {
            Debug.Log("PLC와의 연결하는데 실패하였습니다.");
        }
    }

    private short[] datas = new short[2];
    // Update is called once per frame
    void Update()
    {
        if (reader == null)
            return;

        if(reader.ReadDeviceRandom2("Y10\nY11", 2, out datas[0]) == 0)
        {
            IsOnForward = datas[0] != 0;
            IsOnReverse = datas[1] != 0;
        }
        else
        {
            Debug.Log("데이터를 읽어오는데 실패했습니다.");
        }
    }

    private void OnDestroy()
    {
        if (reader != null)
        {
            if (reader.Close() == 0)
            {
                Debug.Log("연결을 성공적으로 해제했습니다.");
            }
            else
            {
                Debug.Log("연결해제 하는데 실패했습니다.");
            }
        }
    }


    public void PressForwardButton(bool isPressed)
    {
        reader.SetDevice2("X10", (short)(isPressed ? 1 : 0));
    }

    public void PressReverseButton(bool isPressed)
    {
        reader.SetDevice2("X11", (short)(isPressed ? 1 : 0));
    }
}
