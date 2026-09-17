using UnityEngine;

public enum MovementType
{
    Static,
    Dynamic
}

public enum HandType
{
    OneHand,
    TwoHands
}

[CreateAssetMenu(fileName = "NewSpellCard", menuName = "WeaveSign/Spell Card")]
public class SpellCard : ScriptableObject
{
    public string cardId;
    public string displayName;
    public string modelLabel;

    [Range(1, 3)]
    public int difficulty = 1;

    public int baseDamage = 10;
    public float duration = 5f;

    public MovementType movementType;
    public HandType handType;

    public Sprite cardImage;
}