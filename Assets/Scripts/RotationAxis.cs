using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RotDirection
{
    ClockWise, CoClockWise
}
public class RotationAxis : MonoBehaviour
{
    public static readonly Vector2 WorldPosition = new Vector2(0, 0.3f);

    public const float BlockClearTime        = 1.0f;
    public const float BlockLowerTime        = 1.2f;
    public const float BlockReleaseTime      = 1.0f;
    public const float Clear_BlockLowerTime  = 0.25f;
    public const float Clear_OtherLowerTime  = 0.35f;
    public const float RotDirChangePercent   = 0.5f;
    public const float InsideCircleRatio     = 0.6f;
    public const float BlockFittingTimeScale = 0.035f;
    public const float SpeedRewindTime       = 1.2f;
    public const float Lower_SpeedRewindTime = 0.9f;

    public const float TimeToThinkPrepareNextBlock = 0.4f;

    public float[] AxisAngle;

    [SerializeField] private RotDirection _RotDirection;
    [SerializeField] private float _RotSpeed;
    private float _OriginalRotSpeed;

    [SerializeField] private TwinkleCircleEffect _TwinkleCircle;
    [SerializeField] private AnimationCurve _FittingCurve;
    [SerializeField] private AnimationCurve _RewindCurve;
    [SerializeField] private float[] _RatioLevels;

    private List<ArchBlock> _StackingArchBlocks;

    private ArchBlockPack _LastArchPack;

