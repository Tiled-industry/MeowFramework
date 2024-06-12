using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Panty
{
    public class TextDialog : EditorWindow
    {
        private Vector2 scrollPosition;
        private string longText = "";
        private Action succeed, fail;

        public static void Open(string msg, Action succeed = null, Action fail = null)
        {
            var wd = GetWindow<TextDialog>("������ʾ��", true);
            wd.succeed = succeed;
            wd.fail = fail;
            wd.longText = msg;
        }
        private void Awake()
        {
            position.Set(position.x, position.y, 100f, 300f);
        }
        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("��������Ů�����绰�������ӵĻ���ϴ��Ŷ", MessageType.Info);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            var style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            GUILayout.Label(longText, style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            var e = Event.current;
            var op = new GUILayoutOption[] { GUILayout.Height(30f) };
            bool trigger = hasFocus && e.type == EventType.KeyDown;
            if (GUILayout.Button("ȷ��", op))
            {
                succeed?.Invoke();
                Close();
            }
            else if (trigger && e.keyCode == KeyCode.Return)
            {
                e.Use();
                succeed?.Invoke();
                Close();
            }
            if (GUILayout.Button("ȡ��", op))
            {
                fail?.Invoke();
                Close();
            }
            else if (trigger && e.keyCode == KeyCode.Escape)
            {
                fail?.Invoke();
                e.Use();
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
        private void OnDestroy()
        {
            succeed = null;
            fail = null;
        }
    }
    public class QuickCmdEditor : EditorWindow
    {
        private bool IsAsync;
        private string mCmd = "help";
        private string mPath = "Codes";
        private string mSpace = "Panty.Test";
        private string mHub;
        private TextField mField;

        private MonoScript SCRIPT;

        [MenuItem("PnTool/QuickCmd &Q")]
        private static void OpenSelf()
        {
            if (EditorKit.ShowOrHide<QuickCmdEditor>(out var win))
            {
                win.maxSize = new Vector2(960f, 40f);
                win.minSize = new Vector2(360f, 40f);
            }
        }
        private void OnEnable()
        {
            string n = nameof(QuickCmdEditor);
            mCmd = EditorPrefs.GetString($"{n}Cmd", mCmd);
            mPath = EditorPrefs.GetString($"{n}Path", mPath);
            mSpace = EditorPrefs.GetString($"{n}Space", mSpace);
            mHub = EditorPrefs.GetString($"{n}Hub", mHub);
        }
        private void OnDisable()
        {
            string n = nameof(QuickCmdEditor);
            EditorPrefs.SetString($"{n}Cmd", mCmd);
            EditorPrefs.SetString($"{n}Path", mPath);
            if (!string.IsNullOrEmpty(mSpace))
                EditorPrefs.SetString($"{n}Space", mSpace);
            if (!string.IsNullOrEmpty(mHub))
                EditorPrefs.SetString($"{n}Hub", mHub);
        }
        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));

            mField = new TextField
            {
                value = mCmd,
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    fontSize = 24,
                    flexGrow = 1 // ռ��ʣ��ռ�
                }
            };
            mField.RegisterCallback<ChangeEvent<string>>(evt => mCmd = evt.newValue);
            mField.RegisterCallback<KeyDownEvent>(OnKeyDown);
            mField.RegisterCallback<DragPerformEvent>(OnDragPerform);
            mField.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            // ��TextField��ӵ���Ԫ��
            root.Add(mField);
        }
        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
        private void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();
            if (DragAndDrop.paths.Length > 0)
            {
                var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(DragAndDrop.paths[0]);
                if (mono == null) "�޷���ȡ��ҷ����".Log();
                else
                {
                    Type scriptType = mono.GetClass();
                    if (scriptType != null)
                    {
                        if (scriptType.IsSubclassOf(typeof(ScriptableObject)))
                        {
                            SCRIPT = mono;
                            mField.value = "SoIns:" + mono.name;
                            $"SO => {mono.name}�ѱ��".Log();
                        }
                        else $"{mono.name}����ScriptableObject".Log();
                    }
                }
            }
            evt.StopImmediatePropagation();
        }
        private static bool Eq(string source, string en, string ch) =>
            StringComparer.OrdinalIgnoreCase.Equals(source, en) || source == ch;
        private void CreateModule(string name, string tag)
        {
            string tmp = $"namespace {mSpace}\r\n{{\r\n    public interface I@{tag} : IModule\r\n    {{\r\n\r\n    }}\r\n    public class @{tag} : AbsModule, I@{tag}\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}";
            EditorKit.CreatScript(mPath, name, tag, tmp);
            mField.value = $"{tag}:";
        }
        private void CreateScript(string name, string tag)
        {
            if (string.IsNullOrEmpty(mHub))
            {
                mField.value = "hub:";
                $"�������üܹ� {mCmd}�ܹ���".Log();
                return;
            }
            string father = tag switch
            {
                "Mono" => "MonoBehaviour",
                "So" => "ScriptableObject",
                _ => mHub + tag
            };
            string tmp = $"using UnityEngine;\r\n\r\nnamespace {mSpace}\r\n{{\r\n    public class @ : {father}\r\n    {{\r\n\r\n    }}\r\n}}";
            EditorKit.CreatScript(mPath, name, "", tmp);
            mField.value = $"{tag}:";
        }
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                // �����ָ�������
                if (string.IsNullOrWhiteSpace(mCmd))
                {
                    evt.StopPropagation();
                    return;
                }
                // ȥ��ָ��ͷβ
                string cmd = mCmd.Trim();
                // ����ָ��
                if (Eq(cmd, "help", "����"))
                {
                    string instructions = $"// ����ָ���Ѽ��ݴ�Сд��ȫ�ǰ��\r\n\r\n[���ָ��] -> ͨ����һ��������� ���磺help\r\n[���ָ��] -> ͨ��������+������� ���磺hub:Project\r\n[�ָ�ָ��] -> ͨ���ɣ�:���򣨣������\r\n\r\n// ���ָ��\r\n\r\n[ help   ��������] -> ��ʾ������� ���Զ�����ʾ����ʾһЩ��Ϣ\r\n[ path   ��·����] -> ��Console ������ʾ�ѱ��·��\r\n[ space���ռ䣩] -> ��Console ������ʾ�ѱ�������ռ�\r\n\r\n// ����ָ��\r\n\r\n[ path : ·���ַ��� ] -> ����Զ���·�� ���·�������� ���Դ���Ŀ¼\r\n[ space : �����ռ� ] -> ����Զ��������ռ� ������\r\n\r\n// ������ָ��  �����ļ���·���� path��·���� �ı��Ϊ׼\r\n\r\n[ hub : Name ] �� [ �ܹ� : ���� ]\r\n���Դ���һ�������ֵļܹ���Framework��\r\n\r\n[ Mono : Name ] �� [ �ű� : ���� ]\r\n���Դ���һ�������ֵ���ͨ�ű���Script��\r\n\r\n[ UI : Name ] �� [ ���� : ���� ]\r\n���Դ���һ�������ֵ�UI�ű���UI��\r\n\r\n[ Game : Name ] �� [ ��Ϸ : ���� ]\r\n���Դ���һ�������ֵĿ��ƽű���Control�� \r\n\r\n[ Model : Name ] �� [ ���� : ���� ]\r\n���Դ���һ�������ֵ����ݽű���Model�� \r\n\r\n[ System : Name ] �� [ ϵͳ : ���� ]\r\n���Դ���һ�������ֵ�ϵͳ�ű���System��\r\n\r\n[ Module : Name ] �� [ ģ�� : ���� ]\r\n���Դ���һ�������ֵ�ģ��ű���Module��";
                    string sc = SCRIPT == null ? "null" : SCRIPT.name;
                    TextDialog.Open($"{instructions}\r\n\r\n�ѱ�Ǽܹ���{mHub}Hub\r\n�ѱ��·����Assets/{mPath}\r\n�ѱ�������ռ䣺{mSpace}\r\n�ѱ��SO��Դ��{sc}\r\n");
                    mField.value = "";
                }
                else if (Eq(cmd, "clear", "����"))
                {
                    mField.value = null;
                    SCRIPT = null;
                    "����ɹ�".Log();
                }
                // ���ָ��
                else if (Eq(cmd, "check", "������"))
                {
                    if (!IsAsync)
                    {
                        string url = "https://gitee.com/PantyNeko/MeowFramework/raw/main/Assets/VersionInfo.txt";
                        RequestInfo(url, "���ڼ����� ���Ժ�...", txt =>
                        {
                            string[] res = txt.Split("@");
                            string version = HubTool.version;
                            if (res[0] == version)
                            {
                                TextDialog.Open($"��ǰΪ���°汾��[ {version} ] > �������\r\n{res[1]}");
                            }
                            else
                            {
                                TextDialog.Open($"��ǰ�汾��{version}\r\n���°汾��{res[0]}\r\n\r\n{res[1]}");
                            }
                            mField.value = "";
                        });
                    }
                }
                else // ����׺��ָ��
                {
                    string[] cmds = cmd.Split(':', '��');
                    string info = cmds[0].TrimEnd();
                    if (Eq(info, "space", "�����ռ�"))
                    {
                        if (cmds.Length > 1)
                        {
                            cmd = cmds[1].TrimStart();
                            if (char.IsLetterOrDigit(cmd[cmd.Length - 1]))
                                mSpace = cmd;
                        }
                        $"��ǰ�����ռ�Ϊ:{mSpace}".Log();
                        mField.value = "space:";
                    }
                    else if (Eq(info, "path", "·��"))
                    {
                        if (cmds.Length == 1)
                        {
                            $"��ǰ·��Ϊ:Assets/{mPath}".Log();
                        }
                        else
                        {
                            string sub = cmds[1].TrimStart();
                            string[] arr = sub.Split('/');

                            if (arr[0] == "Assets")
                                sub = sub.Substring(arr.Length > 1 ? 7 : 6);

                            if (Eq(sub, "base", "����"))
                            {
                                string[] fileNames = { "ArtRes/Sprites", "Resources/Audios/Bgm", "Resources/Audios/Sound", "Resources/Prefabs", "Scripts/Framework", "Project/Game", "StreamingAssets/Csv" };
                                for (int i = 0; i < fileNames.Length; i++)
                                {
                                    string path = $"Assets/{fileNames[i]}";
                                    if (FileKit.TryCreateDirectory(path)) $"{path}�����ɹ�".Log();
                                }
                                AssetDatabase.Refresh();
                            }
                            else
                            {
                                cmd = $"Assets/{sub}";
                                if (FileKit.EnsurePathExists(ref cmd))
                                {
                                    if (Directory.Exists(cmd))
                                    {
                                        $"·������,�ѱ��{cmd}".Log();
                                        mPath = sub;
                                        mField.value = "path:";
                                    }
                                    else if (EditorKit.Dialog($"ȷ��Ҫ������·����\r\nPath��{cmd}"))
                                    {
                                        mField.value = "path:";
                                        mPath = sub;
                                        $"{cmd}�����ɹ�,�ѱ�Ǹ�·��".Log();
                                        Directory.CreateDirectory(cmd);
                                        AssetDatabase.Refresh();
                                    }
                                    else $"ȡ������,���ԭ·�� Assets/{mPath}".Log();
                                }
                                else $"·�����Ϸ� {cmd}".Log();
                            }
                        }
                    }
                    else if (cmds.Length > 1)
                    {
                        string sub = cmds[1].TrimStart();
                        if (Eq(info, "SoIns", "Soʵ��"))
                        {
                            if (SCRIPT != null)
                            {
                                string path = $"Assets/{mPath}";
                                FileKit.TryCreateDirectory(path);
                                path = $"{path}/{sub}.asset";
                                if (File.Exists(path))
                                {
                                    $"{sub}.asset �Ѵ���".Log();
                                }
                                else
                                {
                                    string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(path);
                                    var instance = new SerializedObject(CreateInstance(SCRIPT.GetClass()));
                                    instance.Update();
                                    instance.ApplyModifiedPropertiesWithoutUndo();
                                    AssetDatabase.CreateAsset(instance.targetObject, uniqueFileName);
                                    AssetDatabase.ImportAsset(uniqueFileName);
                                    // ���serializedObject���Ա���������
                                    // serializedObject = null;
                                    $"{sub}.asset �����ɹ�".Log();
                                }
                            }
                        }
                        else if (Eq(info, "Module", "ģ��")) CreateModule(sub, "Module");
                        else if (Eq(info, "System", "ϵͳ")) CreateModule(sub, "System");
                        else if (Eq(info, "Model", "����")) CreateModule(sub, "Model");
                        else if (Eq(info, "Game", "��Ϸ")) CreateScript(sub, "Game");
                        else if (Eq(info, "UI", "����")) CreateScript(sub, "UI");
                        else if (Eq(info, "Mono", "�ű�")) CreateScript(sub, "Mono");
                        else if (Eq(info, "so", "SO")) CreateScript(sub, "So");
                        else if (Eq(info, "hub", "�ܹ�"))
                        {
                            string tmp = $"using UnityEngine;\r\n\r\nnamespace {mSpace}\r\n{{\r\n    public class @Hub : ModuleHub<@Hub>\r\n    {{\r\n        protected override void BuildModule()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n    public class @Game : MonoBehaviour, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n    public class @UI : UIPanel, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n}}";
                            EditorKit.CreatScript(mPath, sub, "Hub", tmp);
                            $"�ѱ��{sub}Hub�ܹ�".Log();
                            mField.value = $"{info}:";
                            mHub = sub;
                        }
                    }
                    else $"{cmd}ָ�����".Log();
                }
                evt.StopPropagation();
            }
        }
        private async void RequestInfo(string url, string tips, Action<string> call)
        {
            try
            {
                using (var client = new HttpClient())
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8))) // ���ó�ʱʱ��
                {
                    IsAsync = true;
                    mField.value = (tips);
                    var response = await client.GetAsync(url, cts.Token);
                    var content = response.EnsureSuccessStatusCode().Content;
                    call?.Invoke(await content.ReadAsStringAsync());
                }
            }
            catch (TaskCanceledException e)
            {
                bool trigger = e.CancellationToken.IsCancellationRequested;
                EditorKit.Tips(trigger ? "�����û�ȡ����" : "����ʱ!");
            }
            catch (HttpRequestException e)
            {
                EditorKit.Tips($"�������: {e.Message}");
            }
            finally
            {
                IsAsync = false;
            }
        }
    }
}