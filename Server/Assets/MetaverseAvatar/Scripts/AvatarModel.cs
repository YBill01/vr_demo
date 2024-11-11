using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarModel : MonoBehaviour
{

    public Animator animator;

    private AvatarInfo _info;



	public void SetSkin(string[] skinNames)
	{








	}

	public void RandomizeAvatar()
	{




	}


    public void PlayAnimation(AnimType animType)
    {
        switch (animType)
        {
            case AnimType.Idle:
                animator.SetTrigger("Idle");
                break;
            case AnimType.Walk:
                animator.SetTrigger("Walk");
                break;
            case AnimType.Yes:
                animator.SetTrigger("Yes");
                break;
            case AnimType.No:
                animator.SetTrigger("No");
                break;
            case AnimType.Surprise:
                animator.SetTrigger("Surprise");
                break;
            case AnimType.TurnSpread:
                animator.SetTrigger("TurnSpread");
                break;
            case AnimType.Spin:
                animator.SetTrigger("Spin");
                break;
            case AnimType.Dance:
                animator.SetTrigger("Dance");
                break;
            case AnimType.MoonWalk:
                animator.SetTrigger("MoonWalk");
                break;
            case AnimType.Jump:
                animator.SetTrigger("Jump");
                break;
            case AnimType.Sitting:
                animator.SetTrigger("Sitting");
                break;
            case AnimType.Tablet:
                animator.SetTrigger("Tablet");
                break;
            case AnimType.RideBicycle:
                animator.SetTrigger("RideBicycle");
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