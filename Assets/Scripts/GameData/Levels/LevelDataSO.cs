using UnityEngine;

/// <summary>
/// ������ ���������� ������ �� �������������� �����.
/// ���������� �������� ������, ��� ����������� � ����� � ������� ��������.
/// </summary>
/// <remarks>
/// <para><b>�������������:</b> ���������� ScriptableObject ��� ������� ������ � ����
/// � ������������ ����������� �������� � ���������� Unity.</para>
/// </remarks>
[CreateAssetMenu(fileName = "Level_", menuName = "Iron Tide/Level Data")]
public class LevelDataSO : ScriptableObject
{
    /// <summary>
    /// ���������� ������������� ������.
    /// ������ ��������������� ����� ����� ������.
    /// </summary>
    [Tooltip("���������� ������������� ������ (������ ��������� � ������ �����).")]
    public string levelId;

    /// <summary>
    /// ������������ �������� ������ � UI.
    /// </summary>
    [Tooltip("������������ �������� ������ � ����������.")]
    public string displayName;

    /// <summary>
    /// �������� ������ ��� UI.
    /// </summary>
    [Tooltip("�������� ������, ������������ � ����������.")]
    [TextArea(2, 4)]
    public string description;

    /// <summary>
    /// ��������� ������ ��� UI.
    /// </summary>
    [Tooltip("��������� ������ ��� ����������� �� �����.")]
    public Sprite thumbnail;

    /// <summary>
    /// ������� �� ����������� ������ � ��������.
    /// </summary>
    [Tooltip("���������� ��������, ���������� �� ����������� ������.")]
    public int blueprintReward = 10;

    /// <summary>
    /// ������, ������� ������ ���� �������� ��� �������� ������� ������.
    /// ���� ������ ������ - ������� �������� � ������ ����.
    /// </summary>
    [Tooltip("������, ����������� ��� �������� ������� ������. �������� ������ ��� ���������� ������.")]
    public LevelDataSO[] requiredLevels;

    /// <summary>
    /// ������, ������� ����������� ����� ����������� ������� ������.
    /// </summary>
    [Tooltip("������, ������� ���������� ���������� ����� ����������� ������� ������.")]
    public LevelDataSO[] unlockedLevels;

    /// <summary>
    /// ������� ������ �� �������������� �����.
    /// </summary>
    [Tooltip("������� ������ �� �������������� ����� � ������������� �����������.")]
    public Vector2 mapPosition;

    /// <summary>
    /// ���������, �������� �� ������� ��� ������ �������.
    /// </summary>
    /// <param name="saveManager">�������� ���������� ��� �������� ���������.</param>
    /// <returns>true - ���� ������� ��������, false - ���� ������������.</returns>
    public bool IsLevelAvailable(SaveManager saveManager)
    {
        // ���� ��� ���������� - ������� ��������
        if (requiredLevels == null || requiredLevels.Length == 0)
            return true;

        // ���������, �������� �� ��� ��������� ������
        foreach (var requiredLevel in requiredLevels)
        {
            if (requiredLevel != null && !saveManager.IsLevelCompleted(requiredLevel.levelId))
                return false;
        }

        return true;
    }

    /// <summary>
    /// �������� ��������� ������ ��� ����������� � UI.
    /// </summary>
    /// <param name="saveManager">�������� ���������� ��� �������� ���������.</param>
    /// <returns>��������� ������.</returns>
    public LevelState GetLevelState(SaveManager saveManager)
    {
        if (saveManager.IsLevelCompleted(levelId))
            return LevelState.Completed;

        if (IsLevelAvailable(saveManager))
            return LevelState.Available;

        return LevelState.Locked;
    }
}