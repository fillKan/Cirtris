using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArchBlockShooter : Singleton<ArchBlockShooter>
{
    public const float PrepareNextBlockTime = 0.5f;
    public const float     AwakeRoutineTime = 0.4f;

    public event Action<ArchBlockPack> ShootingEvent;
    public event Action<ArchBlockPack> ShootEndEvent;

    public event Action<ArchBlock> MoveLowerEndEvent;

    [HideInInspector] public ArchBlockPack TargetPack;
    public ArchBlockPack NextArchPack { get; private set; }

    [SerializeField] private AnimationCurve _ArchFillingCurve;
    [SerializeField] private AnimationCurve _MoveLowerCurve;

    public float ShootSpeed;
    public bool ShootingStop;

    private Coroutine _ShootRoutine;

    private void Awake()
    {
        _ShootRoutine = new Coroutine(this);
        StartCoroutine(AwakeRoutine(AwakeRoutineTime));
    }
    private IEnumerator AwakeRoutine(float time)
    {
        for (float w = 0f; w < time; w += Time.deltaTime)
            yield return null;

        PrepareNextBlock();
    }
    public void ShootOrder()
    {
        if (TargetPack == null && NextArchPack != null)
        {
            TargetPack = NextArchPack;
            NextArchPack = null;

            Shoot();
        }
    }
    public void Awaken()
    {
        StartCoroutine(AwakeRoutine(AwakeRoutineTime));
    }
    private void Shoot()
    {
        ShootingStop = false;

        _ShootRoutine.Start(ShootRoutine());
        ShootingEvent?.Invoke(TargetPack);
    }
    public void PrepareNextBlock()
    {
        if (!InGameSceneSystem.Instance.IsGameOver)
        {
            var pack = ArchBlockPool.Instance.Get(out Color color);
            var time = PrepareNextBlockTime;

            for (int i = 0; i < pack.ArchBlocks.Length; i++)
                StartCoroutine(ApperNextBlockRoutine(pack[i], color, time));

            StartCoroutine(ApperNextPackEnd(pack, time));
        }
    }
    public void MoveLower(ArchBlock block, float lower, float time)
    {
        if (lower > block.Ratio)
        {
            StartCoroutine(MoveLowerRoutine(block, lower, time));
        }
    }
    private IEnumerator MoveLowerRoutine(ArchBlock block, float lower, float time)
    {
        float start = block.Ratio;

        for (float i = 0f; i < time; i += Time.deltaTime * ShootSpeed)
        {
            float ratio = Mathf.Clamp(i / time, 0f, 1f);
            block.Ratio = Mathf.Lerp(start, lower, _MoveLowerCurve.Evaluate(ratio));
            yield return null;
        }
        block.Ratio = lower;
        MoveLowerEndEvent?.Invoke(block);
    }
    private IEnumerator ShootRoutine()
    {
        var shootingBlock = TargetPack;

        float goalRatio = ArchBlock.BlockThicknessF;
        float crntRatio = TargetPack.Ratio;

        float time = 0.2f;

        while (TargetPack.Ratio < 1f && !ShootingStop)
        {
            for (float i = 0f; i < time && !ShootingStop; i += ShootSpeed * Time.deltaTime)
            {
                float ratio = Mathf.Min(1f, i / time);
                TargetPack.Ratio = Mathf.Lerp(crntRatio, goalRatio, ratio);

                yield return null;
            }
            if (ShootingStop) break;

            crntRatio = TargetPack.Ratio;
            goalRatio += ArchBlock.BlockThicknessF;
        }
        // ========== Ratio 보정 ========== //
        float approxRate  = 1f;
        float approxRatio = 1f;

        for (float ratio = 1f; ratio > 0f; ratio -= ArchBlock.BlockThicknessF)
        {
            float rate = Mathf.Abs(TargetPack.Ratio - ratio);
            if (rate < approxRate)
            {
                approxRate  =  rate;
                approxRatio = ratio;
            }
        }
        TargetPack.Ratio = approxRatio;
        // ========== Ratio 보정 ========== //

        ShootEndEvent?.Invoke(shootingBlock);
        _ShootRoutine.Finish();
    }
    private IEnumerator ApperNextBlockRoutine(ArchBlock archBlock, Color color, float time)
    {
        var ratio = 0f;
        var controller = archBlock.ArchController;

        float   endArchDeg = controller.ArcDegree;
        float   endArchMax = controller.MaxRadius;
        float beginArchMax = controller.MaxRadius + 2.0f;
        float   endArchBet = controller.BetweenMaskRadius;

        for (float i = 0f; ratio < 1f; i+=Time.deltaTime)
        {
            ratio = Mathf.Min(1f, i / time);

            float value = _ArchFillingCurve.Evaluate(ratio);

            controller.ArcDegree         = Mathf.Lerp(0, endArchDeg, value);
            controller.MaxRadius         = Mathf.Lerp(beginArchMax, endArchMax, value);
            controller.BetweenMaskRadius = Mathf.Lerp(0, endArchBet, value);

            yield return null;
            archBlock.ArchBuilder.color = color;
        }
    }
    private IEnumerator ApperNextPackEnd(ArchBlockPack pack,float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
            yield return null;

        NextArchPack = pack;
    }
}
