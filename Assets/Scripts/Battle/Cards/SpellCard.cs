using UnityEngine;

public enum CardType
{
    Attack,
    Heal
}

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

    public CardType cardType;

    [Range(1, 3)]
    public int difficulty = 1;

    public int basePower = 10;

    public MovementType movementType;
    public HandType handType;

    public Sprite cardImage;
}