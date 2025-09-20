using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResearchState", menuName = "Research System/Research State")]
public class ResearchStateSO : ScriptableObject
{
    [System.Serializable]
    public class ResearchStateData
    {
        public string nodeId;
        public ResearchNodeState state;
    }

    [System.Serializable]
    public class ResearchSaveData
    {
        public List<ResearchStateData> researchedNodes = new List<ResearchStateData>();
    }

    public List<ResearchStateData> researchedNodes = new List<ResearchStateData>();

    public void SetNodeState(string nodeId, ResearchNodeState state)
    {
        ResearchStateData existing = researchedNodes.Find(n => n.nodeId == nodeId);
        if (existing != null)
        {
            existing.state = state;
        }
        else
        {
            researchedNodes.Add(new ResearchStateData { nodeId = nodeId, state = state });
        }

        // Автосохранение при изменении
        SaveState();
    }

    public ResearchNodeState GetNodeState(string nodeId)
    {
        ResearchStateData data = researchedNodes.Find(n => n.nodeId == nodeId);
        return data?.state ?? ResearchNodeState.Locked;
    }

    public void SaveState()
    {
        ResearchSaveData saveData = new ResearchSaveData { researchedNodes = researchedNodes };
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("ResearchState", json);
        Debug.Log("Research state saved");
    }

    public void LoadState()
    {
        if (PlayerPrefs.HasKey("ResearchState"))
        {
            string json = PlayerPrefs.GetString("ResearchState");
            ResearchSaveData saveData = JsonUtility.FromJson<ResearchSaveData>(json);
            if (saveData != null)
            {
                researchedNodes = saveData.researchedNodes;
                Debug.Log("Research state loaded");
            }
        }
        else
        {
            Debug.Log("No research state found - initializing new");
            researchedNodes = new List<ResearchStateData>();
        }
    }

    public void ResetState()
    {
        researchedNodes.Clear();
        PlayerPrefs.DeleteKey("ResearchState");
        Debug.Log("Research state reset");
    }
}