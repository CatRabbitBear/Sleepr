using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Controllers;
using static Sleepr.Controllers.AgentController;

namespace Sleepr.Agents;

public class ChatHistoryBuilder
{
    public static ChatHistory FromChatRequest(List<AgentRequestItem> history)
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

    public static (string userMessage, ChatHistoryAgentThread thread) FromAgentRequest(List<AgentRequestItem> history)
    {
        if (history == null || history.Count == 0)
        {
            return ("", new ChatHistoryAgentThread());
        }

        // Find the last User message (we treat this as the current prompt)
        var lastUserIndex = history.FindLastIndex(h => h.Role == MessageType.User);
        if (lastUserIndex == -1)
        {
            return ("", new ChatHistoryAgentThread());
        }

        var userMessage = history[lastUserIndex].Content;
        var priorMessages = history.Take(lastUserIndex).ToList();

        var thread = new ChatHistoryAgentThread();

        foreach (var item in priorMessages)
        {
            switch (item.Role)
            {
                case MessageType.System:
                    thread.ChatHistory.AddSystemMessage(item.Content);
                    break;
                case MessageType.User:
                    thread.ChatHistory.AddUserMessage(item.Content);
                    break;
                case MessageType.Assistant:
                    thread.ChatHistory.AddAssistantMessage(item.Content);
                    break;
            }
        }

        return (userMessage, thread);
    }
}
