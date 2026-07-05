using UnityEngine;

[CreateAssetMenu(fileName = "SlimeTypeDefinition", menuName = "Huck Amok/Slime Type Definition")]
public class SlimeTypeDefinition : ScriptableObject
{
    public SlimeType type;
    public Color displayColor = Color.white;
}
