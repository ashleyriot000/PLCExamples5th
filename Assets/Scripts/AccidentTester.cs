using UnityEngine;

public class AccidentTester : MonoBehaviour
{
    public Transform[] accidentZones;
    public GameObject prefab;

    public void GenerateAccident()
    {
        foreach (var zone in accidentZones)
        {
            Instantiate(prefab, zone.position, zone.rotation);
        }
    }

}
