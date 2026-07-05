using UnityEngine;

[CreateAssetMenu(fileName = "SlimeTypeDatabase", menuName = "Huck Amok/Slime Type Database")]
public class SlimeTypeDatabase : ScriptableObject
{
    [SerializeField] private SlimeTypeDefinition[] definitions;

    public Color GetColor(SlimeType type)
    {
        if (definitions == null)
            return Color.white;

        foreach (var definition in definitions)
        {
            if (definition != null && definition.type == type)
                return definition.displayColor;
        }

        return Color.white;
    }
}
