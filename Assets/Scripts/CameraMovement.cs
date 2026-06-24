using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 15f;
    public float lookSensitivity = 0.1f;
    public float panSpeed = 0.05f;
    public float lerpSpeed = 10f; // 부드러운 정도

    [Header("Focus Settings")]
    public float focusDistance = 5f;
    public float minFocusDistance = 2f;  // 최소 줌 거리
    public float maxFocusDistance = 20f; // 최대 줌 거리
    public float scrollSensitivity = 0.01f; // 휠 감도

    // 상태 변수
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float zoomInput;

    private Transform focusTarget;
    private bool isFocusing = false;

    // 마우스 버튼 상태
    private bool isRightPressed;
    private bool isMiddlePressed;
    private bool isLeftPressed;

    void Awake()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        HandleCalculations();
        ApplySmoothing();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (moveInput.sqrMagnitude > 0.01f) StopFocus();
    }

    void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();

    }

    void OnOrbit(InputValue value)
    {
        isLeftPressed = value.isPressed;
    }

    void OnFreeLook(InputValue value)
    {
        isRightPressed = value.isPressed;
        if (isRightPressed) StopFocus();
    }

    void OnPan(InputValue value)
    {
        isMiddlePressed = value.isPressed;
        if (isMiddlePressed) StopFocus();
    }

    void OnZoom(InputValue value)
    {
        zoomInput = value.Get<float>();
    }

    void HandleCalculations()
    {
        if (isFocusing && focusTarget != null)
        {
            // 1. 휠 스크롤로 포커싱 거리 조절
            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                // 휠을 위로 밀면(양수) 거리가 가까워지고, 아래로 당기면(음수) 멀어짐
                focusDistance -= zoomInput * scrollSensitivity;
                // 거리 제한 (Clamp)
                focusDistance = Mathf.Clamp(focusDistance, minFocusDistance, maxFocusDistance);
            }

            // 2. 궤도 회전 (좌클릭 드래그 시)
            if (isLeftPressed && lookInput.sqrMagnitude > 0.01f)
            {
                float x = lookInput.x * lookSensitivity;
                float y = lookInput.y * lookSensitivity;
                targetRotation *= Quaternion.Euler(-y, x, 0);
                Vector3 euler = targetRotation.eulerAngles;
                targetRotation = Quaternion.Euler(euler.x, euler.y, 0);
            }

            // 3. 최종 위치 계산: 타겟 중심으로부터 회전된 방향 * 설정된 거리
            targetPosition = focusTarget.position - (targetRotation * Vector3.forward * focusDistance);
        }
        else
        {
            // 자유 이동 시에도 휠 스크롤로 앞/뒤 이동 가능 (선택 사항)
            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                targetPosition += transform.forward * zoomInput * scrollSensitivity;
            }

            // 일반 WASD 이동
            if (moveInput.sqrMagnitude > 0.01f)
            {
                Vector3 dir = (transform.forward * moveInput.y) + (transform.right * moveInput.x);
                targetPosition += dir * moveSpeed * Time.deltaTime;
            }

            // 우클릭 회전 등... (기존 로직 동일)
            if (isRightPressed && lookInput.sqrMagnitude > 0.01f)
            {
                float x = lookInput.x * lookSensitivity;
                float y = lookInput.y * lookSensitivity;
                targetRotation *= Quaternion.Euler(-y, x, 0);
                Vector3 euler = targetRotation.eulerAngles;
                targetRotation = Quaternion.Euler(euler.x, euler.y, 0);
            }

            if (isMiddlePressed && lookInput.sqrMagnitude > 0.01f)
            {
                Vector3 pan = (transform.up * -lookInput.y + transform.right * -lookInput.x) * 0.1f;
                targetPosition += pan;
            }
        }

        // 입력값 사용 후 초기화 (다음 프레임에 중복 적용 방지)
        zoomInput = 0;
    }

    void ApplySmoothing()
    {
        // 선형 보간(Lerp)을 이용한 부드러운 이동 및 회전
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * lerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);
    }

    public void OnFocus()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            focusTarget = hit.transform;
            isFocusing = true;
            // 포커싱 시 현재 회전값을 기반으로 타겟을 바라보게 정렬 (선택 사항)
            targetRotation = Quaternion.LookRotation(focusTarget.position - transform.position);
        }
    }

    void StopFocus()
    {
        if (isFocusing)
        {
            isFocusing = false;
            // 현재 위치를 타겟 위치로 동기화하여 튕김 방지
            targetPosition = transform.position;
            focusTarget = null;
        }
    }

    public void MoveToDestination(Transform destination)
    {
        // 1. 기존 포커싱 상태 해제
        StopFocus();

        // 2. 목표 위치와 회전값 설정
        targetPosition = destination.position;
        targetRotation = destination.rotation;
    }
}