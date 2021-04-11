using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArchPackFormList", menuName = "Scriptable/ArchPackFormList")]
public class ArchPackFormList : ScriptableObject
{
    [SerializeField] private BlockLevel _ListLevel;
    public BlockLevel ListLevel => _ListLevel;

    [Header("Contain Forms")]
    [SerializeField] private ArchPackForm[] _Forms;
    public ArchPackForm[] Forms => _Forms;

    public ArchPackForm Random()
    {
        return _Forms[UnityEngine.Random.Range(0, _Forms.Length)];
    }
}
