using UnityEngine;
using UnityEditor;

namespace AudioVisualizer.Examples
{
    /// <summary>
    /// 텍스처 결합 시스템의 예제 설정을 보여주는 스크립트입니다.
    /// 이 스크립트는 씬에 필요한 모든 구성 요소를 자동으로 설정합니다.
    /// </summary>
    public class TextureCombinerExample : MonoBehaviour
    {
        [Header("카메라 설정")]
        public Vector3 leftCameraPosition = new Vector3(-5, 1, 0);
        public Vector3 frontCameraPosition = new Vector3(0, 1, -5);
        public Vector3 rightCameraPosition = new Vector3(5, 1, 0);
        public Vector3 downCameraPosition = new Vector3(0, 5, 0);
        
        [Header("디스플레이 설정")]
        public Vector3 combinedDisplayPosition = new Vector3(0, 1, 5);
        public Vector3 downDisplayPosition = new Vector3(0, 1, 8);
        public Vector2 displayScale = new Vector2(16, 9);
        
        [Header("텍스처 설정")]
        public int textureWidth = 1280;
        public int textureHeight = 720;
        public int combinedTextureWidth = 3840;
        public int combinedTextureHeight = 1080;
        
        [Header("레이어 설정")]
        public int combinedDisplayLayer = 8; // "CombinedDisplay" 레이어
        public int downDisplayLayer = 9;     // "DownDisplay" 레이어
        
        // 생성된 객체 참조
        private Camera leftCamera;
        private Camera frontCamera;
        private Camera rightCamera;
        private Camera downCamera;
        private Camera mainCamera1;
        private Camera mainCamera2;
        private GameObject combinedDisplay;
        private GameObject downDisplay;
        private TextureCombiner textureCombiner;
        
        void Start()
        {
            // 레이어 확인 및 생성
            CheckAndCreateLayers();
            
            // 카메라 생성
            CreateCameras();
            
            // 디스플레이 생성
            CreateDisplays();
            
            // 텍스처 결합기 생성
            CreateTextureCombiner();
            
            // 메인 카메라 생성
            CreateMainCameras();
            
            // 테스트 객체 생성
            CreateTestObjects();
            
            Debug.Log("텍스처 결합 예제가 설정되었습니다.");
        }
        
