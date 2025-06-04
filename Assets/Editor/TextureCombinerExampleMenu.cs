using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class TextureCombinerExampleMenu : Editor
{
    [MenuItem("AudioVisualizer/예제/텍스처 결합 예제 씬 생성")]
    static void CreateTextureCombinerExampleScene()
    {
        // 사용자에게 저장 여부 확인
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }
        
        // 새 씬 생성
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        
        // 기본 메인 카메라 비활성화 (예제에서 자체 카메라를 생성하기 때문)
        Camera defaultCamera = GameObject.FindObjectOfType<Camera>();
        if (defaultCamera != null)
        {
            defaultCamera.gameObject.SetActive(false);
        }
        
        // 예제 오브젝트 생성
        GameObject exampleObj = new GameObject("TextureCombinerExample");
        exampleObj.AddComponent<AudioVisualizer.Examples.TextureCombinerExample>();
        
        // 씬 저장 경로 설정
        string scenePath = "Assets/AudioVisualizer/Examples/TextureCombinerExample.unity";
        
        // Examples 폴더가 없으면 생성
        string dirPath = Path.GetDirectoryName(scenePath);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        
        // 씬 저장
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
        
        Debug.Log("텍스처 결합 예제 씬이 생성되었습니다: " + scenePath);
        
        // 프로젝트 창에서 씬 파일 선택
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(scenePath);
    }
} 