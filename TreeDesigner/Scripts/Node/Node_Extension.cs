#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TreeDesigner
{
    public abstract partial class BaseNode 
    {
        [SerializeField]
        protected bool m_Expanded;
        public bool Expanded { get => m_Expanded; set => m_Expanded = value; }

        [SerializeField]
        protected bool m_ShowPanel;
        public bool ShowPanel { get => m_ShowPanel; set => m_ShowPanel = value; }

        [SerializeField]
        protected Vector2 m_Position;
        public Vector2 Position { get => m_Position; set => m_Position = value; }

        [NonSerialized]
        protected Action m_OnNodeChanged;
        public Action OnNodeChanged { get => m_OnNodeChanged; set => m_OnNodeChanged = value; }

        public virtual NodeCapabilities Capabilities => NodeCapabilities.Selectable |
                                                        NodeCapabilities.Movable |
                                                        NodeCapabilities.Deletable |
                                                        NodeCapabilities.Ascendable |
                                                        NodeCapabilities.Copiable |
                                                        NodeCapabilities.Snappable |
                                                        NodeCapabilities.Groupable;

        public virtual bool Refresh()
        {
            bool isDirty = false;

            List<PropertyPort> inputPropertyPorts = new List<PropertyPort>();
            List<PropertyPort> outPropertyPorts = new List<PropertyPort>();

            foreach (var fieldInfo in this.GetAllFields())
            {
                if (fieldInfo.GetValue(this) is PropertyPort propertyPort)
                {
                    if(propertyPort.Name != fieldInfo.Name)
                    {
                        isDirty = true;
                        propertyPort.Name = fieldInfo.Name;
                    }
                    
                    var propertyPortAttributes = fieldInfo.GetCustomAttributes<PropertyPortAttribute>();
                    if (propertyPortAttributes.Count() > 0)
                    {
                        PropertyPortAttribute propertyPortAttribute = propertyPortAttributes.ElementAt(0);
                        if (propertyPort.Direction != propertyPortAttribute.Direction)
                        {
                            isDirty = true;
                            propertyPort.Direction = propertyPortAttribute.Direction;
                        }

                        switch (propertyPort.Direction)
                        {
                            case PortDirection.Input:
                                inputPropertyPorts.Add(propertyPort);
                                break;
                            case PortDirection.Output:
                                outPropertyPorts.Add(propertyPort);
                                break;
                        }
                        if (propertyPort.Index == -1)
                        {
                            isDirty = true;
                            propertyPort.Index = propertyPortAttribute.Priority;
                        }
                    }

                    var variablePropertyPortAttributes = fieldInfo.GetCustomAttributes<VariablePropertyPortAttribute>();
                    if(variablePropertyPortAttributes.Count() > 0)
                    {
                        VariablePropertyPortAttribute variablePropertyPortAttribute = variablePropertyPortAttributes.ElementAt(0);
                        if (propertyPort.Direction != variablePropertyPortAttribute.Direction)
                        {
                            isDirty = true;
                            propertyPort.Direction = variablePropertyPortAttribute.Direction;
                        }

                        switch (propertyPort.Direction)
                        {
                            case PortDirection.Input:
                                inputPropertyPorts.Add(propertyPort);
                                break;
                            case PortDirection.Output:
                                outPropertyPorts.Add(propertyPort);
                                break;
                        }
                        if (propertyPort.Index == -1)
                        {
                            isDirty = true;
                            propertyPort.Index = variablePropertyPortAttribute.Priority;
                        }
                    }
                }
            }

            inputPropertyPorts = inputPropertyPorts.OrderBy(i => i.Index).ToList();
            outPropertyPorts = outPropertyPorts.OrderBy(i => i.Index).ToList();
            for (int i = 0; i < inputPropertyPorts.Count; i++)
            {
                inputPropertyPorts[i].Index = i;
            }
            for (int i = 0; i < outPropertyPorts.Count; i++)
            {
                outPropertyPorts[i].Index = i;
            }

            return isDirty;
        }
        public virtual void OnInputLinked(BaseEdge edge) { }
        public virtual void OnInputUnlinked(BaseEdge edge) { }
        public virtual void OnOutputLinked(BaseEdge edge) { }
        public virtual void OnOutputUnlinked(BaseEdge edge) { }
        
        public virtual void OnInputPropertyLinked(PropertyEdge propertyEdge) { }
        public virtual void OnInputPropertyUnLinked(PropertyEdge propertyEdge) { }
        public virtual void OnOutputPropertyLinked(PropertyEdge propertyEdge) { }
        public virtual void OnOutputPropertyUnLinked(PropertyEdge propertyEdge) { }
        public virtual void OnMoved() { }

        public virtual PropertyPort SetPropertyPort(string propertyPortName, Type propertyPortType, PortDirection direction)
        {
            FieldInfo fieldInfo = this.GetField(propertyPortName);
            if(fieldInfo == null)
                return null;

            PropertyPort propertyPort = fieldInfo.GetValue(this) as PropertyPort;
            if(propertyPort.GetType() != propertyPortType)
            {
                propertyPort = Activator.CreateInstance(propertyPortType) as PropertyPort;
                fieldInfo.SetValue(this, propertyPort);
            }
            propertyPort.Name = propertyPortName;
            propertyPort.Direction = direction;
            m_PropertyPortMap[propertyPortName] = propertyPort;
            propertyPort.Init(this);
            return propertyPort;
        }
        public virtual PropertyPort AddPropertyPort(string fieldName, string propertyPortName, Type propertyPortType, PortDirection direction)
        {
            PropertyPort propertyPort = Activator.CreateInstance(propertyPortType) as PropertyPort;
            propertyPort.Name = propertyPortName;
            propertyPort.Direction = direction;
            
            FieldInfo fieldInfo = this.GetField(fieldName);
            List<PropertyPort> propertyPorts = fieldInfo.GetValue(this) as List<PropertyPort>;
            propertyPorts.Add(propertyPort);
            fieldInfo.SetValue(this, propertyPorts);

            m_PropertyPortMap.Add(propertyPortName, propertyPort);
            propertyPort.Init(this);
            return propertyPort;
        }
        public virtual void RemovePropertyPort(string fieldName, PropertyPort propertyPort)
        {
            FieldInfo fieldInfo = this.GetField(fieldName);
            List<PropertyPort> propertyPorts = fieldInfo.GetValue(this) as List<PropertyPort>;
            if (propertyPorts.Contains(propertyPort))
            {
                propertyPorts.Remove(propertyPort);
                fieldInfo.SetValue(this, propertyPorts);
                m_PropertyPortMap.Remove(propertyPort.Name);
            }
        }

        public virtual void OnNodeChangedCallback()
        {
            m_OnNodeChanged?.Invoke();
        }
    }

    public abstract partial class ActionNode : BaseNode
    {
        public override void OnInputLinked(BaseEdge edge)
        {
            base.OnInputLinked(edge);
            m_InputEdgeGUID = edge.GUID;
            m_Parent = edge.StartNode;
        }
        public override void OnInputUnlinked(BaseEdge edge)
        {
            base.OnInputUnlinked(edge);

            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
        }

        public override void OnMoved()
        {
            base.OnMoved();
            if (m_Parent is CompositeNode compositeNode)
                compositeNode.OrderChildren();
        }
    }

    public abstract partial class CompositeNode : BaseNode
    {
        public override void OnInputLinked(BaseEdge edge)
        {
            base.OnInputLinked(edge);

            m_InputEdgeGUID = edge.GUID;
            m_Parent = edge.StartNode;
        }
        public override void OnInputUnlinked(BaseEdge edge)
        {
            base.OnInputUnlinked(edge);

            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
        }
        public override void OnOutputLinked(BaseEdge edge)
        {
            base.OnOutputLinked(edge);

            m_OutputEdgeGUIDs.Add(edge.GUID);
            m_Children.Add(edge.EndNode);

            OrderChildren();
        }
        public override void OnOutputUnlinked(BaseEdge edge)
        {
            base.OnOutputUnlinked(edge);

            m_OutputEdgeGUIDs.Remove(edge.GUID);
            m_Children.Remove(edge.EndNode);

            OrderChildren();
        }
        public override void OnMoved()
        {
            base.OnMoved();
            if (m_Parent is CompositeNode compositeNode)
                compositeNode.OrderChildren();
        }
        public void OrderChildren()
        {
            m_Children = m_Children.OrderBy(i => i.Position.y).ToList();
            m_OutputEdgeGUIDs = m_OutputEdgeGUIDs.OrderBy(i => m_Owner.GUIDEdgeMap[i].EndNode.Position.y).ToList();
        }
    }

    public abstract partial class DecoratorNode : BaseNode
    {
        public override void OnInputLinked(BaseEdge edge)
        {
            base.OnInputLinked(edge);
            m_InputEdgeGUID = edge.GUID;
            m_Parent = edge.StartNode;
        }
        public override void OnInputUnlinked(BaseEdge edge)
        {
            base.OnInputUnlinked(edge);

            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
        }
        public override void OnOutputLinked(BaseEdge edge)
        {
            base.OnOutputLinked(edge);

            m_OutputEdgeGUID = edge.GUID;
            m_Child = edge.EndNode;
        }
        public override void OnOutputUnlinked(BaseEdge edge)
        {
            base.OnOutputUnlinked(edge);

            m_OutputEdgeGUID = string.Empty;
            m_Child = null;
        }
        public override void OnMoved()
        {
            base.OnMoved();
            if (m_Parent is CompositeNode compositeNode)
                compositeNode.OrderChildren();
        }
    }

    public abstract partial class TriggerNode : BaseNode
    {
        public override void OnOutputLinked(BaseEdge edge)
        {
            base.OnOutputLinked(edge);

            m_OutputEdgeGUID = edge.GUID;
            m_Child = edge.EndNode;
        }
        public override void OnOutputUnlinked(BaseEdge edge)
        {
            base.OnOutputUnlinked(edge);

            m_OutputEdgeGUID = string.Empty;
            m_Child = null;
        }
    }

    
    public partial class RootNode : BaseNode
    {
        public override NodeCapabilities Capabilities => base.Capabilities & ~NodeCapabilities.Deletable & ~NodeCapabilities.Copiable & ~NodeCapabilities.Groupable & ~NodeCapabilities.Stackable;
        public override void OnOutputLinked(BaseEdge edge)
        {
            base.OnOutputLinked(edge);

            m_OutputEdgeGUID = edge.GUID;
            m_Child = edge.EndNode;
        }
        public override void OnOutputUnlinked(BaseEdge edge)
        {
            base.OnOutputUnlinked(edge);

            m_OutputEdgeGUID = string.Empty;
            m_Child = null;
        }
    }

    public partial class ExposedPropertyNode : BaseNode
    {
        public static ExposedPropertyNode Create(BaseExposedProperty exposedProperty)
        {
            ExposedPropertyNode exposedPropertyNode = exposedProperty.Owner.CreateNode(typeof(ExposedPropertyNode)) as ExposedPropertyNode;
            exposedPropertyNode.SetExposedProperty(exposedProperty);
            return exposedPropertyNode;
        }
        public override void OnInputLinked(BaseEdge edge)
        {
            base.OnInputLinked(edge);
            m_InputEdgeGUID = edge.GUID;
            m_Parent = edge.StartNode;
        }
        public override void OnInputUnlinked(BaseEdge edge)
        {
            base.OnInputUnlinked(edge);
            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
        }
        public override void OnMoved()
        {
            base.OnMoved();
            if (m_Parent is CompositeNode compositeNode)
                compositeNode.OrderChildren();
        }

        public void SetNodeType(ExposedPropertyNodeType nodeType)
        {
            m_NodeType = nodeType;
        }
        public void SetExposedProperty(BaseExposedProperty exposedProperty)
        {
            foreach (var targetTypePair in PropertyPortUtility.TargetTypeMap)
            {
                if (targetTypePair.Value == ExposedPropertyUtility.TargetType(exposedProperty.GetType()))
                {
                    switch (m_NodeType)
                    {
                        case ExposedPropertyNodeType.Get:
                            SetPropertyPort("m_Value", targetTypePair.Key, PortDirection.Output);
                            break;
                        case ExposedPropertyNodeType.Set:
                            SetPropertyPort("m_Value", targetTypePair.Key, PortDirection.Input);
                            break;
                    }
                    break;
                }
            }
            m_ExposedPropertyGUID = exposedProperty.GUID;
            m_ExposedProperty = exposedProperty;
        }
        public void SetExposedPropertyWithoutChangePropertyPort(BaseExposedProperty exposedProperty)
        {
            m_ExposedPropertyGUID = exposedProperty.GUID;
            m_ExposedProperty = exposedProperty;
        }
        public void RemoveExposedProperty()
        {
            switch (m_NodeType)
            {
                case ExposedPropertyNodeType.Get:
                    SetPropertyPort("m_Value", typeof(PropertyPort), PortDirection.Output);
                    break;
                case ExposedPropertyNodeType.Set:
                    SetPropertyPort("m_Value", typeof(PropertyPort), PortDirection.Input);
                    break;
            }
            m_ExposedPropertyGUID = string.Empty;
            m_ExposedProperty = null;
        }
    }

    public partial class TreeValueNode : BaseNode
    {
        [SerializeField, ShowInPanel("Preview")]
        BaseTree m_PreviewTree;
        public BaseTree PreviewTree => m_PreviewTree;

        public override void OnInputLinked(BaseEdge edge)
        {
            base.OnInputLinked(edge);
            m_InputEdgeGUID = edge.GUID;
            m_Parent = edge.StartNode;
        }
        public override void OnInputUnlinked(BaseEdge edge)
        {
            base.OnInputUnlinked(edge);
            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
        }
        public override void OnMoved()
        {
            base.OnMoved();
            if (m_Parent is CompositeNode compositeNode)
                compositeNode.OrderChildren();
        }

        public void SetNodeType(TreeValueNodeType nodeType)
        {
            m_NodeType = nodeType;
        }
        public void SetExposedProperty(BaseExposedProperty exposedProperty)
        {
            foreach (var targetTypePair in PropertyPortUtility.TargetTypeMap)
            {
                if (targetTypePair.Value == ExposedPropertyUtility.TargetType(exposedProperty.GetType()))
                {
                    switch (m_NodeType)
                    {
                        case TreeValueNodeType.Get:
                            SetPropertyPort("m_Value", targetTypePair.Key, PortDirection.Output).Index = 1;
                            break;
                        case TreeValueNodeType.Set:
                            SetPropertyPort("m_Value", targetTypePair.Key, PortDirection.Input).Index = 1;
                            break;
                    }
                    break;
                }
            }
            m_ExposedPropertyName = exposedProperty.Name;
        }
        public void SetExposedPropertyWithoutChangePropertyPort(BaseExposedProperty exposedProperty)
        {
            m_ExposedPropertyName = exposedProperty.Name;
        }
        public void RemoveExposedProperty()
        {
            switch (m_NodeType)
            {
                case TreeValueNodeType.Get:
                    SetPropertyPort("m_Value", typeof(PropertyPort), PortDirection.Output);
                    break;
                case TreeValueNodeType.Set:
                    SetPropertyPort("m_Value", typeof(PropertyPort), PortDirection.Input);
                    break;
            }
            m_ExposedPropertyName = string.Empty;
        }
    }

    public partial class SubTreeNode : BaseNode
    {
        public override void OnInputLinked(BaseEdge edge)
        {
            base.OnInputLinked(edge);
            m_InputEdgeGUID = edge.GUID;
            m_Parent = edge.StartNode;
        }
        public override void OnInputUnlinked(BaseEdge edge)
        {
            base.OnInputUnlinked(edge);

            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
        }
        public override void OnOutputLinked(BaseEdge edge)
        {
            base.OnOutputLinked(edge);

            m_OutputEdgeGUID = edge.GUID;
            m_Child = edge.EndNode;
        }
        public override void OnOutputUnlinked(BaseEdge edge)
        {
            base.OnOutputUnlinked(edge);

            m_OutputEdgeGUID = string.Empty;
            m_Child = null;
        }
        public override void OnMoved()
        {
            base.OnMoved();
            if (m_Parent is CompositeNode compositeNode)
                compositeNode.OrderChildren();
        }
    }


    public partial class ForNode : DecoratorNode 
    {
        public override void OnInputPropertyLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyLinked(propertyEdge);
            switch (propertyEdge.EndPortName)
            {
                case "m_List":
                    if (!IsConnected("m_Element"))
                    {
                        SetPropertyPort("m_Element", propertyEdge.EndPort.GetType().GetElementPropertyPortType(), PortDirection.Output);
                    }
                    break;
            }
        }
        public override void OnInputPropertyUnLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyUnLinked(propertyEdge);
            switch (propertyEdge.EndPortName)
            {
                case "m_List":
                    if (!IsConnected("m_Element"))
                    {
                        SetPropertyPort("m_List", typeof(PropertyPort), PortDirection.Input);
                        SetPropertyPort("m_Element", typeof(PropertyPort), PortDirection.Output);
                    }
                    break;
            }
        }
        public override void OnOutputPropertyLinked(PropertyEdge propertyEdge)
        {
            base.OnOutputPropertyLinked(propertyEdge);
            if (propertyEdge.StartPortName == "m_Element")
            {
                if (!IsConnected("m_List"))
                {
                    SetPropertyPort("m_List", propertyEdge.StartPort.GetType().GetListPropertyPortType(), PortDirection.Input);
                }
            }    
        }
        public override void OnOutputPropertyUnLinked(PropertyEdge propertyEdge)
        {
            base.OnOutputPropertyUnLinked(propertyEdge);
            if (propertyEdge.StartPortName == "m_Element")
            {
                if (!IsConnected("m_List") && !IsConnected("m_Element"))
                {
                    SetPropertyPort("m_List", typeof(PropertyPort), PortDirection.Input);
                    SetPropertyPort("m_Element", typeof(PropertyPort), PortDirection.Output);
                }
            }
        }

        List<Type> AcceptableTypes(string name)
        {
            switch (name)
            {
                case "m_List":
                    return new List<Type> { typeof(List<>) };
                case "m_Element":
                    List<Type> acceptableTypes = new List<Type>();
                    foreach (var item in PropertyPortUtility.PropertyPortTypeMap)
                    {
                        if (!item.Value.ValueType.IsSubClassOfRawGeneric(typeof(List<>)))
                            acceptableTypes.Add(item.Value.ValueType);
                    }
                    return acceptableTypes;
                default:
                    return null;
            }
        }
    }

    public partial class ToListNode : ValueNode
    {
        public override void OnInputPropertyLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyLinked(propertyEdge);
            switch (propertyEdge.EndPortName)
            {
                case "m_Element":
                    if (!IsConnected("m_List"))
                    {
                        SetPropertyPort("m_List", propertyEdge.EndPort.GetType().GetListPropertyPortType(), PortDirection.Output);
                    }
                    break;
            }
        }
        public override void OnInputPropertyUnLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyUnLinked(propertyEdge);
            switch (propertyEdge.EndPortName)
            {
                case "m_Element":
                    if (!IsConnected("m_List"))
                    {
                        SetPropertyPort("m_Element", typeof(PropertyPort), PortDirection.Input);
                        SetPropertyPort("m_List", typeof(PropertyPort), PortDirection.Output);
                    }
                    break;
            }
        }
        public override void OnOutputPropertyLinked(PropertyEdge propertyEdge)
        {
            base.OnOutputPropertyLinked(propertyEdge);
            if (propertyEdge.StartPortName == "m_List")
            {
                if (!IsConnected("m_Element"))
                {
                    SetPropertyPort("m_Element", propertyEdge.StartPort.GetType().GetElementPropertyPortType(), PortDirection.Input);
                }
            }
        }
        public override void OnOutputPropertyUnLinked(PropertyEdge propertyEdge)
        {
            base.OnOutputPropertyUnLinked(propertyEdge);
            if (propertyEdge.StartPortName == "m_List")
            {
                if (!IsConnected("m_List") && !IsConnected("m_Element"))
                {
                    SetPropertyPort("m_Element", typeof(PropertyPort), PortDirection.Input);
                    SetPropertyPort("m_List", typeof(PropertyPort), PortDirection.Output);
                }
            }
        }

        List<Type> AcceptableTypes(string name)
        {
            switch (name)
            {
                case "m_Element":
                    List<Type> acceptableTypes = new List<Type>();
                    foreach (var item in PropertyPortUtility.PropertyPortTypeMap)
                    {
                        if (!item.Value.ValueType.IsSubClassOfRawGeneric(typeof(List<>)))
                            acceptableTypes.Add(item.Value.ValueType);
                    }
                    return acceptableTypes;
                case "m_List":
                    return new List<Type> { typeof(List<>) };
                default:
                    return null;
            }
        }
    }

    public partial class ToStringNode : ValueNode 
    {
        public override string ToString()
        {
            return "ToString";
        }
        public override void OnInputPropertyUnLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyUnLinked(propertyEdge);
            SetPropertyPort("m_Value", typeof(PropertyPort), PortDirection.Input);
        }

        List<Type> AcceptableTypes(string name)
        {
            return new List<Type> { typeof(bool), typeof(int), typeof(float), };
        }
    }

    public abstract partial class MathNode : ValueNode 
    {
        public override void OnInputPropertyLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyLinked(propertyEdge);
            switch (propertyEdge.EndPortName)
            {
                case "m_InputValue1":
                    if(!IsConnected("m_InputValue2"))
                        SetPropertyPort("m_InputValue2", propertyEdge.EndPort.GetType(), PortDirection.Input);
                    if (!IsConnected("m_OutputValue"))
                        SetPropertyPort("m_OutputValue", propertyEdge.EndPort.GetType(), PortDirection.Output);
                    break;
                case "m_InputValue2":
                    if (!IsConnected("m_InputValue1"))
                        SetPropertyPort("m_InputValue1", propertyEdge.EndPort.GetType(), PortDirection.Input);
                    if (!IsConnected("m_OutputValue"))
                        SetPropertyPort("m_OutputValue", propertyEdge.EndPort.GetType(), PortDirection.Output);
                    break;
            }
        }
        public override void OnInputPropertyUnLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyUnLinked(propertyEdge);
            switch (propertyEdge.EndPortName)
            {
                case "m_InputValue1":
                    if (!IsConnected("m_InputValue2") && !IsConnected("m_OutputValue"))
                    {
                        SetPropertyPort("m_InputValue1", typeof(PropertyPort), PortDirection.Input);
                        SetPropertyPort("m_InputValue2", typeof(PropertyPort), PortDirection.Input);
                        SetPropertyPort("m_OutputValue", typeof(PropertyPort), PortDirection.Output);
                    }
                    break;
                case "m_InputValue2":
                    if (!IsConnected("m_InputValue1") && !IsConnected("m_OutputValue"))
                    {
                        SetPropertyPort("m_InputValue1", typeof(PropertyPort), PortDirection.Input);
                        SetPropertyPort("m_InputValue2", typeof(PropertyPort), PortDirection.Input);
                        SetPropertyPort("m_OutputValue", typeof(PropertyPort), PortDirection.Output);
                    }
                    break;
            }
        }
        public override void OnOutputPropertyLinked(PropertyEdge propertyEdge)
        {
            base.OnOutputPropertyLinked(propertyEdge);
            if(propertyEdge.StartPortName == "m_OutputValue")
            {
                if (!IsConnected("m_InputValue1"))
                    SetPropertyPort("m_InputValue1", propertyEdge.EndPort.GetType(), PortDirection.Input);
                if (!IsConnected("m_InputValue2"))
                    SetPropertyPort("m_InputValue2", propertyEdge.EndPort.GetType(), PortDirection.Input);
            }
        }
        public override void OnOutputPropertyUnLinked(PropertyEdge propertyEdge)
        {
            base.OnOutputPropertyUnLinked(propertyEdge);
            if(propertyEdge.StartPortName == "m_OutputValue")
            {
                if (!IsConnected("m_InputValue1") && !IsConnected("m_InputValue2") && !IsConnected("m_OutputValue"))
                {
                    SetPropertyPort("m_InputValue1", typeof(PropertyPort), PortDirection.Input);
                    SetPropertyPort("m_InputValue2", typeof(PropertyPort), PortDirection.Input);
                    SetPropertyPort("m_OutputValue", typeof(PropertyPort), PortDirection.Output);
                }
            }
        }
    }

    public partial class EqualNode : ValueNode
    {
        public override void OnInputPropertyLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyLinked(propertyEdge);
            switch (propertyEdge.EndPortName)
            {
                case "m_InputValue1":
                    if (!IsConnected("m_InputValue2"))
                        SetPropertyPort("m_InputValue2", propertyEdge.EndPort.GetType(), PortDirection.Input);
                    break;
                case "m_InputValue2":
                    if (!IsConnected("m_InputValue1"))
                        SetPropertyPort("m_InputValue1", propertyEdge.EndPort.GetType(), PortDirection.Input);
                    break;
            }
        }
        public override void OnInputPropertyUnLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyUnLinked(propertyEdge);
            switch (propertyEdge.EndPortName)
            {
                case "m_InputValue1":
                    if (!IsConnected("m_InputValue2") && !IsConnected("m_OutputValue"))
                    {
                        SetPropertyPort("m_InputValue1", typeof(PropertyPort), PortDirection.Input);
                        SetPropertyPort("m_InputValue2", typeof(PropertyPort), PortDirection.Input);
                    }
                    break;
                case "m_InputValue2":
                    if (!IsConnected("m_InputValue1") && !IsConnected("m_OutputValue"))
                    {
                        SetPropertyPort("m_InputValue1", typeof(PropertyPort), PortDirection.Input);
                        SetPropertyPort("m_InputValue2", typeof(PropertyPort), PortDirection.Input);
                    }
                    break;
            }
        }

        List<Type> AcceptableTypes(string name)
        {
            List<Type> acceptableTypes = new List<Type>();
            foreach (var item in PropertyPortUtility.PropertyPortTypeMap)
            {
                acceptableTypes.Add(item.Value.ValueType);
            }
            return acceptableTypes;
        }
    }

    public partial class ValidNode : ValueNode
    {
        public override void OnInputPropertyUnLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyUnLinked(propertyEdge);
            SetPropertyPort("m_InputValue", typeof(PropertyPort), PortDirection.Input);
        }

        List<Type> AcceptableTypes(string name)
        {
            List<Type> acceptableTypes = new List<Type>();
            foreach (var propertyPortTypePair in PropertyPortUtility.PropertyPortTypeMap)
            {
                if(propertyPortTypePair.Value.ValueType.IsClass)
                    acceptableTypes.Add(propertyPortTypePair.Value.ValueType);
            }
            return acceptableTypes;
        }
    }

    public partial class CompareNode : ValueNode
    {
        public override void OnInputPropertyLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyLinked(propertyEdge);
            switch (propertyEdge.EndPortName)
            {
                case "m_InputValue1":
                    if (!IsConnected("m_InputValue2"))
                        SetPropertyPort("m_InputValue2", propertyEdge.EndPort.GetType(), PortDirection.Input);
                    break;
                case "m_InputValue2":
                    if (!IsConnected("m_InputValue1"))
                        SetPropertyPort("m_InputValue1", propertyEdge.EndPort.GetType(), PortDirection.Input);
                    break;
            }
        }
        public override void OnInputPropertyUnLinked(PropertyEdge propertyEdge)
        {
            base.OnInputPropertyUnLinked(propertyEdge);
            switch (propertyEdge.EndPortName)
            {
                case "m_InputValue1":
                    if (!IsConnected("m_InputValue2") && !IsConnected("m_OutputValue"))
                    {
                        SetPropertyPort("m_InputValue1", typeof(PropertyPort), PortDirection.Input);
                        SetPropertyPort("m_InputValue2", typeof(PropertyPort), PortDirection.Input);
                    }
                    break;
                case "m_InputValue2":
                    if (!IsConnected("m_InputValue1") && !IsConnected("m_OutputValue"))
                    {
                        SetPropertyPort("m_InputValue1", typeof(PropertyPort), PortDirection.Input);
                        SetPropertyPort("m_InputValue2", typeof(PropertyPort), PortDirection.Input);
                    }
                    break;
            }
        }
    }

    public partial class TimelineNode : BaseNode
    {
        public override void OnInputLinked(BaseEdge edge)
        {
            base.OnInputLinked(edge);
            m_InputEdgeGUID = edge.GUID;
            m_Parent = edge.StartNode;
        }
        public override void OnInputUnlinked(BaseEdge edge)
        {
            base.OnInputUnlinked(edge);

            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
        }
        public override void OnOutputLinked(BaseEdge edge)
        {
            base.OnOutputLinked(edge);
            int index = int.Parse(edge.StartPortName);
            m_OutputEdgeGUIDs[index] = edge.GUID;
            m_Children[index] = edge.EndNode;
        }
        public override void OnOutputUnlinked(BaseEdge edge)
        {
            base.OnOutputUnlinked(edge);
            int index = int.Parse(edge.StartPortName);
            m_OutputEdgeGUIDs[index] = string.Empty;
            m_Children[index] = null;
        }
    }
}
#endif