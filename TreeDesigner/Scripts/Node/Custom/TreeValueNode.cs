using System;
using UnityEngine;

namespace TreeDesigner
{
    [Serializable]
    [NodeName("TreeValue")]
    [NodePath("Base/Action/TreeValue")]
    [NodeView("TreeValueNodeView")]
    public partial class TreeValueNode : BaseNode
    {
        [SerializeField]
        TreeValueNodeType m_NodeType;
        public TreeValueNodeType NodeType => m_NodeType;

        [SerializeField, PropertyPort(PortDirection.Input, "Tree"), ReadOnly]
        TreePropertyPort m_Tree = new TreePropertyPort();
        public BaseTree Tree => m_Tree.Value;
        
        [SerializeReference]
        PropertyPort m_Value = new PropertyPort() { Direction = PortDirection.Output };
        public PropertyPort Value => m_Value;

        [SerializeField]
        string m_InputEdgeGUID;
        public string InputEdgeGUID => m_InputEdgeGUID;

        [SerializeField]
        string m_ExposedPropertyName;
        public string ExposedPropertyName => m_ExposedPropertyName;

        [NonSerialized]
        protected BaseNode m_Parent;
        public BaseNode Parent => m_Parent;

        public override void Init(BaseTree tree)
        {
            base.Init(tree);

            if (!string.IsNullOrEmpty(m_InputEdgeGUID))
                m_Parent = m_Owner.GUIDEdgeMap[m_InputEdgeGUID].StartNode;
        }
        public override void Dispose()
        {
            base.Dispose();

            m_Parent = null;
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
            if (m_NodeType == TreeValueNodeType.Get && Tree && !string.IsNullOrEmpty(m_ExposedPropertyName) && Tree.GetExposedProperty(m_ExposedPropertyName) is BaseExposedProperty exposedProperty)
                m_Value.SetValue(exposedProperty.GetValue());
        }
        protected override void OnStart()
        {
            base.OnStart();
            if (m_Parent.State == State.Running && m_NodeType == TreeValueNodeType.Set && Tree && !string.IsNullOrEmpty(m_ExposedPropertyName) && Tree.GetExposedProperty(m_ExposedPropertyName) is BaseExposedProperty exposedProperty)
                exposedProperty.SetValue(m_Value.GetValue());
        }
        protected override State OnUpdate()
        {
            return State.Success;
        }
    }

    public enum TreeValueNodeType { Get, Set }
}