using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineChainController : MonoBehaviour
{
    public enum UpdateType
    {
        None,
        FixedUpdate,
        Update,
        LateUpdate
    }

    [Header("연결 요소")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private GameObject[] chainLinkPrefabs;
    [SerializeField] private Transform[] motorGear;
    [SerializeField] private Transform connectedShaft;
    [SerializeField] private Vector3 connectedGearRatio;
    [SerializeField] private UpdateType uType = UpdateType.FixedUpdate;

    [Header("기어 및 체인 설정")]
    [SerializeField, Delayed] private int gearTeethCount = 28;
    [SerializeField, Delayed] private float linkLength = 0.5f;
    [SerializeField, Delayed] private Vector3 rotateAxis = Vector3.forward;
    [SerializeField, Delayed] private float motorRotationSpeed = 90f;
    [SerializeField, Delayed] private Vector3 ChainDirection = Vector3.forward;

    private List<Transform> spawnedLinks = new List<Transform>();
    private float gearCircumferencePerRotation;
    private float distancePerDegree;
    private float currentDistanceOffset = 0f;
    private float totalSplineLength = 0f;
    private int totalLinksCount = 0;
    private Vector3 prevRotation;

    void Start()
    {
        if (splineContainer == null || chainLinkPrefabs == null)
        {
            Debug.LogError("필수 컴포넌트가 할당되지 않았습니다.");
            return;
        }

        gearCircumferencePerRotation = gearTeethCount * linkLength;
        distancePerDegree = gearCircumferencePerRotation / 360f;
        totalSplineLength = splineContainer.CalculateLength();
        totalLinksCount = Mathf.FloorToInt(totalSplineLength / linkLength);

        SpawnChainLinks();
        UpdateChainPositions();
        if (connectedShaft != null)
        {
            prevRotation = connectedShaft.eulerAngles;
        }
    }

    void Update()
    {
        if (uType != UpdateType.Update)
            return;

        CalculateAndRotate(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (uType != UpdateType.FixedUpdate)
            return;

        CalculateAndRotate(Time.fixedDeltaTime);
    }
    private void LateUpdate()
    {
        if (uType != UpdateType.LateUpdate)
            return;

        CalculateAndRotate(Time.deltaTime);
    }


    private void CalculateAndRotate(float deltaTime)
    {
        float movedDistance = 0f;
        float rotationAmount = 0;
        //0. 회전량 계산
        if (connectedShaft != null)
        {
            Vector3 currentRotation = connectedShaft.eulerAngles;
            Vector3 deltaRotation;
            deltaRotation.x = Mathf.DeltaAngle(prevRotation.x, currentRotation.x);
            deltaRotation.y = Mathf.DeltaAngle(prevRotation.y, currentRotation.y);
            deltaRotation.z = Mathf.DeltaAngle(prevRotation.z, currentRotation.z);

            // 보정된 회전 차이값과 기어비를 내적(Dot)하여 최종 회전량 산출
            rotationAmount = Vector3.Dot(deltaRotation, connectedGearRatio);
            prevRotation = currentRotation;
        }
        else
        {
            rotationAmount = motorRotationSpeed * deltaTime;
        }


        // 1. 모터 기어 시각적 회전 
        foreach (var gear in motorGear)
        {
            gear.Rotate(rotateAxis, rotationAmount);
        }
        // 2. 체인 이동 거리 계산        
        movedDistance = rotationAmount * distancePerDegree;
        currentDistanceOffset = Mathf.Repeat(currentDistanceOffset + movedDistance, totalSplineLength);

        // 3. 위치 업데이트
        UpdateChainPositions();
    }
    private void SpawnChainLinks()
    {
        for (int i = 0; i < totalLinksCount; i++)
        {
            GameObject link = Instantiate(chainLinkPrefabs[i % chainLinkPrefabs.Length], splineContainer.transform);
            spawnedLinks.Add(link.transform);
        }
    }

    private void UpdateChainPositions()
    {
        for (int i = 0; i < spawnedLinks.Count; i++)
        {
            float targetDistance = (i * linkLength) + currentDistanceOffset;
            targetDistance = ((targetDistance % totalSplineLength) + totalSplineLength) % totalSplineLength;

            float t = targetDistance / totalSplineLength;

            // [핵심 수정 부분]
            // SplineContainer는 기본적으로 Local Space 기준으로 좌표를 반환합니다.
            Vector3 localPosition = splineContainer.EvaluatePosition(t);
            Vector3 localForward = splineContainer.EvaluateTangent(t); // 앞방향
            Vector3 up = Vector3.Cross(ChainDirection, localForward).normalized;

            // 자식 오브젝트이므로 localPosition과 localRotation을 사용합니다.
            spawnedLinks[i].localPosition = localPosition;

            if (localForward != Vector3.zero)
            {
                spawnedLinks[i].localRotation = Quaternion.LookRotation(localForward, up);
            }
        }
    }

    public void SetMotorSpeed(float speed)
    {
        motorRotationSpeed = speed;
    }
}