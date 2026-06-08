using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    //트리거된 리지드바디를 담아넣는 리스트 배열
    public List<Rigidbody> rigidList = new List<Rigidbody>();
    public float moveSpeed = 1f;        //컨베이어 이동 속도

    //콜라이더 영역에 다른 콜라이더가 들어올 경우 호출되는 콜백함수
    private void OnTriggerEnter(Collider other)
    {
        //Contains함수 => 괄호안에 있는 객체가 리스트 안에 있는지 확인해주는 함수 들어있으면 true반환, 없으면 false
        if (rigidList.Contains(other.attachedRigidbody))
            return;

        //Add함수는 리스트에 추가하기 
        rigidList.Add(other.attachedRigidbody);        
    }

    //콜라이더 영역에 들어와 있던 다른 콜라이더가 나갈 경우 호출되는 콜백함수
    private void OnTriggerExit(Collider other)
    {
        if (!rigidList.Contains(other.attachedRigidbody))
            return;

        //Remove함수는 리스트에서 제거하기
        rigidList.Remove(other.attachedRigidbody);
    }
    //물리 기반 업데이트 함수
    private void FixedUpdate()
    {
        foreach(Rigidbody r in rigidList)
        {
            //MovePosition(절대위치) => 지정된 위치로 물리적으로 이동시킴.
            r.MovePosition(r.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
        }
    }
}
