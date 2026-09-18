using System;
using System.Collections;
using UnityEngine;

public abstract class RecognitionServiceBase : MonoBehaviour
{
    public abstract IEnumerator Recognize(
        SpellCard targetCard,
        Action<RecognitionResult> onResult
    );
}