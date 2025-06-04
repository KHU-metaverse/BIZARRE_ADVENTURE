using UnityEngine;
using System.Collections.Generic;

// 컴포넌트 메뉴에 추가
[AddComponentMenu("Audio Visualizer/Cross Face Particle Manager")]
public class CrossFaceParticleManager : MonoBehaviour
{
    [System.Serializable]
    public class ProjectionFace
    {
        public string name;
        public Transform faceTransform;
        public Vector3 faceNormal;
        public Vector3 faceSize;
        
        // 법선 벡터를 자동으로 계산하는 메서드
        public void CalculateNormal()
        {
            if (faceTransform != null)
            {
                // Transform의 forward 방향을 법선 벡터로 사용
                faceNormal = faceTransform.forward.normalized;
            }
        }
    }
    
    public ProjectionFace[] projectionFaces;
    public ParticleSystem crossFaceParticleSystem;
    
    [Header("Flow Settings")]
    public float particleSpeed = 1f;
    public float faceCrossingDistance = 0.1f; // 면 사이를 이동할 때 필요한 거리
    public int maxParticles = 1000;
    
    private ParticleSystem.Particle[] particles;
    private Dictionary<int, int> particleFaceMap = new Dictionary<int, int>(); // 각 입자가 현재 어느 면에 있는지 추적
    
    void Start()
    {
        particles = new ParticleSystem.Particle[maxParticles];
        InitializeParticleSystem();
    }
    
    // 모든 면의 법선 벡터를 자동으로 계산
    [ContextMenu("자동 법선 벡터 계산")]
    public void AutoCalculateAllNormals()
    {
        if (projectionFaces == null || projectionFaces.Length == 0)
        {
            Debug.LogWarning("투영 면이 설정되지 않았습니다.");
            return;
        }
        
        foreach (var face in projectionFaces)
        {
            if (face.faceTransform == null)
            {
                Debug.LogWarning($"면 '{face.name}'의 Transform이 설정되지 않았습니다.");
                continue;
            }
            
            face.CalculateNormal();
        }
        
        Debug.Log($"{projectionFaces.Length}개 면의 법선 벡터가 자동으로 계산되었습니다.");
    }
    
    // 인스펙터에서 값이 변경될 때 호출되는 메서드
    void OnValidate()
    {
        // 새로운 면이 추가되었을 때 자동으로 이름 설정
        if (projectionFaces != null)
        {
            for (int i = 0; i < projectionFaces.Length; i++)
            {
                if (string.IsNullOrEmpty(projectionFaces[i].name))
                {
                    projectionFaces[i].name = $"Face {i+1}";
                }
            }
        }
    }
    
    // 에디터에서 컴포넌트가 추가될 때 호출
    void Reset()
    {
        // 기본 설정 추가
        projectionFaces = new ProjectionFace[0];
        particleSpeed = 1f;
        faceCrossingDistance = 0.1f;
        maxParticles = 1000;
    }
    
    void InitializeParticleSystem()
    {
        // 시작 전에 모든 법선 벡터 확인 및 계산
        for (int i = 0; i < projectionFaces.Length; i++)
        {
            if (projectionFaces[i].faceTransform != null && projectionFaces[i].faceNormal == Vector3.zero)
            {
                projectionFaces[i].CalculateNormal();
            }
        }
        
        var main = crossFaceParticleSystem.main;
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = 30f;
        main.startSpeed = 0f; // 속도는 직접 제어
        
        // 초기 입자 생성 - 각 면에 균등하게 분배
        int particlesPerFace = maxParticles / projectionFaces.Length;
        for (int faceIndex = 0; faceIndex < projectionFaces.Length; faceIndex++)
        {
            for (int i = 0; i < particlesPerFace; i++)
            {
                // 임의의 위치에 입자 생성
                Vector3 randomPosition = GetRandomPositionOnFace(faceIndex);
                crossFaceParticleSystem.Emit(new ParticleSystem.EmitParams
                {
                    position = randomPosition,
                    startLifetime = 30f,
                    startSize = Random.Range(0.05f, 0.2f)
                }, 1);
                
                // 이 입자가 어느 면에 있는지 저장
                particleFaceMap[faceIndex * particlesPerFace + i] = faceIndex;
            }
        }
    }
    
    // 면 크기 자동 계산 (옵션)
    [ContextMenu("자동 면 크기 계산")]
    public void AutoCalculateFaceSizes()
    {
        if (projectionFaces == null || projectionFaces.Length == 0)
        {
            Debug.LogWarning("투영 면이 설정되지 않았습니다.");
            return;
        }
        
        foreach (var face in projectionFaces)
        {
            if (face.faceTransform == null)
            {
                Debug.LogWarning($"면 '{face.name}'의 Transform이 설정되지 않았습니다.");
                continue;
            }
            
            // Transform의 로컬 스케일을 사용하여 면 크기 추정
            face.faceSize = face.faceTransform.localScale;
        }
        
        Debug.Log($"{projectionFaces.Length}개 면의 크기가 자동으로 계산되었습니다.");
    }
    
