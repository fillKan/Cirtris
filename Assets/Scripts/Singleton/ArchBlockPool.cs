using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockLevel
{
    L1, L2, L3, L4
}
[System.Serializable]
public struct ArchPack
{
    public BlockLevel Level;
    public ArchBlockPack ArchBlockPack;
}
public class ArchBlockPool : Singleton<ArchBlockPool>
{
    private const float Probablity_L1 = 0.25f;
    private const float Probablity_L2 = 0.45f;
    private const float Probablity_L3 = 0.30f;
    
    private readonly float[] _Probablities = new float[3]
    {
        Probablity_L1, Probablity_L2, Probablity_L3
    };

    [SerializeField] private Transform _CircleAxis;

    [SerializeField] private ArchBlock _ArchBlock;
    private Queue<ArchBlock> _ArchBlockPool;

    [Header("BlockColor Property")]
    [SerializeField] private Color[] _BlockColorArray;

    [Header("ArchBlock Property")]
    [SerializeField] private ArchPackFormList[] _ArchPackFormList;
    private Dictionary<BlockLevel, ArchPackFormList> _ArchPackFormDic;

    private ArchBlockPack _DummyArchPack;

    private void Awake()
    {
        _ArchBlockPool = new Queue<ArchBlock>();

        _ArchPackFormDic = new Dictionary<BlockLevel, ArchPackFormList>();
        for(int i = 0; i < _ArchPackFormList.Length; i++)
        {
            var list = _ArchPackFormList[i];
            _ArchPackFormDic.Add(list.ListLevel, list);
        }
        new GameObject("ArchBlockPack", typeof(ArchBlockPack)).TryGetComponent(out _DummyArchPack);
    }
    public Color GetRandomColor()
    {
        return _BlockColorArray[Random.Range(0, _BlockColorArray.Length)];
    }
    public void Add(ArchBlock block)
    {
        _ArchBlockPool.Enqueue(block);
        block.gameObject.SetActive(false);
    }
    public ArchBlockPack Get(out Color randomColor)
    {
        var form  = GetArchPackForm();
        var forms = form.Forms;

        _DummyArchPack.FitAxisOffset = form.FitAxisOffset;
        _DummyArchPack.ArchBlocks = new ArchBlock[forms.Length];

        // add pool object
        if (_ArchBlockPool.Count < forms.Length)
        {
            int count = forms.Length - _ArchBlockPool.Count;
            for (int i = 0; i < count; i++)
            {
                _ArchBlockPool.Enqueue(Instantiate(_ArchBlock));
            }
        }
        for (int i = 0; i < forms.Length; i++)
        {
            _DummyArchPack.ArchBlocks[i] = _ArchBlockPool.Dequeue();
            _DummyArchPack.ArchBlocks[i].gameObject.SetActive(true);

            _DummyArchPack[i].Ratio =
                _DummyArchPack[i].InitRatio = forms[i].InitRatio;

            _DummyArchPack[i].RatioOffset = forms[i].RatioOffset;

            _DummyArchPack[i].RotationZ =
                _DummyArchPack[i].DefaultRotationZ = forms[i].DefaultRotationZ;
        }
        randomColor = GetRandomColor();
        _DummyArchPack.Ratio = 0f;
        
        return _DummyArchPack;
    }
    private ArchPackForm GetArchPackForm()
    {
        float pro  = 0f;
        float rand = Random.value;

        ArchPackForm form = _ArchPackFormList[0].Forms[0];

        for (int i = 0; i < _Probablities.Length; i++)
        {
            if (rand <= (pro += _Probablities[i]))
            {
                form = _ArchPackFormDic[(BlockLevel)i].Random();
                break;
            }
        }
        return form;
    }
}
