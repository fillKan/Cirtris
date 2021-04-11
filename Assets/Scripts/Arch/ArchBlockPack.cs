using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchBlockPack : MonoBehaviour
{
    public  float  Ratio
    {
        get => _Ratio;
        set
        {
            value = Mathf.Min(value, 1f);

            int length = Length;
            for (int i = 0; i < length; i++)
            {
                var block = _ArchBlocks[i];
                block.Ratio = Mathf.Max(value + block.RatioOffset, block.InitRatio);
            }
            _Ratio = value;
        }
    }
    private float _Ratio = 0f;

    public float FitAxisOffset;

    [SerializeField] private ArchPackForm _Form;
    [SerializeField] private ArchBlock[] _ArchBlocks;

    public int Length => _ArchBlocks.Length;
    public ArchBlock[] ArchBlocks 
    {
        get => _ArchBlocks;
        set => _ArchBlocks = value;
    }
    public ArchBlock this[int index]
    {
        get
        {
            if (index < _ArchBlocks.Length)
            {
                return _ArchBlocks[index];
            }   return null;
        }
        set
        {
            if (index < _ArchBlocks.Length)
                _ArchBlocks[index] = value;
        }
    }


    private void Reset()
    {
        int length = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out ArchBlock archBlock))
            {
                length++;
            }
        }
        _ArchBlocks = new ArchBlock[length];

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out ArchBlock archBlock))
            {
                _ArchBlocks[i] = archBlock;
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("ArchPackForm Init")]
    private void ArchPackFormInit()
    {
        _Form.FitAxisOffset = FitAxisOffset;
        _Form.Forms = new Form[_ArchBlocks.Length];

        for (int i = 0; i < _Form.Length; i++)
        {
            _Form.Forms[i].DefaultRotationZ = _ArchBlocks[i].DefaultRotationZ;
            _Form.Forms[i].InitRatio = _ArchBlocks[i].InitRatio;
            _Form.Forms[i].RatioOffset = _ArchBlocks[i].RatioOffset;
        }
        UnityEditor.EditorUtility.SetDirty(_Form);
    }
#endif
}