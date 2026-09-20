using UnityEngine;
using UnityEditor;

public class WordLearningDataGenerator
{
    private const string FolderPath =
        "Assets/Resources/Learning/Words";

    private static readonly string[] DisplayNames =
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

    // Bunlar B'nin mevcut model/dataset label isimleri.
    private static readonly string[] ModelLabels =
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

    private static readonly string[] FileNames =
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

    [MenuItem("WeaveSign/Generate Word Learning Data")]
    public static void GenerateWordData()
    {
        for (int i = 0; i < DisplayNames.Length; i++)
        {
            string assetPath =
                FolderPath +
                "/Word_" +
                (i + 1).ToString("00") +
                "_" +
                FileNames[i] +
                ".asset";

            LearningItemData item =
                AssetDatabase.LoadAssetAtPath<LearningItemData>(
                    assetPath
                );

            if (item == null)
            {
                item =
                    ScriptableObject.CreateInstance<LearningItemData>();

                AssetDatabase.CreateAsset(
                    item,
                    assetPath
                );
            }

            item.displayName =
                DisplayNames[i];

            item.modelLabel =
                ModelLabels[i];

            item.modelType =
                RecognitionModelType.Word;

            EditorUtility.SetDirty(item);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "20 kelime için LearningItemData oluşturuldu/güncellendi."
        );
    }
}