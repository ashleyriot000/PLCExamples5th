using System.Collections.Generic;
using UnityEngine;

public class ArcConveyorController : MonoBehaviour
{
    [Header("Conveyor Path Points")]
    [Tooltip("아크의 시작점")] public Transform pStart;
    [Tooltip("아크의 경유점")] public Transform pVia;
    [Tooltip("아크의 도착점")] public Transform pEnd;

    [Header("Conveyor Settings")]
    [Tooltip("컨베이어 이동 속도 (mm/s 또는 m/s)")]
    public float moveSpeed = 1f;

    [Tooltip("정회전 신호 (체크하면 이동, 해제하면 정지)")]
    public bool isForwardOn = false;

    // 컨베이어 영역 안의 상품 리스트
    private List<Transform> productsInZone = new List<Transform>();

    // 내부 연산용 캐싱 변수
    private Vector3 center;
    private Vector3 normal;
    private bool isArcValid = false;

    private void Start()
    {
        InitializeArc();
    }

    /// <summary>
    /// 외부(PLC나 상위 컨트롤러)에서 정회전 신호를 켜고 끌 때 호출하는 함수
    /// </summary>
    public void SetForwardSignal(bool isOn)
    {
        isForwardOn = isOn;
        if (isOn)
            Debug.Log($"{gameObject.name} : 정회전 모터 구동 시작");
        else
            Debug.Log($"{gameObject.name} : 모터 정지");
    }

    private void InitializeArc()
    {
        if (pStart == null || pVia == null || pEnd == null)
        {
            Debug.LogWarning("컨베이어 경로 포인트가 할당되지 않았습니다.");
            return;
        }

        // 1. 아크의 중심점 및 회전축(Normal) 계산
        center = CalculateCircleCenter(pStart.position, pVia.position, pEnd.position, out normal);

        if (float.IsInfinity(center.x) || float.IsNaN(center.x))
        {
            Debug.LogError("세 점이 일직선에 있어 컨베이어 아크를 생성할 수 없습니다.");
            isArcValid = false;
        }
        else
        {
            isArcValid = true;
        }
    }

    private void Update()
    {
        // 정회전 신호가 없거나, 궤적 생성이 실패했거나, 위에 물건이 없으면 연산 안 함
        if (!isForwardOn || !isArcValid || productsInZone.Count == 0) return;

        float dt = Time.deltaTime;

        // 리스트를 역순으로 순회 (이동 중 물체가 파괴되거나 삭제될 경우를 대비)
        for (int i = productsInZone.Count - 1; i >= 0; i--)
        {
            Transform product = productsInZone[i];

            // 물체가 파괴되었다면 리스트에서 제거
            if (product == null)
            {
                productsInZone.RemoveAt(i);
                continue;
            }

            // 2. 현재 물체의 위치를 기준으로 중심점부터의 벡터와 반지름 도출
            Vector3 vPos = product.position - center;
            float radius = vPos.magnitude;

            // 3. 선형 속도(v)를 각속도(ω)로 변환: ω = v / r
            // (Mathf.Rad2Deg를 곱해 라디안을 일반 각도(Degree)로 변환)
            float angleStep = (moveSpeed / radius) * Mathf.Rad2Deg * dt;

            // 4. 회전축(normal)을 기준으로 이번 프레임에 회전할 쿼터니언 도출
            Quaternion stepRotation = Quaternion.AngleAxis(angleStep, normal);

            // 5. 위치 갱신: 중심점에서 vPos를 회전시킨 위치로 적용
            product.position = center + (stepRotation * vPos);

            // 6. 자세 갱신: 곡률에 맞춰 물체의 머리 방향도 함께 부드럽게 회전
            product.rotation = stepRotation * product.rotation;
        }
    }

    // ==========================================
    // 물리 충돌체 감지 (Trigger 기반)
    // ==========================================
    private void OnTriggerEnter(Collider other)
    {
        // 태그 검사 등으로 특정 물체만 이동시키고 싶다면 여기에 조건 추가
        // if (!other.CompareTag("Product")) return;

        Transform productRoot = other.transform; // 필요에 따라 attachedRigidbody.transform 등 사용 가능

        if (!productsInZone.Contains(productRoot))
        {
            productsInZone.Add(productRoot);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Transform productRoot = other.transform;

        if (productsInZone.Contains(productRoot))
        {
            productsInZone.Remove(productRoot);
        }
    }

    // ==========================================
    // 기하학 연산 및 에디터 렌더링
    // ==========================================
    private Vector3 CalculateCircleCenter(Vector3 a, Vector3 b, Vector3 c, out Vector3 normal)
    {
        Vector3 v1 = b - a;
        Vector3 v2 = c - a;
        normal = Vector3.Cross(v1, v2);

        if (normal.sqrMagnitude < 0.00001f)
            return Vector3.Lerp(a, c, 0.5f);

        return a + Vector3.Cross(v1.sqrMagnitude * v2 - v2.sqrMagnitude * v1, normal) / (2f * normal.sqrMagnitude);
    }

    private void OnDrawGizmos()
    {
        if (pStart == null || pVia == null || pEnd == null) return;
        if (Application.isPlaying) return;

        Vector3 c = CalculateCircleCenter(pStart.position, pVia.position, pEnd.position, out Vector3 n);
        if (float.IsInfinity(c.x) || float.IsNaN(c.x)) return;

        Vector3 vS = pStart.position - c;
        Vector3 vE = pEnd.position - c;
        float a = Vector3.SignedAngle(vS, vE, n);
        if (a < 0f) a += 360f;

        Gizmos.color = Color.cyan;
        Vector3 prevPos = pStart.position;
        int segments = 20;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            Quaternion rot = Quaternion.AngleAxis(a * t, n);
            Vector3 nextPos = c + (rot * vS);
            Gizmos.DrawLine(prevPos, nextPos);
            prevPos = nextPos;
        }
    }
}