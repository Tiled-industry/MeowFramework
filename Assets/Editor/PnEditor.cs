﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Panty
{
    public abstract class PnEditor : EditorWindow
    {
        protected enum E_Path : byte
        {
            PersistentDataPath,
            StreamingAssetsPath,
            TemporaryCachePath,
            DesktopPath,
            DocumentsPath,
            DataPath,
            SelectPath,
            UpdateFK,
            Custom,
        }
        protected string inputText = "喵喵工具箱";
        protected bool IsAsync;
        protected E_Path mPath;
        protected GUIStyle HelpBoxStyle;
        protected bool mIsShowBtn = true, mShowBaseInfo, mDisabledInputBox = true, mCanInit = true;

        private (string name, Action call)[] btnInfos;

        protected GUILayoutOption[] btnLayoutOps;
        private GUIContent[] mMenuItemContent;
        private GenericMenu.MenuFunction[] mMenuItemFunc;

        protected const float textSpacing = 8f;
        protected const byte MaxLineItemCount = 4;

        private void OnEnable() => mCanInit = true;
        private void OnInspectorUpdate()
        {
            if (EditorApplication.isPlaying) Repaint();
        }
        private void OnGUI()
        {
            if (mCanInit)
            {
                btnLayoutOps = new GUILayoutOption[] { GUILayout.Height(30) };
                HelpBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(6, 6, 6, 6)
                };
                btnInfos = InitBtnInfo();
                var menu = RightClickMenu();
                if (menu != null && menu.Length > 0)
                {
                    mMenuItemContent = new GUIContent[menu.Length];
                    mMenuItemFunc = new GenericMenu.MenuFunction[menu.Length];
                    for (int i = 0; i < menu.Length; i++)
                    {
                        mMenuItemContent[i] = new GUIContent(menu[i].name);
                        mMenuItemFunc[i] = menu[i].call;
                    }
                }
                float len = btnInfos == null ? 0f : btnInfos.Length;
                // 计算最小窗口宽度为按钮宽度的总和加上间隔，再加上额外宽度
                float buttonWidth = GUI.skin.button.CalcSize(new GUIContent("0000000000")).x;
                float minWindowWidth = MaxLineItemCount * buttonWidth + (MaxLineItemCount - 1) * textSpacing;
                float btnLine = MathF.Ceiling(len / MaxLineItemCount);
                float minWindowHeight = 300 + (btnLine + 1) * 10;
                // 设置初始窗口位置为屏幕中央
                float x = Screen.currentResolution.width * 0.3f;
                float y = Screen.currentResolution.height * 0.1f;
                position = new Rect(x, y, minWindowWidth, minWindowHeight);
                mCanInit = false;
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            mIsShowBtn = GUILayout.Toggle(mIsShowBtn, "显示功能按钮");
            mDisabledInputBox = GUILayout.Toggle(mDisabledInputBox, "禁用输入框");
            mShowBaseInfo = GUILayout.Toggle(mShowBaseInfo, "基础信息");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(mDisabledInputBox);
            var op = GUILayout.MaxHeight(18);
            inputText = EditorGUILayout.TextField(inputText, op);
            EditorGUI.EndDisabledGroup();
            mPath = (E_Path)EditorGUILayout.EnumPopup(mPath);
            EditorGUILayout.EndHorizontal();

            if (Event.current.type == EventType.ContextClick)
            {
                // 创建右键菜单
                var menu = new GenericMenu();
                if (mMenuItemFunc != null)
                {
                    for (int i = 0; i < mMenuItemFunc.Length; i++)
                    {
                        menu.AddItem(mMenuItemContent[i], false, mMenuItemFunc[i]);
                    }
                }
                menu.AddItem(new GUIContent("创建基础目录"), false, () =>
                {
                    string[] fileNames = { "ArtRes/Sprites", "Resources/Audios/Bgm", "Resources/Audios/Sound", "Resources/Prefabs", "Scripts/Framework", "Project/Game", "StreamingAssets/Csv" };
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        string path = Application.dataPath + "/" + fileNames[i];
                        FileKit.TryCreateDirectory(path);
                    }
                    AssetDatabase.Refresh();
                });
                menu.AddItem(new GUIContent("拉取核心代码"), false, () =>
                {
                    if (IsAsync) return;
                    string url = "https://gitee.com/PantyNeko/MeowFramework/raw/main/Assets/FK/ModuleHub.cs";
                    RequestInfo(url, "正在拉取最新代码 请稍后...", txt =>
                    {
                        string msg = "点击 Yes 代码将传入剪贴板 使用 Ctrl + V 替换原始架构";
                        if (EditorKit.Dialog(msg)) GUIUtility.systemCopyBuffer = txt;
                    });
                });
                // 显示右键菜单
                menu.ShowAsContext();
                Event.current.Use();
            }
            EditorGUILayout.BeginHorizontal();
            if (OnClick("重置状态"))
            {
                mIsShowBtn = true;
                mDisabledInputBox = true;
                mShowBaseInfo = false;
                inputText = "状态已重置";
            }
            else if (OnClick("打开路径"))
            {
                inputText = mPath switch
                {
                    E_Path.DataPath => Application.dataPath,
                    E_Path.PersistentDataPath => Application.persistentDataPath,
                    E_Path.StreamingAssetsPath => Application.streamingAssetsPath,
                    E_Path.TemporaryCachePath => Application.temporaryCachePath,
                    E_Path.DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    E_Path.DocumentsPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    E_Path.SelectPath => "all",
                    E_Path.UpdateFK => "https://github.com/pantyneko/MeowFramework",
                    _ => mDisabledInputBox ? "https://gitee.com/PantyNeko" : inputText,
                };
                try
                {
                    if (string.IsNullOrEmpty(inputText)) inputText = "路径为空";
                    else if (inputText == "all")
                    {
                        foreach (var path in EditorKit.EnsureSelectedIsFolder())
                        {
                            System.Diagnostics.Process.Start(path);
                        }
                    }
                    else System.Diagnostics.Process.Start(inputText);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    inputText = ex.Message;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (mIsShowBtn)
            {
                GUILayout.Label("动态按钮区域 重写InitBtnInfo进行添加 ↓", HelpBoxStyle);
                ShowBtns(btnInfos);
            }
            ExtensionControl();

            if (mShowBaseInfo)
            {
                GUILayout.Label("动态调试信息 重写ShowLogInfo进行添加 ↓", HelpBoxStyle);
                ShowLogInfo();
            }
        }
        protected async void RequestInfo(string url, string tips, Action<string> call)
        {
            try
            {
                using (var client = new HttpClient())
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8))) // 设置超时时间
                {
                    IsAsync = true;
                    inputText = tips;
                    var response = await client.GetAsync(url, cts.Token);
                    var content = response.EnsureSuccessStatusCode().Content;
                    call?.Invoke(await content.ReadAsStringAsync());
                }
            }
            catch (TaskCanceledException e)
            {
                EditorKit.Tips(e.CancellationToken.IsCancellationRequested ? "请求被用户取消。" : "请求超时!");
            }
            catch (HttpRequestException e)
            {
                EditorKit.Tips($"请求错误: {e.Message}");
            }
            finally
            {
                IsAsync = false;
            }
        }
        protected virtual void ShowLogInfo()
        {
            GUILayout.Label($"世界 : {Camera.main.ScreenToWorldPoint(Input.mousePosition)}");
            GUILayout.Label($"视口 : {Camera.main.ScreenToViewportPoint(Input.mousePosition)}");
            GUILayout.Label($"屏幕 : {Input.mousePosition}");
            GUILayout.Label("帧率 : " + (1F / Time.deltaTime).ToString("F0"));
        }
        private void ShowBtns((string name, Action call)[] btns)
        {
            if (btns == null) return;
            int row = btns.Length / MaxLineItemCount;
            if (btns.Length > MaxLineItemCount)
            {
                // 如果按钮的数量大于一行 调整按钮行布局，三行按钮
                for (byte r = 0; r < row; r++)
                {
                    EditorGUILayout.BeginHorizontal();
                    int index = r * MaxLineItemCount;
                    for (byte i = 0; i < MaxLineItemCount; i++)
                    {
                        var pair = btns[i + index];
                        if (OnClick(pair.name))
                        {
                            pair.call();
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.BeginHorizontal();
            int num = btns.Length % MaxLineItemCount;
            int max = row * MaxLineItemCount;

            for (int i = 0; i < num; i++)
            {
                var pair = btns[i + max];
                if (OnClick(pair.name))
                {
                    pair.call();
                    break;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        protected virtual void ExtensionControl() { }
        protected virtual (string name, GenericMenu.MenuFunction call)[] RightClickMenu() => null;
        protected virtual (string, Action)[] InitBtnInfo() => null;
        protected bool OnClick(string name)
        {
            return GUILayout.Button(name, btnLayoutOps) && Event.current.button == 0;
        }
    }
}