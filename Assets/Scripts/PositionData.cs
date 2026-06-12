using UnityEngine;
using UnityEngine.UI;

public class PositionData : MonoBehaviour
{
    public PositioningManager manager;
    public ServoAmp axis1;
    public ServoAmp axis2;
    public ServoAmp axis3;

    public Text idText;
    public Text position1Text;
    public Text position2Text;
    public Text position3Text;

    [SerializeField] private int pos1;
    [SerializeField] private int pos2;
    [SerializeField] private int pos3;

    //데이터가 처음 UI로 표시될 때
    public void Initialize(int index, int position1, int position2, int position3)
    {
        this.pos1 = position1;
        this.pos2 = position2;
        this.pos3 = position3;
        idText.text = "P" + index.ToString("D3");
        position1Text.text = pos1.ToString();
        position2Text.text = pos2.ToString();
        position3Text.text = pos3.ToString();
    }

    //순서가 변경될 때 함수
    public void ChangeIndex(int index)
    {
        idText.text = "P" + index.ToString("D3");
    }

    //삭제될 때 호출되는 함수
    public void Delete(bool removeData)
    {
        if (removeData)
            manager.RemoveData(this);

        Destroy(gameObject);
    }

    public void Go()
    {
        axis1.Positioning(pos1);
        axis2.Positioning(pos2);
        axis3.Positioning(pos3);
    }

}
