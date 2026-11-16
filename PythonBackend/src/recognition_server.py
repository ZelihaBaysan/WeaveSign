from pathlib import Path
import base64
import json

import cv2
import joblib
import mediapipe as mp
import numpy as np
import tensorflow as tf

from fastapi import FastAPI
from pydantic import BaseModel


# =========================================================
# PATHLER
# =========================================================

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


# Yeni GRU dosyalari
DYNAMIC_WORD_MODEL_PATH = (
    PROJECT_ROOT
    / "models"
    / "dynamic_words_gru.keras"
)

DYNAMIC_WORD_LABEL_PATH = (
    PROJECT_ROOT
    / "models"
    / "dynamic_words_labels.json"
)

DYNAMIC_WORD_SCALER_PATH = (
    PROJECT_ROOT
    / "models"
    / "dynamic_words_scaler.npz"
)


SEQUENCE_LENGTH = 30
FEATURE_COUNT = 126


# =========================================================
# RANDOM FOREST MODELLERI
# =========================================================

letter_model = joblib.load(
    LETTER_MODEL_PATH
)

letter_model.n_jobs = 1


word_model = joblib.load(
    WORD_MODEL_PATH
)

word_model.n_jobs = 1


# =========================================================
# GRU MODELI
# =========================================================

dynamic_word_model = tf.keras.models.load_model(
    DYNAMIC_WORD_MODEL_PATH
)


with open(
    DYNAMIC_WORD_LABEL_PATH,
    "r",
    encoding="utf-8"
) as file:
    dynamic_word_labels = json.load(file)


dynamic_scaler = np.load(
    DYNAMIC_WORD_SCALER_PATH
)


dynamic_mean = np.squeeze(
    dynamic_scaler["mean"]
)

dynamic_std = np.squeeze(
    dynamic_scaler["std"]
)


print(
    "GRU kelime siniflari:",
    dynamic_word_labels
)


# =========================================================
# MEDIAPIPE
# =========================================================

BaseOptions = mp.tasks.BaseOptions

HandLandmarker = (
    mp.tasks.vision.HandLandmarker
)

HandLandmarkerOptions = (
    mp.tasks.vision.HandLandmarkerOptions
)

RunningMode = (
    mp.tasks.vision.RunningMode
)


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


# =========================================================
# LANDMARK NORMALIZASYONU
# =========================================================

def normalize_hand(
    hand_landmarks
):

    wrist = hand_landmarks[0]

    middle_mcp = hand_landmarks[9]


    scale = (
        (
            middle_mcp.x
            - wrist.x
        ) ** 2
        +
        (
            middle_mcp.y
            - wrist.y
        ) ** 2
        +
        (
            middle_mcp.z
            - wrist.z
        ) ** 2
    ) ** 0.5


    if scale < 1e-6:
        scale = 1.0


    features = []


    for landmark in hand_landmarks:

        features.extend([
            (
                landmark.x
                - wrist.x
            ) / scale,

            (
                landmark.y
                - wrist.y
            ) / scale,

            (
                landmark.z
                - wrist.z
            ) / scale
        ])


    return features


# =========================================================
# MEDIAPIPE RESULT -> 126 FEATURE
# =========================================================

def extract_features_from_result(
    result
):

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


# =========================================================
# BASE64 -> FRAME
# =========================================================

def decode_base64_image(
    image_base64: str
):

    try:

        if "," in image_base64:

            image_base64 = (
                image_base64
                .split(",", 1)[1]
            )


        image_bytes = (
            base64.b64decode(
                image_base64
            )
        )


        image_array = (
            np.frombuffer(
                image_bytes,
                dtype=np.uint8
            )
        )


        frame = cv2.imdecode(
            image_array,
            cv2.IMREAD_COLOR
        )


        return frame


    except Exception as error:

        print(
            "Base64 decode hatasi:",
            error
        )

        return None


# =========================================================
# TEK GORUNTUDEN FEATURE
# =========================================================

def features_from_base64(
    image_base64: str
):

    try:

        frame = decode_base64_image(
            image_base64
        )


        if frame is None:
            return None


        rgb = cv2.cvtColor(
            frame,
            cv2.COLOR_BGR2RGB
        )


        mp_image = mp.Image(
            image_format=
            mp.ImageFormat.SRGB,

            data=rgb
        )


        result = hand_detector.detect(
            mp_image
        )


        return (
            extract_features_from_result(
                result
            )
        )


    except Exception as error:

        print(
            "Goruntu isleme hatasi:",
            error
        )

        return None


# =========================================================
# 30 KARE -> GRU SEQUENCE
# =========================================================

def sequence_from_base64_frames(
    frames_base64
):

    sequence = []

    last_features = None

    valid_hand_frames = 0


    for image_base64 in frames_base64:

        frame = decode_base64_image(
            image_base64
        )


        if frame is None:
            continue


        rgb = cv2.cvtColor(
            frame,
            cv2.COLOR_BGR2RGB
        )


        mp_image = mp.Image(
            image_format=
            mp.ImageFormat.SRGB,

            data=rgb
        )


        result = hand_detector.detect(
            mp_image
        )


        features = (
            extract_features_from_result(
                result
            )
        )


        # El gorulduyse normal feature
        if features is not None:

            features = np.array(
                features,
                dtype=np.float32
            )

            last_features = (
                features.copy()
            )

            valid_hand_frames += 1


        # El kaybolduysa son gecerli feature
        else:

            if last_features is not None:

                features = (
                    last_features.copy()
                )

            else:

                features = np.zeros(
                    FEATURE_COUNT,
                    dtype=np.float32
                )


        sequence.append(
            features
        )


    if len(sequence) != SEQUENCE_LENGTH:

        print(
            "GRU sequence uzunlugu hatali:",
            len(sequence)
        )

        return None


    minimum_valid_frames = int(
        SEQUENCE_LENGTH * 0.20
    )


    if valid_hand_frames < minimum_valid_frames:

        print(
            "GRU icin yeterli el yok:",
            f"{valid_hand_frames}/"
            f"{SEQUENCE_LENGTH}"
        )

        return None


    sequence = np.array(
        sequence,
        dtype=np.float32
    )


    print(
        "GRU sequence hazir:",
        sequence.shape,
        "| El:",
        f"{valid_hand_frames}/30"
    )


    return sequence


