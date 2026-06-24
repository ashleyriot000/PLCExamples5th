using realvirtual;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class ConveyorController : MonoBehaviour
{
    public ConveyorBelt belt;
    public LayerMask movableLayer;      //컨베이어에서 움직여지는 레이어 설정
    public string movableTag;           //움직여져야 하는 태그 설정(비어있으면 검사 안함)
    public string movableName;          //움직여져야 하는 이름 설정(비어있으면 검사 안함)

    public float maxSpeed = 1f;         //초당 최대 이동속도
    public float targetSpeed;           //초당 지령 속도

    private float currentSpeed;         //초당 현재 속도
    private float _targetSpeed;         //명령내려진 속도

    public UnityEvent<bool> onChangedForward;
    public UnityEvent<bool> onChangedReverse;

    private bool isOnForward;
    public bool IsOnForward
    {
        get => isOnForward;
        set
        {
            if (isOnForward == value)
                return;

            if(isOnForward = value)
            {
                isOnReverse = false;
                onChangedReverse?.Invoke(isOnReverse);
            }

            onChangedForward?.Invoke(isOnForward);
            SetMoveDirection();
        }
    }

    private bool isOnReverse;
    public bool IsOnReverse
    {
        get => isOnReverse;
        set
        {
            if (isOnReverse == value)
                return;

            if (isOnReverse = value)
            {
                isOnForward = false;
                onChangedForward?.Invoke(isOnForward);
            }

            onChangedReverse?.Invoke(isOnReverse);
            SetMoveDirection();
        }
    }



    public List<Rigidbody> triggerList = new List<Rigidbody>();

    private void Awake()
    {
        belt = GetComponent<ConveyorBelt>();
    }


    private void FixedUpdate()
    {

        currentSpeed = Mathf.MoveTowards(currentSpeed, _targetSpeed, maxSpeed * Time.fixedDeltaTime);
        belt.speed = currentSpeed;
        Vector3 moveDelta = currentSpeed * Time.fixedDeltaTime * transform.forward;
        foreach(Rigidbody r in triggerList)
        {
            r.MovePosition(r.position + moveDelta);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //허용된 레이어가 아닐 경우 무시해라
        if ((movableLayer.value & 1 << other.gameObject.layer) == 0)
            return;

        //허용된 태그가 입력되어 있고, 트리거된 게임오브젝트의 태그가 허용태그와 다르면 무시해라
        if (!string.IsNullOrEmpty(movableTag) && movableTag != other.gameObject.tag)
            return;

        //허용된 이름이 입력되어 있고, 트리거된 게임오브젝트의 이름 안에 허용하는 이름이 포함되지 않으면 무시해라
        if (!string.IsNullOrEmpty(movableName) && !other.gameObject.name.Contains(movableName))
            return;

        Rigidbody rb = other.attachedRigidbody;
        //트리거된 게임오브젝트에 리지드바디가 들어있고, 트리거 리스트안에 가지고 있지 않을 경우
        if(rb != null && !triggerList.Contains(rb))
        {
            //추가해서 넣어라.
            triggerList.Add(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {

        Debug.Log($"{other.gameObject.name} => {other.attachedRigidbody}");
        //허용된 레이어가 아닐 경우 무시해라
        if ((movableLayer.value & 1 << other.gameObject.layer) == 0)
            return;

        //허용된 태그가 입력되어 있고, 트리거된 게임오브젝트의 태그가 허용태그와 다르면 무시해라
        if (!string.IsNullOrEmpty(movableTag) && movableTag != other.gameObject.tag)
            return;

        //허용된 이름이 입력되어 있고, 트리거된 게임오브젝트의 이름 안에 허용하는 이름이 포함되지 않으면 무시해라
        if (!string.IsNullOrEmpty(movableName) && !other.gameObject.name.Contains(movableName))
            return;

        Rigidbody rb = other.attachedRigidbody;
        //트리거된 게임오브젝트에 리지드바디가 들어있고, 트리거 리스트안에 가지고 있지 않을 경우
        if (rb != null && triggerList.Contains(rb))
        {
            //추가해서 넣어라.
            triggerList.Remove(rb);
        }
    }

    //속도 지령 함수
    public void SetTargetSpeed(float speed)
    {
        _targetSpeed = 0f;
        targetSpeed = speed;
        if(speed > maxSpeed)
        {
            if(isOnForward)
                _targetSpeed = maxSpeed;
            if (isOnReverse)
                _targetSpeed += -maxSpeed;
            
            return;
        }

        if(speed < 0f)
        {
            _targetSpeed = 0f;
            return;
        }

        if (isOnForward)
            _targetSpeed = speed;
        if (isOnReverse)
            _targetSpeed += -speed;
    }

    private void SetMoveDirection()
    {
        _targetSpeed = 0f;
        if (isOnForward)
            _targetSpeed = targetSpeed;
        
        if (isOnReverse)
            _targetSpeed += -targetSpeed;
    }
}
