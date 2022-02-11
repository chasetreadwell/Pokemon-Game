using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float hpNormalized) {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    public IEnumerator SetHPSmooth(float newHp, bool isDamage) {
        float curHP = health.transform.localScale.x;
        float changeAmt = curHP - newHp;
        if(changeAmt != 0) {
            if(isDamage) {
                while (curHP - newHp > Mathf.Epsilon) {
                    curHP -= changeAmt * Time.deltaTime;
                    SetHP(curHP);
                    yield return null;
                }
            } else {
                while (curHP - newHp < Mathf.Epsilon) {
                    curHP -= changeAmt * Time.deltaTime;
                    SetHP(curHP);
                    yield return null;
                }
            }
            SetHP(newHp);
        }
    }
}
