using ActUtlType64Lib;
using UnityEngine;
using UnityEngine.Events;

public class PLCReader1 : MonoBehaviour
{
    private ActUtlType64 reader;

    public UnityEvent<bool> onChangedRed;
    private bool red;
    public bool Red
    {
        get => red;
        set
        {
            if (red == value)
                return;

            red = value;
            onChangedRed?.Invoke(value);
        }
    }

    public UnityEvent<bool> onChangedYellow;
    private bool yellow;
    public bool Yellow
    {
        get => yellow;
        set
        {
            if (yellow == value)
                return;

            yellow = value;
            onChangedYellow?.Invoke(value);
        }
    }
    private bool pb1;

    public bool Pb1
    {
        get => pb1;
        set
        {
            if (pb1 == value)
                return;

            pb1 = value;
            reader.SetDevice2("X0", (short)(value ? 1 : 0));
        }
    }
    private bool pb2;

    public bool Pb2
    {
        get => pb2;
        set
        {
            if (pb2 == value)
                return;

            pb2 = value;
            reader.SetDevice2("X1", (short)(value ? 1 : 0));
        }
    }



    private void Start()
    {
        reader = new ActUtlType64();
        reader.ActLogicalStationNumber = 1;
        if(reader.Open() == 0)
        {
            Debug.Log("연결에 성공했습니다.");
        }
        else
        {
            Debug.Log("연결에 실패했습니다.");
        }
    }

    private void Update()
    {
        if (reader == null)
            return;

        if(reader.ReadDeviceRandom2("Y0", 1, out short result) == 0)
        {
            Red = result != 0;
        }
        if (reader.ReadDeviceRandom2("Y1", 1, out short result1) == 0)
        {
            Yellow = result1 != 0;
        }

    }

    private void OnDestroy()
    {
        if(reader != null)
        {
            if(reader.Close() == 0)
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
