using Unity.Mathematics;
using UnityEngine;

public class Floater : MonoBehaviour
{
    [field: SerializeField]
    private Rigidbody Rb { get; set; }

    [field: SerializeField]
    private float DepthBeforeSub { get; set; } = 1f;

    [field: SerializeField]
    private float DisplacementAmount { get; set; } = 3f;

    [field: SerializeField]
    private float WaterDrag { get; set; } = .99f;

    [field: SerializeField]
    private float WaterAngularDrag { get; set; } = .5f;

    [field: SerializeField]
    private int FloatersCount { get; set; } = 1;

    [field: SerializeField]
    private Material Waves { get; set; }

    void FixedUpdate()
    {
        float waveHeight = GetWaveHeight(transform.position);
        
        // Força da gravidade
        Rb.AddForceAtPosition(Physics.gravity / FloatersCount, transform.position, ForceMode.Acceleration);

        // Se o objeto estiver abaixo da superfície da água
        if (transform.position.y < waveHeight)
        {
            float displacementMult = Mathf.Clamp01((waveHeight - transform.position.y) / DepthBeforeSub) * DisplacementAmount;

            // Empuxo
            Rb.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMult, 0f),
                                  transform.position,
                                  ForceMode.Acceleration);

            // Arrasto e torque
            Rb.AddForce(displacementMult * -Rb.linearVelocity * WaterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            Rb.AddTorque(displacementMult * -Rb.angularVelocity * WaterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    float GetWaveHeight(Vector3 worldPos)
    {
        // Lê parâmetros do material
        Vector4 waveA = Waves.GetVector("_WaveA");
        Vector4 waveB = Waves.GetVector("_WaveB");
        Vector4 waveC = Waves.GetVector("_WaveC");

        float time = Time.time;

        // Soma as alturas das três ondas
        float y = 0f;
        y += GerstnerWaveY(waveA, worldPos, time);
        y += GerstnerWaveY(waveB, worldPos, time);
        y += GerstnerWaveY(waveC, worldPos, time);

        return y;
    }


    float GerstnerWaveY(Vector4 wave, Vector3 worldPos, float time)
    {
        float steepness = wave.z;
        float wavelength = wave.w;
        float k = 2 * Mathf.PI / wavelength;
        float c = Mathf.Sqrt(9.8f / k);
        Vector2 d = new Vector2(wave.x, wave.y).normalized;
        float f = k * (Vector2.Dot(d, new Vector2(worldPos.x, worldPos.z)) - c * time);
        float a = steepness / k;
        return a * Mathf.Sin(f);
    }

}
