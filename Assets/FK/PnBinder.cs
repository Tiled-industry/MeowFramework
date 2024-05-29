﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Panty
{
    public abstract class PnBinder<V>
    {
        protected Action<V> mCallBack;
        protected V mValue;
        /// <summary>
        /// 支持对值的隐式转换
        /// </summary>
        public static implicit operator V(PnBinder<V> binder) => binder.mValue;
        public IRmv RegisterWithInitValue(Action<V> onValueChanged)
        {
            onValueChanged(mValue);
            return Register(onValueChanged);
        }
        public IRmv Register(Action<V> onValueChanged)
        {
            mCallBack += onValueChanged;
            return new CustomRmv(() => Unregister(onValueChanged));
        }
        public void Unregister(Action<V> onValueChanged) => mCallBack -= onValueChanged;
        public void SetOnly(V value) => mValue = value;
    }
    public class ValueBinder<V> : PnBinder<V> where V : struct, IEquatable<V>
    {
        public V Value
        {
            get => mValue;
            set
            {
                if (mValue.Equals(value)) return;
                mValue = value;
                mCallBack?.Invoke(value);
            }
        }
        public ValueBinder(V value = default) => mValue = value;
        public static bool operator ==(ValueBinder<V> binder, V value) => binder.mValue.Equals(value);
        public static bool operator !=(ValueBinder<V> binder, V value) => !binder.mValue.Equals(value);

        /// <summary>
        /// 使用当前值对内部进行设置 注意该方法为重新构建 禁止在初始化以外的地方胡乱使用
        /// </summary>
        public static implicit operator ValueBinder<V>(V value) => new ValueBinder<V>(value);
        public override bool Equals(object obj) =>
            obj is ValueBinder<V> binder && mValue.Equals(binder.mValue);
        public override string ToString() => mValue.ToString();
        public override int GetHashCode() => mValue.GetHashCode();
    }
    public class StringBinder : PnBinder<string>
    {
        public string Value
        {
            get => mValue;
            set
            {
                if (mValue == value) return;
                mValue = value;
                mCallBack?.Invoke(value);
            }
        }
        public StringBinder(string value = default) => mValue = value;
        public static bool operator ==(StringBinder binder, string value) => binder.mValue == value;
        public static bool operator !=(StringBinder binder, string value) => binder.mValue != value;
        public static implicit operator StringBinder(string value) => new StringBinder(value);
        public override bool Equals(object obj) =>
            obj is StringBinder binder && mValue == binder.mValue;
        public override string ToString() => mValue;
        public override int GetHashCode() => mValue == null ? 0 : mValue.GetHashCode();
    }
    public class ObjectBinder<O> : PnBinder<O> where O : class
    {
        public ObjectBinder(O value)
        {
#if DEBUG
            if (value == null) throw new Exception("Value is Empty");
#endif
            mValue = value;
        }
        public void Modify<D>(D newValue, string fieldOrPropName)
        {
#if DEBUG
            if (string.IsNullOrEmpty(fieldOrPropName))
                throw new ArgumentNullException(nameof(fieldOrPropName));
            if (mValue == null) throw new Exception($"{typeof(O)} is null");
#endif
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var memberInfo = typeof(O).GetMember(fieldOrPropName, flags).FirstOrDefault();
#if DEBUG 
            if (memberInfo == null)
                throw new Exception($"Field or property '{fieldOrPropName}' not found in type {typeof(O)}");
#endif
            switch (memberInfo)
            {
                case PropertyInfo prop:
                    if (prop.CanWrite)
                    {
                        if (EqualityComparer<D>.Default.Equals((D)prop.GetValue(mValue), newValue)) return;
                        prop.SetValue(mValue, newValue);
                        mCallBack?.Invoke(mValue);
                    }
                    break;
                case FieldInfo field:
                    if (EqualityComparer<D>.Default.Equals((D)field.GetValue(mValue), newValue)) return;
                    field.SetValue(mValue, newValue);
                    mCallBack?.Invoke(mValue);
                    break;
            }
        }
        public void Modify<D>(D newValue, Func<O, D> oldValue, Action<O, D> modifyAction)
        {
#if DEBUG
            if (modifyAction == null) throw new Exception($"必须设置[modifyAction]");
            if (mValue == null) throw new Exception($"{typeof(O)}为空");
#endif
            if (EqualityComparer<D>.Default.Equals(oldValue(mValue), newValue)) return;
            modifyAction(mValue, newValue);
            mCallBack?.Invoke(mValue);
        }
        public static implicit operator ObjectBinder<O>(O value) => new ObjectBinder<O>(value);
        public override int GetHashCode() => mValue == null ? 0 : mValue.GetHashCode();
        public override string ToString() => mValue == null ? "null" : mValue.ToString();
    }
}