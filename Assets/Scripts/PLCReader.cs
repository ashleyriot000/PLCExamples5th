using ActUtlType64Lib;
using UnityEngine;
using UnityEngine.Events;

public class PLCReader : MonoBehaviour
{
    private ActUtlType64 plc;

    private bool x0;
    public bool X0
    {
        get => x0;
        set
        {
            if (x0 == value)
                return;

            x0 = value;
            plc.WriteDeviceRandom2("X0",  1, (short)(value ? 1 : 0));
        }
    }

    private bool x1;
    public bool X1
    {
        get => x1;
        set
        {
            if (x1 == value)
                return;

            x1 = value;
            plc.WriteDeviceRandom2("X1", 1, (short)(value ? 1 : 0));
        }
    }

    private bool y0;
    public bool Y0
    {
        get => y0;
        set
        {
            if (y0 == value)
                return;

            y0 = value;
            onChangedY0?.Invoke(value);
        }
    }

    private bool y1;
    public bool Y1
    {
        get => y1;
        set
        {
            if (y1 == value)
                return;

            y1 = value;
            onChangedY1?.Invoke(value);
        }
    }

    private bool y2;
    public bool Y2
    {
        get => y2;
        set
        {
            if (y2 == value)
                return;

            y2 = value;
            onChangedY2?.Invoke(value);
        }
    }


    public UnityEvent<bool> onChangedY0;
    public UnityEvent<bool> onChangedY1;
    public UnityEvent<bool> onChangedY2;


    void Start()
    {
        plc = new ActUtlType64();
        plc.ActLogicalStationNumber = 1;
        if(plc.Open() == 0)
        {
            Debug.Log("연결에 성공했습니다.");
        }
        else
        {
            Debug.Log("연결에 실패했습니다.");
        }
    }


        
    short[] results = new short[10];
    private void Update()
    {
        if(plc.ReadDeviceRandom2("Y0\nY1\nY2", 3, out results[0]) == 0)
        {
            Y0 = results[0] != 0;
            Y1 = results[1] != 0;
            Y2 = results[2] != 0;
        }
        else
        {
            Debug.Log("읽기 실패");
        }
    }

    private void OnDestroy()
    {
        if(plc != null)
        {
            if(plc.Close() == 0)
            {
                Debug.Log("연결해제했습니다.");
            }
            else
            {
                Debug.Log("연결해제하지 못했습니다.");
            }
        }
    }

}
