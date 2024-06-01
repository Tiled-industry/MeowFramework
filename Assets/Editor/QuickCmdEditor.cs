using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Panty
{
    public class QuickCmdEditor : EditorWindow
    {
        private string mCmd = "#help";
        private string mPath = "Codes";
        private string mSpace = "Panty.Test";
        private string mHub;
        private TextField mField;

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
            mCmd = EditorPrefs.GetString($"{nameof(QuickCmdEditor)}Cmd", mCmd);
            mPath = EditorPrefs.GetString($"{nameof(QuickCmdEditor)}Path", mPath);
            mSpace = EditorPrefs.GetString($"{nameof(QuickCmdEditor)}Space", mSpace);
            mHub = EditorPrefs.GetString($"{nameof(QuickCmdEditor)}Hub", mHub);
        }
        private void OnDisable()
        {
            EditorPrefs.SetString($"{nameof(QuickCmdEditor)}Cmd", mCmd);
            EditorPrefs.SetString($"{nameof(QuickCmdEditor)}Path", mPath);
            EditorPrefs.SetString($"{nameof(QuickCmdEditor)}Space", mSpace);
            if (string.IsNullOrEmpty(mHub)) return;
            EditorPrefs.SetString($"{nameof(QuickCmdEditor)}Hub", mHub);
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
            // ��TextField��ӵ���Ԫ��
            root.Add(mField);
        }
        private static bool Eq(string source, string en, string ch) => StringComparer.OrdinalIgnoreCase.Equals(source, en) || source == ch;
        private void CreateModule(string name, string tag)
        {
            string tmp = $"namespace {mSpace}\r\n{{\r\n    public interface I@{tag} : IModule\r\n    {{\r\n\r\n    }}\r\n    public class @{tag} : AbsModule, I@{tag}\r\n    {{\r\n        protected override void OnInit()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}";
            EditorKit.CreatScript(mPath, name, tag, tmp);
            mCmd = $"F:{tag}:";
            mField.SetValueWithoutNotify(mCmd);
        }
        private void CreateScript(string name, string tag, string type = null)
        {
            if (string.IsNullOrEmpty(mHub))
            {
                mCmd = "F:hub:";
                $"�������üܹ� {mCmd}�ܹ���".Log();
                mField.SetValueWithoutNotify(mCmd);
                return;
            }
            string father = type == null ? "MonoBehaviour" : mHub + type;
            string tmp = $"using UnityEngine;\r\n\r\nnamespace {mSpace}\r\n{{\r\n    public class @ : {father}\r\n    {{\r\n\r\n    }}\r\n}}";
            EditorKit.CreatScript(mPath, name, "", tmp);
            mCmd = $"F:{tag}:";
            mField.SetValueWithoutNotify(mCmd);
        }
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                if (string.IsNullOrEmpty(mCmd)) return; // ���ܷ�������
                string cmd = mCmd.Trim(); // ȥ��ͷβ�Ŀո�
                if (cmd[0] == '#') // ˵���ǿ��ָ��
                {
                    cmd = cmd.TrimStart('#', ' ');
                    if (!string.IsNullOrEmpty(cmd))
                    {
                        if (Eq(cmd, "help", "����"))
                        {
                            string instructions = $"// ����ָ���Ѽ��ݴ�Сд��ȫ�ǰ��\r\n\r\n[#  ] : ���ָ�� �����׺  ���磺#help\r\n[    ] : ���ָ�� ͨ����ָ��ͷ+����+������� ���磺@f:hub:Project\r\n[�� ] : �ָ���(ָʾ����) ���磺@ָ��:����:����\r\n[f/p]�� ��ʶ�����ļ���·�� δ��\r\n\r\n// ���ָ��\r\n\r\n#help \r\n��ʾ������� ��ʾһЩ��ʾ��Ϣ Ҳ��ʹ�� #����\r\n#path \r\n��Console ������ʾ�ѱ��·�� Ҳ��ʹ�� #·��\r\n#space \r\n��Console ������ʾ�ѱ�������ռ� Ҳ����ʹ�� #�����ռ�\r\n\r\n// ����ָ��\r\n\r\n#path:·���ַ���\r\n����Զ���·�� ���·�������� ���Դ���Ŀ¼\r\n\r\n#space:�����ռ��ַ���\r\n����Զ��������ռ� ������\r\n\r\n// ������ָ��  �����ļ���·���� #path �ı��Ϊ׼\r\n\r\nf:hub:Name �� f:�ܹ�:�ܹ���\r\n���Դ���һ�������ֵļܹ�  \r\n\r\nf:Mono:Name �� f:�ű�:�ű���\r\n���Դ���һ�������ֵ���ͨ�ű�  \r\n\r\nf:UI:Name �� f:����:�ű���\r\n���Դ���һ�������ֵ�UI�ű�  \r\n\r\nf:Game:Name �� f:��Ϸ:�ű���\r\n���Դ���һ�������ֵ�Game�ű� \r\n\r\nf:Model:Name �� f:����:�ű���\r\n���Դ���һ�������ֵ�Model�ű� \r\n\r\nf:System:Name �� f:ϵͳ:�ű���\r\n���Դ���һ�������ֵ�System�ű� \r\n\r\nf:Module:Name �� f:ģ��:�ű���\r\n���Դ���һ�������ֵ�Module�ű� ";
                            TextDialog.Open($"{instructions}\r\n\r\n�ѱ��·����Assets/{mPath}\r\n�ѱ�������ռ䣺{mSpace}\r\n�ѱ�Ǽܹ���{mHub}Hub\r\n");
                        }
                        else
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
                            }
                            else if (Eq(info, "path", "·��"))
                            {
                                if (cmds.Length == 1)
                                {
                                    $"��ǰ·��Ϊ:Assets/{mPath}".Log();
                                }
                                else
                                {
                                    string sub = cmds[1].Trim();
                                    cmd = "Assets/" + sub;
                                    if (FileKit.EnsurePathExists(ref cmd))
                                    {
                                        if (!Directory.Exists(cmd))
                                        {
                                            if (EditorKit.Dialog($"ȷ��Ҫ������·����\r\nPath��{cmd}"))
                                            {
                                                mPath = sub;
                                                $"{cmd}�����ɹ�,�ѱ�Ǹ�·��".Log();
                                                Directory.CreateDirectory(cmd);
                                                AssetDatabase.Refresh();
                                            }
                                            else $"ȡ������,���ԭ·�� Assets/{mPath}".Log();
                                        }
                                        else
                                        {
                                            $"·������,�ѱ��{cmd}".Log();
                                            mPath = sub;
                                        }
                                    }
                                    else $"·�����Ϸ� {cmd}".Log();
                                }
                            }
                        }
                    }
                }
                else
                {
                    string[] cmds = cmd.Split(':', '��');
                    if (cmds.Length >= 3)
                    {
                        string inst = cmds[0].Trim();
                        switch (inst)
                        {
                            case "F":
                            case "f":
                                string info = cmds[1].Trim();
                                // �����ļ�
                                string name = cmds[2].TrimStart();
                                if (Eq(info, "Module", "ģ��")) CreateModule(name, "Module");
                                else if (Eq(info, "System", "ϵͳ")) CreateModule(name, "System");
                                else if (Eq(info, "Model", "����")) CreateModule(name, "Model");
                                else if (Eq(info, "Game", "��Ϸ")) CreateScript(name, "Game", "Game");
                                else if (Eq(info, "UI", "����")) CreateScript(name, "UI", "UI");
                                else if (Eq(info, "Mono", "�ű�")) CreateScript(name, "Mono");
                                else if (Eq(info, "hub", "�ܹ�"))
                                {
                                    string tmp = $"using UnityEngine;\r\n\r\nnamespace {mSpace}\r\n{{\r\n    public class @Hub : ModuleHub<@Hub>\r\n    {{\r\n        protected override void BuildModule()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n    public class @Game : MonoBehaviour, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n    public class @UI : UIPanel, IPermissionProvider\r\n    {{\r\n        IModuleHub IPermissionProvider.Hub => @Hub.GetIns();\r\n    }}\r\n}}";
                                    EditorKit.CreatScript(mPath, name, "Hub", tmp);
                                    mCmd = $"F:{info}:";
                                    mField.SetValueWithoutNotify(mCmd);
                                    mHub = name;
                                }
                                else $"{cmd}ָ�����".Log();
                                break;
                        }
                    }
                }
                evt.StopPropagation();
            }
        }
    }
}