using UnityEngine;

[CreateAssetMenu(
    fileName = "NewLearningItem",
    menuName = "WeaveSign/Learning Item"
)]
public class LearningItemData : ScriptableObject
{
    // Kullanıcıya gösterilecek isim
    public string displayName;

    // B'nin modelinden gelecek gerçek label
    public string modelLabel;

    // Harf mi, kelime mi?
    public RecognitionModelType modelType;

    // Eğitim ekranında gösterilecek el işareti görseli
    public Sprite signImage;
}