    private Coroutine _UpdateRoutine;
    private Coroutine _SpeedRewindRoutine;
    public bool CanBlockCler(float ratioLevel, out List<ArchBlock> clearBlocklist)
    {
        clearBlocklist = new List<ArchBlock>();
        float blockThickness = ArchBlock.BlockThicknessF / 2;
        float DegreeSum = 0f;

        foreach (var archBlock in _StackingArchBlocks) {
            if (Mathf.Abs(archBlock.Ratio - ratioLevel) < blockThickness)
            {
                DegreeSum += archBlock.ArchController.ArcDegree;
                clearBlocklist.Add(archBlock);
            }
        }
        return DegreeSum >= 360.0f;
    }
    private IEnumerator CheckBlockClear()
    {
        var blockThickness = ArchBlock.BlockThicknessF * 1.5f;
        var blockCleared   = false;

        for (float i = 1f; i > 0f; i -= ArchBlock.BlockThicknessF)
        {
            if (CanBlockCler(i, out var list))
            {
                _TwinkleCircle.gameObject.SetActive(true);

                ScoreSystem.Instance.ScoreUp(1);
                foreach (var clearBlock in list)
                {
                    clearBlock.Release(BlockReleaseTime);
                    _StackingArchBlocks.Remove(clearBlock);

                    blockCleared = true;
                }
                if (blockCleared)
                {
                    var movedBlocks = _StackingArchBlocks.ToList();

                    for (float w = 0f; w < BlockClearTime; w += Time.deltaTime * Time.timeScale) 
                        yield return null;

                    for (float ratio = i; ratio > 0f; ratio -= ArchBlock.BlockThicknessF)
                    {
                        for (int index = 0; index < movedBlocks.Count; index++)
                        {
                            ArchBlock block = movedBlocks[index];
                            if (block.Ratio < ratio)
                            {
                                if (Mathf.Abs(block.Ratio - ratio) < blockThickness)
                                {
                                    ArchBlockShooter.Instance.MoveLower(block, Mathf.Floor(ratio * 10) * 0.1f, Clear_BlockLowerTime);
                                    movedBlocks.RemoveAt(index--);
                                }
                            }
                        }
                        if (movedBlocks.Any(o => o.Ratio < ratio))
                        {
                            for (float w = 0f; w < Clear_OtherLowerTime; w += Time.deltaTime * Time.timeScale)
                                yield return null;
                        }
                    }
                }
                break;
            }
        }
        for (float w = 0f; w < TimeToThinkPrepareNextBlock; w += Time.deltaTime * Time.timeScale)
            yield return null;
        ArchBlockShooter.Instance.TargetPack = null;

        ArchBlockShooter.Instance.PrepareNextBlock();
    }
    public float FindCloestAxisAngle(float angle)
    {
        var cloestAngle = float.MaxValue;
        var cloestDistance = float.MaxValue;

        for (int i = 0; i < AxisAngle.Length; i++)
        {
            var distance = Mathf.Abs(AxisAngle[i] - angle);

            if (distance < cloestDistance)
            {
                cloestAngle = AxisAngle[i];
                cloestDistance = distance;
            }
        }
        return cloestAngle;
    }
    private IEnumerator FittingRoutine(ArchBlock archBlock, float axisAngle, float time)
    {
        float start = archBlock.RotationZ;

        float ratio;
        for (float i = 0f; i < time; i += Time.deltaTime * Time.timeScale)
        {
            ratio = Mathf.Clamp(i / time, 0f, 1f);
            archBlock.RotationZ = Mathf.Lerp(start, axisAngle, _FittingCurve.Evaluate(ratio));
            yield return null;
        }
        archBlock.RotationZ = axisAngle;
    }
    private void LowerThePack(ArchBlockPack pack, out float waitTime)
    {
        waitTime = 0.0f;
        double betweenRatio = 1.0d;

        for (int i = 0; i < pack.Length; i++)
        {
            var block = pack[i];

            foreach (var other in _StackingArchBlocks)
            {
                if (Mathf.Abs(other.RotationZ - block.RotationZ) < 1.0f)
                {
                    betweenRatio = Math.Min(other.Ratio - block.Ratio, betweenRatio);
                }
            }
        }
        betweenRatio = Math.Ceiling(betweenRatio * 10) * 0.1d;
        for (int i = 0; i < pack.Length; i++)
        {
            var block = pack[i];
            var ratio = Mathf.Min((float)(block.Ratio + betweenRatio - ArchBlock.BlockThicknessD), 1.0f);

            float time = (ratio - block.Ratio + block.RatioOffset + ArchBlock.BlockThicknessF) * BlockLowerTime;
            ArchBlockShooter.Instance.MoveLower(block, (ratio + block.RatioOffset <= block.Ratio ? ratio : ratio + block.RatioOffset), time);

            waitTime = Mathf.Max(waitTime, time);
        }
    }
    private bool CheckLowerTheBlock(ArchBlock block)
    {
        if (block.Ratio < 1f)
        {
            double blockRatio = Math.Truncate(block.Ratio * 10) * 0.1d;
            double   maxRatio = blockRatio;

            bool hasRotationEqualBlock = false;

            foreach (ArchBlock other in _StackingArchBlocks)
            {
                if (Mathf.Abs(other.RotationZ - block.RotationZ) < 1f)
                {
                    hasRotationEqualBlock = true;
                    double ratio = Math.Truncate(other.Ratio * 10) * 0.1d;

                    if (blockRatio < maxRatio)
                        maxRatio = Math.Min(ratio - ArchBlock.BlockThicknessD, maxRatio);
                    else
                        maxRatio = Math.Max(ratio - ArchBlock.BlockThicknessD, maxRatio);
                }
            }
            if (hasRotationEqualBlock)
            {
                maxRatio = Math.Truncate(maxRatio * 10) * 0.1d;
                return maxRatio > blockRatio;
            }
            else
                return true;
        }
        return false;
    }
    private IEnumerator SpeedRewindRoutine(float rewindTime)
    {
        _RotSpeed = 0f;
        _UpdateRoutine.Start(UpdateRoutine());

        if (RotDirChangePercent >= UnityEngine.Random.value)
        {
            _RotDirection = _RotDirection == RotDirection.CoClockWise ? RotDirection.ClockWise : RotDirection.CoClockWise;
        }
        for (float i = 0f; i < rewindTime; i += Time.deltaTime * Time.timeScale)
        {
            float ratio = Mathf.Clamp(i / rewindTime, 0f, 1f);
            _RotSpeed = Mathf.Lerp(0f, _OriginalRotSpeed, _RewindCurve.Evaluate(ratio));

            yield return null;
        }
        _RotSpeed = _OriginalRotSpeed;
        _SpeedRewindRoutine.Finish();
    }
    private IEnumerator PostProcessing(float waitTime, Action processingOverAction = null)
    {
        for (float i = 0f; i < waitTime; i += Time.deltaTime) 
            yield return null;

        bool canLower = true;
        for (int i = 0; i < _LastArchPack.Length; i++)
        {
            if (!(canLower = CheckLowerTheBlock(_LastArchPack[i])))
                break;
        }
        if (canLower)
        {
            LowerThePack(_LastArchPack, out float wait);

            for (float w = 0f; w < wait; w += Time.deltaTime)
                yield return null;
        }
        processingOverAction?.Invoke();
    }
    private IEnumerator UpdateRoutine()
    {
        while (gameObject.activeSelf)
        {
            float rotation = _RotSpeed * Time.deltaTime;

            if (_RotDirection == RotDirection.ClockWise)
            {
                rotation *= -1f;
            }
            foreach (var block in _StackingArchBlocks)
            {
                if (block.Ratio < InsideCircleRatio)
                {
                    ArchBlockManager.Instance.AllBlockDestroy = true;
                    break;
                }
                block.DeltaRotationZ(rotation);
            }
            yield return null;
        }
        _UpdateRoutine.Finish();
    }
    private void Start()
    {
        _StackingArchBlocks = ArchBlockManager.Instance._StackingBlocks;

        _UpdateRoutine = new Coroutine(this);
        _UpdateRoutine.Start(UpdateRoutine());

        _OriginalRotSpeed = _RotSpeed;
        _SpeedRewindRoutine = new Coroutine(this);

        ArchBlockShooter.Instance.ShootEndEvent += pack =>
        {
            _LastArchPack = pack;

            if (_StackingArchBlocks.Count > 0)
            {
                _UpdateRoutine.Stop();

                float rotZ = ArchBlockManager.Instance.ContactBlock.RotationZ - 90f;
                rotZ = rotZ < 0f ? 360f + rotZ : rotZ;

                float axisAngle = FindCloestAxisAngle(rotZ);
                float closet = rotZ - axisAngle + pack.FitAxisOffset;

                float time = Mathf.Abs(closet * BlockFittingTimeScale);
                
                foreach (var archBlock in _StackingArchBlocks)
                    StartCoroutine(FittingRoutine(archBlock, archBlock.RotationZ - closet, time));

                StartCoroutine(PostProcessing(time, () =>
                {
                    for (int i = 0; i < _LastArchPack.Length; i++)
                    {
                        var block = _LastArchPack[i];

                        if(!_StackingArchBlocks.Contains(block))
                            _StackingArchBlocks.Add(block);
                    }
                    if (_SpeedRewindRoutine.IsOver)
                        _SpeedRewindRoutine.Start(SpeedRewindRoutine(SpeedRewindTime));

                    StartCoroutine(CheckBlockClear());
                }));
            }
            else
            {
                StartCoroutine(PostProcessing(BlockFittingTimeScale * 1.5f, () =>
                {
                    for (int i = 0; i < _LastArchPack.Length; i++)
                        _StackingArchBlocks.Add(_LastArchPack[i]);

                    if (_SpeedRewindRoutine.IsOver)
                        _SpeedRewindRoutine.Start(SpeedRewindRoutine(SpeedRewindTime));

                    ArchBlockShooter.Instance.TargetPack = null;

                    ArchBlockShooter.Instance.PrepareNextBlock();
                }));
            }
        };
    }
    private void OnDrawGizmos()
    {
        for (int i = 0; i < AxisAngle.Length; i++)
        {
            float angle = AxisAngle[i] * Mathf.Deg2Rad;
            Vector3 axis = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + axis * 3.57f);
        }
    }
}
