using UnityEngine;
using UnityEngine.Events;

public class SignController : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    //유니티에서 만든 델리게이트
    public UnityEvent<bool> onChangedPower;

    private bool isOn;
    //프로퍼티
    public bool IsOn
    {
        get     //Get함수
        {
            return isOn;
        }
        
        set     //Set함수
        {
            if (isOn == value)
                return;

            if(isOn = value)
            {
                //매터리얼의 Emission 체크
                meshRenderer.material.EnableKeyword("_EMISSION");
            }
            else
            {
                //매터리얼의 Emission 체크 해제
                meshRenderer.material.DisableKeyword("_EMISSION");
            }
                
            onChangedPower?.Invoke(value);
        }
    }

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        IsOn = true;
        IsOn = false;
    }
}
