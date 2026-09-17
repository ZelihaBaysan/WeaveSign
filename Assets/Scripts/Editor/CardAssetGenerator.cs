using UnityEngine;
using UnityEditor;

public static class CardAssetGenerator
{
    private const string FolderPath = "Assets/ScriptableObjects/Cards";

    [MenuItem("WeaveSign/Generate Duel Cards")]
    public static void GenerateCards()
    {
        string[] displayNames =
        {
            "Anne",
            "Arkadaş",
            "Baba",
            "Dur",
            "Ev",
            "Evet",
            "Hayır",
            "Kardeş",
            "Merhaba",
            "Nasıl",
            "Nerede",
            "Özür Dilemek",
            "Tamam",
            "Telefon",
            "Teşekkürler",
            "Tuvalet",
            "Yemek",
            "İçmek",
            "İyi",
            "Kötü"
        };

        string[] modelLabels =
        {
            "Anne",
            "Arkadas",
            "Baba",
            "Dur",
            "Ev",
            "Evet",
            "Hayir",
            "Kardes",
            "Merhaba",
            "Nasil",
            "Nerede",
            "Ozur-Dilemek",
            "Tamam",
            "Telefon",
            "Tesekkurler",
            "Tuvalet",
            "Yemek",
            "icmek",
            "iyi",
            "kotu"
        };

        string[] fileNames =
        {
            "Anne",
            "Arkadas",
            "Baba",
            "Dur",
            "Ev",
            "Evet",
            "Hayir",
            "Kardes",
            "Merhaba",
            "Nasil",
            "Nerede",
            "Ozur_Dilemek",
            "Tamam",
            "Telefon",
            "Tesekkurler",
            "Tuvalet",
            "Yemek",
            "icmek",
            "iyi",
            "kotu"
        };

        for (int i = 0; i < displayNames.Length; i++)
        {
            string id = $"sign_{i + 1:000}";
            string assetPath = $"{FolderPath}/Sign_{i + 1:000}_{fileNames[i]}.asset";

            // Kart zaten varsa tekrar oluşturma.
            SpellCard existingCard =
                AssetDatabase.LoadAssetAtPath<SpellCard>(assetPath);

            if (existingCard != null)
            {
                Debug.Log($"Zaten var, atlandı: {assetPath}");
                continue;
            }

            SpellCard card = ScriptableObject.CreateInstance<SpellCard>();

            card.cardId = id;
            card.displayName = displayNames[i];
            card.modelLabel = modelLabels[i];

            // Bunları daha sonra kartlara göre dengeleyeceğiz.
            card.difficulty = 1;
            card.baseDamage = 10;
            card.duration = 5f;

            AssetDatabase.CreateAsset(card, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("WeaveSign: Düello kartları oluşturuldu!");
    }
}