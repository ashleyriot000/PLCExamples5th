using ActUtlType64Lib;
using UnityEngine;
using UnityEngine.Events;

public class ReaderForCylinder : MonoBehaviour
{
    private bool isOnForward;
    public bool IsOnForward
    {
        get => isOnForward;
        set
        {
            if (isOnForward == value)
                return;

            isOnForward = value;
            reader.SetDevice2("X0", (short)(value ? 1 : 0));
        }
    }


    private bool isOnReverse;
    public bool IsOnReverse
    {
        get => isOnReverse;
        set
        {
            if (isOnReverse == value)
                return;

            isOnReverse = value;
            reader.SetDevice2("X1", (short)(value ? 1 : 0));
        }
    }

    private bool isOnDetected;
    public bool IsOnDetected
    {
        get => isOnDetected;
        set
        {
            if (isOnDetected == value)
                return;

            isOnDetected = value;
            reader.SetDevice2("X2", (short)(value ? 1 : 0));
        }
    }

    private bool isForward;
    private bool isReverse;
    public UnityEvent<bool> onChangedForward;
    public UnityEvent<bool> onChangedReverse;

    private ActUtlType64 reader = null;

    void Start()
    {
        reader = new ActUtlType64();
        reader.ActLogicalStationNumber = 1;
        if (reader.Open() == 0)
        {
            Debug.Log("PLC와의 성공적으로 연결되었습니다.");
        }
        else
        {
            Debug.Log("PLC와의 연결하는데 실패하였습니다.");
        }
    }

    private short[] datas = new short[2];
    void Update()
    {
        if (reader == null)
            return;

        if (reader.ReadDeviceRandom2("Y0\nY1", 2, out datas[0]) == 0)
        {
            bool forward = datas[0] != 0;
            bool reverse = datas[1] != 0;

            if(isForward != forward)
            {
                onChangedForward?.Invoke(forward);
                isForward = forward;
            }
            if(isReverse != reverse)
            {
                onChangedReverse?.Invoke(reverse);
                isReverse = reverse;
            }
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
}
