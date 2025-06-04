using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // UI 요소를 위한 네임스페이스 추가

public class ClickForceFieldController : MonoBehaviour
{
    [Header("메인 카메라")]
    public Camera mainCamera1;    // Combined Display(좌측, 정면, 우측) 렌더링
    public Camera mainCamera2;    // Down Display 렌더링

    [Header("서브 카메라 (Combined Display 영역 분할)")]
    public Camera leftCamera;
    public Camera frontCamera;
    public Camera rightCamera;

    [Header("디스플레이 플레인")]
    [Tooltip("Combined Display Plane (예: 왼쪽, 정면, 오른쪽이 표시되는 평면)")]
    public GameObject combinedPlane;
    [Tooltip("Down Display Plane (하단 평면)")]
    public GameObject downDisplayPlane;

    [Header("레이어 마스크")]
    [Tooltip("Combined Plane의 Collider가 있는 레이어")]
    public LayerMask combinedPlaneLayerMask;
    [Tooltip("Down Display Plane의 Collider가 있는 레이어")]
    public LayerMask downDisplayPlaneLayerMask;
    [Tooltip("실제 벽이나 Force Field가 적용될 대상 오브젝트의 레이어")]
    public LayerMask wallLayerMask;

    [Header("Force Field 설정")]
    [Tooltip("Force Field의 영향 반경 (Inspector에서 조정 가능)")]
    public float forceFieldRadius = 3.0f;
    [Tooltip("Force Field의 중력값 (예: -0.2)")]
    public float forceFieldGravity = -0.2f;
    [Tooltip("Force Field의 지속 시간 (초)")]
    public float forceFieldDuration = 5.0f;
    [Tooltip("동시에 존재할 수 있는 최대 Force Field 개수")]
    public int maxForceFields = 5;
    [Tooltip("Force Field 시각화 파티클 프리팹 (선택 사항)")]
    public GameObject forceFieldVisualizerPrefab;
    [Tooltip("Force Field 색상")]
    public Color forceFieldColor = new Color(0.2f, 0.6f, 1.0f, 0.5f);

    [Header("디버깅 설정")]
    [Tooltip("터치 위치에 표시할 디버그 마커 프리팹")]
    public GameObject debugMarkerPrefab;
    [Tooltip("디버그 텍스트를 표시할 UI 텍스트")]
    public Text debugText;
    [Tooltip("디버그 마커의 지속 시간")]
    public float debugMarkerDuration = 2.0f;
    [Tooltip("디버그 마커의 색상")]
    public Color debugMarkerColor = Color.red;

    // Force Field 관리를 위한 리스트
    private List<ForceFieldInfo> activeForceFields = new List<ForceFieldInfo>();

    private TextureCombiner textureCombiner;
    private ProjectionSetup projectionSetup;

    private List<GameObject> debugMarkers = new List<GameObject>();

    // Force Field 정보를 저장하는 클래스
    private class ForceFieldInfo
    {
        public GameObject forceFieldObject;
        public float creationTime;
        public float duration;

        public ForceFieldInfo(GameObject obj, float time, float dur)
        {
            forceFieldObject = obj;
            creationTime = time;
            duration = dur;
        }
    }

    void Awake()
    {
        textureCombiner = FindAnyObjectByType<TextureCombiner>();
        projectionSetup = FindAnyObjectByType<ProjectionSetup>();
        
        // 초기 설정 시도
        TryFindDisplayPlanes();
    }

    void Start()
    {
        // 시작 시 한 번 더 확인
        TryFindDisplayPlanes();
    }

    // 디스플레이 플레인을 찾는 메서드
    private bool TryFindDisplayPlanes()
    {
        bool planesFound = true;
        
        // combinedPlane이 없으면 찾기 시도
        if (combinedPlane == null)
        {
            if (textureCombiner != null && textureCombiner.combinedDisplayPlane != null)
            {
                combinedPlane = textureCombiner.combinedDisplayPlane;
                Debug.Log("combinedPlane을 textureCombiner에서 찾았습니다.");
            }
            else
            {
                Debug.LogWarning("combinedPlane을 찾을 수 없습니다.");
                planesFound = false;
            }
        }
        
        // downDisplayPlane이 없으면 찾기 시도
        if (downDisplayPlane == null)
        {
            if (textureCombiner != null && textureCombiner.downDisplayPlane != null)
            {
                downDisplayPlane = textureCombiner.downDisplayPlane;
                Debug.Log("downDisplayPlane을 textureCombiner에서 찾았습니다.");
            }
            else
            {
                Debug.LogWarning("downDisplayPlane을 찾을 수 없습니다.");
                planesFound = false;
            }
        }
        
        return planesFound;
    }