# =========================================================
# FASTAPI
# =========================================================

app = FastAPI()


class RecognitionRequest(BaseModel):

    modelType: str

    expectedLabel: str | None = None

    features: list[float] | None = None

    imageBase64: str | None = None

    # Yeni:
    # Unity kelime tanimada 30 kare gonderecek
    framesBase64: list[str] | None = None


class RecognitionResult(BaseModel):

    predictedLabel: str

    confidence: float


# =========================================================
# HEALTH
# =========================================================

@app.get("/health")
def health():

    return {
        "status": "ok",
        "dynamicWordModel": True
    }


# =========================================================
# RECOGNIZE
# =========================================================

@app.post(
    "/recognize",
    response_model=RecognitionResult
)
def recognize(
    request: RecognitionRequest
):

    print(
        "\nIstek geldi:",
        request.modelType,
        "| Beklenen:",
        request.expectedLabel
    )


    model_type = (
        request.modelType
        .lower()
    )


    # =====================================================
    # HARF
    # RANDOM FOREST
    # =====================================================

    if model_type == "letter":

        features = request.features


        if (
            features is None
            and request.imageBase64
        ):

            features = (
                features_from_base64(
                    request.imageBase64
                )
            )


        if features is None:

            return RecognitionResult(
                predictedLabel="",
                confidence=0.0
            )


        if len(features) != FEATURE_COUNT:

            return RecognitionResult(
                predictedLabel="",
                confidence=0.0
            )


        X = np.array(
            features,
            dtype=np.float32
        ).reshape(
            1,
            -1
        )


        probabilities = (
            letter_model
            .predict_proba(X)[0]
        )


        best_index = int(
            np.argmax(
                probabilities
            )
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


        print(
            "LETTER RF ->",
            predicted_label,
            f"%{confidence * 100:.2f}"
        )


        return RecognitionResult(
            predictedLabel=
            str(predicted_label),

            confidence=
            confidence
        )


    # =====================================================
    # KELIME
    # GRU
    # =====================================================

    elif model_type == "word":

        # Unity 30 kare gonderdiyse
        # yeni GRU modelini kullan
        if request.framesBase64:

            if (
                len(
                    request.framesBase64
                )
                !=
                SEQUENCE_LENGTH
            ):

                print(
                    "30 kare bekleniyordu, gelen:",
                    len(
                        request.framesBase64
                    )
                )

                return RecognitionResult(
                    predictedLabel="",
                    confidence=0.0
                )


            sequence = (
                sequence_from_base64_frames(
                    request.framesBase64
                )
            )


            if sequence is None:

                return RecognitionResult(
                    predictedLabel="",
                    confidence=0.0
                )


            # Scaler
            sequence = (
                sequence
                - dynamic_mean
            ) / dynamic_std


            # (30,126)
            # ->
            # (1,30,126)
            X = np.expand_dims(
                sequence,
                axis=0
            )


            prediction = (
                dynamic_word_model.predict(
                    X,
                    verbose=0
                )[0]
            )


            best_index = int(
                np.argmax(
                    prediction
                )
            )


            predicted_label = (
                dynamic_word_labels[
                    best_index
                ]
            )


            confidence = float(
                prediction[
                    best_index
                ]
            )


            print(
                "WORD GRU ->",
                predicted_label,
                f"%{confidence * 100:.2f}"
            )


            return RecognitionResult(
                predictedLabel=
                predicted_label,

                confidence=
                confidence
            )


        # ================================================
        # GECICI FALLBACK:
        # Unity henuz 30 kare gondermiyorsa
        # eski Random Forest calissin
        # ================================================

        print(
            "30 kare gelmedi."
            " Word Random Forest fallback."
        )


        features = request.features


        if (
            features is None
            and request.imageBase64
        ):

            features = (
                features_from_base64(
                    request.imageBase64
                )
            )


        if features is None:

            return RecognitionResult(
                predictedLabel="",
                confidence=0.0
            )


        if len(features) != FEATURE_COUNT:

            return RecognitionResult(
                predictedLabel="",
                confidence=0.0
            )


        X = np.array(
            features,
            dtype=np.float32
        ).reshape(
            1,
            -1
        )


        probabilities = (
            word_model
            .predict_proba(X)[0]
        )


        best_index = int(
            np.argmax(
                probabilities
            )
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


        print(
            "WORD RF FALLBACK ->",
            predicted_label,
            f"%{confidence * 100:.2f}"
        )


        return RecognitionResult(
            predictedLabel=
            str(predicted_label),

            confidence=
            confidence
        )


    # =====================================================
    # BILINMEYEN MODEL TYPE
    # =====================================================

    return RecognitionResult(
        predictedLabel="",
        confidence=0.0
    )