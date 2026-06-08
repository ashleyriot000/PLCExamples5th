using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    public Transform[] waypoints;
    public float moveSpeed = 1f;
    private int index;

    private void Update()
    {
        transform.position = 
            Vector3.MoveTowards(transform.position, waypoints[index].position, moveSpeed * Time.deltaTime);

        if(transform.position == waypoints[index].position)
        {
            index++;
            if (index >= waypoints.Length)
                index = 0;
        }
    }
}