    void Update()
    {
        int numParticles = crossFaceParticleSystem.GetParticles(particles);
        
        for (int i = 0; i < numParticles; i++)
        {
            // 현재 입자가 어느 면에 있는지 확인
            if (!particleFaceMap.TryGetValue(i, out int currentFaceIndex))
            {
                currentFaceIndex = DetermineFace(particles[i].position);
                particleFaceMap[i] = currentFaceIndex;
            }
            
            // 임의의 방향으로 이동
            Vector3 moveDirection = new Vector3(
                Mathf.Sin(Time.time * 0.5f + i * 0.1f),
                Mathf.Cos(Time.time * 0.7f + i * 0.2f),
                Mathf.Sin(Time.time * 0.3f + i * 0.3f)
            ).normalized;
            
            // 면의 경계에 가까워지면 다음 면으로 이동할 준비
            if (IsNearFaceBoundary(particles[i].position, currentFaceIndex))
            {
                // 다음 면으로 이동 방향 계산
                int nextFaceIndex = GetNextFace(currentFaceIndex, particles[i].position);
                moveDirection = CalculateCrossFaceDirection(currentFaceIndex, nextFaceIndex);
                
                // 충분히 다음 면 쪽으로 이동했으면 면 소속 변경
                if (ShouldChangeFace(particles[i].position, currentFaceIndex, nextFaceIndex))
                {
                    particleFaceMap[i] = nextFaceIndex;
                }
            }
            
            // 입자 위치 업데이트
            particles[i].position += moveDirection * particleSpeed * Time.deltaTime;
            
            // 면 밖으로 너무 많이 벗어나면 다시 면 안으로
            if (!IsOnAnyFace(particles[i].position))
            {
                particles[i].position = GetRandomPositionOnFace(Random.Range(0, projectionFaces.Length));
            }
        }
        
        // 수정된 입자 데이터 적용
        crossFaceParticleSystem.SetParticles(particles, numParticles);
    }
    
    // 임의의 면 위치 생성
    Vector3 GetRandomPositionOnFace(int faceIndex)
    {
        if (faceIndex >= 0 && faceIndex < projectionFaces.Length)
        {
            ProjectionFace face = projectionFaces[faceIndex];
            Vector3 randomOffset = new Vector3(
                Random.Range(-face.faceSize.x / 2, face.faceSize.x / 2),
                Random.Range(-face.faceSize.y / 2, face.faceSize.y / 2),
                Random.Range(-face.faceSize.z / 2, face.faceSize.z / 2)
            );
            return face.faceTransform.position + face.faceTransform.TransformDirection(randomOffset);
        }
        return Vector3.zero;
    }
    
    // 주어진 위치가 어느 면에 속하는지 결정
    int DetermineFace(Vector3 position)
    {
        int closestFaceIndex = -1;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < projectionFaces.Length; i++)
        {
            ProjectionFace face = projectionFaces[i];
            
            // 면의 로컬 좌표로 변환
            Vector3 localPos = face.faceTransform.InverseTransformPoint(position);
            
            // 면의 영역 내에 있는지 확인
            if (Mathf.Abs(localPos.x) <= face.faceSize.x / 2 &&
                Mathf.Abs(localPos.y) <= face.faceSize.y / 2 &&
                Mathf.Abs(localPos.z) <= face.faceSize.z / 2)
            {
                // 면의 법선 방향으로의 거리 계산
                float distanceToFace = Mathf.Abs(Vector3.Dot(position - face.faceTransform.position, face.faceNormal));
                
                if (distanceToFace < closestDistance)
                {
                    closestDistance = distanceToFace;
                    closestFaceIndex = i;
                }
            }
        }
        
