[System.Serializable]
public class ResourceCost
{
    public ResourceType resourceType;
    public int amount;

    public ResourceCost(ResourceType type, int cost)
    {
        resourceType = type;
        amount = cost;
    }
}