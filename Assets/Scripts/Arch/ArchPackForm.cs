using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Form
{
    public float DefaultRotationZ;

    [Header("Ratio Property")]
    [Range( 0f, 1f)] public float InitRatio;
    [Range(-1f, 1f)] public float RatioOffset;
}
[CreateAssetMenu(fileName = "ArchPackForm", menuName = "Scriptable/ArchPackForm")]
public class ArchPackForm : ScriptableObject
{
    public BlockLevel BlockLevel;
    public float FitAxisOffset;
    [SerializeField] private Form[] _Forms;

    public Form[] Forms
    {
        get => _Forms;
        set => _Forms = value;
    }
    public int Length => _Forms.Length;
}