        return closestFaceIndex != -1 ? closestFaceIndex : Random.Range(0, projectionFaces.Length);
    }
    
    // 입자가 면의 경계에 가까워졌는지 확인
    bool IsNearFaceBoundary(Vector3 position, int faceIndex)
    {
        if (faceIndex >= 0 && faceIndex < projectionFaces.Length)
        {
            ProjectionFace face = projectionFaces[faceIndex];
            Vector3 localPos = face.faceTransform.InverseTransformPoint(position);
            
            float marginX = face.faceSize.x * 0.1f;
            float marginY = face.faceSize.y * 0.1f;
            float marginZ = face.faceSize.z * 0.1f;
            
            return Mathf.Abs(localPos.x) > face.faceSize.x / 2 - marginX ||
                   Mathf.Abs(localPos.y) > face.faceSize.y / 2 - marginY ||
                   Mathf.Abs(localPos.z) > face.faceSize.z / 2 - marginZ;
        }
        
        return false;
    }
    
    // 현재 면에서 다음 면으로 이동하기 위한 방향 계산
    Vector3 CalculateCrossFaceDirection(int currentFaceIndex, int nextFaceIndex)
    {
        if (currentFaceIndex >= 0 && currentFaceIndex < projectionFaces.Length &&
            nextFaceIndex >= 0 && nextFaceIndex < projectionFaces.Length)
        {
            return (projectionFaces[nextFaceIndex].faceTransform.position - 
                    projectionFaces[currentFaceIndex].faceTransform.position).normalized;
        }
        
        return Random.onUnitSphere;
    }
    
    // 다음으로 이동할 면 결정
    int GetNextFace(int currentFaceIndex, Vector3 position)
    {
        // 현재 위치에서 가장 가까운 이웃 면 찾기
        int closestNeighborIndex = -1;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < projectionFaces.Length; i++)
        {
            if (i == currentFaceIndex) continue;
            
            float distance = Vector3.Distance(position, projectionFaces[i].faceTransform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNeighborIndex = i;
            }
        }
        
        return closestNeighborIndex != -1 ? closestNeighborIndex : (currentFaceIndex + 1) % projectionFaces.Length;
    }
    
    // 면 변경이 필요한지 확인
    bool ShouldChangeFace(Vector3 position, int currentFaceIndex, int nextFaceIndex)
    {
        if (currentFaceIndex >= 0 && currentFaceIndex < projectionFaces.Length &&
            nextFaceIndex >= 0 && nextFaceIndex < projectionFaces.Length)
        {
            Vector3 currentFacePos = projectionFaces[currentFaceIndex].faceTransform.position;
            Vector3 nextFacePos = projectionFaces[nextFaceIndex].faceTransform.position;
            
            Vector3 currentToNext = nextFacePos - currentFacePos;
            Vector3 currentToParticle = position - currentFacePos;
            
            // 다음 면 방향으로 얼마나 이동했는지 계산
            float projectionDistance = Vector3.Dot(currentToParticle, currentToNext.normalized);
            
            return projectionDistance > Vector3.Distance(currentFacePos, nextFacePos) * 0.5f;
        }
        
        return false;
    }
    
    // 입자가 어느 면에든 있는지 확인
    bool IsOnAnyFace(Vector3 position)
    {
        for (int i = 0; i < projectionFaces.Length; i++)
        {
            ProjectionFace face = projectionFaces[i];
            Vector3 localPos = face.faceTransform.InverseTransformPoint(position);
            
            float marginX = face.faceSize.x * 1.5f;
            float marginY = face.faceSize.y * 1.5f;
            float marginZ = face.faceSize.z * 1.5f;
            
            if (Mathf.Abs(localPos.x) <= marginX &&
                Mathf.Abs(localPos.y) <= marginY &&
                Mathf.Abs(localPos.z) <= marginZ)
            {
                return true;
            }
        }
        
        return false;
    }
    
#if UNITY_EDITOR
    // 에디터 전용 기능 - 면 설정 도우미
    [ContextMenu("면 설정 도우미")]
    public void SetupFacesWizard()
    {
        UnityEditor.EditorUtility.DisplayDialog(
            "면 설정 도우미",
            "이 기능은 다음 단계를 수행합니다:\n\n" +
            "1. 각 면의 이름을 자동으로 설정합니다.\n" +
            "2. 각 면의 법선 벡터를 계산합니다.\n" +
            "3. 각 면의 크기를 Transform의 스케일 기반으로 설정합니다.\n\n" +
            "계속하시겠습니까?",
            "예", "아니오"
        );
        
        // 이름 설정
        for (int i = 0; i < projectionFaces.Length; i++)
        {
            if (string.IsNullOrEmpty(projectionFaces[i].name))
            {
                projectionFaces[i].name = $"Face {i+1}";
            }
        }
        
        // 법선 벡터 계산
        AutoCalculateAllNormals();
        
        // 면 크기 계산
        AutoCalculateFaceSizes();
        
        UnityEditor.EditorUtility.DisplayDialog(
            "완료",
            "모든 면 설정이 완료되었습니다.",
            "확인"
        );
    }
    
    // 에디터에서 시각화
    void OnDrawGizmosSelected()
    {
        if (projectionFaces == null) return;
        
        for (int i = 0; i < projectionFaces.Length; i++)
        {
            ProjectionFace face = projectionFaces[i];
            if (face.faceTransform == null) continue;
            
            // 면의 색상 설정 (각 면마다 다른 색상)
            Color faceColor = new Color(
                (i * 0.618f) % 1f,
                (i * 0.418f) % 1f,
                (i * 0.781f) % 1f,
                0.5f
            );
            Gizmos.color = faceColor;
            
            // 면의 위치에 큐브 그리기
            Vector3 size = face.faceSize;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                face.faceTransform.position,
                face.faceTransform.rotation,
                Vector3.one
            );
            
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawCube(Vector3.zero, size);
            
            // 법선 벡터 표시
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(Vector3.zero, face.faceNormal.normalized * size.magnitude * 0.5f);
            
            // 면 이름 표시
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(face.faceTransform.position, face.name);
        }
        
        // 원래 행렬로 복원
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}