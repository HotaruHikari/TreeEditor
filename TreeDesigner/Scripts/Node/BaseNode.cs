using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeDesigner
{
    [Serializable]
    public abstract partial class BaseNode
    {
        [SerializeField]
        protected string m_GUID;
        public string GUID { get => m_GUID; set => m_GUID = value; }

        [NonSerialized]
        protected BaseTree m_Owner;
        /// <summary>
        /// 被持有对象
        /// </summary>
        public BaseTree Owner { get => m_Owner; set => m_Owner = value; }

        [NonSerialized]
        protected State m_State;
        public State State { get => m_State; set => m_State = value; }

        [NonSerialized]
        protected Dictionary<string, PropertyPort> m_PropertyPortMap = new Dictionary<string, PropertyPort>();
        /// <summary>
        /// 节点所持有的端口信息
        /// </summary>
        public Dictionary<string, PropertyPort> PropertyPortMap => m_PropertyPortMap;
        
        [NonSerialized]
        protected List<BaseNode> m_InputPropertyNodes = new List<BaseNode>();
        /// <summary>
        /// 输入进来的其它节点
        /// </summary>
        public List<BaseNode> InputPropertyNodes => m_InputPropertyNodes;

        public Action OnUpdateCallback;

        public virtual void BeforeInit()
        {
            m_PropertyPortMap.Clear();
            foreach (var fieldInfo in this.GetAllFields()) // 记录所有端口
            {
                if (fieldInfo.GetValue(this) is PropertyPort propertyPort)
                {
                    if (m_PropertyPortMap.ContainsKey(propertyPort.Name))
                    {

                    }
                    else
                        m_PropertyPortMap.Add(propertyPort.Name, propertyPort);
                }
                else if (fieldInfo.GetValue(this) is List<PropertyPort> propertyPorts)
                    propertyPorts.ForEach(i => m_PropertyPortMap.Add(i.Name, i));
            }
        }
        public virtual void Init(BaseTree tree)
        {
            m_Owner = tree;
            foreach (var propertyPair in m_PropertyPortMap)
            {
                propertyPair.Value.Init(this); //初始化端口
            }
        }
        public virtual void AfterInit()
        {
            m_InputPropertyNodes.Clear();
            foreach (var propertyPortPair in m_PropertyPortMap)
            {
                if (propertyPortPair.Value.Direction == PortDirection.Input &&
                    propertyPortPair.Value.SourcePort != null &&
                    !m_InputPropertyNodes.Contains(propertyPortPair.Value.SourcePort.Owner))
                    m_InputPropertyNodes.Add(propertyPortPair.Value.SourcePort.Owner);
            }
        }
        public virtual void Dispose()
        {
            m_Owner = null;
            foreach (var propertyPair in m_PropertyPortMap)
            {
                propertyPair.Value.Dispose();
            }
            m_PropertyPortMap.Clear();
            m_InputPropertyNodes.Clear();
        }
        public virtual State UpdateNode() 
        {
            if(m_State != State.Running)
            {
                OnStart();
            }
            if (m_State == State.Running)
            {
                m_State = OnUpdate();
            }
            if(m_State == State.Success || m_State == State.Failure)
            {
                OnStop();
            }
            OnUpdateCallback?.Invoke();
            return m_State;
        }
        public virtual void ResetNode() 
        {
            m_State = State.None;
            OnReset();
            OnUpdateCallback?.Invoke();
        }

        protected virtual void OnStart()
        {
            m_State = State.Running;
            InputValue();
        }
        protected virtual State OnUpdate()
        {
            return State.None;
        }
        protected virtual void OnStop()
        {
        }
        protected virtual void OnReset()
        {
        }

        protected virtual void InputValue()
        {
            m_InputPropertyNodes.ForEach(i => i.OutputValue()); // 执行所有接入节点的输出
            foreach (var propertyPortPair in m_PropertyPortMap)
            {
                if (propertyPortPair.Value.Direction == PortDirection.Input)
                    propertyPortPair.Value.GetSourceValue();
            }
        }
        protected virtual void OutputValue(){ }
        public virtual void OnBeforeSerialize() { }
        public virtual void OnAfterDeserialize() 
        {
            m_Owner = null;
            m_State = State.None;
            m_PropertyPortMap.Clear();
            m_InputPropertyNodes.Clear();

            foreach (var fieldInfo in this.GetAllFields())
            {
                if (fieldInfo.GetValue(this) is PropertyPort propertyPort)
                {
                    propertyPort.OnAfterDeserialize();
                }
            }
        }
        public virtual void OnSpawn() { }
        public virtual void OnUnspawn() { }

        public virtual bool IsConnected(string name)
        {
            return m_PropertyPortMap.TryGetValue(name, out PropertyPort propertyPort) && (!string.IsNullOrEmpty(propertyPort.InputEdgeGUID) || propertyPort.OutputEdgeGUIDs.Count > 0);
        }

        public static implicit operator bool(BaseNode exists) => exists != null;
    }
}