using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Controllers;
using Sleepr.Interfaces;

namespace Sleepr.Agents;

public class ChatCompletionsRunner : IChatCompletionsRunner
{
    private readonly ILogger<ChatCompletionsRunner> _logger;
    private readonly Kernel _kernel;
    private readonly IAgentOutput _outputManager;

    public ChatCompletionsRunner(ILogger<ChatCompletionsRunner> logger, Kernel kernel, IAgentOutput outputManager)
    {
        _logger = logger;
        _kernel = kernel;
        _outputManager = outputManager;
    }

    public async Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history)
    {
        try
        {
            ChatHistory chatHistory = ChatHistoryBuilder.FromChatRequest(history);

            var chatService = _kernel.GetRequiredService<IChatCompletionService>();
            var result = await chatService.GetChatMessageContentAsync(chatHistory);

            if (result.Content != null)
            {
                // Save the result to a file if needed
                var filePath = await _outputManager.SaveAsync(result.Content);
                return new AgentResponse
                {
                    Result = result.Content,
                    FilePath = filePath
                };
            }
            else
            {
                return new AgentResponse { Result = result.ToString(), FilePath = "" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during chat completion");
            return new AgentResponse { Result = "", FilePath = ""};
        }
    }
}