        // 레이어 확인 및 생성 (에디터에서만 작동)
        private void CheckAndCreateLayers()
        {
#if UNITY_EDITOR
            // 레이어 생성
            CreateLayer("CombinedDisplay", combinedDisplayLayer);
            CreateLayer("DownDisplay", downDisplayLayer);
#endif
        }
        
#if UNITY_EDITOR
        // 레이어 생성 헬퍼 메서드
        private void CreateLayer(string layerName, int layerIndex)
        {
            // 레이어가 이미 존재하는지 확인
            SerializedObject tagManager = new SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
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
#endif
        
        private void CreateCameras()
        {
            // 좌측 카메라
            GameObject leftCamObj = new GameObject("LeftCamera");
            leftCamera = leftCamObj.AddComponent<Camera>();
            leftCamera.transform.position = leftCameraPosition;
            leftCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
            leftCamera.backgroundColor = new Color(0.2f, 0, 0);
            leftCamera.clearFlags = CameraClearFlags.SolidColor;
            
            // 정면 카메라
            GameObject frontCamObj = new GameObject("FrontCamera");
            frontCamera = frontCamObj.AddComponent<Camera>();
            frontCamera.transform.position = frontCameraPosition;
            frontCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
            frontCamera.backgroundColor = new Color(0, 0.2f, 0);
            frontCamera.clearFlags = CameraClearFlags.SolidColor;
            
            // 우측 카메라
            GameObject rightCamObj = new GameObject("RightCamera");
            rightCamera = rightCamObj.AddComponent<Camera>();
            rightCamera.transform.position = rightCameraPosition;
            rightCamera.transform.rotation = Quaternion.Euler(0, -90, 0);
            rightCamera.backgroundColor = new Color(0, 0, 0.2f);
            rightCamera.clearFlags = CameraClearFlags.SolidColor;
            
            // 하단 카메라
            GameObject downCamObj = new GameObject("DownCamera");
            downCamera = downCamObj.AddComponent<Camera>();
            downCamera.transform.position = downCameraPosition;
            downCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            downCamera.backgroundColor = new Color(0.2f, 0.2f, 0);
            downCamera.clearFlags = CameraClearFlags.SolidColor;
            
            // 렌더 텍스처 생성 및 할당
            leftCamera.targetTexture = CreateRenderTexture("LeftRT", textureWidth, textureHeight);
            frontCamera.targetTexture = CreateRenderTexture("FrontRT", textureWidth, textureHeight);
            rightCamera.targetTexture = CreateRenderTexture("RightRT", textureWidth, textureHeight);
            downCamera.targetTexture = CreateRenderTexture("DownRT", textureWidth, textureHeight);
        }
        
        private void CreateDisplays()
        {
            // 결합된 디스플레이 생성
            combinedDisplay = GameObject.CreatePrimitive(PrimitiveType.Quad);
            combinedDisplay.name = "CombinedDisplay";
            combinedDisplay.transform.position = combinedDisplayPosition;
            combinedDisplay.transform.localScale = new Vector3(displayScale.x, displayScale.y, 1);
            combinedDisplay.layer = combinedDisplayLayer;
            
            // 하단 디스플레이 생성
            downDisplay = GameObject.CreatePrimitive(PrimitiveType.Quad);
            downDisplay.name = "DownDisplay";
            downDisplay.transform.position = downDisplayPosition;
            downDisplay.transform.localScale = new Vector3(displayScale.x / 2, displayScale.y / 2, 1);
            downDisplay.layer = downDisplayLayer;
        }
        
        private void CreateTextureCombiner()
        {
            // 텍스처 결합기 생성
            GameObject combinerObj = new GameObject("TextureCombiner");
            textureCombiner = combinerObj.AddComponent<TextureCombiner>();
            
            // 입력 텍스처 설정
            textureCombiner.leftTexture = leftCamera.targetTexture;
            textureCombiner.frontTexture = frontCamera.targetTexture;
            textureCombiner.rightTexture = rightCamera.targetTexture;
            textureCombiner.downTexture = downCamera.targetTexture;
            
            // 출력 텍스처 생성
            textureCombiner.combinedTexture = CreateRenderTexture("CombinedRT", combinedTextureWidth, combinedTextureHeight);
            
            // 셰이더 로드 및 머티리얼 생성
            Shader combinedShader = Shader.Find("Custom/CombinedTexture");
            if (combinedShader == null)
            {
                Debug.LogError("Custom/CombinedTexture 셰이더를 찾을 수 없습니다!");
                return;
            }
            
            // 머티리얼 생성
            textureCombiner.combinedMaterial = new Material(combinedShader);
            textureCombiner.downMaterial = new Material(Shader.Find("Unlit/Texture"));
            
            // 디스플레이 설정
            textureCombiner.combinedDisplayPlane = combinedDisplay;
            textureCombiner.downDisplayPlane = downDisplay;
            
            // 레이어 설정
            textureCombiner.combinedDisplayLayer = combinedDisplayLayer;
            textureCombiner.downDisplayLayer = downDisplayLayer;
            
            // 밝기 및 대비 설정
            textureCombiner.brightness = 1.1f;
            textureCombiner.contrast = 1.1f;
            
            // 자동 업데이트 활성화
            textureCombiner.autoUpdate = true;
        }
        
        private void CreateMainCameras()
        {
            // 메인 카메라 1 (결합된 디스플레이용)
            GameObject mainCam1Obj = new GameObject("MainCamera1");
            mainCamera1 = mainCam1Obj.AddComponent<Camera>();
            mainCamera1.transform.position = new Vector3(0, 1, 10);
            mainCamera1.transform.rotation = Quaternion.Euler(0, 180, 0);
            mainCamera1.clearFlags = CameraClearFlags.SolidColor;
            mainCamera1.backgroundColor = Color.black;
            
            // 메인 카메라 1은 결합된 디스플레이 레이어만 렌더링
            mainCamera1.cullingMask = 1 << combinedDisplayLayer;
            
            // 메인 카메라 2 (하단 디스플레이용)
            GameObject mainCam2Obj = new GameObject("MainCamera2");
            mainCamera2 = mainCam2Obj.AddComponent<Camera>();
            mainCamera2.transform.position = new Vector3(0, 1, 10);
            mainCamera2.transform.rotation = Quaternion.Euler(0, 180, 0);
            mainCamera2.clearFlags = CameraClearFlags.SolidColor;
            mainCamera2.backgroundColor = Color.black;
            
            // 메인 카메라 2는 하단 디스플레이 레이어만 렌더링
            mainCamera2.cullingMask = 1 << downDisplayLayer;
            
            // 멀티 디스플레이 설정
            if (Display.displays.Length > 1)
            {
                // 두 번째 디스플레이 활성화
                Display.displays[1].Activate();
                
                // 디스플레이 할당
                mainCamera1.targetDisplay = 0; // 첫 번째 모니터
                mainCamera2.targetDisplay = 1; // 두 번째 모니터
                
                Debug.Log($"멀티 디스플레이 활성화: {Display.displays.Length}개 감지됨");
            }
            else
            {
                Debug.LogWarning("멀티 디스플레이를 사용할 수 없습니다. 디스플레이가 하나만 감지되었습니다.");
                
                // 단일 디스플레이 모드에서는 메인 카메라 2를 비활성화
                mainCamera2.gameObject.SetActive(false);
            }
            
            // 씬 카메라 비활성화 (에디터에서만 작동)
#if UNITY_EDITOR
            Camera sceneCamera = GameObject.FindObjectOfType<Camera>();
            if (sceneCamera != null && sceneCamera != mainCamera1 && sceneCamera != mainCamera2)
            {
                sceneCamera.gameObject.SetActive(false);
            }
#endif
        }
        
        private void CreateTestObjects()
        {
            // 좌측 카메라 앞에 빨간 큐브 생성
            CreateColoredCube(leftCamera.transform.position + leftCamera.transform.forward * 3, Color.red);
            
            // 정면 카메라 앞에 녹색 구 생성
            CreateColoredSphere(frontCamera.transform.position + frontCamera.transform.forward * 3, Color.green);
            
            // 우측 카메라 앞에 파란 캡슐 생성
            CreateColoredCapsule(rightCamera.transform.position + rightCamera.transform.forward * 3, Color.blue);
            
            // 하단 카메라 아래에 노란 실린더 생성
            CreateColoredCylinder(new Vector3(0, 0, 0), Color.yellow);
        }
        
        private RenderTexture CreateRenderTexture(string name, int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 24);
            rt.name = name;
            return rt;
        }
        
        private void CreateColoredCube(Vector3 position, Color color)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;
        }
        
        private void CreateColoredSphere(Vector3 position, Color color)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            Renderer renderer = sphere.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;
        }
        
        private void CreateColoredCapsule(Vector3 position, Color color)
        {
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.position = position;
            Renderer renderer = capsule.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;
        }
        
        private void CreateColoredCylinder(Vector3 position, Color color)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.position = position;
            Renderer renderer = cylinder.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;
        }
    }
}
