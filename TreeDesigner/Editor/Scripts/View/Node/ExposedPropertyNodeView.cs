using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace TreeDesigner.Editor
{
    public class ExposedPropertyNodeView : BaseNodeView
    {
        Label m_ExposedPropertyDropdownButton;
        EnumMenuView m_NodeTypeDropdownMenuView;

        public ExposedPropertyNode ExposedPropertyNode => m_Node as ExposedPropertyNode;
        public BaseExposedProperty ExposedProperty => ExposedPropertyNode.ExposedProperty;

        public ExposedPropertyNodeView(BaseNode node, BaseTreeWindow treeWindow) : base(node, treeWindow, AssetDatabase.GUIDToAssetPath("def728764ea97d447bd64c946ffe66a1"))
        {
            m_ExposedPropertyDropdownButton = this.Q<Label>("property-dropdown-button");
            m_ExposedPropertyDropdownButton.AddManipulator(new DropdownMenuManipulator(ChangeExposedPropertyMenu, MouseButton.LeftMouse));

            m_NodeTypeDropdownMenuView = this.Q<EnumMenuView>("nodeType-field");
            m_NodeTypeDropdownMenuView.Init(ExposedPropertyNode.NodeType, string.Empty, ChangeNodeTypeMenu);

            OnNodeTypeChanged();
            OnExposedPropertyChanged();
        }
        public override void Refresh()
        {
            base.Refresh();
            if (ExposedProperty)
            {
                SerializedProperty serializedProperty = m_Node.GetNodeSerializedProperty("m_Value").FindPropertyRelative("m_Value");          
                m_NodePanel.AddPropertyPortField(serializedProperty, ExposedPropertyNode.Value, "Value");
                if (ExposedPropertyNode.NodeType == ExposedPropertyNodeType.Get)
                    m_NodePanel.SetPropertyPortFieldEnable("m_Value", false);

                if (ExposedPropertyNode.NodeType == ExposedPropertyNodeType.Set)
                {
                    if (ExposedPropertyNode.Value.ValueType.IsSubClassOfRawGeneric(typeof(List<>)))
                        m_NodeInputFieldContainer.AddEmptyField("m_Value");
                    else
                    {
                        m_NodeInputFieldContainer.AddPropertyPortField(serializedProperty, ExposedPropertyNode.Value);
                        m_NodeInputFieldContainer.SetPropertyPortFieldEnable("m_Value", !ExposedPropertyNode.IsConnected("m_Value"));
                    }
                }
                RefreshShowPanelState();
            }
        }
        protected override void RefreshCollapseButton()
        {
            m_CollapseButton.SetEnabled(!m_CollapseButton.enabledSelf);
            m_CollapseButton.SetEnabled(true);
        }
        protected override void GeneratePorts()
        {
            base.GeneratePorts();
            switch (ExposedPropertyNode.NodeType)
            {
                case ExposedPropertyNodeType.Get:
                    
                    break;
                case ExposedPropertyNodeType.Set:
                    m_InputPortContainer.AddPort("Input", Direction.Input, Port.Capacity.Single);
                    break;
            }
        }
        protected override void GeneratePropertyPorts()
        {
            base.GeneratePropertyPorts();
            switch (ExposedPropertyNode.NodeType)
            {
                case ExposedPropertyNodeType.Get:
                    m_OutputPortContainer.AddPropertyPort(ExposedPropertyNode.Value, "Value", Port.Capacity.Multi);
                    break;
                case ExposedPropertyNodeType.Set:
                    m_InputPortContainer.AddPropertyPort(ExposedPropertyNode.Value, "Value", Port.Capacity.Single);
                    break;
            }
        }

        void ChangeExposedPropertyMenu(DropdownMenu menu)
        {
            foreach (var exposedProperty in m_Node.Owner.ExposedProperties.OrderBy(i => i.Index))
            {
                menu.AppendAction(exposedProperty.Name, (a) =>
                {
                    if (exposedProperty != ExposedProperty)
                    {
                        m_Node.ApplyModify("Change ExposedProperty", () =>
                        {
                            OnExposedPropertyCleared();
                            if (!ExposedProperty || exposedProperty.GetType() != ExposedProperty.GetType())
                            {
                                switch (ExposedPropertyNode.NodeType)
                                {
                                    case ExposedPropertyNodeType.Get:
                                        TreeView.DeleteElements(OutputPropertyPorts["m_Value"].connections);
                                        ExposedPropertyNode.SetExposedProperty(exposedProperty);
                                        OutputPropertyPorts["m_Value"].SetPropertyPort(ExposedPropertyNode.Value);
                                        break;
                                    case ExposedPropertyNodeType.Set:
                                        TreeView.DeleteElements(InputPropertyPorts["m_Value"].connections);
                                        ExposedPropertyNode.SetExposedProperty(exposedProperty);
                                        InputPropertyPorts["m_Value"].SetPropertyPort(ExposedPropertyNode.Value);
                                        break;
                                }
                            }
                            else
                            {
                                switch (ExposedPropertyNode.NodeType)
                                {
                                    case ExposedPropertyNodeType.Get:
                                        ExposedPropertyNode.SetExposedPropertyWithoutChangePropertyPort(exposedProperty);
                                        break;
                                    case ExposedPropertyNodeType.Set:
                                        ExposedPropertyNode.SetExposedPropertyWithoutChangePropertyPort(exposedProperty);
                                        break;
                                }
                            }
                            m_Node.GetNewSerializedTree();
                            OnExposedPropertyChanged();
                            Refresh();
                        });
                    }
                }, (DropdownMenuAction a) => exposedProperty != ExposedProperty ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Checked);
            }
            if (!ExposedProperty)
                menu.AppendAction("Empty", (a) => { }, (DropdownMenuAction a) => DropdownMenuAction.Status.Checked);
        }
        void ChangeNodeTypeMenu(object value)
        {
            ExposedPropertyNode.ApplyModify("ChangeNodeType", () =>
            {
                ExposedPropertyNodeType selectedNodeType = (ExposedPropertyNodeType)value;
                if (ExposedPropertyNode.NodeType != selectedNodeType)
                {
                    switch (ExposedPropertyNode.NodeType)
                    {
                        case ExposedPropertyNodeType.Get:
                            m_OutputPortContainer.RemovePropertyPort(ExposedPropertyNode.Value);
                            break;
                        case ExposedPropertyNodeType.Set:
                            m_InputPortContainer.RemovePort("Input");
                            m_InputPortContainer.RemovePropertyPort(ExposedPropertyNode.Value);
                            break;
                    }
                    switch (selectedNodeType)
                    {
                        case ExposedPropertyNodeType.Get:
                            ExposedPropertyNode.Value.Direction = PortDirection.Output;
                            break;
                        case ExposedPropertyNodeType.Set:
                            ExposedPropertyNode.Value.Direction = PortDirection.Input;
                            break;
                    }
                    switch (selectedNodeType)
                    {
                        case ExposedPropertyNodeType.Get:
                            m_OutputPortContainer.AddPropertyPort(ExposedPropertyNode.Value, "Value", Port.Capacity.Multi);
                            break;
                        case ExposedPropertyNodeType.Set:
                            m_InputPortContainer.AddPort("Input", Direction.Input, Port.Capacity.Single);
                            m_InputPortContainer.AddPropertyPort(ExposedPropertyNode.Value, "Value", Port.Capacity.Single);
                            break;
                    }
                    ExposedPropertyNode.SetNodeType(selectedNodeType);
                    Refresh();
                    RefreshPorts();
                    OnNodeTypeChanged();
                }
            });
        }

        void OnNodeTypeChanged()
        {
            RemoveFromClassList("get");
            RemoveFromClassList("set");
            switch (ExposedPropertyNode.NodeType)
            {
                case ExposedPropertyNodeType.Get:
                    AddToClassList("get");
                    titleContainer.style.backgroundColor = new Color(74, 42, 192, 255) / 255;
                    break;
                case ExposedPropertyNodeType.Set:
                    AddToClassList("set");
                    titleContainer.style.backgroundColor = new Color(239, 71, 111, 255) / 255;
                    break;
            }
        }
        void OnExposedPropertyChanged()
        {
            if (ExposedProperty)
            {
                ExposedProperty.OnRemoved += OnExposedPropertyRemoved;
                ExposedProperty.OnNameChanged += OnExposedPropertyNameChanged;
                ExposedProperty.OnSelected += OnExposedPropertySelected;
                m_ExposedPropertyDropdownButton.text = ExposedProperty.Name;
            }
            OnExposedPropertyNameChanged();
        }
        void OnExposedPropertyCleared()
        {
            if (ExposedProperty)
            {
                ExposedProperty.OnRemoved -= OnExposedPropertyRemoved;
                ExposedProperty.OnNameChanged -= OnExposedPropertyNameChanged;
                ExposedProperty.OnSelected -= OnExposedPropertySelected;
            }
            OnExposedPropertyNameChanged();
        }
        void OnExposedPropertyRemoved()
        {
            OnExposedPropertyCleared();
            switch (ExposedPropertyNode.NodeType)
            {
                case ExposedPropertyNodeType.Get:
                    TreeView.DeleteElements(OutputPropertyPorts["m_Value"].connections);
                    ExposedPropertyNode.RemoveExposedProperty();
                    OutputPropertyPorts["m_Value"].SetPropertyPort(ExposedPropertyNode.Value);
                    break;
                case ExposedPropertyNodeType.Set:
                    TreeView.DeleteElements(InputPropertyPorts["m_Value"].connections);
                    ExposedPropertyNode.RemoveExposedProperty();
                    InputPropertyPorts["m_Value"].SetPropertyPort(ExposedPropertyNode.Value);
                    break;
            }
            OnExposedPropertyNameChanged();
            Refresh();
        }
        void OnExposedPropertyNameChanged()
        {
            
            if (ExposedProperty)
            {
                title = ExposedProperty.Name;
                m_ExposedPropertyDropdownButton.text = ExposedProperty.Name;
            }
            else
            {
                title = "ExposedPropertyNode";
                m_ExposedPropertyDropdownButton.text = "Empty";
            }
        }
        void OnExposedPropertySelected()
        {
            TreeView.AddToSelection(this);
        }
    }
}