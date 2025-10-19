using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����-��������� ��� ���� ������ ���������� ������.
/// ������������� � JSON ��� �������� � PlayerPrefs.
/// </summary>
/// <remarks>
/// <para><b>��������� ������:</b> �������� ��� ����������� ������ ��� �������������� ��������� ����
/// ����� ��������. ��� ���� �������� ��� [Serializable] ��� ���������� ������ � JSON.</para>
/// </remarks>
[Serializable]
public class PlayerSaveData
{
    /// <summary>
    /// ������ ��������� ������ ����������.
    /// ������������ ��� �������� ������ ��� ����������� ����.
    /// </summary>
    public string saveVersion = "1.0.0";

    /// <summary>
    /// ������ ��������������� ������� ���������� �������.
    /// ������������ ��� ����������� ��������� ������� �� �������������� �����.
    /// </summary>
    /// <example>
    /// ������ �����������: ["Level01", "Level02", "Level03"]
    /// </example>
    public List<string> completedLevels = new List<string>();

    /// <summary>
    /// ������� ���������� �������� ������.
    /// ����: ResourceType (������������� � string)
    /// ��������: ���������� �������
    /// </summary>
    /// <example>
    /// ������ �����������: { "Blueprints": 25, "OtherCurrency": 100 }
    /// </example>
    public Dictionary<string, int> globalResources = new Dictionary<string, int>();

    /// <summary>
    /// ���� � ����� ���������� ����������.
    /// ������������ ��� ����������� ���������� � ���������� � UI.
    /// </summary>
    public string lastSaveTime;

    /// <summary>
    /// ����� ���������� ��������, ������������ �� ��� ����� ����.
    /// ������������ ��� ���������� � ����������.
    /// </summary>
    public int totalBlueprintsEarned;

    /// <summary>
    /// ������� ����� ��������� PlayerSaveData � ���������� ����������.
    /// </summary>
    public PlayerSaveData()
    {
        lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        totalBlueprintsEarned = 0;
    }

    /// <summary>
    /// ��������� ������� � ������ ����������, ���� ��� ��� ��� ���.
    /// </summary>
    /// <param name="levelId">������������� ������ ��� ����������.</param>
    /// <returns>true - ���� ������� ��� ��������, false - ���� ��� ��� � ������.</returns>
    public bool AddCompletedLevel(string levelId)
    {
        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogWarning("[PlayerSaveData] ������� �������� ������ ������������� ������");
            return false;
        }

        if (!completedLevels.Contains(levelId))
        {
            completedLevels.Add(levelId);
            return true;
        }
        return false;
    }

    /// <summary>
    /// ���������, ������� �� ��������� �������.
    /// </summary>
    /// <param name="levelId">������������� ������ ��� ��������.</param>
    /// <returns>true - ���� ������� �������, false - � ��������� ������.</returns>
    public bool IsLevelCompleted(string levelId)
    {
        return completedLevels.Contains(levelId);
    }

    /// <summary>
    /// ������������� ���������� ����������� �������.
    /// </summary>
    /// <param name="resourceType">��� �������.</param>
    /// <param name="amount">���������� �������.</param>
    public void SetGlobalResource(ResourceType resourceType, int amount)
    {
        string resourceKey = resourceType.ToString();
        globalResources[resourceKey] = amount;
    }

    /// <summary>
    /// �������� ���������� ����������� �������.
    /// </summary>
    /// <param name="resourceType">��� �������.</param>
    /// <returns>���������� �������. ���� ������ �� ������, ���������� 0.</returns>
    public int GetGlobalResource(ResourceType resourceType)
    {
        string resourceKey = resourceType.ToString();
        return globalResources.ContainsKey(resourceKey) ? globalResources[resourceKey] : 0;
    }

    /// <summary>
    /// ��������� ����� ���������� ����������.
    /// </summary>
    public void UpdateSaveTime()
    {
        lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}