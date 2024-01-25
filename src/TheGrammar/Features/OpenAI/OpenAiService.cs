using OpenAI_API;
using Serilog;
using TheGrammar.Domain;

namespace TheGrammar.Features.OpenAI;

public class ChatVersionState
{
    public Dictionary<ChatVersion, OpenAI_API.Models.Model> PossibleModels { get; set; }

    public OpenAI_API.Models.Model CurrentModel { get; private set; }

    public ChatVersionState()
    {
        var defaultModel = OpenAI_API.Models.Model.GPT4_Turbo;
        CurrentModel = defaultModel;

        PossibleModels = new Dictionary<ChatVersion, OpenAI_API.Models.Model>
        {
            { ChatVersion.Turbo35, OpenAI_API.Models.Model.ChatGPTTurbo },
            { ChatVersion.Turbo4, defaultModel }
        };
    }

    public void SetCurrentModel(ChatVersion chatVersion)
    {
        if (PossibleModels.TryGetValue(chatVersion, out var model))
        {
            CurrentModel = model;
        }
        else
        {
            Log.Error($"Failed to set current model to {chatVersion}");
        }
    }

    public ChatVersion GetCurrentChatVersion()
    {
        return PossibleModels.FirstOrDefault(x => x.Value == CurrentModel).Key;
    }
}

public class OpenApiResult
{
    public required string OriginalText { get; set; }
    public required string ModifiedText { get; set; }
    public required ChatVersion ChatVersion { get; set; }
}


public class OpenAiService
{
    private readonly IOpenAIAPI _openAIAPI;
    private readonly ChatVersionState _chatVersionState;

    public OpenAiService(IOpenAIAPI openAIAPI, ChatVersionState chatVersionState)
    {
        _openAIAPI = openAIAPI;
        _chatVersionState = chatVersionState;
    }

    public async Task<OpenApiResult> ProcessAsync(string promt, string input)
    {
        try
        {
            var chat = _openAIAPI.Chat.CreateConversation();
            chat.Model = _chatVersionState.CurrentModel;

            chat.RequestParameters.Temperature = 0;

            chat.AppendSystemMessage(promt);
            chat.AppendUserInput(input);

            var response = await chat.GetResponseFromChatbotAsync();

            return new OpenApiResult
            {
                OriginalText = input,
                ModifiedText = response,
                ChatVersion = _chatVersionState.GetCurrentChatVersion()
            };
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to process get response from chatbot: {ex.Message}");
            throw;
        }
    }
}
