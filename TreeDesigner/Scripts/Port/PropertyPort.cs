using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeDesigner
{
    /// <summary>
    /// �˿�
    /// </summary>
    [Serializable]
    public partial class PropertyPort
    {
        [SerializeField]
        protected string m_Name;
        public string Name { get => m_Name; set => m_Name = value; }

        [SerializeField]
        protected PortDirection m_Direction;
        /// <summary>
        /// �����������
        /// </summary>
        public PortDirection Direction { get => m_Direction; set => m_Direction = value; }

        [SerializeField]
        protected string m_InputEdgeGUID;
        public string InputEdgeGUID => m_InputEdgeGUID;

        [SerializeField]
        protected List<string> m_OutputEdgeGUIDs = new List<string>();
        public List<string> OutputEdgeGUIDs => m_OutputEdgeGUIDs;


        [NonSerialized]
        protected BaseNode m_Owner;
        /// <summary>
        /// ����Node
        /// </summary>
        public BaseNode Owner => m_Owner;

        [NonSerialized]
        protected PropertyPort m_SourcePort;
        /// <summary>
        /// ��֮�����Ķ˿�
        /// </summary>
        public PropertyPort SourcePort => m_SourcePort;

        [NonSerialized]
        protected List<PropertyPort> m_TargetPorts = new List<PropertyPort>();
        public List<PropertyPort> TargetPorts => m_TargetPorts;

        public virtual Type ValueType => null;

        public bool InputLinked => !string.IsNullOrEmpty(m_InputEdgeGUID);

        public PropertyPort() { }

        public virtual void Init(BaseNode node)
        {
            m_Owner = node;
            if (!string.IsNullOrEmpty(m_InputEdgeGUID))
                m_SourcePort = m_Owner.Owner.GUIDPropertyEdgeMap[m_InputEdgeGUID].StartPort;

            m_TargetPorts.Clear();
            m_OutputEdgeGUIDs.ForEach(i => m_TargetPorts.Add(m_Owner.Owner.GUIDPropertyEdgeMap[i].EndPort));
        }
        public virtual void Dispose()
        {
            m_Owner = null;
            m_SourcePort = null;
            m_TargetPorts.Clear();
        }
        public virtual void OnAfterDeserialize()
        {
            m_InputEdgeGUID = string.Empty;
            m_OutputEdgeGUIDs.Clear();
            m_Owner = null;
            m_SourcePort = null;
            m_TargetPorts.Clear();
        }

        public virtual object GetValue()
        {
            return null;
        }
        public virtual void SetValue(object value) { }
        public virtual void GetSourceValue() { }

        public static implicit operator bool(PropertyPort exists) => exists != null;
    }

    [Serializable]
    public partial class PropertyPort<T> : PropertyPort
    {
        [SerializeField]
        protected T m_Value;
        public virtual T Value { get => m_Value; set => m_Value = value; }

        [NonSerialized]
        protected PropertyPort<T> m_SameTypeSourcePropertyPort;
        [NonSerialized]
        protected PropertyPort m_DefferentTypeSourcePropertyPort;
        [NonSerialized]
        protected List<PropertyPort<T>> m_TargetPropertyPorts = new List<PropertyPort<T>>();

        public override Type ValueType => typeof(T);

        public override void Init(BaseNode node)
        {
            base.Init(node);
            if (m_SourcePort)
            {
                if (m_SourcePort.ValueType == ValueType)
                    m_SameTypeSourcePropertyPort = m_SourcePort as PropertyPort<T>;
                else
                    m_DefferentTypeSourcePropertyPort = m_SourcePort;
            }

            m_TargetPropertyPorts.Clear();
            m_TargetPorts.ForEach(i => m_TargetPropertyPorts.Add(i as PropertyPort<T>));
        }
        public override void Dispose()
        {
            base.Dispose();
            m_SameTypeSourcePropertyPort = null;
            m_DefferentTypeSourcePropertyPort = null;
            m_TargetPropertyPorts.Clear();
        }
        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            m_SameTypeSourcePropertyPort = null;
            m_DefferentTypeSourcePropertyPort = null;
            m_TargetPropertyPorts.Clear();
        }
        public override object GetValue()
        {
            return m_Value;
        }
        public override void SetValue(object value)
        {
            m_Value = (T)value;
        }
        public override void GetSourceValue()
        {
            if (m_SameTypeSourcePropertyPort)
                m_Value = m_SameTypeSourcePropertyPort.Value;
            if (m_DefferentTypeSourcePropertyPort)
                m_Value = GetDefferentTypeSourceValue(m_DefferentTypeSourcePropertyPort.GetValue());
        }
        public virtual T GetDefferentTypeSourceValue(object value)
        {
            return (T)value;
        }
    }

    [Serializable]
    [PropertyColor(210, 210, 210)]
    public class objectPropertyPort : PropertyPort<object>
    {
        public objectPropertyPort() { }
    }

    [Serializable]
    [PropertyColor(210, 210, 210)]
    public class BoolPropertyPort : PropertyPort<bool>
    {
        public BoolPropertyPort() { }
    }

    [Serializable]
    [PropertyColor(148, 129, 230)]
    public class IntPropertyPort : PropertyPort<int>
    {
        public IntPropertyPort() { }
    }

    [Serializable]
    [PropertyColor(132, 228, 231)]
    [CompatiblePorts(typeof(int))]
    public class FloatPropertyPort : PropertyPort<float>
    {
        public FloatPropertyPort() { }
        public override float GetDefferentTypeSourceValue(object value)
        {
            if (value is int a)
                return a;
            else
                return 0;
        }
    }

    [Serializable]
    [PropertyColor(252, 218, 110)]
    public class StringPropertyPort : PropertyPort<string>
    {
        public StringPropertyPort() { }
    }

    [Serializable]
    [PropertyColor(246, 255, 154)]
    public class Vector3PropertyPort : PropertyPort<Vector3>
    {
        public Vector3PropertyPort() { }
    }

    [Serializable]
    [PropertyColor(154, 239, 146)]
    public class Vector2PropertyPort : PropertyPort<Vector2>
    {
        public Vector2PropertyPort() { }
    }

    [Serializable]
    [PropertyColor(154, 239, 146)]
    public class TransformProperty : PropertyPort<Transform>
    {
        public TransformProperty() { }
    }


    [Serializable]
    [PropertyColor(148, 129, 230)]
    public class IntListPropertyPort : PropertyPort<List<int>>
    {
        public IntListPropertyPort() { }
    }

    [Serializable]
    [PropertyColor(132, 228, 231)]
    public class FloatListPropertyPort : PropertyPort<List<float>>
    {
        public FloatListPropertyPort() { }
    }

    [Serializable]
    [PropertyColor(252, 218, 110)]
    public class StringListPropertyPort : PropertyPort<List<string>>
    {
        public StringListPropertyPort() { }
    }

    [Serializable]
    [PropertyColor(239, 163, 146)]
    public class TreePropertyPort : PropertyPort<BaseTree>
    {
        public TreePropertyPort() { }
    }
}