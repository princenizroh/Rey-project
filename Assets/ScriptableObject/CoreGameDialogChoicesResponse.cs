using UnityEngine;
[System.Serializable]
public class CoreGameDialogChoicesResponse
{
    public enum NpcName { Rey, Ibu, Ayah, Bidan, Hantu, None }
    public NpcName npcName;
    [TextArea(2, 5)]
    public string npcResponse;
}
