using System;
using UnityEngine;

public class AvatarModel : MonoBehaviour
{
    private Animator _animator;

    public Animator Animator => _animator;


    public void OnEnable()
    {
        /*if (AvatarVisualManager.Instance.ActiveSkin is { Count: > 1 })
        {
            SetSkin(AvatarVisualManager.Instance.ActiveSkin[0].name);
            SetSkin(AvatarVisualManager.Instance.ActiveSkin[1].name);
        }*/
    }

    public void SetSkin(string name)
    {/*
        var skin = _skinHolder.GetSkinToName(name);
        skin?.SetActive(true);*/
    }
    
    public void PlayAnimation(AnimType animType)
    {
        switch (animType)
        {
            case AnimType.Idle:
                _animator.SetTrigger("Idle");
                break;
            case AnimType.Walk:
                _animator.SetTrigger("Walk");
                break;
            case AnimType.Yes:
                _animator.SetTrigger("Yes");
                break;
            case AnimType.No:
                _animator.SetTrigger("No");
                break;
            case AnimType.Surprise:
                _animator.SetTrigger("Surprise");
                break;
            case AnimType.TurnSpread:
                _animator.SetTrigger("TurnSpread");
                break;
            case AnimType.Spin:
                _animator.SetTrigger("Spin");
                break;
            case AnimType.Dance:
                _animator.SetTrigger("Dance");
                break;
            case AnimType.MoonWalk:
                _animator.SetTrigger("MoonWalk");
                break;
            case AnimType.Jump:
                _animator.SetTrigger("Jump");
                break;
            case AnimType.Sitting:
                _animator.SetTrigger("Sitting");
                break;
            case AnimType.Tablet:
                _animator.SetTrigger("Tablet");
                break;
            case AnimType.RideBicycle:
                _animator.SetTrigger("RideBicycle");
                break;
        }
    }
    
    public enum AnimType : byte
    {
        RideBicycle,
        Dance,
        Idle,
        Jump,
        MoonWalk,
        Run,
        Sitting,
        Spin,
        Surprise,
        TurnSpread,
        Walk,
        Yes,
        No,
        Tablet
    }
}