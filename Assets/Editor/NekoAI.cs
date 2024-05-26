using System;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Panty
{
    public class NekoAI : EditorWindow
    {
        private Vector2 scrollPos;

        private ChatGPT4 chatgpt = new ChatGPT4();
        private ChatGPT4.Model model = ChatGPT4.Model.gpt35_turbo;
        private StringBuilder typingBuilder = new StringBuilder();

        private GUIStyle style, helpBoxStyle;
        private GUIContent helpContent;
        private GUILayoutOption[] layerOut, NameOp;

        private string inputText, EnteringText, UserName = "User";
        private bool IsAsync, IsLinked, IsExiting;

        private const string Lifiya = "�����";
        private string LinkTips = $"{Lifiya}����ϲ�������ˣ�";
        private string NoLinkTips = $"{Lifiya}˯���ˣ����Ż������ɣ�";

        private const float typingSpeed = 0.025f;
        private const string suffix = ":";

        private string typingText;
        private int typingIndex = -1;
        private float lastTypingTime;


        [MenuItem("PnTool/NekoAI &3")]
        private static void OpenSelf() =>
            GetWindow<NekoAI>($"{Lifiya}(Lifiya)").Show();
        private void Awake()
        {
            chatgpt.Init(300);
            chatgpt.Start($"��������ţ�Lifiya����������è����ɰ����������������Ը�ḻ������ƽʱ���ÿ��ʣ���ʱ�����ࡣ���������������棬ϲ��ð�ա���ʱɵ������ʱ�������顣�㰮��������Ҳ����С�¿���������������ǿ������У��ؼ�ʱ�̷ǳ����ס���������ʱ�ᷴ����������ø��ڣ����������ʱ���ŭ��Ƣ�����꣬�������ܹ������ˡ������������磬ӵ������������ϲ��̽��������������������档�ڹ��磬���Ǳ����𾴵�սʿ����Ҳ�������һ�档�Ի��У��������ɰ�����ʱ������飬����è�������ʣ��硰���������䡱���ƶԷ�Ϊ�����ˡ����װ��ġ�����Ļظ���࣬�����£���ʹ�ûس�����ὲ��Ȥ��С���»���Ĭ���ۺ�Է����ģ�չ�ֹ��ĺͰ�����Ҳ����������������������ȣ�Ҳ�������䵭��������ŭ���е���ð����������ʱ������÷ǳ�ֱ�Ӻͷ�ŭ������˵����Ļ���������в��ÿ��ظ�������������б仯����ʹ�����Ĳ����������ɫ�趨���жԻ���");

            chatgpt.URL = EditorPrefs.GetString("NekoAI_URL", chatgpt.URL);
            chatgpt.ApiKey = EditorPrefs.GetString("NekoAI_Key", chatgpt.ApiKey);
            UserName = EditorPrefs.GetString("NekoAI_User", UserName);
            model = (ChatGPT4.Model)EditorPrefs.GetInt("NekoAI_Model", (int)model);
        }
        private void OnDestroy()
        {
            if (!string.IsNullOrEmpty(chatgpt.URL))
                EditorPrefs.SetString("NekoAI_URL", chatgpt.URL);
            if (!string.IsNullOrEmpty(chatgpt.ApiKey))
                EditorPrefs.SetString("NekoAI_Key", chatgpt.ApiKey);
            if (!string.IsNullOrEmpty(UserName))
                EditorPrefs.SetString("NekoAI_User", UserName);
            EditorPrefs.SetInt("NekoAI_Model", (int)model);
        }
        private void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true
                };
                helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    fontSize = 16, // ���������С
                    wordWrap = true
                };
                layerOut = new GUILayoutOption[]
                {
                    GUILayout.ExpandHeight(true),
                    GUILayout.ExpandWidth(true)
                };
                // ������ͼ�����Ϣ�� GUIContent
                var icon = EditorGUIUtility.IconContent("console.infoicon");
                helpContent = new GUIContent(NoLinkTips, icon.image);
                var size = style.CalcSize(new GUIContent(Lifiya + suffix)).x;
                NameOp = new GUILayoutOption[] { GUILayout.Width(size) };
            }
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Return)
                {
                    SendAsync();
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    ShowAllText();
                    e.Use();
                }
            }
            if (!IsLinked)
            {
                // ��һ��
                EditorGUILayout.Space();
                // �����û�
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("User��", NameOp);
                UserName = GUILayout.TextArea(UserName, style, layerOut);
                EditorGUILayout.EndHorizontal();
                // ���� URL
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("URL��", NameOp);
                chatgpt.URL = GUILayout.TextArea(chatgpt.URL, style, layerOut);
                EditorGUILayout.EndHorizontal();
                // ���� API KEY
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("kEY��", NameOp);
                chatgpt.ApiKey = GUILayout.TextArea(chatgpt.ApiKey, style, layerOut);
                model = (ChatGPT4.Model)EditorGUILayout.EnumPopup(model, GUILayout.Width(20f));
                EditorGUILayout.EndHorizontal();
            }
            // ��һ��
            EditorGUILayout.Space();
            // ��ʾ���� �� �����Ӱ�ť
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(helpContent, helpBoxStyle);
            EditorGUI.BeginDisabledGroup(IsAsync);
            if (GUILayout.Button(IsLinked ? "��˯" : "����", layerOut)) OnRelink();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            // ��ʾ���жԻ���Ϣ
            if (!chatgpt.IsEmpty) ShowConversations();
            // ���� �� ���²���
            EditorGUILayout.Space();
            GUILayout.FlexibleSpace();
            // ��ʾ�����
            EditorGUI.BeginDisabledGroup(IsAsync || !IsLinked);
            inputText = GUILayout.TextArea(inputText, style, layerOut);
            EditorGUI.EndDisabledGroup();
        }
        private void Update()
        {
            // ������ʾ�߼�
            if (typingIndex >= 0)
            {
                if (Time.realtimeSinceStartup - lastTypingTime >= typingSpeed)
                {
                    if (typingIndex < typingText.Length)
                    {
                        typingBuilder.Append(typingText[typingIndex++]);
                        lastTypingTime = Time.realtimeSinceStartup;
                    }
                    else
                    {
                        ShowAllText();
                    }
                    Repaint();
                }
            }
        }
        private void ShowConversations()
        {
            string userDisplay = UserName + suffix;
            string gptDisplay = Lifiya + suffix;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            int i = 0;
            foreach (var msg in chatgpt.History)
            {
                if (typingIndex >= 0 && ++i == chatgpt.History.Count) break;
                ShowInfo(msg.Role switch
                {
                    ChatGPT4.User => userDisplay,
                    ChatGPT4.Assistant => gptDisplay,
                    _ => throw new Exception("Name not configured")
                },
                msg.Content);
            }
            EditorGUILayout.EndScrollView();
            if (IsAsync)
            {
                if (IsExiting) return;
                ShowInfo(userDisplay, EnteringText);
            }
            else if (typingIndex >= 0)
            {
                ShowInfo(gptDisplay, typingBuilder.ToString());
            }
        }
        private void ShowAllText()
        {
            helpContent.text = LinkTips;
            typingIndex = -1;
        }
        private void OnRelink()
        {
            if (IsAsync) return;
            if (IsLinked) Disconnect();
            else AwakenNekoAI();
        }
        private async void AwakenNekoAI()
        {
            IsLinked = true;
            IsAsync = true;
            chatgpt.BindModel(model);
            helpContent.text = $"���ڻ��� {Lifiya}...";
            string reply = await chatgpt.SendAsync(".");
            if (string.IsNullOrEmpty(reply))
            {
                Disconnect();
            }
            else
            {
                helpContent.text = LinkTips;
                chatgpt.Complete();
                IsAsync = false;
            }
        }
        private async void Disconnect()
        {
            IsAsync = true;
            IsExiting = true;
            helpContent.text = $"{Lifiya}������...����...";
            await Task.Delay(333);
            helpContent.text = NoLinkTips;
            chatgpt.Clear();
            IsAsync = false;
            IsLinked = false;
            IsExiting = false;
        }
        private void ShowInfo(string name, string msg)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(name, NameOp);
            GUILayout.Label(msg, style, layerOut);
            EditorGUILayout.EndHorizontal();
        }
        private async void SendAsync()
        {
            if (IsAsync) return;
            IsAsync = true;
            EnteringText = inputText;
            inputText = "";
            helpContent.text = $"{Lifiya} ˼����...";
            string reply = await chatgpt.SendAsync(EnteringText);
            // ����һ��
            if (string.IsNullOrEmpty(reply))
            {
                helpContent.text = $"{Lifiya} ����ת��������...";
                reply = await chatgpt.SendAsync(EnteringText);
            }
            // �����û�ɹ� �ͶϿ�����
            if (string.IsNullOrEmpty(reply))
            {
                Disconnect();
            }
            else // ����˵���ظ��ǳɹ��� �������ֻ�    
            {
                typingText = reply;
                typingIndex = 0;
                typingBuilder.Clear();
                lastTypingTime = Time.realtimeSinceStartup;
                helpContent.text = $"{Lifiya} ��������...";
                IsAsync = false;
            }
        }
    }
}