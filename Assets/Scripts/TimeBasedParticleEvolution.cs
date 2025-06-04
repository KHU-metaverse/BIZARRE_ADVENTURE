using UnityEngine;
using System.Collections.Generic;

public class TimeBasedParticleEvolution : MonoBehaviour
{
    [System.Serializable]
    public class ParticleEvolutionState
    {
        public float time;                // 이 상태에 도달하는 시간(초)
        public Color particleColor;       // 입자 색상
        public float particleSize;        // 입자 크기
        public float emissionRate;        // 방출 속도
        public float particleLifetime;    // 입자 수명
    }
    
    public List<ParticleEvolutionState> evolutionStates = new List<ParticleEvolutionState>();
    public ParticleSystem[] particleSystems;
    
    private float startTime;
    private int currentStateIndex = 0;
    
    void Start()
    {
        startTime = Time.time;
        
        // 시간 순으로 상태 정렬
        evolutionStates.Sort((a, b) => a.time.CompareTo(b.time));
    }
    
    void Update()
    {
        float elapsedTime = Time.time - startTime;
        
        // 다음 상태로 전환해야 하는지 확인
        if (currentStateIndex < evolutionStates.Count - 1 && 
            elapsedTime >= evolutionStates[currentStateIndex + 1].time)
        {
            currentStateIndex++;
        }
        
        // 현재 상태와 다음 상태 사이 보간
        if (currentStateIndex < evolutionStates.Count - 1)
        {
            float currentStateTime = evolutionStates[currentStateIndex].time;
            float nextStateTime = evolutionStates[currentStateIndex + 1].time;
            float t = Mathf.InverseLerp(currentStateTime, nextStateTime, elapsedTime);
            
            // 두 상태 사이 값 보간
            UpdateParticleProperties(
                Mathf.Lerp(evolutionStates[currentStateIndex].particleSize, 
                          evolutionStates[currentStateIndex + 1].particleSize, t),
                Color.Lerp(evolutionStates[currentStateIndex].particleColor, 
                          evolutionStates[currentStateIndex + 1].particleColor, t),
                Mathf.Lerp(evolutionStates[currentStateIndex].emissionRate, 
                          evolutionStates[currentStateIndex + 1].emissionRate, t),
                Mathf.Lerp(evolutionStates[currentStateIndex].particleLifetime, 
                          evolutionStates[currentStateIndex + 1].particleLifetime, t)
            );
        }
        else if (currentStateIndex < evolutionStates.Count)
        {
            // 마지막 상태 적용
            UpdateParticleProperties(
                evolutionStates[currentStateIndex].particleSize,
                evolutionStates[currentStateIndex].particleColor,
                evolutionStates[currentStateIndex].emissionRate,
                evolutionStates[currentStateIndex].particleLifetime
            );
        }
    }
    
    void UpdateParticleProperties(float size, Color color, float emissionRate, float lifetime)
    {
        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;
            main.startSize = size;
            main.startColor = color;
            main.startLifetime = lifetime;
            
            var emission = ps.emission;
            emission.rateOverTime = emissionRate;
        }
    }
    
    // 필요시 시퀀스 재시작
    public void RestartSequence()
    {
        startTime = Time.time;
        currentStateIndex = 0;
    }
}