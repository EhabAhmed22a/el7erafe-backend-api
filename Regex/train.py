import fasttext

print("Starting training...")
# This reads your text file and learns the patterns
model = fasttext.train_supervised(input="training_data.txt", epoch=100, lr=0.1, wordNgrams=2)

# This saves the "brain" to a file
model.save_model("intent_model.bin")
print("Model successfully trained and saved as intent_model.bin!")