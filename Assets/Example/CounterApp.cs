using System;
using UnityEngine;

namespace Panty.Test
{
    public interface ICounterModel : IModule
    {
        ValueBinder<float> A { get; }
        ValueBinder<float> B { get; }
        string GetOpIcon(int id);
        string[] GetItems();
    }
    public class CounterModel : AbsModule, ICounterModel
    {
        ValueBinder<float> ICounterModel.A { get; } = new ValueBinder<float>();
        ValueBinder<float> ICounterModel.B { get; } = new ValueBinder<float>();

        private string[] Items;

        protected override void OnInit()
        {
            "��һ�ε��� ��ģ�� ʱ ִ��".Log();
            Items = new string[] { "+", "-", "*", "/" };
        }
        protected override void OnDeInit()
        {
            "��Ӧ�û�༭���˳� ʱ ִ��".Log();
        }
        string ICounterModel.GetOpIcon(int id)
        {
            return Items[id];
        }
        string[] ICounterModel.GetItems()
        {
            return Items;
        }
    }
    public struct ChangeOpCmd : ICmd<int>
    {
        public void Do(IModuleHub hub, int id)
        {
            var model = hub.Module<ICounterModel>();
            hub.SendEvent(new ChangeOpIconEvent() { icon = model.GetOpIcon(id) });
            hub.SendNotify<OperationSuccessfulNotify>();
        }
    }
    public struct RandomValueCmd : ICmd
    {
        public void Do(IModuleHub hub)
        {
            var model = hub.Module<ICounterModel>();
            model.A.Value = UnityEngine.Random.Range(1, 100);
            model.B.Value = UnityEngine.Random.Range(1, 100);
        }
    }
    public struct ResultQuery : IQuery<CounterApp.Op, float>
    {
        public float Do(IModuleHub hub, CounterApp.Op op)
        {
            var model = hub.Module<ICounterModel>();
            float a = model.A.Value;
            float b = model.B.Value;
            return op switch
            {
                CounterApp.Op.Add => a + b,
                CounterApp.Op.Sub => a - b,
                CounterApp.Op.Mul => a * b,
                CounterApp.Op.Div => a / b,
                _ => throw new Exception("δʶ�������"),
            };
        }
    }
    public struct ChangeOpIconEvent
    {
        public string icon;
    }
    public struct OperationSuccessfulNotify { }
    public struct OperationFailedNotify { }

    public class CounterApp : CounterGame
    {
        public enum Op : byte
        {
            Add, Sub, Mul, Div
        }
        private float startW, startH;
        private GUIStyle style, btnStyle, inputStyle;
        private string A, B, R;
        private string opText = "+";

        private int mSelect;
        private bool ShowList;

        private ICounterModel model;

        private void Start()
        {
            startW = Screen.width >> 1;
            startH = Screen.height >> 1;

            model = this.Model<ICounterModel>();
            model.A.RegisterWithInitValue(OnAChange);
            model.B.RegisterWithInitValue(OnBChange);

            this.AddEvent<ChangeOpIconEvent>(OnChangeOp);
            this.AddNotify<OperationSuccessfulNotify>(OnOperationSuccessful);
            this.AddNotify<OperationFailedNotify>(OnOperationFailed);
        }
        private void OnOperationSuccessful()
        {
            "�����ɹ�".Log();
        }
        private void OnOperationFailed()
        {
            "����ʧ��".Log();
        }
        private void OnChangeOp(ChangeOpIconEvent e)
        {
            opText = e.icon;
        }
        private void OnDestroy()
        {
            var model = this.Model<ICounterModel>();
            model.A.UnRegister(OnAChange);
            model.B.UnRegister(OnBChange);

            this.RmvEvent<ChangeOpIconEvent>(OnChangeOp);
            this.RmvNotify<OperationSuccessfulNotify>(OnOperationSuccessful);
            this.RmvNotify<OperationFailedNotify>(OnOperationFailed);
        }
        private void OnAChange(float a)
        {
            A = a.ToString();
        }
        private void OnBChange(float b)
        {
            B = b.ToString();
        }
        private void OnGUI()
        {
            if (style == null)
            {
                style = GUI.skin.label;
                style.fontSize = 30;
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.white;

                btnStyle = GUI.skin.button;
                btnStyle.fontSize = style.fontSize;
                btnStyle.alignment = TextAnchor.MiddleCenter;

                inputStyle = GUI.skin.textField;
                inputStyle.fontSize = style.fontSize;
                inputStyle.alignment = TextAnchor.MiddleCenter;
            }
            float size = 50f;
            var startX = startW - size * 4f;
            var rect = new Rect(startX, startH - size, size * 6f, size);
            if (GUI.Button(rect, "RandomNum", btnStyle))
            {
                this.SendCmd<RandomValueCmd>();
            }
            rect = new Rect(startX, startH, size, size);
            string a = GUI.TextField(rect, A, inputStyle);
            if (a != A)
            {
                if (int.TryParse(a, out int r))
                {
                    A = a;
                    model.A.Value = r;
                }
                else
                {
                    this.SendNotify<OperationFailedNotify>();
                }
            }
            rect.x += size;
            GUI.Label(rect, opText, style);
            rect.x += size;
            string b = GUI.TextField(rect, B, inputStyle);
            if (b != B)
            {
                if (int.TryParse(b, out int r))
                {
                    B = b;
                    model.B.Value = r;
                }
                else
                {
                    this.SendNotify<OperationFailedNotify>();
                }
            }
            rect.x += size;
            GUI.Label(rect, "=", style);
            rect.x += size;
            rect.width = size * 2f;
            GUI.Label(rect, R, inputStyle);
            rect.y += size;
            rect.x = startW - size;
            rect.width = size * 3f;
            if (GUI.Button(rect, "Calc", btnStyle))
            {
                var op = (Op)mSelect;
                float r = this.Query<ResultQuery, Op, float>(op);
                R = op == Op.Div ? r.ToString("F2") : r.ToString();
                this.SendNotify<OperationSuccessfulNotify>();
            }
            rect.x = startX;
            if (GUI.Button(rect, "Operator", btnStyle))
            {
                ShowList = !ShowList;
            }
            if (ShowList)
            {
                rect.height = size * 3f;
                rect.y += size;
                var sel = GUI.SelectionGrid(rect, mSelect, Enum.GetNames(typeof(Op)), 1);
                if (mSelect == sel) return;
                mSelect = sel;
                this.SendCmd<ChangeOpCmd, int>(sel);
                ShowList = false;
            }
        }
    }
}