using UnityEngine;

[CreateAssetMenu(fileName = "WaveDataSO", menuName = "Scriptable Objects/WaveDataSO")]
public class WaveDataSO : ScriptableObject
{
    public string waveId;
    public WaveEnemy[] enemiesInWave;
    public float nextWaveDelay;
}
