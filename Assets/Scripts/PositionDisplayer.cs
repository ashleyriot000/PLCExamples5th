using UnityEngine;
using UnityEngine.UI;

public class PositionDisplayer : MonoBehaviour
{
    public Text pulseText;
    public Text unitText;

    public void OnChangedPulse(int pulse)
    {
        pulseText.text = pulse.ToString("N");
    }

    public void OnChangedUnit(float unit)
    {
        unitText.text = unit.ToString() + "mm";
    }
   
}
