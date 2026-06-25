using System.Collections.Generic;
using UnityEngine;

public class ClearManager : MonoBehaviour
{
    public List<Rigidbody> triggerList = new List<Rigidbody>();
    private void OnTriggerEnter(Collider other)
    {
        if (triggerList.Contains(other.attachedRigidbody))
            return;

        triggerList.Add(other.attachedRigidbody);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!triggerList.Contains(other.attachedRigidbody))
            return;

        triggerList.Remove(other.attachedRigidbody);
    }

    public void Clear()
    {
        foreach(var t in triggerList)
        {
            t.MovePosition(-Vector3.one * 100f);
        }
    }

}
