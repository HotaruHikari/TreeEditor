using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable All

namespace TreeDesigner
{
    public class TreeRunner : MonoBehaviour
    {
        [SerializeField]
        protected BaseTree m_Tree;
        [SerializeField, Min(1)]
        protected int m_UpdateInterval;
        [SerializeField, Min(1)]
        protected bool m_Loop;

        bool m_Running;

        void Update()
        {
            if (!m_Tree || !m_Running || Time.frameCount % m_UpdateInterval != 0)
                return;
            if (!m_Tree.Running && m_Loop)
            {
                m_Tree.ResetTree();
                m_Tree.UpdateTree();
            }
            else
            {
                m_Tree.UpdateTree();
            }
        }

        [ContextMenu("CloneTree")]
        void CloneTree()
        {
            m_Tree = Instantiate(m_Tree);
            m_Tree.OnSpawn();
        }
        [ContextMenu("InitTree")]
        void InitTree()
        {
            m_Tree?.InitTree();
        }
        [ContextMenu("UpdateTree")]
        void UpdateTree()
        {
            m_Tree?.UpdateTree();
            m_Running = true;
        }
        [ContextMenu("ResetTree")]
        void ResetTree()
        {
            m_Tree?.ResetTree();
            m_Tree.Running = false;
            m_Running = false;
        }
        [ContextMenu("PauseTree")]
        void PauseTree()
        {
            m_Running = false;
        }
        [ContextMenu("ResumeTree")]
        void ResumeTree()
        {
            m_Running = true;
        }
    }
}