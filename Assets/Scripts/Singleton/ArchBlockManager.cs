using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchBlockManager : Singleton<ArchBlockManager>
{
    public AnimationCurve ArchBlockReleaseCurve;
    public bool AllBlockDestroy;
    public bool AllBlockRelease;

    [SerializeField]
    private TwinkleCircleEffect _TwinkleCircles;

    public List<ArchBlock> _StackingBlocks
    {
        get;
        private set;
    }
    public ArchBlock ContactBlock
    {
        get;
        private set;
    }

    private void Awake()
    {
        _StackingBlocks = new List<ArchBlock>();
    }
    private void LateUpdate()
    {
        var shootingPack = ArchBlockShooter.Instance.TargetPack;
        if (ContactBlock == null && _StackingBlocks.Count > 0)
        {
            ContactBlock = _StackingBlocks[_StackingBlocks.Count - 1];
        }
        for (int i = 0; i < _StackingBlocks.Count; i++)
        {
            var block = _StackingBlocks[i];

            if (AllBlockDestroy)
            {
                block.Destroy();
                _StackingBlocks.RemoveAt(i--);

                continue;
            }
            if (AllBlockRelease)
            {
                block.Release();
                continue;
            }
            block.ArchUpdate();

            if (shootingPack)
            {
                for (int j = 0; j < shootingPack.Length; j++)
                {
                    if (Mathf.Abs(shootingPack[j].Ratio - block.Ratio) > ArchBlock.BlockThicknessF) 
                        continue;
                    
                    var shootingBlock = shootingPack[j];

                    float         blockAngle =         block.RotationZ > 180f ?         block.RotationZ - 360f :         block.RotationZ;
                    float shootingBlockAngle = shootingBlock.RotationZ > 180f ? shootingBlock.RotationZ - 360f : shootingBlock.RotationZ;

                    if (Mathf.Abs(shootingBlockAngle - blockAngle) <= ArchBlock.DefaultBlockDegree)
                    {
                        ContactBlock = block;

                        ArchBlockShooter.Instance.ShootingStop = true;
                        break;
                    }
                }
            }
        }
        if (AllBlockDestroy)
        {
            AllBlockDestroy = false;
            InGameSceneSystem.Instance.GameOver();

            _TwinkleCircles.gameObject.SetActive(true);
        }
        else if (AllBlockRelease)
        {
            AllBlockRelease = false;
            if (shootingPack != null)
            {
                for (int i = 0; i < shootingPack.Length; i++)
                {
                    shootingPack[i].Release();
                }
            }
            var nextPack = ArchBlockShooter.Instance.NextArchPack;
            if (nextPack != null)
            {
                for (int i = 0; i < nextPack.Length; i++)
                {
                    nextPack[i].Release();
                }
            }
        }
    }
}
