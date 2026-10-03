from pathlib import Path
import base64
import cv2
import joblib
import mediapipe as mp
import numpy as np

from fastapi import FastAPI
from pydantic import BaseModel


PROJECT_ROOT = Path(__file__).resolve().parent.parent

LETTER_MODEL_PATH = (
    PROJECT_ROOT
    / "models"
    / "alphabet_random_forest_with_cedilla.pkl"
)

WORD_MODEL_PATH = (
    PROJECT_ROOT
    / "models"
    / "word_random_forest.pkl"
)

HAND_MODEL_PATH = (
    PROJECT_ROOT
    / "models"
    / "hand_landmarker.task"
)


letter_model = joblib.load(
    LETTER_MODEL_PATH
)

letter_model.n_jobs = 1


word_model = joblib.load(
    WORD_MODEL_PATH
)

word_model.n_jobs = 1


BaseOptions = mp.tasks.BaseOptions
HandLandmarker = mp.tasks.vision.HandLandmarker
HandLandmarkerOptions = mp.tasks.vision.HandLandmarkerOptions
RunningMode = mp.tasks.vision.RunningMode


hand_options = HandLandmarkerOptions(
    base_options=BaseOptions(
        model_asset_path=str(
            HAND_MODEL_PATH
        )
    ),
    running_mode=RunningMode.IMAGE,
    num_hands=2,
    min_hand_detection_confidence=0.3,
    min_hand_presence_confidence=0.3,
    min_tracking_confidence=0.3
)


hand_detector = (
    HandLandmarker.create_from_options(
        hand_options
    )
)


def normalize_hand(hand_landmarks):
    wrist = hand_landmarks[0]
    middle_mcp = hand_landmarks[9]

    scale = (
        (middle_mcp.x - wrist.x) ** 2
        + (middle_mcp.y - wrist.y) ** 2
        + (middle_mcp.z - wrist.z) ** 2
    ) ** 0.5

    if scale < 1e-6:
        scale = 1.0

    features = []

    for landmark in hand_landmarks:
        features.extend([
            (landmark.x - wrist.x) / scale,
            (landmark.y - wrist.y) / scale,
            (landmark.z - wrist.z) / scale
        ])

    return features


def extract_features_from_result(result):
    left_hand = [0.0] * 63
    right_hand = [0.0] * 63

    if not result.hand_landmarks:
        return None

    for i, hand_landmarks in enumerate(
        result.hand_landmarks
    ):
        handedness = (
            result.handedness[i][0]
            .category_name
        )

        features = normalize_hand(
            hand_landmarks
        )

        if handedness == "Left":
            left_hand = features

        elif handedness == "Right":
            right_hand = features

    return left_hand + right_hand


def features_from_base64(image_base64: str):
    try:
        if "," in image_base64:
            image_base64 = (
                image_base64.split(",", 1)[1]
            )

        image_bytes = base64.b64decode(
            image_base64
        )

        image_array = np.frombuffer(
            image_bytes,
            dtype=np.uint8
        )

        frame = cv2.imdecode(
            image_array,
            cv2.IMREAD_COLOR
        )

        if frame is None:
            return None

        rgb = cv2.cvtColor(
            frame,
            cv2.COLOR_BGR2RGB
        )

        mp_image = mp.Image(
            image_format=mp.ImageFormat.SRGB,
            data=rgb
        )

        result = hand_detector.detect(
            mp_image
        )

        return extract_features_from_result(
            result
        )

    except Exception as error:
        print(
            "Goruntu isleme hatasi:",
            error
        )

        return None


app = FastAPI()


class RecognitionRequest(BaseModel):
    modelType: str
    expectedLabel: str | None = None
    features: list[float] | None = None
    imageBase64: str | None = None


class RecognitionResult(BaseModel):
    predictedLabel: str
    confidence: float


@app.get("/health")
def health():
    return {
        "status": "ok"
    }


@app.post(
    "/recognize",
    response_model=RecognitionResult
)
def recognize(
    request: RecognitionRequest
):
    print(
        "Istek geldi:",
        request.modelType,
        request.expectedLabel
    )

    features = request.features

    if features is None and request.imageBase64:
        features = features_from_base64(
            request.imageBase64
        )

    if features is None:
        return RecognitionResult(
            predictedLabel="",
            confidence=0.0
        )

    if len(features) != 126:
        return RecognitionResult(
            predictedLabel="",
            confidence=0.0
        )

    X = np.array(
        features,
        dtype=np.float32
    ).reshape(1, -1)

    if request.modelType.lower() == "letter":

        probabilities = (
            letter_model.predict_proba(X)[0]
        )

        best_index = int(
            np.argmax(probabilities)
        )

        predicted_label = (
            letter_model.classes_[
                best_index
            ]
        )

        confidence = float(
            probabilities[
                best_index
            ]
        )

    elif request.modelType.lower() == "word":

        probabilities = (
            word_model.predict_proba(X)[0]
        )

        best_index = int(
            np.argmax(probabilities)
        )

        predicted_label = (
            word_model.classes_[
                best_index
            ]
        )

        confidence = float(
            probabilities[
                best_index
            ]
        )

    else:
        predicted_label = ""
        confidence = 0.0

    return RecognitionResult(
        predictedLabel=predicted_label,
        confidence=confidence
    )