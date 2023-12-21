using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeDesigner
{
    [TreeWindow("OpenBaseTreeWindow")]
    [AcceptableNodePaths("Base")]
    public partial class BaseTree : ScriptableObject
    {
        [SerializeReference]
        protected List<BaseNode> m_Nodes = new List<BaseNode>();
        public List<BaseNode> Nodes => m_Nodes;

        [SerializeField]
        protected List<BaseEdge> m_Edges = new List<BaseEdge>();
        public List<BaseEdge> Edges => m_Edges;

        [SerializeField]
        protected List<PropertyEdge> m_PropertyEdges = new List<PropertyEdge>();

        public List<PropertyEdge> PropertyEdges => m_PropertyEdges;

        [SerializeReference]
        protected List<BaseExposedProperty> m_ExposedProperties = new List<BaseExposedProperty>();
        /// <summary>
        /// 黑板属性
        /// </summary>
        public List<BaseExposedProperty> ExposedProperties => m_ExposedProperties;

        [SerializeField]
        protected string m_RootGUID;
        public string RootGUID { get => m_RootGUID; set => m_RootGUID = value; }

        [NonSerialized, ShowInInspector("Running")]
        protected bool m_Running;
        public bool Running { get => m_Running; set => m_Running = value; }

        [NonSerialized, ShowInInspector("State")]
        protected State m_State;
        public State State { get => m_State; set => m_State = value; }

        [NonSerialized]
        protected BaseNode m_Root;
        public BaseNode Root => m_Root;

        [NonSerialized]
        protected Dictionary<string, BaseNode> m_GUIDNodeMap = new Dictionary<string, BaseNode>();
        public Dictionary<string, BaseNode> GUIDNodeMap => m_GUIDNodeMap;
        
        [NonSerialized]
        protected Dictionary<string, BaseEdge> m_GUIDEdgeMap = new Dictionary<string, BaseEdge>();
        public Dictionary<string, BaseEdge> GUIDEdgeMap => m_GUIDEdgeMap;

        [NonSerialized]
        protected Dictionary<string, PropertyEdge> m_GUIDPropertyEdgeMap = new Dictionary<string, PropertyEdge>();
        public Dictionary<string, PropertyEdge> GUIDPropertyEdgeMap => m_GUIDPropertyEdgeMap;

        [NonSerialized]
        protected Dictionary<string, BaseExposedProperty> m_GUIDExposedPropertyMap = new Dictionary<string, BaseExposedProperty>();
        public Dictionary<string, BaseExposedProperty> GUIDExposedPropertyMap => m_GUIDExposedPropertyMap;

        [NonSerialized]
        protected Dictionary<string,BaseExposedProperty> m_NameExposedPropertyMap = new Dictionary<string, BaseExposedProperty>();
        [NonSerialized]
        protected Dictionary<BaseExposedProperty, object> m_ExposedPropertyOriginalValueMap = new Dictionary<BaseExposedProperty, object>();

        public event Action OnStopCallback;
        /// <summary>
        /// 构建整个图的所有关系
        /// </summary>
        public virtual void InitTree()
        {
            m_GUIDNodeMap.Clear();
            m_GUIDEdgeMap.Clear();
            m_GUIDPropertyEdgeMap.Clear();
            m_GUIDExposedPropertyMap.Clear();
            m_NameExposedPropertyMap.Clear();

            m_Nodes.ForEach(i => 
            {
                m_GUIDNodeMap.Add(i.GUID, i);
                i.BeforeInit();
            });
            m_Edges.ForEach(i => m_GUIDEdgeMap.Add(i.GUID, i));
            m_PropertyEdges.ForEach(i => m_GUIDPropertyEdgeMap.Add(i.GUID, i));
            m_ExposedProperties.ForEach(i => 
            {
                m_GUIDExposedPropertyMap.Add(i.GUID, i);
                m_NameExposedPropertyMap.Add(i.Name, i);
            });

            m_Edges.ForEach(i => i.Init(this));
            m_PropertyEdges.ForEach(i => i.Init(this));
            m_Nodes.ForEach(i => i.Init(this));
            m_Nodes.ForEach(i => i.AfterInit());
            m_ExposedProperties.ForEach(i => i.Init(this));
            if (!string.IsNullOrEmpty(m_RootGUID))
                m_Root = m_GUIDNodeMap[m_RootGUID];
        }
        public virtual void DisposeTree()
        {
            m_Root = null;
            m_Nodes.ForEach(i => i.Dispose());
            m_Edges.ForEach(i => i.Dispose());
            m_PropertyEdges.ForEach(i => i.Dispose());
            m_ExposedProperties.ForEach(i => i.Dispose());
            
            m_GUIDNodeMap.Clear();
            m_GUIDEdgeMap.Clear();
            m_GUIDPropertyEdgeMap.Clear();
            m_GUIDExposedPropertyMap.Clear();
        }
        public virtual State UpdateTree()
        {
            if(!m_Running && m_State == State.None)
            {
                OnStart();
            }
            if(m_Running && m_State == State.Running)
            {
                //m_State = m_Root.UpdateNode();
                State state = m_Root.UpdateNode(); // 从根节点开始
                if (m_Running && m_State == State.Running)
                    m_State = state;
            }
            if(m_Running && m_State == State.Success || m_State == State.Failure)
            {
                OnStop();
            }
            return m_State;
        }
        public virtual void ResetTree()
        {
            m_State = State.None;
            m_Root.ResetNode();
        }

        public virtual void OnStart()
        {
            m_Running = true;
            m_State = State.Running;
        }
        public virtual void OnStop()
        {
            m_Running = false;
            OnStopCallback?.Invoke();
        }
        public virtual void OnSpawn()
        {
            m_ExposedPropertyOriginalValueMap.Clear();
            m_ExposedProperties.ForEach(i => m_ExposedPropertyOriginalValueMap.Add(i, i.GetValue()));
            m_Nodes.ForEach(i => i.OnSpawn());
        }
        public virtual void OnUnspawn()
        {
            m_ExposedProperties.ForEach(i => i.SetValue(m_ExposedPropertyOriginalValueMap[i]));
            m_Nodes.ForEach(i => i.OnUnspawn());
        }

        public BaseExposedProperty GetExposedProperty(string name)
        {
            if (m_NameExposedPropertyMap.TryGetValue(name, out BaseExposedProperty exposedProperty))
                return exposedProperty;
            return null;
        }
        public T GetExposedProperty<T>(string name) where T : BaseExposedProperty
        {
            if (m_NameExposedPropertyMap.TryGetValue(name, out BaseExposedProperty exposedProperty))
                return exposedProperty as T;
            return null;
        }
    }
}