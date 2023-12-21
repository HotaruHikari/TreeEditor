using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TreeDesigner.Editor
{
    public class EnumMenuView : VisualElement
    {
        protected const string m_VisualTreeName = "EnumMenu";

        public new class UxmlFactory : UxmlFactory<EnumMenuView, UxmlTraits> { }

        Label m_Label;
        Label m_SelectedLabel;
        List<object> m_Elements = new List<object>();
        
        public event Action<object> OnSelected;
        
        public string SelectedElement => m_SelectedLabel.text;
        

        public EnumMenuView() 
        {
            VisualTreeAsset template = Resources.Load<VisualTreeAsset>($"VisualTree/{m_VisualTreeName}");
            template.CloneTree(this);
            AddToClassList("dropdownMenu");

            m_Label = this.Q<Label>("label");
            m_SelectedLabel = this.Q<Label>("title");


            m_SelectedLabel.AddManipulator(new DropdownMenuManipulator((menu) => 
            {
                foreach (var element in m_Elements)
                {
                    menu.AppendAction(element.ToString(), (s) =>
                    {
                        m_SelectedLabel.text = element.ToString();
                        OnSelected?.Invoke(element);
                    }, (DropdownMenuAction a) => SelectedElement == element.ToString() ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
                }
            }, MouseButton.LeftMouse));
        }

        public void Init(object selectedType, string label = null, Action<object> onSelectedCallback = null)
        {
            Array array = Enum.GetValues(selectedType.GetType());
            List<object> elements = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                elements.Add(array.GetValue(i));
            }
            Init(elements, selectedType.ToString(), label, onSelectedCallback);
        }
        public void Init(List<object> elements, string selectedElement, string label = null, Action<object> onSelectedCallback = null)
        {
            m_Elements = elements;
            m_SelectedLabel.text = selectedElement;
            m_Label.text = label;
            OnSelected += onSelectedCallback;
        }
    }
}