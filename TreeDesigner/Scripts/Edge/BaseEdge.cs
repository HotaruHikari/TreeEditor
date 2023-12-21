using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeDesigner
{
    /// <summary>
    /// ��
    /// </summary>
    [Serializable]
    public partial class BaseEdge
    {
        [SerializeField]
        protected string m_GUID;
        public string GUID => m_GUID;

        [SerializeField]
        protected string m_StartNodeGUID;
        public string StartNodeGUID => m_StartNodeGUID;

        [SerializeField]
        protected string m_EndNodeGUID;
        public string EndNodeGUID => m_EndNodeGUID;

        [SerializeField]
        protected string m_StartPortName;
        public string StartPortName => m_StartPortName;

        [SerializeField]
        protected string m_EndPortName;
        public string EndPortName => m_EndPortName;

        [NonSerialized]
        protected BaseTree m_Owner;
        public BaseTree Owner => m_Owner;

        [NonSerialized]
        protected BaseNode m_StartNode;
        public BaseNode StartNode => m_StartNode;

        [NonSerialized]
        protected BaseNode m_EndNode;
        public BaseNode EndNode => m_EndNode;

        public BaseEdge() { }
        public BaseEdge(BaseNode startNode, BaseNode endNode, string startPortName, string endPortName)
        {
            m_GUID = Guid.NewGuid().ToString();
            m_StartNodeGUID = startNode.GUID;
            m_EndNodeGUID = endNode.GUID;
            m_StartPortName = startPortName;
            m_EndPortName = endPortName;

            m_StartNode = startNode;
            m_EndNode = endNode;
        }

        public virtual void Init(BaseTree tree)
        {
            m_Owner = tree;
            if (m_Owner.GUIDNodeMap.TryGetValue(m_StartNodeGUID, out BaseNode startNode))
                m_StartNode = startNode;
            if (m_Owner.GUIDNodeMap.TryGetValue(m_EndNodeGUID, out BaseNode endNode))
                m_EndNode = endNode;
        }
        public virtual void Dispose()
        {
            m_Owner = null;
            m_StartNode = null;
            m_EndNode = null;
        }

        public static implicit operator bool(BaseEdge exists) => exists != null;
    }
}
