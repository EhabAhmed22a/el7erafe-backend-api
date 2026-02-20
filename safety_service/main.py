from fastapi import FastAPI
from pydantic import BaseModel
import re
from flashtext import KeywordProcessor

app = FastAPI()

# --- 1. CONFIGURATION & DICTIONARIES ---

# Map words to digits (English & Arabic)
WORD_TO_DIGIT = {
    # English
    "zero": "0", "one": "1", "two": "2", "three": "3", "four": "4",
    "five": "5", "six": "6", "seven": "7", "eight": "8", "nine": "9",
    # Arabic (Standard & Egyptian Slang)
    "صفر": "0", "واحد": "1", "اتنين": "2", "اثنين": "2", "تلاتة": "3", "ثلاثة": "3",
    "اربعة": "4", "أربعة": "4", "خمسة": "5", "سته": "6", "ستة": "6",
    "سبعة": "7", "تمانية": "8", "ثمانية": "8", "تسعة": "9"
}

# Regex for Egyptian Phones (010, 011, 012, 015)
# Matches:
# 1. The prefix (010, 011, etc)
# 2. Flexible separators (spaces, dashes, dots)
# 3. The remaining 8 digits
EGYPT_PHONE_REGEX = r"(?:010|011|012|015|٠١٠|٠١١|٠١٢|٠١٥)[\s\-\.]*(?:\d|[\u0660-\u0669])[\s\-\.]*(?:\d|[\u0660-\u0669]){7}"

# Initialize Keyword Processor for specific unsafe words
keyword_processor = KeywordProcessor()
unsafe_keywords = [
    "Call me", "WhatsApp", "Outside app", "Cash", "Phone",
    "كلمني", "واتس", "كاش", "برا الابلكيشن", "فون", "رقمي"
]
keyword_processor.add_keywords_from_list(unsafe_keywords)

# Initialize a separate processor just for normalizing numbers (High Speed)
number_normalizer = KeywordProcessor()
number_normalizer.add_keywords_from_dict(
    {v: [k] for k, v in WORD_TO_DIGIT.items()} 
    # This inverts the dict so "zero" maps to "0", "one" maps to "1"
)

# --- 2. DATA MODELS ---
class TextPayload(BaseModel):
    text: str

class SafetyResponse(BaseModel):
    is_safe: bool
    reason: str = None
    highlighted_text: str = None

# --- 3. CORE LOGIC ---
def normalize_and_scan(text: str):
    """
    1. Converts words to digits (zero -> 0).
    2. Checks for phone patterns in the 'clean' text.
    """
    # Step A: Convert words to digits ("zero one zero" -> "0 1 0")
    # We use FlashText here because it's 100x faster than Regex replace
    text_with_digits = number_normalizer.replace_keywords(text)
    
    # Step B: Remove spaces ONLY between digits to catch "0 1 0" -> "010"
    # This regex looks for: (Digit) + (Space/Dash) + (Digit) and removes the middle
    clean_text = re.sub(r'(?<=\d)[\s\-\.]+(?=\d)', '', text_with_digits)
    
    # Step C: Scan for the phone pattern
    match = re.search(EGYPT_PHONE_REGEX, clean_text)
    
    return match, clean_text

@app.post("/check", response_model=SafetyResponse)
async def check_text(payload: TextPayload):
    original_text = payload.text
    
    # 1. Check Keywords (on original text)
    found_keywords = keyword_processor.extract_keywords(original_text)
    if found_keywords:
        return SafetyResponse(
            is_safe=False,
            reason="keyword_detected",
            highlighted_text=found_keywords[0]
        )

    # 2. Check Phone Numbers (using Normalization)
    match, clean_text = normalize_and_scan(original_text)
    
    if match:
        return SafetyResponse(
            is_safe=False,
            reason="phone_number_detected",
            highlighted_text=match.group() # Returns the detected number (e.g. "01012345678")
        )

    # 3. Safe
    return SafetyResponse(is_safe=True)