using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class ProjectionSetupMenu : Editor
{
    [MenuItem("AudioVisualizer/프로젝션 매핑/새 프로젝션 설정 생성")]
    static void CreateNewProjectionSetup()
    {
        // 사용자에게 저장 여부 확인
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }
        
        // 새 씬 생성 여부 확인
        bool createNewScene = EditorUtility.DisplayDialog(
            "새 씬 생성",
            "새 씬을 생성하시겠습니까? '아니오'를 선택하면 현재 씬에 프로젝션 설정이 추가됩니다.",
            "예",
            "아니오"
        );
        
        if (createNewScene)
        {
            // 새 씬 생성
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            
            // 기본 메인 카메라 비활성화
            Camera defaultCamera = GameObject.FindObjectOfType<Camera>();
            if (defaultCamera != null)
            {
                defaultCamera.gameObject.SetActive(false);
            }
        }
        
        // 프로젝션 설정 오브젝트 생성
        GameObject projectionObj = new GameObject("ProjectionSetup");
        ProjectionSetup setup = projectionObj.AddComponent<ProjectionSetup>();
        
        // 카메라 생성
        CreateCameras(setup);
        
        // 메인 카메라 생성
        CreateMainCameras(setup);
        
        // 선택
        Selection.activeGameObject = projectionObj;
        
        if (createNewScene)
        {
            // 씬 저장 경로 설정
            string scenePath = "Assets/AudioVisualizer/Scenes/ProjectionSetup.unity";
            
            // Scenes 폴더가 없으면 생성
            string dirPath = Path.GetDirectoryName(scenePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            
            // 씬 저장
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
            
            Debug.Log("새 프로젝션 설정 씬이 생성되었습니다: " + scenePath);
            
            // 프로젝트 창에서 씬 파일 선택
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(scenePath);
        }
        else
        {
            Debug.Log("현재 씬에 프로젝션 설정이 추가되었습니다.");
        }
    }
    
    [MenuItem("AudioVisualizer/프로젝션 매핑/텍스처 결합기 추가")]
    static void AddTextureCombiner()
    {
        // 선택된 게임 오브젝트 확인
        GameObject selectedObj = Selection.activeGameObject;
        
        if (selectedObj == null)
        {
            // 선택된 오브젝트가 없으면 새로 생성
            selectedObj = new GameObject("TextureCombiner");
        }
        
        // TextureCombiner 컴포넌트 추가
        TextureCombiner combiner = selectedObj.GetComponent<TextureCombiner>();
        if (combiner == null)
        {
            combiner = selectedObj.AddComponent<TextureCombiner>();
        }
        
        // 선택
        Selection.activeGameObject = selectedObj;
        
        Debug.Log("텍스처 결합기가 추가되었습니다.");
    }
    
    [MenuItem("AudioVisualizer/프로젝션 매핑/레이어 설정")]
    static void SetupLayers()
    {
        // 레이어 생성
        CheckAndCreateLayer("CombinedDisplay", 8);
        CheckAndCreateLayer("DownDisplay", 9);
        
        Debug.Log("프로젝션 매핑 레이어가 설정되었습니다.");
    }
    
    private static void CreateCameras(ProjectionSetup setup)
    {
        // 방 중앙 위치 계산
        Vector3 roomCenter = Vector3.zero;
        
        // 좌측 카메라 - 방 중앙에서 좌측을 바라봄
        GameObject leftCamObj = new GameObject("LeftCamera");
        Camera leftCamera = leftCamObj.AddComponent<Camera>();
        leftCamera.transform.position = roomCenter;
        leftCamera.transform.rotation = Quaternion.Euler(0, -90, 0); // 좌측(-X 방향)을 바라봄
        leftCamera.backgroundColor = new Color(0.2f, 0, 0);
        leftCamera.clearFlags = CameraClearFlags.SolidColor;
        
        // 정면 카메라 - 방 중앙에서 정면을 바라봄
        GameObject frontCamObj = new GameObject("FrontCamera");
        Camera frontCamera = frontCamObj.AddComponent<Camera>();
        frontCamera.transform.position = roomCenter;
        frontCamera.transform.rotation = Quaternion.Euler(0, 0, 0); // 정면(+Z 방향)을 바라봄
        frontCamera.backgroundColor = new Color(0, 0.2f, 0);
        frontCamera.clearFlags = CameraClearFlags.SolidColor;
        
        // 우측 카메라 - 방 중앙에서 우측을 바라봄
        GameObject rightCamObj = new GameObject("RightCamera");
        Camera rightCamera = rightCamObj.AddComponent<Camera>();
        rightCamera.transform.position = roomCenter;
        rightCamera.transform.rotation = Quaternion.Euler(0, 90, 0); // 우측(+X 방향)을 바라봄
        rightCamera.backgroundColor = new Color(0, 0, 0.2f);
        rightCamera.clearFlags = CameraClearFlags.SolidColor;
        
        // 하단 카메라 - 방 중앙에서 아래를 바라봄
        GameObject downCamObj = new GameObject("DownCamera");
        Camera downCamera = downCamObj.AddComponent<Camera>();
        downCamera.transform.position = roomCenter;
        downCamera.transform.rotation = Quaternion.Euler(90, 0, 0); // 아래(-Y 방향)를 바라봄
        downCamera.backgroundColor = new Color(0.2f, 0.2f, 0);
        downCamera.clearFlags = CameraClearFlags.SolidColor;
        
        // 카메라 할당
        setup.frontCamera = frontCamera;
        setup.leftCamera = leftCamera;
        setup.rightCamera = rightCamera;
        setup.downCamera = downCamera;
    }
    
    private static void CreateMainCameras(ProjectionSetup setup)
    {
        // 메인 카메라 1 (결합된 디스플레이용)
        GameObject mainCam1Obj = new GameObject("MainCamera1");
        Camera mainCamera1 = mainCam1Obj.AddComponent<Camera>();
        mainCamera1.transform.position = new Vector3(0, 0, -10);
        mainCamera1.transform.rotation = Quaternion.identity; // 정면을 바라봄
        mainCamera1.clearFlags = CameraClearFlags.SolidColor;
        mainCamera1.backgroundColor = Color.black;
        mainCamera1.orthographic = true; // 직교 투영 설정
        mainCamera1.orthographicSize = 4.5f; // 적절한 크기 설정
        mainCamera1.targetDisplay = setup.mainCamera1Display; // 타겟 디스플레이 설정
        
        // 메인 카메라 2 (하단 디스플레이용)
        GameObject mainCam2Obj = new GameObject("MainCamera2");
        Camera mainCamera2 = mainCam2Obj.AddComponent<Camera>();
        mainCamera2.transform.position = new Vector3(0, 0, -10);
        mainCamera2.transform.rotation = Quaternion.identity; // 정면을 바라봄
        mainCamera2.clearFlags = CameraClearFlags.SolidColor;
        mainCamera2.backgroundColor = Color.black;
        mainCamera2.orthographic = true; // 직교 투영 설정
        mainCamera2.orthographicSize = 4.5f; // 적절한 크기 설정
        mainCamera2.targetDisplay = setup.mainCamera2Display; // 타겟 디스플레이 설정
        
        // 메인 카메라 할당
        setup.mainCamera1 = mainCamera1;
        setup.mainCamera2 = mainCamera2;
        
        // 디스플레이 플레인 생성 (에디터에서 미리 생성)
        if (setup.useCombinedTexture)
        {
            // 디스플레이 1의 비율 계산
            float display1Aspect = GetDisplayAspect(setup.mainCamera1Display);
            
            // 결합된 디스플레이 플레인 생성
            GameObject combinedPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            combinedPlane.name = "CombinedDisplayPlane";
            combinedPlane.transform.position = mainCamera1.transform.position + mainCamera1.transform.forward * 5;
            combinedPlane.transform.rotation = Quaternion.Euler(0, 180, 0); // 180도 회전하여 카메라를 향하도록 함
            
            // 플레인 스케일 조정 - 디스플레이 비율에 맞게 스트레치
            combinedPlane.transform.localScale = new Vector3(9.0f * display1Aspect, 9.0f, 1);
            
            // 디스플레이 2의 비율 계산
            float display2Aspect = GetDisplayAspect(setup.mainCamera2Display);
            
            // 하단 디스플레이 플레인 생성
            GameObject downPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            downPlane.name = "DownDisplayPlane";
            downPlane.transform.position = mainCamera2.transform.position + mainCamera2.transform.forward * 5;
            downPlane.transform.rotation = Quaternion.Euler(0, 180, 0); // 180도 회전하여 카메라를 향하도록 함
            
            // 플레인 스케일 조정 - 디스플레이 비율에 맞게 스트레치
            downPlane.transform.localScale = new Vector3(9.0f * display2Aspect, 9.0f, 1);
            
            // 레이어 설정
            if (LayerMask.NameToLayer("CombinedDisplay") != -1)
                combinedPlane.layer = LayerMask.NameToLayer("CombinedDisplay");
                
            if (LayerMask.NameToLayer("DownDisplay") != -1)
                downPlane.layer = LayerMask.NameToLayer("DownDisplay");
        }
    }
    
    // 디스플레이 비율 계산 헬퍼 메서드
    private static float GetDisplayAspect(int displayIndex)
    {
        // 에디터에서는 실제 디스플레이 정보를 가져올 수 없으므로 일반적인 비율 사용
        // 실제 게임 실행 시에는 Display.displays를 통해 정확한 비율을 가져옴
        if (displayIndex == 0)
        {
            // 메인 디스플레이는 현재 게임 뷰의 비율 사용
            return (float)Screen.width / Screen.height;
        }
        else
        {
            // 보조 디스플레이는 일반적인 16:9 비율 사용 (추정)
            return 16.0f / 9.0f;
        }
    }
    
    private static void CheckAndCreateLayer(string layerName, int layerIndex)
    {
        // 레이어가 이미 존재하는지 확인
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        
        // 레이어 인덱스가 유효한지 확인
        if (layerIndex >= 8 && layerIndex <= 31)
        {
            SerializedProperty layerProp = layers.GetArrayElementAtIndex(layerIndex);
            
            // 레이어가 비어있으면 새 레이어 이름 설정
            if (string.IsNullOrEmpty(layerProp.stringValue))
            {
                layerProp.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"레이어 생성됨: {layerName} (인덱스: {layerIndex})");
            }
            else if (layerProp.stringValue != layerName)
            {
                Debug.LogWarning($"레이어 인덱스 {layerIndex}에 이미 다른 레이어가 있습니다: {layerProp.stringValue}");
            }
        }
        else
        {
            Debug.LogError($"유효하지 않은 레이어 인덱스: {layerIndex}. 8-31 사이의 값이어야 합니다.");
        }
    }
} 