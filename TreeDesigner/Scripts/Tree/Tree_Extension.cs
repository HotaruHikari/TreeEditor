#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeDesigner
{
    public partial class BaseTree : ScriptableObject
    {
        [SerializeField, ShowInInspector("Description")]
        string m_Description;
        public string Description => m_Description;

        [SerializeField]
        List<NodeGroup> m_NodeGroups = new List<NodeGroup>();
        public List<NodeGroup> NodeGroups => m_NodeGroups;

        [SerializeField]
        List<StackNode> m_StackNodes = new List<StackNode>();
        public List<StackNode> StackNodes => m_StackNodes;

        public Action OnExposedPropertyChanged;

        public virtual bool CheckInit()
        {
            bool dirty = false;
            m_GUIDNodeMap = new Dictionary<string, BaseNode>();
            m_GUIDEdgeMap = new Dictionary<string, BaseEdge>();
            m_GUIDPropertyEdgeMap = new Dictionary<string, PropertyEdge>();
            m_GUIDExposedPropertyMap = new Dictionary<string, BaseExposedProperty>();

            m_Nodes.ForEach(i =>
            {
                if (i)
                {
                    m_GUIDNodeMap.Add(i.GUID, i);
                    i.BeforeInit();
                }
                else
                    dirty = true;
            });
            m_Nodes = m_Nodes.Where(i => i).ToList();
            m_Edges.ForEach(i => m_GUIDEdgeMap.Add(i.GUID, i));
            m_PropertyEdges.ForEach(i => m_GUIDPropertyEdgeMap.Add(i.GUID, i));
            m_ExposedProperties.ForEach(i => m_GUIDExposedPropertyMap.Add(i.GUID, i));


            List<BaseEdge> m_ValidEdges = new List<BaseEdge>();
            List<BaseEdge> m_InvalidEdges = new List<BaseEdge>();
            foreach (var edge in m_Edges)
            {
                edge.Init(this);
                if(edge.StartNode && edge.EndNode)
                    m_ValidEdges.Add(edge);
                else
                    m_InvalidEdges.Add(edge);
            }
            if (m_InvalidEdges.Count > 0)
                dirty = true;
            m_InvalidEdges.ForEach(i => UnLink(i));

            List<PropertyEdge> m_ValidPropertyEdges = new List<PropertyEdge>();
            List<PropertyEdge> m_InvalidPropertyEdges = new List<PropertyEdge>();
            foreach (var propertyEdge in m_PropertyEdges)
            {
                propertyEdge.Init(this);
                if (propertyEdge.StartPort && propertyEdge.EndPort)
                    m_ValidPropertyEdges.Add(propertyEdge);
                else
                    m_InvalidPropertyEdges.Add(propertyEdge);
            }
            if (m_InvalidPropertyEdges.Count > 0)
                dirty = true;
            m_InvalidPropertyEdges.ForEach (i => UnLinkProperty(i));

            m_Nodes.ForEach(i => i.Init(this));
            foreach (var exposedPropertyNode in GetNodes<ExposedPropertyNode>())
            {
                if (!exposedPropertyNode.ExposedProperty)
                {
                    dirty = true;
                    exposedPropertyNode.Value.OutputEdgeGUIDs.ToList().ForEach(i => UnLinkProperty(m_GUIDPropertyEdgeMap[i]));
                    exposedPropertyNode.RemoveExposedProperty();
                }
            }
            m_Nodes.ForEach(i => i.AfterInit());
            m_ExposedProperties.ForEach(i => i.Init(this));
            if(!string.IsNullOrEmpty(m_RootGUID))
                m_Root = m_GUIDNodeMap[m_RootGUID];
            return dirty;
        }

        public virtual BaseNode CreateNode(Type type)
        {
            BaseNode node = Activator.CreateInstance(type) as BaseNode;
            node.GUID = Guid.NewGuid().ToString();
            node.Expanded = true;
            node.Refresh();
            AddNode(node);
            return node;
        }
        public virtual void DeleteNode(BaseNode node)
        {
            if (m_Nodes.Contains(node))
                RemoveNode(node);

            m_Edges = m_Edges.Where(i => i.StartNode != node && i.EndNode != node).ToList();
        }
        public virtual void AddNode(BaseNode node)
        {
            m_Nodes.Add(node);
            m_GUIDNodeMap.Add(node.GUID, node);

            //int propertyPortIndex = 0;
            foreach (var fieldInfo in node.GetAllFields())
            {
                if (fieldInfo.GetValue(node) is PropertyPort propertyPort)
                {
                    propertyPort.Name = fieldInfo.Name;
                    //propertyPort.Index = propertyPortIndex;
                    //propertyPortIndex++;
                }
            }


            node.BeforeInit();
            node.Init(this);
        }
        public virtual void RemoveNode(BaseNode node)
        {
            m_Nodes.Remove(node);
            m_GUIDNodeMap.Remove(node.GUID);
        }

        public virtual BaseExposedProperty CreateExposedProperty(Type type)
        {
            BaseExposedProperty exposedProperty = Activator.CreateInstance(type) as BaseExposedProperty;
            exposedProperty.GUID = Guid.NewGuid().ToString();
            exposedProperty.Index = m_ExposedProperties.Count;
            exposedProperty.Expanded = true;
            exposedProperty.Init(this);
            m_ExposedProperties.Add(exposedProperty);
            m_GUIDExposedPropertyMap.Add(exposedProperty.GUID, exposedProperty);
            return exposedProperty;
        }
        public virtual void DeleteExposedProperty(BaseExposedProperty exposedProperty)
        {
            if (m_ExposedProperties.Contains(exposedProperty))
            {
                m_ExposedProperties.Remove(exposedProperty);
                m_GUIDExposedPropertyMap.Remove(exposedProperty.GUID);
            }
        }

        public virtual NodeGroup CreateNodeGroup()
        {
            NodeGroup nodeGroup = new NodeGroup();
            m_NodeGroups.Add(nodeGroup);
            return nodeGroup;
        }
        public virtual void DeleteNodeGroup(NodeGroup nodeGroup)
        {
            if (m_NodeGroups.Contains(nodeGroup))
                m_NodeGroups.Remove(nodeGroup);
        }

        public virtual StackNode CreateStackNode()
        {
            StackNode stackNode = new StackNode();
            stackNode.GUID = Guid.NewGuid().ToString();
            m_StackNodes.Add(stackNode);
            return stackNode;
        }
        public virtual void DeleteStackNode(StackNode stackNode)
        {
            if(m_StackNodes.Contains(stackNode))
                m_StackNodes.Remove(stackNode);
        }

        public virtual BaseEdge Link(BaseNode startNode, BaseNode endNode, string startPortName, string endPortName)
        {
            if (m_Edges.Any(i => i.StartNode == startNode && i.EndNode == endNode && i.StartPortName == startPortName && i.EndPortName == endPortName))
                return null;

            BaseEdge edge = new BaseEdge(startNode, endNode, startPortName, endPortName);
            AddLink(edge);
            startNode.OnOutputLinked(edge);
            endNode.OnInputLinked(edge);
            return edge;
        }
        public virtual void UnLink(BaseEdge edge)
        {
            if (m_Edges.Contains(edge))
            {
                RemoveLink(edge);
                edge.StartNode?.OnOutputUnlinked(edge);
                edge.EndNode?.OnInputUnlinked(edge);
            }
        }
        public virtual void AddLink(BaseEdge edge)
        {
            m_Edges.Add(edge);
            m_GUIDEdgeMap.Add(edge.GUID, edge);
            edge.Init(this);
        }
        public virtual void RemoveLink(BaseEdge edge)
        {
            m_Edges.Remove(edge);
            m_GUIDEdgeMap.Remove(edge.GUID);
        }

        public virtual PropertyEdge LinkProperty(BaseNode startNode, BaseNode endNode, PropertyPort startPropertyPort, PropertyPort endPropertyPort)
        {
            if (m_PropertyEdges.Any(i => i.StartNode == startNode && i.EndNode == endNode && i.StartPortName == startPropertyPort.Name && i.EndPortName == endPropertyPort.Name))
                return null;

            PropertyEdge propertyEdge = new PropertyEdge(startNode, endNode, startPropertyPort, endPropertyPort);
            AddPropertyLink(propertyEdge);
            startPropertyPort.OnOutputLinked(propertyEdge);
            endPropertyPort.OnInputLinked(propertyEdge);
            startNode.OnOutputPropertyLinked(propertyEdge);
            endNode.OnInputPropertyLinked(propertyEdge);
            return propertyEdge;
        }
        public virtual void UnLinkProperty(PropertyEdge propertyEdge)
        {
            if (m_PropertyEdges.Contains(propertyEdge))
            {
                RemovePropertyLink(propertyEdge);
                propertyEdge.StartPort?.OnOutputUnlinked(propertyEdge);
                propertyEdge.EndPort?.OnInputUnlinked(propertyEdge);
                propertyEdge.StartNode?.OnOutputPropertyUnLinked(propertyEdge);
                propertyEdge.EndNode?.OnInputPropertyUnLinked(propertyEdge);
            }
        }
        public virtual void AddPropertyLink(PropertyEdge propertyEdge)
        {
            m_PropertyEdges.Add(propertyEdge);
            m_GUIDPropertyEdgeMap.Add(propertyEdge.GUID, propertyEdge);
            propertyEdge.Init(this);
        }
        public virtual void RemovePropertyLink(PropertyEdge propertyEdge)
        {
            m_PropertyEdges.Remove(propertyEdge);
            m_GUIDPropertyEdgeMap.Remove(propertyEdge.GUID);
        }

        public virtual bool Refresh()
        {
            bool isDirty = false;
            foreach (var node in m_Nodes)
            {
                if (node.Refresh())
                    isDirty = true;
            }
            m_ExposedProperties.ForEach(i => i.ClearEvent());
            return isDirty;
        }

        List<T> GetNodes<T>() where T : BaseNode
        {
            List<T> nodes = new List<T>();
            foreach (var node in m_Nodes)
            {
                if(node.GetType() == typeof(T))
                    nodes.Add((T)node);
            }
            return nodes;
        }

        //[ContextMenu("Test")]
        //void Test()
        //{
        //    m_StackNodes.ForEach(i =>
        //    {
        //        i.GUID = Guid.NewGuid().ToString();
        //    });
        //    UnityEditor.EditorUtility.SetDirty(this);
        //}
    }

    public partial class SubTree : BaseTree
    {

    }
}
#endif