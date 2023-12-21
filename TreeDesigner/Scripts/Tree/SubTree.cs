using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeDesigner
{
    [TreeWindow("OpenSubTreeWindow")]
    public partial class SubTree : BaseTree
    {
        [SerializeField]
        protected string m_EndGUID;
        public string EndGUID { get => m_EndGUID; set => m_EndGUID = value; }

        [NonSerialized]
        protected BaseNode m_End;
        public BaseNode End => m_End;

        [NonSerialized]
        protected BaseTree m_Owner;
        public BaseTree Owner => m_Owner;

        public virtual void Init(BaseTree tree)
        {
            m_Owner = tree;
            InitTree();
        }
    }
}