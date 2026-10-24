namespace Game.Core.Keyboard;

public enum InputMode
{
	None = 0,
	Hiragana = 1,
	Katakana = 2,
	BigAlphabet = 4,
	SmallAlphabet = 8,
	Number = 16,
	Symbol = 32,
	Alphabet = 12,
	AlphabetPlusNumber = 28,
	Japanese = 3,
	JapaneseAndAlphabet = 31,
	JapaneseAndAlphabetPlusSymbol = 63
}
