using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArcMaker : MonoBehaviour
{
    public Transform pStart;
    public Transform pVia;
    public Transform pEnd;
    public void Make()
    {

    }


    /// <summary>
    /// 세 점(start, via, end)을 지나는 원호의 궤적 좌표들을 배열로 반환합니다.
    /// </summary>
    /// <param name="start">시작점</param>
    /// <param name="via">경유점</param>
    /// <param name="end">도착점</param>
    /// <param name="segments">원호를 몇 개의 직선 구간으로 쪼갤지 (기본값 20)</param>
    /// <returns>원호를 구성하는 좌표들의 배열</returns>
    public static Vector3[] GetArcPoints(Vector3 start, Vector3 via, Vector3 end, int segments = 20)
    {
        // 1. 세 점을 지나는 원의 중심점과 평면의 법선 벡터(Normal) 계산
        Vector3 center = CalculateCircleCenter(start, via, end, out Vector3 normal);

        // 예외 처리: 세 점이 일직선상에 있어 원호를 그릴 수 없는 경우 직선 좌표 반환
        if (float.IsInfinity(center.x) || float.IsNaN(center.x))
        {
            return new Vector3[] { start, end };
        }

        // 2. 중심점에서 시작점과 도착점을 향하는 벡터 도출
        Vector3 vStart = start - center;
        Vector3 vEnd = end - center;

        // 3. 경유지 방향을 정확히 거쳐가는 전체 회전 각도 계산
        float arcAngle = Vector3.SignedAngle(vStart, vEnd, normal);

        // 외적 기반 법선 벡터를 기준으로 했기 때문에, 음수가 나오면 360도를 더해 무조건 경유지 방향을 거치도록 보정
        if (arcAngle < 0f)
        {
            arcAngle += 360f;
        }

        // 4. 세그먼트 개수만큼 원호 궤적의 좌표들을 생성
        Vector3[] arcPoints = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments; // 0.0 ~ 1.0 비율
            float currentAngle = arcAngle * t;

            // 법선 벡터(Normal)를 축으로 삼아 currentAngle만큼 회전하는 쿼터니언 생성
            Quaternion arcRotation = Quaternion.AngleAxis(currentAngle, normal);

            // 중심점에 회전된 벡터를 더해 최종 좌표 획득
            arcPoints[i] = center + (arcRotation * vStart);
        }

        return arcPoints;
    }

    /// <summary>
    /// 3D 공간에서 세 점을 지나는 삼각형의 외심(Circumcenter)을 계산하는 기하학 함수
    /// </summary>
    private static Vector3 CalculateCircleCenter(Vector3 a, Vector3 b, Vector3 c, out Vector3 normal)
    {
        Vector3 v1 = b - a;
        Vector3 v2 = c - a;

        // 두 벡터의 외적을 통해 세 점이 이루는 평면의 법선 벡터(Normal)를 구함
        normal = Vector3.Cross(v1, v2);

        // 세 점이 일직선상에 있어 삼각형을 이루지 못하는 경우 (예외 처리)
        if (normal.sqrMagnitude < 0.00001f)
        {
            return Vector3.Lerp(a, c, 0.5f);
        }

        // 3D 외심 계산 공식 적용
        Vector3 center = a + Vector3.Cross(v1.sqrMagnitude * v2 - v2.sqrMagnitude * v1, normal) / (2f * normal.sqrMagnitude);

        return center;
    }

    private void OnDrawGizmos()
    {
        if (pStart == null || pVia == null || pEnd == null) return;

        // 위에서 만든 함수를 통해 원호의 궤적 점들을 가져옵니다.
        Vector3[] arcPoints = GetArcPoints(pStart.position, pVia.position, pEnd.position, 30);

        Gizmos.color = Color.magenta;

        // 배열에 담긴 점들을 순차적으로 이어 선을 그립니다.
        for (int i = 0; i < arcPoints.Length - 1; i++)
        {
            Gizmos.DrawLine(arcPoints[i], arcPoints[i + 1]);
        }
    }
}
