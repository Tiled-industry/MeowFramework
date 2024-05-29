using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Panty
{
    public class QuickCmdEditor : EditorWindow
    {
        private string inputText;

        [MenuItem("PnTool/QuickCmd &Q")]
        private static void OpenSelf()
        {
            var window = GetWindow<QuickCmdEditor>("QuickCmdEditor");
            window.maxSize = window.minSize = new Vector2(820f, 80f);
        }
        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));

            var instructionLabel = new Label
            {
                text = "[@]-���� [s]-ѡ�� [#]-���� [f/p]-�ļ�/·�� [:]-�ָ���(ָʾ����) [Name]-�ļ����ļ����� [hub]-�ܹ� [module]-ģ�� [mono]-�ű� [cs]-��",
                style =
                {
                    fontSize = 12,
                    color = new StyleColor(Color.gray),
                    unityTextAlign = TextAnchor.MiddleCenter,
                }
            };
            var msgLabel = new Label
            {
                text = "ʹ�� [#:f:Project:hub] ���� ProjectHub �ܹ�����",
                style =
                {
                    fontSize = 12,
                    color = new StyleColor(Color.gray),
                    unityTextAlign = TextAnchor.MiddleCenter,
                }
            };
            // ����������TextField��ʽ
            var inputField = new TextField
            {
                value = inputText,
                style =
                {
                    fontSize = 24,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    height = 40,
                    flexGrow = 1
                }
            };
            inputField.RegisterCallback<ChangeEvent<string>>(evt => inputText = evt.newValue);
            // ��TextField��ӵ���Ԫ��
            root.Add(inputField);
            root.Add(instructionLabel);
            root.Add(msgLabel);
        }
    }
}