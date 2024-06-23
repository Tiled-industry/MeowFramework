using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text;
using System.Linq;
using UnityEditor.SceneManagement;
using System.Reflection;

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

        private TextField mField;
        private static MonoScript SCRIPT;
        private static PArray<GameObject> mGos = new PArray<GameObject>();

        [MenuItem("PnTool/QuickCmd &Q")]
        private static void OpenSelf()
        {
            if (EditorKit.ShowOrHide<QuickCmdEditor>(out var win))
            {
                win.maxSize = new Vector2(960f, 40f);
                win.minSize = new Vector2(360f, 40f);
            }
        }
        [MenuItem("PnTool/Quick/AddBind &B")]
        private static void AddBind()
        {
            if (Selection.objects.Length == 0) return;
            foreach (var go in Selection.objects.OfType<GameObject>())
            {
                if (go == null) continue;
                var bind = go.GetOrAddComponent<Bind>();
                if (mGos.Count == 1) bind.root = mGos[0];
                EditorUtility.SetDirty(go);
                EditorSceneManager.MarkSceneDirty(go.scene);
            }
        }
        [MenuItem("PnTool/Quick/AddUIRoot &W")]
        private static void AddUIRoot()
        {
            if (Selection.objects.Length == 0) return;
            foreach (var go in Selection.objects.OfType<GameObject>())
            {
                if (go == null) continue;
                AddUIRoot(go);
            }
        }
        private static void GetPathFD(string fileName, out string assetPath)
        {
            assetPath = EditorKit.GetMonoPath(fileName, $"{fileName}.cs");
            assetPath = $"{Path.GetDirectoryName(assetPath)}/F{fileName}.cs";
            if (!File.Exists(assetPath))
            {
                var psth = EditorKit.GetMonoPath(fileName, $"F{fileName}.cs");
                if (psth != null) assetPath = psth;
            }
        }
        private static string GetPrefix(Bind.E_Type type)
        {
            return type switch
            {
                Bind.E_Type.Button => "btn",
                Bind.E_Type.Canvas => "cnv",
                Bind.E_Type.Image => "img",
                Bind.E_Type.Text => "txt",
                Bind.E_Type.Toggle => "tgl",
                Bind.E_Type.Slider => "sld",
                Bind.E_Type.Transform => "tr",
                Bind.E_Type.Dropdown => "drpDwn",
                Bind.E_Type.Scrollbar => "scrlBr",
                Bind.E_Type.ScrollRect => "scrlRt",
                Bind.E_Type.InputField => "inpFld",
                Bind.E_Type.RawImage => "rwImg",
                _ => ""
            };
        }
        private static string CreateUIRootMono(string fileName)
        {
            string tmple = $"using UnityEngine;\r\n\r\nnamespace {I.Space}\r\n{{\r\n    [DisallowMultipleComponent]\r\n    public partial class {fileName} : {I.Hub}UI\r\n    {{\r\n\r\n    }}\r\n}}";
            File.WriteAllText($"{I.TPath}/{fileName}.cs", tmple);
            return $"{I.TPath}/F{fileName}.cs";
        }
        private static void AddUIRoot(GameObject go)
        {
            var binds = go.GetComponentsInChildren<Bind>();
            if (binds.Length == 0)
            {
                "û�пɰ󶨶���".Log();
            }
            else
            {
                string assetPath = null;
                var cp = go.GetComponent<UIPanel>();
                string fileName = $"R_{go.name}";
                string full = $"{I.Space}.{fileName}";
                var type = HubTool.BaseAss.GetType(full);

                if (cp && cp.GetType().FullName == full)
                {
                    GetPathFD(fileName, out assetPath);
                    SetRootData(type, binds, go);
                    $"{type.Name}�ű��ѹ���".Log();
                }
                else
                {
                    // ˵��û�������Դ
                    if (type == null)
                    {
                        assetPath = CreateUIRootMono(fileName);
                        type = HubTool.BaseAss.GetType(full);
                        (type == null ? "�����򼯲����ڸ���" : $"����{type.Name}").Log();
                        "������Դˢ����ɺ� �ٴδ�����ȷ���ű��Ĺ���".Log();
                    }
                    else
                    {
                        if (type.IsSubclassOf(typeof(Component)))
                        {
                            GetPathFD(fileName, out assetPath);
                            SetRootData(type, binds, go);
                            $"{type.Name}�ű��Ѵ��� ����ˢ��������".Log();
                        }
                        else // ˵��ֻ��������
                        {
                            assetPath = CreateUIRootMono(fileName);
                            $"ֻ�ҵ������� ���¹���{type.Name} ������Դˢ����ɺ� �ٴδ�����ȷ���ű��Ĺ���".Log();
                        }
                    }
                }
                string[] data = $"using UnityEngine;\r\nusing UnityEngine.UI;\r\n\r\nnamespace {I.Space}\r\n{{\r\n    public partial class {fileName}\r\n    {{\r\n        @\r\n    }}\r\n}}".Split('@');
                var bd = new StringBuilder(data[0]);
                for (int i = 0, len = binds.Length; i < len; i++)
                {
                    var bind = binds[i];
                    if (bind.root == null)
                        $"{bind}��Root is Null".Log();
                    else if (bind.root == go)
                    {
                        if (i > 1) bd.Append("\t\t");
                        bd.Append($"[SerializeField] private {bind.type} {HandleBind(bind)};");
                        if (i < len - 1) bd.Append("\r\n");
                    }
                    else $"{bind.root}�����ڵ�ǰ����".Log();
                }
                bd.Append(data[1]);
                FileKit.WriteFile(assetPath, bd.ToString());
                AssetDatabase.Refresh();
                $"{fileName}�����Ѹ���".Log();
            }
        }
        private static void SetRootData(Type rootType, Bind[] binds, GameObject go)
        {
            var cmpnt = go.GetOrAddComponent(rootType);
            var fields = rootType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0, len = binds.Length; i < len; i++)
            {
                var bind = binds[i];
                if (bind.root == null) continue;
                if (bind.root == go)
                {
                    string n = HandleBind(bind).Trim();
                    var info = fields.FirstOrDefault(t => t.Name == n);
                    if (info == null) continue;
                    var bindCp = bind.GetComponent(info.FieldType);
                    info.SetValue(cmpnt, Convert.ChangeType(bindCp, info.FieldType));
                }
            }
            EditorUtility.SetDirty(cmpnt);
            EditorSceneManager.MarkSceneDirty(go.scene);
        }
        private static string HandleBind(Bind bind)
        {
            string goName = bind.gameObject.name;
            if (string.IsNullOrWhiteSpace(goName)) bind.usePrefix = true;
            else goName = goName.RemoveSpecialCharacters();
            string prefix = GetPrefix(bind.type) + "_";
            prefix = bind.usePrefix ? prefix : char.IsDigit(goName[0]) ? prefix : "";
            return prefix + goName;
        }
        private void OnEnable()
        {
            string n = nameof(QuickCmdEditor);
            mCmd = EditorPrefs.GetString($"{n}Cmd", mCmd);
            I.TPath = EditorPrefs.GetString($"{n}Path", I.TPath);
            I.Space = EditorPrefs.GetString($"{n}Space", I.Space);
            I.Hub = EditorPrefs.GetString($"{n}Hub", I.Hub);
        }
        private void OnDisable()
        {
            string n = nameof(QuickCmdEditor);
            EditorPrefs.SetString($"{n}Cmd", mCmd);
            EditorPrefs.SetString($"{n}Path", I.TPath);
            if (!string.IsNullOrEmpty(I.Space))
                EditorPrefs.SetString($"{n}Space", I.Space);
            if (!string.IsNullOrEmpty(I.Hub))
                EditorPrefs.SetString($"{n}Hub", I.Hub);
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
            mField.RegisterCallback<ChangeEvent<string>>(OnChangeText);
            mField.RegisterCallback<KeyDownEvent>(OnKeyDown);
            mField.RegisterCallback<DragPerformEvent>(OnDragPerform);
            mField.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);

            mField.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            // ��TextField��ӵ���Ԫ��
            root.Add(mField);
        }
        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // ������в˵���
            evt.menu.MenuItems().Clear();
            // ����µĲ˵���
            evt.menu.AppendAction("��ʾ����", e => ShowHelp());
            evt.menu.AppendAction("�� UI", e => OnUIBind());
            evt.menu.AppendAction("����Ŀ¼", e => BasicCatalog());
            evt.menu.AppendAction("������", e => CheckUpdate());
            evt.menu.AppendAction("�������", e =>
            {
                if (EditorKit.Dialog("���Ҫ��������"))
                {
                    ClearInfo();
                }
            });
        }
        private void OnChangeText(ChangeEvent<string> evt) => mCmd = evt.newValue;
        private void ChangeField(string value) => mField.value = value;
        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            evt.StopImmediatePropagation();
        }
        private void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();
            if (DragAndDrop.paths.Length == 1)
            {
                if (evt.ctrlKey)
                {
                    I.TPath = Path.GetDirectoryName(DragAndDrop.paths[0]);
                    $"�ѱ��:{I.TPath}".Log();
                }
                else
                {
                    var o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(DragAndDrop.paths[0]);
                    if (o is MonoScript mono)
                    {
                        Type scriptType = mono.GetClass();
                        if (scriptType == null)
                        {
                            "�˽ű�δ���������ڱ������".Log();
                        }
                        else if (scriptType.IsSubclassOf(typeof(ScriptableObject)) &&
                                !scriptType.IsSubclassOf(typeof(EditorWindow)))
                        {
                            SCRIPT = mono;
                            mField.value = "SoIns:" + mono.name;
                            $"SO => {mono.name}�ѱ��".Log();
                        }
                        else $"{mono.name}����SO".Log();
                    }
                    else "�޷���ȡ��ҷ����".Log();
                }
            }
            else
            {
                var refs = DragAndDrop.objectReferences;
                if (refs.Length > 0)
                {
                    mGos.ToFirst();
                    foreach (var obj in refs.OfType<GameObject>())
                    {
                        mGos.Push(obj);
                        mField.value = $"�ѻ���{obj.name}��GameObject";
                    }
                }
            }
            evt.StopImmediatePropagation();
        }
        private static bool Eq(string source, string en, string ch) =>
            StringComparer.OrdinalIgnoreCase.Equals(source, en) || source == ch;
        private void CreateModule(string name, string tag)
        {
            string tmp = $"namespace {I.Space}\r\n{{\r\n    public interface I@{tag} : IModule\r\n    {{\r\n\r\n    }}\r\n    public class @{tag} : AbsModule, I@{tag}\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}";
            EditorKit.CreatScript(I.TPath, name, tag, tmp);
            mField.value = $"{tag}:";
        }
        private void CreateScript(string name, string tag)
        {
            if (string.IsNullOrEmpty(I.Hub))
            {
                mField.value = "hub:";
                $"�������üܹ� {mCmd}�ܹ���".Log();
                return;
            }
            string father = tag switch
            {
                "Mono" => "MonoBehaviour",
                "So" => "ScriptableObject",
                _ => I.Hub + tag
            };
            string tmp = $"using UnityEngine;\r\n\r\nnamespace {I.Space}\r\n{{\r\n    public class @ : {father}\r\n    {{\r\n\r\n    }}\r\n}}";
            EditorKit.CreatScript(I.TPath, name, "", tmp);
            mField.value = $"{tag}:";
        }
        private void ShowHelp()
        {
            string instructions = $"# Panty ���߼�ʹ���ֲ�\r\n\r\n```\r\n==============================\r\n  Panty ���߼�ʹ���ֲ�\r\n==============================\r\n\r\nĿ¼:\r\n1. TextDialog ʹ��ָ��\r\n2. QuickCmdEditor ʹ��ָ��\r\n   - ��������\r\n   - ����˵��\r\n   - ��ҷ�����������Ĳ˵�\r\n\r\n------------------------------\r\n1. TextDialog ʹ��ָ��\r\n------------------------------\r\n\r\n����ʾ��:\r\n1. ���� TextDialog.Open ������������Ҫ��ʾ����Ϣ�Ϳ�ѡ�Ļص�����:\r\n   TextDialog.Open(\"�����Ϣ\", ȷ�ϻص�, ȡ���ص�);\r\n2. ����ʾ���У��û�����ͨ�����ȷ�ϻ�ȡ����ť������Ӧ�Ĳ�����\r\n\r\n------------------------------\r\n2. QuickCmdEditor ʹ��ָ��\r\n------------------------------\r\n\r\n### ��������\r\n\r\n���ٴ�:\r\n1. �� Unity �༭�������˵���ѡ�� PnTool/QuickCmd &Q �� QuickCmdEditor ���ڡ�\r\n   - ��ݼ�: Alt + Q\r\n\r\n��Ӱ�:\r\n1. �ڳ�����ѡ��һ������ GameObject��\r\n2. �� Unity �༭�������˵���ѡ�� PnTool/Quick/AddBind &B Ϊ��ѡ������Ӱ������\r\n   - ��ݼ�: Alt + B\r\n\r\n���UI���ڵ�:\r\n1. �ڳ�����ѡ��һ������ GameObject��\r\n2. �� Unity �༭�������˵���ѡ�� PnTool/Quick/AddUIRoot &W ����ѡ��������Ϊ UI ���ڵ㡣\r\n   - ��ݼ�: Alt + W\r\n\r\n������:\r\n1. �� QuickCmdEditor ���ڡ�\r\n2. ����������������� check �� �����£�Ȼ�󰴻س�����\r\n3. ���򽫻�����²���ʾ��Ӧ��Ϣ��\r\n   - �����Ĳ˵�: �Ҽ�������������ѡ�� �����¡�\r\n\r\n### ����˵��\r\n\r\n��������:\r\n- ����: ���� help �� ���� �鿴������Ϣ��\r\n  - �����Ĳ˵�: �Ҽ�������������ѡ�� ��ʾ������\r\n- ����: ���� clear �� ���� ����ѱ�ǵ���Ϣ��\r\n  - �����Ĳ˵�: �Ҽ�������������ѡ�� ������ݡ�\r\n- UI��: ���� uiBind �� UI�� ���� UI �󶨲�����\r\n  - �����Ĳ˵�: �Ҽ�������������ѡ�� �� UI��\r\n- ������: ���� check �� ������ �����¡�\r\n  - �����Ĳ˵�: �Ҽ�������������ѡ�� �����¡�\r\n\r\nģ��ͽű���������:\r\n- �����ռ�: ���� space:�����ռ� ���������ռ䣬���� space:MyNamespace��\r\n- ·��: ���� path:·�� ����·�������� path:Assets/MyPath������ path:base �� path:���� ��������Ŀ¼��\r\n- SoIns: ���� ScriptableObject ʵ�������� SoIns:MyScriptableObject��\r\n- Module: ����ģ�顣���� Module:MyModule��\r\n- System: ����ϵͳ������ System:MySystem��\r\n- Model: ��������ģ�͡����� Model:MyModel��\r\n- Game: ������Ϸ�ű������� Game:MyGameScript��\r\n- UI: �������ֲ�ű������� UI:MyUIScript��\r\n- Mono: ���� MonoBehaviour �ű������� Mono:MyMonoScript��\r\n- so: ���� ScriptableObject �ű������� so:MyScriptableObject��\r\n- hub: �����ܹ������� hub:MyHub��\r\n\r\n### ��ҷ�����������Ĳ˵�\r\n\r\n��ҷ����:\r\n1. ��һ�� MonoScript �ű��ļ��ϵ� QuickCmdEditor �����У�����ס Ctrl ���Ա��·����\r\n2. ��һ������ GameObject �ϵ� QuickCmdEditor �����У��Ի�����Щ���󹩺���������\r\n\r\n�����Ĳ˵�:\r\n1. �Ҽ���� QuickCmdEditor ���������ɵ��������Ĳ˵���ѡ�����²���:\r\n   - ��ʾ����: �鿴������Ϣ��\r\n   - �� UI: ���� UI �󶨲�����\r\n   - ����Ŀ¼: ���û���Ŀ¼��\r\n   - ������: �����¡�\r\n   - �������: ����ѱ�ǵ���Ϣ��\r\n\r\nͨ�����ϲ������û����Կ������� Panty ���߼��� Unity �༭���н��и��ֱ�ݵĲ�����\r\n```";
            string sc = SCRIPT == null ? "null" : SCRIPT.name;
            TextDialog.Open($"{instructions}\r\n\r\n��Ǽܹ���{I.Hub}Hub\r\n���·����{I.TPath}\r\n��������ռ䣺{I.Space}\r\n�������·����{I.Search[0]}\r\n���SO��Դ��{sc}\r\n");
            mField.value = "";
        }
        private void ClearInfo()
        {
            mField.value = null;
            SCRIPT = null;
            I.TPath = "Assets/Scripts";
            I.Search[0] = "Assets";
            $"SCRIPT���ÿ�,����·��:{I.TPath},����·��{I.Search[0]}".Log();
        }
        private void BasicCatalog()
        {
            bool lose = true;
            string[] fileNames = { "ArtRes/Sprites", "Resources/Audios/Bgm", "Resources/Audios/Sound", "Resources/Prefabs", "Scripts/Framework", "Scripts/Project/Game", "StreamingAssets/Csv" };
            for (int i = 0; i < fileNames.Length; i++)
            {
                string path = $"Assets/{fileNames[i]}";
                if (FileKit.TryCreateDirectory(path))
                {
                    $"{path}�����ɹ�".Log();
                    lose = false;
                }
            }
            I.Search[0] = "Assets/Scripts";
            if (lose) "�����ļ����Ѿ�λ".Log();
            else AssetDatabase.Refresh();
        }
        private void CheckUpdate()
        {
            if (IsAsync) return;
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
        private void OnUIBind()
        {
            if (mGos.Count == 0)
                "�޿ɲ�������".Log();
            else
            {
                foreach (var go in mGos)
                {
                    if (go == null) continue;
                    AddUIRoot(go);
                }
                mGos.ToFirst();
            }
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
                if (Eq(cmd, "uiBind", "UI��")) OnUIBind();
                else if (Eq(cmd, "help", "����")) ShowHelp();
                else if (Eq(cmd, "clear", "����")) ClearInfo();
                else if (Eq(cmd, "check", "������")) CheckUpdate();
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
                                I.Space = cmd;
                        }
                        $"��ǰ�����ռ�Ϊ:{I.Space}".Log();
                        mField.value = "space:";
                    }
                    else if (Eq(info, "path", "·��"))
                    {
                        if (cmds.Length == 1)
                        {
                            $"��ǰ·��Ϊ:{I.TPath}".Log();
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(cmds[1]))
                            {
                                $"��ǰ·��Ϊ:{I.TPath}".Log();
                            }
                            else
                            {
                                // �õ�ʵ��·�� ��ȥ���ո� 2����� ���·��ģʽ ����·��ģʽ
                                string sub = cmds[1].Trim();
                                if (char.IsDigit(sub[0]))
                                {
                                    "ָ��������ֿ�ͷ".Log();
                                }
                                else if (Eq(sub, "base", "����"))
                                    BasicCatalog();
                                else
                                {
                                    // ���·������
                                    if (sub[0] == '/') cmd = I.TPath + sub;
                                    // ���û�и�Ŀ¼ ��Ҫ���ϸ�Ŀ¼
                                    else cmd = sub.Split('/')[0] == "Assets" ? sub : $"Assets/{sub}";
                                    // ����·���Ϸ����ж�
                                    if (FileKit.EnsurePathExists(ref cmd))
                                    {
                                        if (Directory.Exists(cmd))
                                        {
                                            $"·������,�ѱ��{cmd}".Log();
                                            I.TPath = cmd;
                                            mField.value = "path:";
                                        }
                                        else if (EditorKit.Dialog($"ȷ��Ҫ������·����\r\nPath��{cmd}"))
                                        {
                                            mField.value = "path:";
                                            I.TPath = cmd;
                                            $"{cmd}�����ɹ�,�ѱ�Ǹ�·��".Log();
                                            Directory.CreateDirectory(cmd);
                                            AssetDatabase.Refresh();
                                        }
                                        else $"ȡ������,���ԭ·�� {I.TPath}".Log();
                                    }
                                    else $"·�����Ϸ� {cmd}".Log();
                                }
                            }
                        }
                    }
                    else if (cmds.Length > 1)
                    {
                        string sub = cmds[1].TrimStart();
                        if (char.IsDigit(sub[0]))
                        {
                            "ָ��������ֿ�ͷ".Log();
                        }
                        else if (Eq(info, "SoIns", "Soʵ��"))
                        {
                            if (SCRIPT != null)
                            {
                                string path = I.TPath;
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
                            string tmp = $"using UnityEngine;\r\n\r\nnamespace {I.Space}\r\n{{\r\n    public class @Hub : ModuleHub<@Hub>\r\n    {{\r\n        protected override void BuildModule()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n    public class @Game : MonoBehaviour, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n    public class @UI : UIPanel, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n}}";
                            EditorKit.CreatScript(I.TPath, sub, "Hub", tmp);
                            $"�ѱ��{sub}Hub�ܹ�".Log();
                            mField.value = $"{info}:";
                            I.Hub = sub;
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