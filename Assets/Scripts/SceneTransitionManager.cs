using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneTransitionManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneTransitionCondition
    {
        public string sceneName;
        public string requiredItem; // 필요한 아이템이나 조건
        public bool isUnlocked;
    }

    public List<SceneTransitionCondition> sceneTransitions = new List<SceneTransitionCondition>();
    public string currentSceneName;

    private void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    // 특정 씬으로 이동
    public void TransitionToScene(string targetSceneName)
    {
        SceneTransitionCondition transition = sceneTransitions.Find(x => x.sceneName == targetSceneName);
        
        if (transition != null)
        {
            if (transition.isUnlocked)
            {
                SceneManager.LoadScene(targetSceneName);
                Debug.Log($"씬 전환: {targetSceneName}으로 이동합니다.");
            }
            else
            {
                Debug.Log($"씬 전환 실패: {targetSceneName}으로 이동하기 위한 조건이 충족되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"씬 전환 실패: {targetSceneName}에 대한 전환 조건이 설정되지 않았습니다.");
        }
    }

    // 씬 잠금 해제
    public void UnlockScene(string sceneName)
    {
        SceneTransitionCondition transition = sceneTransitions.Find(x => x.sceneName == sceneName);
        if (transition != null)
        {
            transition.isUnlocked = true;
            Debug.Log($"씬 잠금 해제: {sceneName}이(가) 이제 접근 가능합니다.");
        }
    }

    // 다음 씬으로 이동
    public void GoToNextScene()
    {
        int currentIndex = sceneTransitions.FindIndex(x => x.sceneName == currentSceneName);
        if (currentIndex >= 0 && currentIndex < sceneTransitions.Count - 1)
        {
            string nextScene = sceneTransitions[currentIndex + 1].sceneName;
            TransitionToScene(nextScene);
        }
        else
        {
            Debug.Log("마지막 씬입니다!");
        }
    }

    // 이전 씬으로 이동
    public void GoToPreviousScene()
    {
        int currentIndex = sceneTransitions.FindIndex(x => x.sceneName == currentSceneName);
        if (currentIndex > 0)
        {
            string previousScene = sceneTransitions[currentIndex - 1].sceneName;
            TransitionToScene(previousScene);
        }
        else
        {
            Debug.Log("첫 번째 씬입니다!");
        }
    }
} 