using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Controllers;
using static Sleepr.Controllers.AgentController;

namespace Sleepr.Agents;

public class ChatHistoryBuilder
{
    public static ChatHistory FromAgentRequest(List<AgentRequestItem> history)
    {
        var chatHistory = new ChatHistory();
        foreach (var item in history)
        {
            switch (item.Role)
            {
                case MessageType.System:
                    chatHistory.AddSystemMessage(item.Content);
                    break;
                case MessageType.User:
                    chatHistory.AddUserMessage(item.Content);
                    break;
                case MessageType.Assistant:
                    chatHistory.AddAssistantMessage(item.Content);
                    break;
            }
        }

        return chatHistory;
    }
}
