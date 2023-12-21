using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace TreeDesigner.Editor
{
    public class SubTreeNodeView : BaseNodeView
    {
        VisualElement m_InputPortControlContainer;
        VisualElement m_OutputPortControlContainer;
        Label m_AddInputPortButton;
        Label m_RemoveInputPortButton;
        Label m_AddOutputPortButton;
        Label m_RemoveOutputPortButton;

        public SubTreeNode SubTreeNode => m_Node as SubTreeNode;
        public SubTree SubTree => SubTreeNode.SubTree;
        public SubTreeNodeView(BaseNode node, BaseTreeWindow treeWindow) : base(node, treeWindow, AssetDatabase.GUIDToAssetPath("8d935ecb420b3ef4094ee19c709db8d7"))
        {
            m_InputPortControlContainer = this.Q("inputPort-control-container");
            m_OutputPortControlContainer = this.Q("outputPort-control-container");
            m_AddInputPortButton = m_InputPortControlContainer.Q<Label>("add-port-button");
            m_RemoveInputPortButton = m_InputPortControlContainer.Q<Label>("remove-port-button");
            m_AddOutputPortButton = m_OutputPortControlContainer.Q<Label>("add-port-button");
            m_RemoveOutputPortButton = m_OutputPortControlContainer.Q<Label>("remove-port-button");

            m_AddInputPortButton.AddManipulator(new DropdownMenuManipulator((e) =>
            {
                if (SubTree)
                {
                    foreach (var exposedProperty in SubTree.ExposedProperties)
                    {
                        if (SubTreeNode.InputPropertyPorts.Find(i => i.Name == $"{exposedProperty.Name}_Input") == null)
                        {
                            e.AppendAction($"{exposedProperty.Name}", (s) =>
                            {
                                foreach (var targetTypePair in PropertyPortUtility.TargetTypeMap)
                                {
                                    if (targetTypePair.Value == ExposedPropertyUtility.TargetType(exposedProperty.GetType()))
                                    {
                                        SubTreeNode.ApplyModify("Add InputPropertyPort", () =>
                                        {
                                            PropertyPort propertyPort = SubTreeNode.AddPropertyPort("m_InputPropertyPorts", $"{exposedProperty.Name}_Input", targetTypePair.Key, PortDirection.Input);
                                            m_InputPortContainer.AddPropertyPort(propertyPort, exposedProperty.Name, Port.Capacity.Single);
                                            Refresh();
                                            RefreshPorts();
                                        });
                                        break;
                                    }
                                }
                            });
                        }
                    }
                }
            }, MouseButton.LeftMouse));
            m_RemoveInputPortButton.AddManipulator(new DropdownMenuManipulator((e) =>
            {
                foreach (var propertyPort in SubTreeNode.InputPropertyPorts)
                {
                    string portName = propertyPort.Name;
                    portName = portName.Substring(0, portName.Length - "_Input".Length);
                    e.AppendAction($"{portName}", (s) =>
                    {
                        SubTreeNode.ApplyModify("Remove InputPropertyPort", () =>
                        {
                            m_InputPortContainer.RemovePropertyPort(propertyPort);
                            SubTreeNode.RemovePropertyPort("m_InputPropertyPorts", propertyPort);
                            Refresh();
                            RefreshPorts();
                        });
                    });
                }
            }, MouseButton.LeftMouse));
            m_AddOutputPortButton.AddManipulator(new DropdownMenuManipulator((e) =>
            {
                foreach (var exposedProperty in SubTree.ExposedProperties)
                {
                    if (SubTreeNode.OutputPropertyPorts.Find(i => i.Name == $"{exposedProperty.Name}_Output") == null)
                    {
                        e.AppendAction($"{exposedProperty.Name}", (s) =>
                        {
                            foreach (var targetTypePair in PropertyPortUtility.TargetTypeMap)
                            {
                                if (targetTypePair.Value == ExposedPropertyUtility.TargetType(exposedProperty.GetType()))
                                {
                                    SubTreeNode.ApplyModify("Add OutputPropertyPort", () =>
                                    {
                                        PropertyPort propertyPort = SubTreeNode.AddPropertyPort("m_OutputPropertyPorts", $"{exposedProperty.Name}_Output", targetTypePair.Key, PortDirection.Output);
                                        m_OutputPortContainer.AddPropertyPort(propertyPort, exposedProperty.Name, Port.Capacity.Multi);
                                        Refresh();
                                        RefreshPorts();
                                    });
                                    break;
                                }
                            }
                        });
                    }
                }
            }, MouseButton.LeftMouse));
            m_RemoveOutputPortButton.AddManipulator(new DropdownMenuManipulator((e) =>
            {
                foreach (var propertyPort in SubTreeNode.OutputPropertyPorts)
                {
                    string portName = propertyPort.Name;
                    portName = portName.Substring(0, portName.Length - "_Output".Length);
                    e.AppendAction($"{portName}", (s) =>
                    {
                        SubTreeNode.ApplyModify("Remove OutputPropertyPort", () =>
                        {
                            m_OutputPortContainer.RemovePropertyPort(propertyPort);
                            SubTreeNode.RemovePropertyPort("m_OutputPropertyPorts", propertyPort);
                            Refresh();
                            RefreshPorts();
                        });
                    });
                }
            }, MouseButton.LeftMouse));
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            if (evt.target is BaseNodeView)
            {
                evt.menu.AppendAction("Open SubTree", (s) =>
                {
                    TreeWindowUtility.TreeWindowUtilityInstance.OpenSubTreeWindow(SubTree);
                }, (DropdownMenuAction a) => SubTree ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                evt.menu.AppendSeparator();
            }
        }
        public override void Update()
        {
            base.Update();
            if (SubTree)
                title = SubTree.name;
            else
                title = "SubTreeNode";

            m_InputPortControlContainer.style.width = m_InputPortContainer.layout.width;
            m_OutputPortControlContainer.style.width = m_OutputPortContainer.layout.width;
        }
        public override void Refresh()
        {
            base.Refresh();
            for (int i = 0; i < SubTreeNode.InputPropertyPorts.Count; i++)
            {
                PropertyPort propertyPort = SubTreeNode.InputPropertyPorts[i];
                SerializedProperty serializedProperty = m_Node.GetNodeSerializedProperty("m_InputPropertyPorts");
                serializedProperty = serializedProperty.GetArrayElementAtIndex(i);
                serializedProperty = serializedProperty.FindPropertyRelative("m_Value");

                m_NodePanel.AddPropertyPortField(serializedProperty, propertyPort, propertyPort.Name);
                m_NodePanel.SetPropertyPortFieldEnable(propertyPort.Name, !SubTreeNode.IsConnected(propertyPort.Name));

                m_NodeInputFieldContainer.AddPropertyPortField(serializedProperty, propertyPort);
                m_NodeInputFieldContainer.SetPropertyPortFieldEnable(propertyPort.Name, !SubTreeNode.IsConnected(propertyPort.Name));
            }
            for (int i = 0; i < SubTreeNode.OutputPropertyPorts.Count; i++)
            {
                PropertyPort propertyPort = SubTreeNode.OutputPropertyPorts[i];
                SerializedProperty serializedProperty = m_Node.GetNodeSerializedProperty("m_OutputPropertyPorts");
                serializedProperty = serializedProperty.GetArrayElementAtIndex(i);
                serializedProperty = serializedProperty.FindPropertyRelative("m_Value");

                m_NodePanel.AddPropertyPortField(serializedProperty, propertyPort, propertyPort.Name);
                m_NodePanel.SetPropertyPortFieldEnable(propertyPort.Name, false);
            }
        }
        protected override void RefreshCollapseButton()
        {
            m_CollapseButton.SetEnabled(!m_CollapseButton.enabledSelf);
            m_CollapseButton.SetEnabled(true);
        }
        protected override void GeneratePropertyPorts()
        {
            base.GeneratePropertyPorts();
            foreach (var propertyPort in SubTreeNode.InputPropertyPorts)
            {
                string valueLabel = propertyPort.Name;
                valueLabel = valueLabel.Substring(0, valueLabel.Length - "_Input".Length);
                m_InputPortContainer.AddPropertyPort(propertyPort, valueLabel, Port.Capacity.Single);
            }
            foreach (var propertyPort in SubTreeNode.OutputPropertyPorts)
            {
                string valueLabel = propertyPort.Name;
                valueLabel = valueLabel.Substring(0, valueLabel.Length - "_Output".Length);
                m_OutputPortContainer.AddPropertyPort(propertyPort, valueLabel, Port.Capacity.Multi);
            }
        }
    }
}