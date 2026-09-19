using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CardAssetGenerator
{
    private const string FolderPath =
        "Assets/Resources/Cards";

    private class CardConfig
    {
        public string id;
        public string displayName;
        public string modelLabel;

        public CardType cardType;

        public int difficulty;
        public int power;

        public CardConfig(
            string id,
            string displayName,
            string modelLabel,
            CardType cardType,
            int difficulty,
            int power
        )
        {
            this.id = id;
            this.displayName = displayName;
            this.modelLabel = modelLabel;
            this.cardType = cardType;
            this.difficulty = difficulty;
            this.power = power;
        }
    }

    [MenuItem(
        "WeaveSign/Update Duel Cards"
    )]
    public static void UpdateCards()
    {
        CardConfig[] configs =
        {
            new CardConfig(
                "sign_001",
                "Anne",
                "Anne",
                CardType.Heal,
                1,
                6
            ),

            new CardConfig(
                "sign_002",
                "Arkadaş",
                "Arkadas",
                CardType.Heal,
                2,
                9
            ),

            new CardConfig(
                "sign_003",
                "Baba",
                "Baba",
                CardType.Heal,
                1,
                6
            ),

            new CardConfig(
                "sign_004",
                "Dur",
                "Dur",
                CardType.Attack,
                1,
                10
            ),

            new CardConfig(
                "sign_005",
                "Ev",
                "Ev",
                CardType.Heal,
                1,
                6
            ),

            new CardConfig(
                "sign_006",
                "Evet",
                "Evet",
                CardType.Attack,
                1,
                10
            ),

            new CardConfig(
                "sign_007",
                "Hayır",
                "Hayir",
                CardType.Attack,
                2,
                15
            ),

            new CardConfig(
                "sign_008",
                "Kardeş",
                "Kardes",
                CardType.Heal,
                2,
                9
            ),

            new CardConfig(
                "sign_009",
                "Merhaba",
                "Merhaba",
                CardType.Heal,
                1,
                6
            ),

            new CardConfig(
                "sign_010",
                "Nasıl",
                "Nasil",
                CardType.Attack,
                2,
                15
            ),

            new CardConfig(
                "sign_011",
                "Nerede",
                "Nerede",
                CardType.Attack,
                2,
                15
            ),

            new CardConfig(
                "sign_012",
                "Özür Dilemek",
                "Ozur-Dilemek",
                CardType.Heal,
                3,
                12
            ),

            new CardConfig(
                "sign_013",
                "Tamam",
                "Tamam",
                CardType.Attack,
                1,
                10
            ),

            new CardConfig(
                "sign_014",
                "Telefon",
                "Telefon",
                CardType.Attack,
                3,
                20
            ),

            new CardConfig(
                "sign_015",
                "Teşekkürler",
                "Tesekkurler",
                CardType.Heal,
                2,
                9
            ),

            new CardConfig(
                "sign_016",
                "Tuvalet",
                "Tuvalet",
                CardType.Attack,
                3,
                20
            ),

            new CardConfig(
                "sign_017",
                "Yemek",
                "Yemek",
                CardType.Heal,
                3,
                12
            ),

            new CardConfig(
                "sign_018",
                "İçmek",
                "icmek",
                CardType.Heal,
                3,
                12
            ),

            new CardConfig(
                "sign_019",
                "İyi",
                "iyi",
                CardType.Heal,
                1,
                6
            ),

            new CardConfig(
                "sign_020",
                "Kötü",
                "kotu",
                CardType.Attack,
                3,
                20
            )
        };

        string[] guids =
            AssetDatabase.FindAssets(
                "t:SpellCard",
                new[] { FolderPath }
            );

        Dictionary<string, SpellCard>
            existingCards =
            new Dictionary<string, SpellCard>();

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(
                    guid
                );

            SpellCard card =
                AssetDatabase.LoadAssetAtPath
                <SpellCard>(path);

            if (
                card != null &&
                !string.IsNullOrEmpty(
                    card.modelLabel
                )
            )
            {
                existingCards[
                    card.modelLabel
                ] = card;
            }
        }

        foreach (
            CardConfig config in configs
        )
        {
            SpellCard card;

            if (
                existingCards.ContainsKey(
                    config.modelLabel
                )
            )
            {
                card =
                    existingCards[
                        config.modelLabel
                    ];
            }
            else
            {
                card =
                    ScriptableObject
                    .CreateInstance
                    <SpellCard>();

                string newPath =
                    FolderPath +
                    "/" +
                    config.id +
                    ".asset";

                AssetDatabase.CreateAsset(
                    card,
                    newPath
                );
            }

            card.cardId =
                config.id;

            card.displayName =
                config.displayName;

            card.modelLabel =
                config.modelLabel;

            card.cardType =
                config.cardType;

            card.difficulty =
                config.difficulty;

            card.basePower =
                config.power;

            EditorUtility.SetDirty(card);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "20 düello kartı güncellendi!"
        );
    }
}