using Data;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

public class BeholderController : MonsterController
{
    protected override void Init()
    {
        base.Init();
    }
    protected override void UpdateAnimation()
    {
        if (Animator == null || _sprite == null)
        {
            return;
        }
        if (State == CreatureState.Skill)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _sprite.flipX = false;
                    break;
            }

            if (SkillId == 34)
                StartPsychicsCoroutine(PlayRotateKnockBack(SkillId));
            else if (SkillId == 35)
                StartPsychicsCoroutine(PlayMissile(SkillId));
            SkillId = 0;
        }
        else if (State == CreatureState.Idle)
        {
            Animator.Play("IDLE");
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Moving)
        {
            switch (LookDir)
            {
                case LookDir.LookLeft:
                    _sprite.flipX = true;
                    break;
                case LookDir.LookRight:
                    _sprite.flipX = false;
                    break;
            }

            switch(PosInfo.MoveDir)
            {
                case MoveDir.Up:
                    Animator.Play("WALK_BACK");
                    break;
                case MoveDir.Right:
                    Animator.Play("WALK");
                    break;
                case MoveDir.Down:
                    Animator.Play("WALK_FRONT");
                    break;
                case MoveDir.Left:
                    Animator.Play("WALK");
                    break;
            }
        }
        else
        {
            base.UpdateAnimation();
        }
    }
    private IEnumerator PlayLaserRotateBeam(int skillId)
    {
        Managers.Data.SkillDict.TryGetValue(skillId, out SkillData skill);
        if (skill == null)
        {
            Debug.Log("Skill Data is null");
            yield break;
        }

        float duration = skill.duration;
        float elapsed = 0.0f;
        float angle = 0.0f;
        float angleIncrement = 360.0f / (duration);

        // 초기 회전 설정
        if (PosInfo.MoveDir == MoveDir.Up)
            angle = 270;
        else if (PosInfo.MoveDir == MoveDir.Right)
            angle = 0;
        else if (PosInfo.MoveDir == MoveDir.Down)
            angle = 90;
        else if (PosInfo.MoveDir == MoveDir.Left)
            angle = 180;

        if (PosInfo.MoveDir == MoveDir.Up)
            Animator.Play("LASERPREP_BACK");
        else if (PosInfo.MoveDir == MoveDir.Down)
            Animator.Play("LASERPREP_FRONT");
        else if (PosInfo.MoveDir == MoveDir.Left)
        {
            Animator.Play("LASERPREP_SIDE");
            _sprite.flipX = true;
        }
        else if (PosInfo.MoveDir == MoveDir.Right)
        {
            Animator.Play("LASERPREP_SIDE");
            _sprite.flipX = false;
        }

        yield return new WaitForSeconds(skill.terms[0]);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            angle += angleIncrement * Time.deltaTime;
            angle = angle % 360; // 0-360도 사이로 제한
            // 각도에 따라 애니메이션 선택 및 flip 설정
            if (angle >= 0 && angle < 45 || angle >= 315 && angle < 360)
            {
                Animator.Play("LASERLOOP_SIDE");
                _sprite.flipX = false;
            }
            else if (angle >= 45 && angle < 90)
            {
                Animator.Play("LASERLOOP_DIAGONALFRONT");
                _sprite.flipX = false;
            }
            else if (angle >= 90 && angle < 135)
            {
                Animator.Play("LASERLOOP_FRONT");
                _sprite.flipX = false;
            }
            else if (angle >= 135 && angle < 180)
            {
                Animator.Play("LASERLOOP_DIAGONALBACK");
                _sprite.flipX = false;
            }
            else if (angle >= 180 && angle < 225)
            {
                Animator.Play("LASERLOOP_BACK");
                _sprite.flipX = false;
            }
            else if (angle >= 225 && angle < 270)
            {
                Animator.Play("LASERLOOP_DIAGONALBACK");
                _sprite.flipX = true;
            }
            else if (angle >= 270 && angle < 315)
            {
                Animator.Play("LASERLOOP_SIDE");
                _sprite.flipX = true;
            }
            else if (angle >= 315 && angle < 360)
            {
                Animator.Play("LASERLOOP_DIAGONALFRONT");
                _sprite.flipX = true;
            }

            yield return null;
        }
        if (PosInfo.MoveDir == MoveDir.Up)
            Animator.Play("LASEREND_BACK");
        else if (PosInfo.MoveDir == MoveDir.Down)
            Animator.Play("LASEREND_FRONT");
        else if (PosInfo.MoveDir == MoveDir.Left)
        {
            Animator.Play("LASEREND_SIDE");
            _sprite.flipX = true;
        }
        else if (PosInfo.MoveDir == MoveDir.Right)
        {
            Animator.Play("LASEREND_SIDE");
            _sprite.flipX = false;
        }
        yield return new WaitForSeconds(skill.terms[2]);

        State = CreatureState.Idle;            
    }

    private IEnumerator PlayRotateKnockBack(int skillId)
    {
        Managers.Data.SkillDict.TryGetValue(skillId, out SkillData skill);
        if(skill == null)
        {
            Debug.Log("Skill Data is null");
            yield break;
        }
        Animator.Play("SPINPREP");
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed - 0.05f);

        Animator.Play("SPINLOOP");
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed - 0.05f);

        Animator.Play("SPINEND");
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed - 0.05f);

        State = CreatureState.Idle;
    }

    private IEnumerator PlayMissile(int skillId)
    {
        Managers.Data.SkillDict.TryGetValue(skillId, out SkillData skill);
        if (skill == null)
        {
            Debug.Log("Skill Data is null");
            yield break;
        }
        Animator.Play("MISSILEPREP");
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed - 0.05f);

        Animator.Play("MISSILELOOP");
        yield return new WaitForSeconds(skill.duration - 0.05f);

        Animator.Play("MISSILEEND");
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed - 0.05f);

        State = CreatureState.Idle;
    }


    public override void UseSkill(S_Skill skill)
    {
        if (skill.Info.SkillId == 33)
        {
            SkillId = skill.Info.SkillId;
            SkillData skillData = null;
            Managers.Data.SkillDict.TryGetValue(skill.Info.SkillId, out skillData);
            StartCoroutine(UseSkill33(skillData));
            StartCoroutine(PlayLaserRotateBeam(skill.Info.SkillId));

            State = CreatureState.Skill;
        }
        else
        {
            base.UseSkill(skill);
        }
    }

    private IEnumerator UseSkill33(SkillData skillData)
    {
        yield return new WaitForSeconds(skillData.terms[0]);
        GameObject skillObj = Managers.Resource.Instantiate(skillData.prefab, transform);
        Animator animator = skillObj.GetComponent<Animator>();
        animator.Play("START");

        // 초기 회전 설정
        float angle = 0;
        if (PosInfo.MoveDir == MoveDir.Up)
            angle = 270;
        else if (PosInfo.MoveDir == MoveDir.Right)
            angle = 0;
        else if (PosInfo.MoveDir == MoveDir.Down)
            angle = 90;
        else if (PosInfo.MoveDir == MoveDir.Left)
            angle = 180;

        skillObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        float duration = skillData.duration;
        float elapsed = 0.0f;
        float angleIncrement = 360.0f / duration; // 서버 로직과 동일하게 수정

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            angle += angleIncrement * Time.deltaTime;
            angle = angle % 360; // 0-360도 사이로 제한
            skillObj.transform.rotation = Quaternion.Euler(0, 0, angle); // 각도에 따라 회전 설정
            yield return null;
        }

        animator.Play("END");
        yield return new WaitForSeconds(skillData.terms[2]);

        // 회전이 끝난 후 프리팹 제거
        Destroy(skillObj);

        // 회전이 끝난 후 IDLE 애니메이션 재생
        Animator.Play("IDLE");
    }
}