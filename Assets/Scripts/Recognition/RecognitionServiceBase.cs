using System;
using System.Collections;
using UnityEngine;

public abstract class RecognitionServiceBase : MonoBehaviour
{
    public abstract IEnumerator Recognize(
        RecognitionRequest request,
        Action<RecognitionResult> onResult
    );
}