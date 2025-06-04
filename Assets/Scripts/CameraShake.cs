
// 추가 컴포넌트: 카메라 흔들림 효과
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private float shakeAmount = 0;
    private float shakeDuration = 0;
    
    void Start()
    {
        originalPosition = transform.localPosition;
    }
    
    void Update()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition = originalPosition + Random.insideUnitSphere * shakeAmount;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            shakeDuration = 0;
            transform.localPosition = originalPosition;
        }
    }
    
    public void TriggerShake(float duration, float amount)
    {
        shakeDuration = duration;
        shakeAmount = amount;
    }
}