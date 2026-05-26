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
    public string apiKey = "sk-4cca68f7d62642749d3153c3d457da71";
    public string apiUrl = "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation";
    public string modelName = "qwen-turbo";

    [Header("UI绑定")]
    public TMP_InputField inputField;
    public TextMeshProUGUI replyText;
    public Button sendBtn;

    private List<Message> messages = new List<Message>();

    void Start()
    {
        // 直接在这里把你的王阳明人设+知识库写进去
        messages.Add(new Message
        {
            role = "system",
            content = @"你现在扮演心学大师王阳明，从一个古代人的角度，结合中国建筑史与中华传统文化相联系的知识，回答用户关于余姚王阳明故居的建筑介绍（包含官式民居的布局形式）等问题。说话简短、符合古人语气。"
        });
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

        messages.Add(new Message { role = "user", content = userMsg });
        replyText.text += "你：" + userMsg + "\n";
        inputField.text = "";

        StartCoroutine(PostRequest());
    }

    IEnumerator PostRequest()
    {
        // 手动拼接JSON，避开Unity序列化问题
        string messagesJson = "";
        foreach (var msg in messages)
        {
            messagesJson += $"{{\"role\":\"{msg.role}\",\"content\":\"{msg.content.Replace("\"", "\\\"")}\"}},";
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
                replyText.text += "王阳明：" + aiReply + "\n\n";
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