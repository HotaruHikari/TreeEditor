using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace TreeDesigner.Editor
{
    public class VariablePropertyPortView : PropertyPortView
    {
        string m_AcceptableTypesMethodName;
        List<Type> m_AcceptableTypes;

        public List<Type> AcceptableTypes 
        {
            get
            {
                if (string.IsNullOrEmpty(m_AcceptableTypesMethodName))
                    return m_AcceptableTypes;
                else
                    return (List<Type>)NodeView.Node.GetMethod(m_AcceptableTypesMethodName).Invoke(NodeView.Node, new object[] { m_Name });
            }
        }

        protected VariablePropertyPortView(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
        }
        public static VariablePropertyPortView Create<TEdge>(PropertyPort propertyPort, string acceptableTypesMethodName, Type[] acceptableTypes, Orientation orientation, Capacity capacity) where TEdge : Edge, new()
        {
            DefaultEdgeConnectorListener listener = new DefaultEdgeConnectorListener();
            VariablePropertyPortView port = new VariablePropertyPortView(orientation, (Direction)propertyPort.Direction, capacity, propertyPort.ValueType == null ? typeof(object) : propertyPort.ValueType)
            {
                m_EdgeConnector = new EdgeConnector<TEdge>(listener)
            };
            port.m_Name = propertyPort.Name;
            port.m_PropertyPort = propertyPort;
            port.m_AcceptableTypesMethodName = acceptableTypesMethodName;
            port.m_AcceptableTypes = acceptableTypes?.ToList() ?? null;
            port.AddManipulator(port.m_EdgeConnector);

            PortHandle portHandle = new PortHandle(port);
            port.m_PortHandle = portHandle;
            port.Insert(1, portHandle);
            portHandle.style.backgroundColor = port.portColor = propertyPort.Color();

            return port;
        }
    }
}