namespace TheGrammar.Features.Prompts;

public class DuplicateHotkeyException(string message, Exception inner) : Exception(message, inner);