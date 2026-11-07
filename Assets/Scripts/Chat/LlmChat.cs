using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem; // 加这一行

public class LlmChat : MonoBehaviour
{
    [Header("通义千问通用API")]
    [Tooltip("在 Inspector 中填入你自己的 DashScope API Key，不要把真实 Key 提交到公开仓库。")]
    public string apiKey = "";
    public string apiUrl = "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation";
    public string modelName = "qwen-turbo";

    [Header("AI 人设（可选，留空则不发送 system 消息）")]
    [TextArea(3, 8)]
    [Tooltip("项目通用化后不再内置任何角色；需要特定人设时在这里填写，或由场景/配置覆盖。")]
    public string systemPrompt = "你是一个乐于助人的中文智能助手，回答准确、简洁、友好。";

    [Header("UI绑定")]
    public TMP_InputField inputField;
    public TextMeshProUGUI replyText;
    public Button sendBtn;

    [Header("对话显示")]
    [Tooltip("AI 回复在对话区使用的显示名称，可按项目需要修改。")]
    public string assistantName = "AI 助手";

    private List<Message> messages = new List<Message>();

    void Start()
    {
        // 通用化：不再硬编码任何特定人物/故居人设，system 消息由 systemPrompt 决定
        if (!string.IsNullOrEmpty(systemPrompt) && systemPrompt.Trim().Length > 0)
        {
            messages.Add(new Message
            {
                role = "system",
                content = systemPrompt
            });
        }
        sendBtn.onClick.AddListener(SendChat);
        replyText.text = "";
    }

    private void Update()
    {
        // 旧代码：Input.GetKeyDown(KeyCode.Return)
        // 新代码：回车发送
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            sendBtn.onClick.Invoke();
        }
    }

    public void SendChat()
    {
        string userMsg = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMsg)) return;

        if (string.IsNullOrEmpty(apiKey))
        {
            replyText.text += "提示：请先在 Inspector 的 LlmChat 组件中填写 API Key\n";
            return;
        }

        messages.Add(new Message { role = "user", content = userMsg });
        replyText.text += "你：" + userMsg + "\n";
        inputField.text = "";

        StartCoroutine(PostRequest());
    }

    // 转义 JSON 字符串，避免人设/用户输入中的引号、换行、反斜杠破坏请求体
    private static string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
    }

    IEnumerator PostRequest()
    {
        // 手动拼接JSON，避开Unity序列化问题
        string messagesJson = "";
        foreach (var msg in messages)
        {
            messagesJson += $"{{\"role\":\"{msg.role}\",\"content\":\"{EscapeJson(msg.content)}\"}},";
        }
        messagesJson = messagesJson.TrimEnd(',');

        string json = $@"
        {{
            ""model"": ""{modelName}"",
            ""input"": {{ ""messages"": [{messagesJson}] }},
            ""parameters"": {{ ""temperature"": 0.7 }}
        }}";

        using (UnityWebRequest wr = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            wr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            wr.downloadHandler = new DownloadHandlerBuffer();
            wr.SetRequestHeader("Authorization", "Bearer " + apiKey);
            wr.SetRequestHeader("Content-Type", "application/json");

            yield return wr.SendWebRequest();

            if (wr.result == UnityWebRequest.Result.Success)
            {
                QwenResponse res = JsonUtility.FromJson<QwenResponse>(wr.downloadHandler.text);
                string aiReply = res.output.text.Trim();
                replyText.text += assistantName + "：" + aiReply + "\n\n";
                messages.Add(new Message { role = "assistant", content = aiReply });
            }
            else
            {
                replyText.text += "错误：" + wr.error + "\n";
                Debug.LogError("请求失败：" + wr.downloadHandler.text);
            }
        }
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class Output
    {
        public string text;
    }

    [System.Serializable]
    public class QwenResponse
    {
        public Output output;
    }
}