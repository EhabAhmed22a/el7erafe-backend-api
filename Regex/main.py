from fastapi import FastAPI
from pydantic import BaseModel
import re
import fasttext

app = FastAPI()

# Load the ML model into memory when the API starts
nlp_model = fasttext.load_model("intent_model.bin")

class ModerationRequest(BaseModel):
    text: str

# Egyptian Word Map
EGYPTIAN_NUMBER_MAP = {
    "زيرو": "0", "صفر": "0", "zero": "0", 
    "واحد": "1", "one": "1",
    "اتنين": "2", "إتنين": "2", "two": "2",
    "تلاته": "3", "ثلاثه": "3", "three": "3",
    "اربعة": "4", "اربعه": "4", "four": "4",
    "خمسة": "5", "خمسه": "5", "five": "5",
    "ستة": "6", "سته": "6", "six": "6",
    "سبعة": "7", "سبعه": "7", "seven": "7",
    "تمانية": "8", "تمانيه": "8", "eight": "8",
    "تسعة": "9", "تسعه": "9", "nine": "9",
    "عشرة": "10", "عشره": "10", "ten": "10"
}

def transcribe_obfuscated_numbers(text):
    words = text.split()
    transcribed_text = ""
    for word in words:
        clean_word = word.strip()
        if clean_word in EGYPTIAN_NUMBER_MAP:
            transcribed_text += EGYPTIAN_NUMBER_MAP[clean_word]
        else:
            transcribed_text += f" {word} "
    return transcribed_text

def normalize_text(text):
    text = transcribe_obfuscated_numbers(text)
    arabic_digits = "٠١٢٣٤٥٦٧٨٩"
    english_digits = "0123456789"
    table = str.maketrans(arabic_digits, english_digits)
    text = text.translate(table)
    return re.sub(r'[\.\-\_\s]', '', text)

@app.post("/moderate")
async def moderate_text(request: ModerationRequest):
    original_text = request.text
    
    # --- LAYER 1: REGEX (Deterministic) ---
    normalized = normalize_text(original_text)
    phone_pattern = r'(?:\+?20|0)?1[0125]\d{8}'
    keywords = r'(كلمني|واتس|برا|كاش|callme|whatsapp|outside|cash|phone)'
    
    if re.search(phone_pattern, normalized):
        return {"is_safe": False, "reason": "phone_number_detected", "layer": "regex"}
        
    if re.search(keywords, original_text, re.IGNORECASE):
        return {"is_safe": False, "reason": "blacklisted_keyword", "layer": "regex"}

    # --- LAYER 2: MACHINE LEARNING (Probabilistic) ---
    # Strip newlines for FastText
    clean_for_model = original_text.replace('\n', ' ') 
    predictions = nlp_model.predict(clean_for_model)
    
    predicted_label = predictions[0][0] # e.g., '__label__unsafe'
    confidence = float(predictions[1][0]) # e.g., 0.95

    # If the ML model is highly confident it's unsafe
    if predicted_label == "__label__unsafe" and confidence > 0.80:
        return {
            "is_safe": False, 
            "reason": "unsafe_intent_detected", 
            "layer": "machine_learning",
            "confidence": round(confidence, 2)
        }

    return {"is_safe": True, "reason": "clean", "layer": "none"}