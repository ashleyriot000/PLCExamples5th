using UnityEngine;
using System.Collections.Generic;

public class GripperController : MonoBehaviour
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

        if (other.attachedRigidbody.isKinematic)
            return;

        triggerList.Remove(other.attachedRigidbody);
    }

    public void Grap(bool isGrap)
    {
        if(isGrap)
        {
            foreach(Rigidbody rb in triggerList)
            {
                rb.isKinematic = true;
                rb.transform.SetParent(transform);
            }
        }
        else
        {
            foreach (Rigidbody rb in triggerList)
            {
                rb.isKinematic = false;
                rb.transform.SetParent(null);
            }
        }
    }
}