    void Update()
    {
        // Force Field 수명 관리
        ManageForceFields();

        // 터치/클릭 감지
        DetectTouchInput();
    }

    // Force Field 수명 관리
    void ManageForceFields()
    {
        float currentTime = Time.time;
        List<ForceFieldInfo> fieldsToRemove = new List<ForceFieldInfo>();

        // 수명이 다한 Force Field 찾기
        foreach (ForceFieldInfo field in activeForceFields)
        {
            if (currentTime - field.creationTime >= field.duration)
            {
                fieldsToRemove.Add(field);
            }
        }

        // 수명이 다한 Force Field 제거
        foreach (ForceFieldInfo field in fieldsToRemove)
        {
            Destroy(field.forceFieldObject);
            activeForceFields.Remove(field);
        }
    }

    // 터치/클릭 입력 감지
    void DetectTouchInput()
    {
        // 터치 또는 마우스 클릭 감지
        bool inputDetected = false;
        Vector2 inputPosition = Vector2.zero;

        // 터치 입력 확인 (모바일)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                inputDetected = true;
                inputPosition = touch.position;
            }
        }
        // 마우스 입력 확인 (PC)
        else if (Input.GetMouseButtonDown(0))
        {
            inputDetected = true;
            inputPosition = Input.mousePosition;
        }

        // 입력이 감지되면 Force Field 생성 처리
        if (inputDetected)
        {
            ProcessInputForForceField(inputPosition);
        }
    }

    // 입력 위치를 기반으로 Force Field 생성 처리
    void ProcessInputForForceField(Vector2 screenPosition)
    {
        // 디스플레이 플레인이 없으면 찾기 시도
        if (combinedPlane == null || downDisplayPlane == null)
        {
            if (!TryFindDisplayPlanes())
            {
                Debug.LogWarning("디스플레이 플레인을 찾을 수 없어 Force Field를 생성할 수 없습니다.");
                return;
            }
        }
        
        // 우선 mainCamera1 (Combined Display)을 통해 클릭한 경우 처리
        if (mainCamera1 != null && combinedPlane != null)
        {
            // mainCamera1를 이용해 스크린 좌표에서 Ray 생성
            Ray mainRay = mainCamera1.ScreenPointToRay(screenPosition);
            RaycastHit combinedHit;
            
            if (Physics.Raycast(mainRay, out combinedHit, Mathf.Infinity, combinedPlaneLayerMask))
            {
                // combinedPlane에 충돌한 위치의 로컬 좌표를 구함
                Vector3 localPoint = combinedPlane.transform.InverseTransformPoint(combinedHit.point);
                float relativeX = localPoint.x + 0.5f; // 0 ~ 1 범위
                float relativeY = localPoint.y + 0.5f; // 0 ~ 1 범위

                // UV 좌표처럼 판단하여, 왼쪽/중앙/오른쪽 영역을 결정
                Camera chosenCamera = null;
                float newX = 0f;
                string areaName = "";
                
                if (relativeX < 1f / 3f)
                {
                    chosenCamera = leftCamera;
                    newX = relativeX * 3f; // [0, 1/3] → [0, 1]
                    areaName = "왼쪽";
                }
                else if (relativeX < 2f / 3f)
                {
                    chosenCamera = frontCamera;
                    newX = (relativeX - 1f / 3f) * 3f; // [1/3, 2/3] → [0, 1]
                    areaName = "중앙";
                }
                else
                {
                    chosenCamera = rightCamera;
                    newX = (relativeX - 2f / 3f) * 3f; // [2/3, 1] → [0, 1]
                    areaName = "오른쪽";
                }
                
                // 디버그 정보 표시
                ShowDebugInfo(combinedHit.point, areaName, new Vector2(relativeX, relativeY));
                
                if (chosenCamera != null)
                {
                    // 상대적 viewport 좌표 (y는 그대로 사용)
                    Vector3 subViewportPoint = new Vector3(newX, relativeY, 0f);
                    
                    // 선택된 서브 카메라의 viewport 좌표로부터 Ray 생성
                    Ray subRay = chosenCamera.ViewportPointToRay(subViewportPoint);
                    
                    // 실제 벽과의 충돌 검사
                    RaycastHit wallHit;
                    if (Physics.Raycast(subRay, out wallHit, Mathf.Infinity, wallLayerMask))
                    {
                        // 실제 벽(또는 대상 오브젝트)와의 충돌로부터 월드 좌표 획득
                        Debug.Log(wallHit.point);
                        CreateForceField(wallHit.point, wallHit.normal);
                        return;
                    }
                    else
                    {
                        // 벽과 충돌하지 않았다면 일정 거리에 Force Field 생성
                        Vector3 forceFieldPosition = subRay.origin + subRay.direction * 5f; // 10 유닛 거리에 생성
                        CreateForceField(forceFieldPosition, -subRay.direction);
                        return;
                    }
                }
            }
        }
        
        // mainCamera2 (Down Display) 처리
        if (mainCamera2 != null && downDisplayPlane != null)
        {
            Ray downRay = mainCamera2.ScreenPointToRay(screenPosition);
            RaycastHit downHit;
            
            if (Physics.Raycast(downRay, out downHit, Mathf.Infinity, downDisplayPlaneLayerMask))
            {
                // 디버그 정보 표시
                ShowDebugInfo(downHit.point, "바닥", new Vector2(0.5f, 0.5f));
                
                CreateForceField(downHit.point, downHit.normal);
            }
        }
    }

    // 지정된 위치에 Force Field를 생성하는 메서드
    void CreateForceField(Vector3 position, Vector3 normal)
    {
        // 최대 Force Field 개수 제한 확인
        if (activeForceFields.Count >= maxForceFields)
        {
            // 가장 오래된 Force Field 제거
            Destroy(activeForceFields[0].forceFieldObject);
            activeForceFields.RemoveAt(0);
        }

        // Force Field 오브젝트 생성
        GameObject forceFieldObject = new GameObject("ForceField_" + Time.time);
        forceFieldObject.transform.position = position;
        
        // 충돌 표면의 법선 방향으로 약간 오프셋 (벽 안에 생성되는 것 방지)
        forceFieldObject.transform.position += normal * 0.05f;

        // ParticleSystemForceField 컴포넌트 추가 및 설정
        ParticleSystemForceField forceField = forceFieldObject.AddComponent<ParticleSystemForceField>();
        forceField.shape = ParticleSystemForceFieldShape.Sphere;
        forceField.gravity = forceFieldGravity;
        forceField.startRange = 0f;
        forceField.endRange = forceFieldRadius;
        forceField.directionX = 0f;
        forceField.directionY = 0f;
        forceField.directionZ = 0f;

        // 파티클 시스템 비활성화
        ParticleSystem[] particleSystems = forceFieldObject.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Stop();
            ps.gameObject.SetActive(false);
        }

        // Force Field 정보 저장
        ForceFieldInfo fieldInfo = new ForceFieldInfo(forceFieldObject, Time.time, forceFieldDuration);
        activeForceFields.Add(fieldInfo);
        
        // 디버그 로그
        Debug.Log("Force Field 생성됨: " + position + ", 반경: " + forceFieldRadius);
    }

    // 디버그 정보를 표시하는 메서드
    void ShowDebugInfo(Vector3 worldPosition, string areaName, Vector2 relativePosition)
    {
        // 디버그 마커 생성
        if (debugMarkerPrefab != null)
        {
            GameObject marker = Instantiate(debugMarkerPrefab, worldPosition, Quaternion.identity);
            
            // 디버그 마커의 레이어를 CombinedDisplay 레이어로 설정
            if (textureCombiner != null)
            {
                marker.layer = textureCombiner.combinedDisplayLayer;
            }
            
            // 파티클 시스템 설정
            ParticleSystem ps = marker.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = debugMarkerColor;
                main.startSize = 0.2f;
                main.startLifetime = debugMarkerDuration;
                main.loop = false;
                
                var emission = ps.emission;
                emission.rateOverTime = 0;
                emission.SetBurst(0, new ParticleSystem.Burst(0f, 50));
                
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.1f;
                
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.World;
                velocity.radial = new ParticleSystem.MinMaxCurve(1f, 2f);
                
                // 파티클 시스템의 렌더러 설정
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = 1; // 텍스처 위에 렌더링되도록 설정
                }
                
                ps.Play();
            }
            
            debugMarkers.Add(marker);
            Destroy(marker, debugMarkerDuration);
        }

        // 디버그 텍스트 업데이트
        if (debugText != null)
        {
            debugText.text = $"터치 위치:\n" +
                           $"영역: {areaName}\n" +
                           $"월드 좌표: ({worldPosition.x:F2}, {worldPosition.y:F2}, {worldPosition.z:F2})\n" +
                           $"상대 좌표: ({relativePosition.x:F2}, {relativePosition.y:F2})";
        }

        // 콘솔에도 출력
        Debug.Log($"터치 감지 - 영역: {areaName}, 월드 좌표: {worldPosition}, 상대 좌표: {relativePosition}");
    }

    // 모든 Force Field 제거
    public void ClearAllForceFields()
    {
        foreach (ForceFieldInfo field in activeForceFields)
        {
            Destroy(field.forceFieldObject);
        }
        activeForceFields.Clear();

        // 디버그 마커도 함께 제거
        foreach (GameObject marker in debugMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
        debugMarkers.Clear();
    }

    void OnDisable()
    {
        ClearAllForceFields();
    }
}
