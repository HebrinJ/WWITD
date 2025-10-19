using System.Collections;
using UnityEngine;

/// <summary>
/// ��������, ���������� �� ���������� ������� ��������� �� ����������� ������.
/// ������������ ������� ������ � ���������, ������������ ���� �� ���� � ��������� ��������������� ������� ���������� ������.
/// �������� ����������� �����, ����������� ������������� ������� � ���������� ������.
/// </summary>
/// <remarks>
/// <para><b>���� � �����������:</b> ���������� �������� ������� ������ (����, �����) � ��������� �� �� �������
/// ��� ����������� ������ ������ (������/���������). ������ ���� �������� ����� ������������ ���������� ������.</para>
/// </remarks>
public class LevelManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("������ �� ��������� ����� (�������� ������), ������� ���������� ��������.")]
    private Headquarters headquarters;

    [SerializeField]
    [Tooltip("������ �� �������� ��������� �������� ������.")]
    private LocalResourceManager localResourceManager;

    /// <summary>
    /// ����, �����������, ��� ���� �� ������ ��������� (������ ��� ���������).
    /// ������������� ������������� ��������� ������� ����������.
    /// </summary>
    private bool isGameOver = false;

    private void OnEnable()
    {
        // ������������� �� ������� ���������� ������ ����
        EventHub.OnEnemyReachedBase += HandleEnemyReachedBase;

        // ������������� �� ������� ����������� �����
        if (headquarters != null)
        {
            headquarters.OnHeadquartersDestroyed += HandleGameOver;
        }

        // ������������� �� ������� ���������� ���� ����
        EventHub.OnAllWavesCompleted += HandleAllWavesCompleted;        
    }

    private void OnDisable()
    {
        EventHub.OnEnemyReachedBase -= HandleEnemyReachedBase;

        if (headquarters != null)
        {
            headquarters.OnHeadquartersDestroyed -= HandleGameOver;
        }

        EventHub.OnAllWavesCompleted -= HandleAllWavesCompleted;
    }

    private void Start()
    {
        Debug.Log($"[LevelManager] ������� �������: {gameObject.scene.name}");
    }

    /// <summary>
    /// ���������� ������� ���������� ������ �����. ������� ���� ����� �� ������ ������ �����.
    /// </summary>
    /// <param name="enemy">��������� �����, ���������� ����. �������� ������ ��� ������� �����.</param>
    private void HandleEnemyReachedBase(EnemyBehaviour enemy)
    {
        // ������ �� ��������� ����� ���������� ���� ��� ���� ���� null
        if (isGameOver || enemy == null) return;

        // ������� ���� �����, ��������� �������� damageToBase �� ������ �����
        headquarters?.TakeDamage(enemy.Data.damageToBase);
    }

    /// <summary>
    /// ���������� ������� ����������� �����. ��������� �������� ���������.
    /// </summary>
    private void HandleGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("[LevelManager] ������� ��������! ���� ���������.");
        // ��������� ��� ������� � ���������
        EventHub.OnGameOver?.Invoke();

        // ����� 3 ������� ������������ �� ����� (��������� �������)
        StartCoroutine(ReturnToStrategyMapAfterDelay(3f));
    }

    /// <summary>
    /// ���������� ������� ���������� ���� ����. ��������� �������� ������.
    /// </summary>
    private void HandleAllWavesCompleted()
    {
        if (isGameOver) return;

        Debug.Log("[LevelManager] ��� ����� ��������! ������� �������� �������.");
        CompleteLevel(true); // ��������� �������� ������
    }

    /// <summary>
    /// ��������������� �������� ��� �������� �� �������������� ����� ����� ��������.
    /// </summary>
    /// <param name="delay">�������� � �������� ����� ���������.</param>
    /// <returns>IEnumerator ��� ������ ��������.</returns>
    private IEnumerator ReturnToStrategyMapAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // ���������� �������� ���� ��� �������� �� �����
        SceneLoaderManager.Instance.LoadStrategyMap();
    }

    /// <summary>
    /// �������� ����� ��� ���������� ������. ���������� ��� ������ ��� ���������.
    /// </summary>
    /// <param name="victory">true - ��������� ������, false - ��������� ���������.</param>
    public void CompleteLevel(bool victory)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (victory)
        {
            Debug.Log("[LevelManager] ������� ������� �������!");

            // ��������� ����� EventHub � ���������� ������
            string levelId = gameObject.scene.name; // ��� ����������� ����������� �������������
            EventHub.OnLevelCompleted?.Invoke(levelId);

            // ��������� ��� ������� � ������
            EventHub.OnLevelComplete?.Invoke();
        }

        // ������������ �� ����� ����� ��������
        StartCoroutine(ReturnToStrategyMapAfterDelay(3f));
    }
}