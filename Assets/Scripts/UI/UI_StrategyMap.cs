using UnityEngine;

public class UI_StrategyMap : MonoBehaviour
{    public void OnLevelButtonClick(string levelName)
    {
        SceneLoaderManager.Instance.LoadTacticalLevel(levelName);
    }
}
