using System.Collections;
using Architect.Util;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class WalkTarget : MonoBehaviour
{
    public void DoWalk()
    {
        StartCoroutine(Walk());
    }

    public IEnumerator Walk()
    {
        var hero = HeroController.instance;
        var body = hero.GetComponent<Rigidbody2D>();
        var heroTrans = hero.transform;

        hero.RelinquishControl();
        hero.StopAnimationControl();
        hero.GetComponent<tk2dSpriteAnimator>().Play("Walk");

        EditorManager.LostControlToCustomObject = true;

        if (heroTrans.position.x > transform.position.x)
        {
            hero.FaceLeft();
            while (heroTrans.position.x > transform.position.x)
            {
                if (!EditorManager.LostControlToCustomObject) yield break;
                body.velocity = new Vector2(-6, body.velocity.y);
                yield return null;
            }
        }
        else
        {
            hero.FaceRight();
            while (heroTrans.position.x < transform.position.x)
            {
                if (!EditorManager.LostControlToCustomObject) yield break;
                body.velocity = new Vector2(6, body.velocity.y);
                yield return null;
            }
        }

        EditorManager.LostControlToCustomObject = false;
        hero.RegainControl();
        hero.StartAnimationControl();
    }
}