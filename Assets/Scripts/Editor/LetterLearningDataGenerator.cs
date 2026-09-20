using UnityEngine;
using UnityEditor;

public class LetterLearningDataGenerator
{
    private const string FolderPath =
        "Assets/Resources/Learning/Letters";

    private static readonly string[] DisplayNames =
    {
        "A", "B", "C", "Ç", "D", "E", "F",
        "G", "Ğ", "H", "I", "İ", "J", "K",
        "L", "M", "N", "O", "Ö", "P", "R",
        "S", "Ş", "T", "U", "Ü", "V", "Y", "Z"
    };

    // Dosya adlarında sorun çıkmaması için güvenli isimler.
    private static readonly string[] FileNames =
    {
        "A",
        "B",
        "C",
        "C_Cedilla",
        "D",
        "E",
        "F",
        "G",
        "G_Breve",
        "H",
        "I_Dotless",
        "I_Dotted",
        "J",
        "K",
        "L",
        "M",
        "N",
        "O",
        "O_Umlaut",
        "P",
        "R",
        "S",
        "S_Cedilla",
        "T",
        "U",
        "U_Umlaut",
        "V",
        "Y",
        "Z"
    };

    [MenuItem("WeaveSign/Generate Letter Learning Data")]
    public static void GenerateLetterData()
    {
        for (int i = 0; i < DisplayNames.Length; i++)
        {
            string assetPath =
                FolderPath +
                "/Letter_" +
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

            // Şimdilik model label = gösterilen harf.
            // B'nin dataset label isimleri kesinleşince
            // sadece bu alanları güncelleyeceğiz.
            item.modelLabel =
                DisplayNames[i];

            item.modelType =
                RecognitionModelType.Letter;

            EditorUtility.SetDirty(item);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "29 harf için LearningItemData oluşturuldu/güncellendi."
        );
    }
}