using UnityEngine;

public class SinCosForceFieldDirection : MonoBehaviour
{
    public ParticleSystemForceField forceField;

    public float frequencyX = 1f;
    public float frequencyY = 1.5f;
    public float frequencyZ = 2f;

    public float amplitudeX = 1f;
    public float amplitudeY = 1f;
    public float amplitudeZ = 1f;

    void Update()
    {
        float time = Time.time;

        float dirX = Mathf.Sin(time * frequencyX) * amplitudeX;
        float dirY = Mathf.Cos(time * frequencyY) * amplitudeY;
        float dirZ = Mathf.Sin(time * frequencyZ) * amplitudeZ;

        Vector3 dynamicDirection = new Vector3(dirX, dirY, dirZ).normalized;

        forceField.directionX = dynamicDirection.x;
        forceField.directionY = dynamicDirection.y;
        forceField.directionZ = dynamicDirection.z;
    }
} 