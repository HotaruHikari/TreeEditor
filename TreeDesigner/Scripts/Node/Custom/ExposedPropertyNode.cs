using System;
using UnityEngine;

namespace TreeDesigner
{
    [Serializable]
    [NodeName("ExposedProperty")]
    [NodePath("Base/Custom/ExposedProperty")]
    [NodeView("ExposedPropertyNodeView")]
    public partial class ExposedPropertyNode : BaseNode
    {
        [SerializeField]
        ExposedPropertyNodeType m_NodeType;
        public ExposedPropertyNodeType NodeType => m_NodeType;

        [SerializeReference]
        PropertyPort m_Value = new PropertyPort() { Direction = PortDirection.Output };
        public PropertyPort Value => m_Value;

        [SerializeField]
        string m_InputEdgeGUID;
        public string InputEdgeGUID => m_InputEdgeGUID;

        [SerializeField]
        string m_ExposedPropertyGUID;
        public string ExposedPropertyGUID => m_ExposedPropertyGUID;

        [NonSerialized]
        protected BaseNode m_Parent;
        public BaseNode Parent => m_Parent;

        [NonSerialized]
        BaseExposedProperty m_ExposedProperty;
        public BaseExposedProperty ExposedProperty => m_ExposedProperty;

        public override void Init(BaseTree tree)
        {
            base.Init(tree);

            if (!string.IsNullOrEmpty(m_InputEdgeGUID))
                m_Parent = m_Owner.GUIDEdgeMap[m_InputEdgeGUID].StartNode;

            if (!string.IsNullOrEmpty(m_ExposedPropertyGUID) && m_Owner.GUIDExposedPropertyMap.TryGetValue(m_ExposedPropertyGUID,out BaseExposedProperty exposedProperty))
                m_ExposedProperty = exposedProperty;
        }
        public override void Dispose()
        {
            base.Dispose();

            m_Parent = null;
            m_ExposedProperty = null;
        }
        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
        }
        protected override void OutputValue()
        {
            base.OutputValue();
            if (m_NodeType == ExposedPropertyNodeType.Get && m_ExposedProperty)
                m_Value.SetValue(m_ExposedProperty.GetValue());
        }
        protected override void OnStart()
        {
            base.OnStart();
            if (m_Parent.State == State.Running && m_NodeType == ExposedPropertyNodeType.Set && m_ExposedProperty)
                m_ExposedProperty.SetValue(m_Value.GetValue());
        }
        protected override State OnUpdate()
        {
            return State.Success;
        }
    }
    public enum ExposedPropertyNodeType { Get, Set }
}