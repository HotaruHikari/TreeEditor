using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace TreeDesigner.Editor
{
    public class FolderView : VisualElement
    {
        Label m_Label;
        VisualElement m_Button;
        VisualElement m_Icon;
        VisualElement m_Content;
        public VisualElement Content => m_Content;
        
        bool m_Expanded;
        public bool Expanded
        {
            get => m_Expanded;
            set
            {
                m_Expanded = value;
                RefreshExpandedState();
                OnExpandedStateChanged?.Invoke();
            }
        }
        
        public Action OnExpandedStateChanged;

        public FolderView(string name)
        {
            VisualTreeAsset template = Resources.Load<VisualTreeAsset>($"VisualTree/Folder");
            template.CloneTree(this);

            m_Label = this.Q<Label>("folder-title");
            m_Label.text = name;

            m_Button = this.Q("folder-button");
            m_Button.AddManipulator(new Clickable(ToggleCollapsed));

            m_Icon = this.Q("folder-icon");
            m_Content = this.Q("content");

            RefreshExpandedState();
        }

        public void AddContent(VisualElement content)
        {
            m_Content.Add(content);
        }
        void ToggleCollapsed()
        {
            Expanded = !Expanded;
        }
        void RefreshExpandedState()
        {
            m_Button.RemoveFromClassList("expanded");
            m_Button.RemoveFromClassList("collapsed");
            m_Icon.RemoveFromClassList("expanded");
            m_Icon.RemoveFromClassList("collapsed");
            m_Content.RemoveFromClassList("expanded");
            m_Content.RemoveFromClassList("collapsed");

            if (m_Expanded)
            {
                m_Button.AddToClassList("expanded");
                m_Icon.AddToClassList("expanded");
                m_Content.AddToClassList("expanded");
            }
            else
            {
                m_Button.AddToClassList("collapsed");
                m_Icon.AddToClassList("collapsed");
                m_Content.AddToClassList("collapsed");
            }
        }
    }
}