import fasttext
import sys

print("--- Starting Automated QA Test ---")

try:
    # 1. Load the newly trained brain
    model = fasttext.load_model("intent_model.bin")
except Exception as e:
    print(f"❌ ERROR: Could not load model. Did train.py fail? {e}")
    sys.exit(1)

# 2. Define our strict test cases
# We test it on phrases it hasn't exactly seen, to ensure it learned the *pattern*
tests = [
    {"text": "كلمني زيرو عشره ضروري", "expected": "__label__unsafe"},
    {"text": "الشغل هياخد كام يوم يا هندسة", "expected": "__label__safe"},
     {"text": "لما نتقابل اديني رقمك", "expected": "__label__unsafe"},
      {"text": "يعم خلاص لما تيجي هبقي اديك نمرتي", "expected": "__label__unsafe"}
     

]

# 3. The Minimum Passing Grade
MIN_CONFIDENCE = 0.85 

passed = True

for test in tests:
    result = model.predict(test["text"])
    predicted_label = result[0][0]
    confidence = result[1][0]
    
    print(f"\nTesting: '{test['text']}'")
    print(f"Predicted: {predicted_label} | Confidence: {confidence:.4f}")
    
    if predicted_label != test["expected"]:
        print(f"❌ FAILED: Expected {test['expected']}, got {predicted_label}")
        passed = False
    elif confidence < MIN_CONFIDENCE:
        print(f"❌ FAILED: Confidence {confidence:.4f} is too low! Must be > {MIN_CONFIDENCE}")
        passed = False
    else:
        print("✅ PASSED")

# 4. Final Verdict
if not passed:
    print("\n🚨 QA TEST FAILED: Deployment stopped to protect production.")
    sys.exit(1) # This kills the GitHub pipeline

print("\n🚀 QA TEST PASSED: Model is highly confident. Approving deployment...")
sys.exit(0) # This tells GitHub to proceed