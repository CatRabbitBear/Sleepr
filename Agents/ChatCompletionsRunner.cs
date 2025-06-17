using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Controllers;
using Sleepr.Interfaces;

namespace Sleepr.Agents;

public class ChatCompletionsRunner : IChatCompletionsRunner
{
    private readonly Kernel _kernel;
    private readonly IAgentOutput _outputManager;

    public ChatCompletionsRunner(Kernel kernel, IAgentOutput outputManager)
    {
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
            Console.WriteLine($"Error during chat completion: {ex.Message}");
            return new AgentResponse { Result = "", FilePath = ""};
        }
    }
}
