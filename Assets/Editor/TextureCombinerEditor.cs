using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextureCombiner))]
public class TextureCombinerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        TextureCombiner combiner = (TextureCombiner)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("유틸리티", EditorStyles.boldLabel);
        
        if (GUILayout.Button("머티리얼 생성"))
        {
            CreateMaterials(combiner);
        }
        
        if (GUILayout.Button("렌더 텍스처 생성"))
        {
            CreateRenderTextures(combiner);
        }
        
        if (GUILayout.Button("지금 텍스처 결합"))
        {
            combiner.CombineTextures();
        }
    }
    
    private void CreateMaterials(TextureCombiner combiner)
    {
        // 결합된 텍스처용 머티리얼 생성
        if (combiner.combinedMaterial == null)
        {
            // 셰이더 확인
            Shader combinedShader = Shader.Find("Custom/CombinedTexture");
            if (combinedShader == null)
            {
                Debug.LogError("Custom/CombinedTexture 셰이더를 찾을 수 없습니다!");
                return;
            }
            
            // 머티리얼 생성
            Material combinedMat = new Material(combinedShader);
            combinedMat.name = "CombinedTextureMaterial";
            
            // 머티리얼 저장
            string path = "Assets/AudioVisualizer/Materials";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/AudioVisualizer", "Materials");
            }
            
            AssetDatabase.CreateAsset(combinedMat, $"{path}/CombinedTextureMaterial.mat");
            AssetDatabase.SaveAssets();
            
            // 머티리얼 할당
            combiner.combinedMaterial = combinedMat;
            EditorUtility.SetDirty(combiner);
        }
        
        // 하단 텍스처용 머티리얼 생성
        if (combiner.downMaterial == null)
        {
            // 머티리얼 생성
            Material downMat = new Material(Shader.Find("Unlit/Texture"));
            downMat.name = "DownTextureMaterial";
            
            // 머티리얼 저장
            string path = "Assets/AudioVisualizer/Materials";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/AudioVisualizer", "Materials");
            }
            
            AssetDatabase.CreateAsset(downMat, $"{path}/DownTextureMaterial.mat");
            AssetDatabase.SaveAssets();
            
            // 머티리얼 할당
            combiner.downMaterial = downMat;
            EditorUtility.SetDirty(combiner);
        }
        
        Debug.Log("머티리얼이 생성되었습니다.");
    }
    
    private void CreateRenderTextures(TextureCombiner combiner)
    {
        // 결합된 텍스처 생성
        if (combiner.combinedTexture == null)
        {
            // 렌더 텍스처 생성
            RenderTexture combinedRT = new RenderTexture(3840, 1080, 24);
            combinedRT.name = "CombinedRenderTexture";
            
            // 렌더 텍스처 저장
            string path = "Assets/AudioVisualizer/RenderTextures";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/AudioVisualizer", "RenderTextures");
            }
            
            AssetDatabase.CreateAsset(combinedRT, $"{path}/CombinedRenderTexture.asset");
            AssetDatabase.SaveAssets();
            
            // 렌더 텍스처 할당
            combiner.combinedTexture = combinedRT;
            EditorUtility.SetDirty(combiner);
        }
        
        // 입력 텍스처 확인 및 생성
        CheckAndCreateRenderTexture(ref combiner.leftTexture, "LeftRenderTexture", 1280, 720);
        CheckAndCreateRenderTexture(ref combiner.frontTexture, "FrontRenderTexture", 1280, 720);
        CheckAndCreateRenderTexture(ref combiner.rightTexture, "RightRenderTexture", 1280, 720);
        CheckAndCreateRenderTexture(ref combiner.downTexture, "DownRenderTexture", 1280, 720);
        
        Debug.Log("렌더 텍스처가 생성되었습니다.");
    }
    
    private void CheckAndCreateRenderTexture(ref RenderTexture texture, string name, int width, int height)
    {
        if (texture == null)
        {
            // 렌더 텍스처 생성
            RenderTexture rt = new RenderTexture(width, height, 24);
            rt.name = name;
            
            // 렌더 텍스처 저장
            string path = "Assets/AudioVisualizer/RenderTextures";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/AudioVisualizer", "RenderTextures");
            }
            
            AssetDatabase.CreateAsset(rt, $"{path}/{name}.asset");
            AssetDatabase.SaveAssets();
            
            // 렌더 텍스처 할당
            texture = rt;
        }
    }
} 