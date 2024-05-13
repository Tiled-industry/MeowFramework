using UnityEngine;
using UnityEngine.UI;

namespace Panty
{
    /// <summary>
    /// UI���� ��װ��������� �Լ� ע��ί�м�ʹ��[����ʵ�� IController �ӿ�]
    /// �ṩ��ʾ�����ص���Ϊ Ĭ��Awake��ע�������Ӷ���İ�ť
    /// </summary>
    public class UIPanel : MonoBehaviour
    {
        public enum Layer : byte { Top, Mid, Bot, Sys }
        public virtual void OnShow() { }
        public virtual void OnHide() { }
        // ������ò�Ҫд����
        public virtual void Activate(bool active) =>
            gameObject.SetActive(active);
        protected virtual void OnClick(string btnName) { }
        public virtual bool IsOpen => gameObject.activeSelf;
        protected virtual void Awake()
        {
            this.FindChildrenControl<Button>((objName, control) =>
            control.onClick.AddListener(() => OnClick(objName)));
        }
    }
}