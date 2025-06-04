using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(ProjectionSetup))]
public class ProjectionSetupEditor : Editor
{
    private bool showCameraSettings = true;
    private bool showRenderTextureSettings = true;
    private bool showMainCameraSettings = true;
    private bool showMultiDisplaySettings = true;
    private bool showTextureSettings = true;
    private bool showRoomSettings = true;
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        ProjectionSetup setup = (ProjectionSetup)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("프로젝션 매핑 설정", EditorStyles.boldLabel);
        
        // 빠른 설정 버튼들
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("렌더 텍스처 생성", GUILayout.Height(30)))
        {
            CreateRenderTextures(setup);
        }
        
        if (GUILayout.Button("머티리얼 생성", GUILayout.Height(30)))
        {
            CreateMaterials(setup);
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("전체 설정 적용", GUILayout.Height(40)))
        {
            ApplyAllSettings(setup);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        // 카메라 설정
        showCameraSettings = EditorGUILayout.Foldout(showCameraSettings, "카메라 설정", true, EditorStyles.foldoutHeader);
        if (showCameraSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frontCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("leftCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rightCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("downCamera"));
            
            EditorGUI.indentLevel--;
        }
        
        // 렌더 텍스처 설정
        showRenderTextureSettings = EditorGUILayout.Foldout(showRenderTextureSettings, "렌더 텍스처 설정", true, EditorStyles.foldoutHeader);
        if (showRenderTextureSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frontTexture"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("leftTexture"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rightTexture"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("downTexture"));
            
            EditorGUI.indentLevel--;
        }
        
        // 메인 카메라 설정
        showMainCameraSettings = EditorGUILayout.Foldout(showMainCameraSettings, "메인 카메라 설정", true, EditorStyles.foldoutHeader);
        if (showMainCameraSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera1"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera2"));
            
            EditorGUI.indentLevel--;
        }
        
        // 멀티 디스플레이 설정
        showMultiDisplaySettings = EditorGUILayout.Foldout(showMultiDisplaySettings, "멀티 디스플레이 설정", true, EditorStyles.foldoutHeader);
        if (showMultiDisplaySettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useMultipleDisplays"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera1Display"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera2Display"));
            
            EditorGUI.indentLevel--;
        }
        
        // 텍스처 결합 설정
        showTextureSettings = EditorGUILayout.Foldout(showTextureSettings, "텍스처 결합 설정", true, EditorStyles.foldoutHeader);
        if (showTextureSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useCombinedTexture"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("combinedTexture"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("combinedMaterial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("downMaterial"));
            
            EditorGUI.indentLevel--;
        }
        
        // 방 설정
        showRoomSettings = EditorGUILayout.Foldout(showRoomSettings, "방 설정", true, EditorStyles.foldoutHeader);
        if (showRoomSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("roomSize"));
            
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();
        
        // 변경 사항이 있으면 씬을 더티로 표시
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
    
    private void CreateRenderTextures(ProjectionSetup setup)
    {
        // 렌더 텍스처 폴더 확인 및 생성
        string path = "Assets/AudioVisualizer/RenderTextures";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/AudioVisualizer", "RenderTextures");
        }
        
        // 정면 렌더 텍스처 생성
        if (setup.frontTexture == null)
        {
            RenderTexture frontRT = new RenderTexture(1280, 720, 24);
            frontRT.name = "FrontRenderTexture";
            AssetDatabase.CreateAsset(frontRT, $"{path}/FrontRenderTexture.asset");
            setup.frontTexture = frontRT;
        }
        
        // 좌측 렌더 텍스처 생성
        if (setup.leftTexture == null)
        {
            RenderTexture leftRT = new RenderTexture(1280, 720, 24);
            leftRT.name = "LeftRenderTexture";
            AssetDatabase.CreateAsset(leftRT, $"{path}/LeftRenderTexture.asset");
            setup.leftTexture = leftRT;
        }
        
        // 우측 렌더 텍스처 생성
        if (setup.rightTexture == null)
        {
            RenderTexture rightRT = new RenderTexture(1280, 720, 24);
            rightRT.name = "RightRenderTexture";
            AssetDatabase.CreateAsset(rightRT, $"{path}/RightRenderTexture.asset");
            setup.rightTexture = rightRT;
        }
        
        // 하단 렌더 텍스처 생성
        if (setup.downTexture == null)
        {
            RenderTexture downRT = new RenderTexture(1280, 720, 24);
            downRT.name = "DownRenderTexture";
            AssetDatabase.CreateAsset(downRT, $"{path}/DownRenderTexture.asset");
            setup.downTexture = downRT;
        }
        
        // 결합 렌더 텍스처 생성
        if (setup.combinedTexture == null)
        {
            RenderTexture combinedRT = new RenderTexture(3840, 1080, 24);
            combinedRT.name = "CombinedRenderTexture";
            AssetDatabase.CreateAsset(combinedRT, $"{path}/CombinedRenderTexture.asset");
            setup.combinedTexture = combinedRT;
        }
        
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(setup);
        
        Debug.Log("렌더 텍스처가 생성되었습니다.");
    }
    
    private void CreateMaterials(ProjectionSetup setup)
    {
        // 머티리얼 폴더 확인 및 생성
        string path = "Assets/AudioVisualizer/Materials";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/AudioVisualizer", "Materials");
        }
        
        // 결합 텍스처 머티리얼 생성
        if (setup.combinedMaterial == null)
        {
            Shader combinedShader = Shader.Find("Custom/CombinedTexture");
            if (combinedShader != null)
            {
                Material combinedMat = new Material(combinedShader);
                combinedMat.name = "CombinedTextureMaterial";
                AssetDatabase.CreateAsset(combinedMat, $"{path}/CombinedTextureMaterial.mat");
                setup.combinedMaterial = combinedMat;
            }
            else
            {
                Debug.LogError("Custom/CombinedTexture 셰이더를 찾을 수 없습니다!");
            }
        }
        
        // 하단 텍스처 머티리얼 생성
        if (setup.downMaterial == null)
        {
            Material downMat = new Material(Shader.Find("Unlit/Texture"));
            downMat.name = "DownTextureMaterial";
            AssetDatabase.CreateAsset(downMat, $"{path}/DownTextureMaterial.mat");
            setup.downMaterial = downMat;
        }
        
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(setup);
        
        Debug.Log("머티리얼이 생성되었습니다.");
    }
    
    private void ApplyAllSettings(ProjectionSetup setup)
    {
        // 렌더 텍스처 생성
        CreateRenderTextures(setup);
        
        // 머티리얼 생성
        CreateMaterials(setup);
        
        // 카메라에 렌더 텍스처 할당
        if (setup.frontCamera != null && setup.frontTexture != null)
            setup.frontCamera.targetTexture = setup.frontTexture;
            
        if (setup.leftCamera != null && setup.leftTexture != null)
            setup.leftCamera.targetTexture = setup.leftTexture;
            
        if (setup.rightCamera != null && setup.rightTexture != null)
            setup.rightCamera.targetTexture = setup.rightTexture;
            
        if (setup.downCamera != null && setup.downTexture != null)
            setup.downCamera.targetTexture = setup.downTexture;
        
        // 메인 카메라 디스플레이 설정
        if (setup.useMultipleDisplays)
        {
            if (setup.mainCamera1 != null)
                setup.mainCamera1.targetDisplay = setup.mainCamera1Display;
                
            if (setup.mainCamera2 != null)
                setup.mainCamera2.targetDisplay = setup.mainCamera2Display;
        }
        
        // 텍스처 결합 설정
        if (setup.useCombinedTexture)
        {
            // TextureCombiner 컴포넌트 확인 및 생성
            TextureCombiner combiner = setup.GetComponent<TextureCombiner>();
            if (combiner == null)
                combiner = setup.gameObject.AddComponent<TextureCombiner>();
                
            // TextureCombiner 설정
            combiner.leftTexture = setup.leftTexture;
            combiner.frontTexture = setup.frontTexture;
            combiner.rightTexture = setup.rightTexture;
            combiner.downTexture = setup.downTexture;
            combiner.combinedTexture = setup.combinedTexture;
            combiner.combinedMaterial = setup.combinedMaterial;
            combiner.downMaterial = setup.downMaterial;
            combiner.autoUpdate = true;
            
            EditorUtility.SetDirty(combiner);
        }
        
        // 레이어 확인 및 생성
        CheckAndCreateLayer("CombinedDisplay", 8);
        CheckAndCreateLayer("DownDisplay", 9);
        
        EditorUtility.SetDirty(setup);
        
        Debug.Log("모든 설정이 적용되었습니다.");
    }
    
    private void CheckAndCreateLayer(string layerName, int layerIndex)
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
    
    // 씬 뷰에서 추가 정보 표시
    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    static void DrawGizmosSelected(ProjectionSetup setup, GizmoType gizmoType)
    {
        // 방 크기 시각화
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        Gizmos.DrawWireCube(Vector3.zero, setup.roomSize);
        
        // 카메라 위치 및 방향 시각화
        DrawCameraGizmo(setup.frontCamera, Color.green);
        DrawCameraGizmo(setup.leftCamera, Color.red);
        DrawCameraGizmo(setup.rightCamera, Color.blue);
        DrawCameraGizmo(setup.downCamera, Color.yellow);
    }
    
    static void DrawCameraGizmo(Camera camera, Color color)
    {
        if (camera == null) return;
        
        Gizmos.color = color;
        Gizmos.DrawSphere(camera.transform.position, 0.2f);
        Gizmos.DrawRay(camera.transform.position, camera.transform.forward * 2);
    }
} 