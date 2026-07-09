using UnityEngine;
using UnityEngine.Splines;

public class ChainConnector : MonoBehaviour
{
    public bool connectTrigger;
    public SplineContainer container;
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;
    public Vector3 lockPosition;

    private void OnValidate()
    {
        container = GetComponent<SplineContainer>();
        int count = transform.childCount;
        container.Spline.Clear();
        for(int i = 0; i < count; ++i)
        {
            var child = transform.GetChild(i);
            var position = child.position;
            if (lockX)
                position.x = lockPosition.x;
            if(lockY)
                position.y = lockPosition.y;
            if(lockZ)
                position.z = lockPosition.z;

            var knot = new BezierKnot(position);            
            container.Spline.Add(knot);
        }
    }

}
