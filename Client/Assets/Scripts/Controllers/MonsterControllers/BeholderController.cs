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
            if (SkillId == 33)
                StartPsychicsCoroutine(PlayLaserRotateBeam(SkillId));
            else if (SkillId == 34)
                StartPsychicsCoroutine(PlayRotateKnockBack(SkillId));
            else if (SkillId == 35)
                StartPsychicsCoroutine(PlayMissile(SkillId));


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
        else
        {
            base.UpdateAnimation();
        }
    }
    private IEnumerator PlayLaserRotateBeam(int skillId)
    {
        Managers.Data.SkillDict.TryGetValue(skillId, out SkillData skill);
        float duration = skill.duration;
        float elapsed = 0.0f;
        float rotationSpeed = 360.0f / duration; // duration 동안 360도 회전

        if (PosInfo.MoveDir == MoveDir.Up)
            Animator.Play("LASERPREP_BACK");
        else if (PosInfo.MoveDir == MoveDir.Down)
            Animator.Play("LASERPREP_FRONT");
        else if (PosInfo.MoveDir == MoveDir.Left)
        {
            Animator.Play("LASERPREP_SIDE");
            _sprite.flipX = true;
        }
        else if (PosInfo.MoveDir == MoveDir.Right) {
            Animator.Play("LASERPREP_SIDE");
            _sprite.flipX = false;
        }

        float prepAnimationLength = Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed; // SKILL_PREP 애니메이션의 실제 재생 시간 계산
        yield return new WaitForSeconds(prepAnimationLength);


        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float angle = elapsed * rotationSpeed;
            angle = angle % 360; // 0-360도 사이로 제한

            // 각도에 따라 애니메이션 선택 및 flip 설정
            if (angle >= 0 && angle < 45 || angle >= 315 && angle < 360)
            {
                Animator.Play("LASERLOOP_FRONT");
                _sprite.flipX = false;
            }
            else if (angle >= 45 && angle < 90)
            {
                Animator.Play("LASERLOOP_DIAGONALFRONT");
                _sprite.flipX = false;
            }
            else if (angle >= 90 && angle < 135)
            {
                Animator.Play("LASERLOOP_SIDE");
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

            yield return new WaitForSeconds(duration / 8);
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
    }

    private IEnumerator PlayRotateKnockBack(int skillId)
    {
        Managers.Data.SkillDict.TryGetValue(skillId, out SkillData skill);
        Animator.Play("SPINPREP");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);

        Animator.Play("SPINLOOP");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);

        Animator.Play("SPINEND");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);

        State = CreatureState.Idle;
    }

    private IEnumerator PlayMissile(int skillId)
    {
        Managers.Data.SkillDict.TryGetValue(skillId, out SkillData skill);
        Animator.Play("MISSILEPREP");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);

        Animator.Play("MISSILELOOP");
        yield return new WaitForSeconds(skill.duration*1000);

        Animator.Play("MISSILEEND");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / Animator.speed);

        State = CreatureState.Idle;
    }


    public override void UseSkill(S_Skill skill)
    {
        if (skill.Info.SkillId == 33)
        {
            SkillId = skill.Info.SkillId;
            State = CreatureState.Skill;
            SkillData skillData = null;
            Managers.Data.SkillDict.TryGetValue(skill.Info.SkillId, out skillData);
            StartPsychicsCoroutine(UseSkill33(skillData));
        }
        else
        {
            base.UseSkill(skill);
        }
    }

    private IEnumerator UseSkill33(SkillData skillData)
    {
        yield return new WaitForSeconds((float)(1.5 * 1000));
        GameObject skillObj = Managers.Resource.Instantiate($"{skillData.prefab}", transform);
        Animator animator = skillObj.GetComponent<Animator>();
        animator.Play("START");

        // 초기 회전 설정
        if (PosInfo.MoveDir == MoveDir.Up)
            skillObj.transform.rotation = Quaternion.Euler(0, 0, 90);
        else if (PosInfo.MoveDir == MoveDir.Down)
            skillObj.transform.rotation = Quaternion.Euler(0, 0, -90);
        else if (PosInfo.MoveDir == MoveDir.Left)
            skillObj.transform.rotation = Quaternion.Euler(0, 0, 180);
        else if (PosInfo.MoveDir == MoveDir.Right)
            skillObj.transform.rotation = Quaternion.Euler(0, 0, 0);

        float duration = skillData.duration;
        float elapsed = 0.0f;
        float rotationSpeed = 360.0f / duration; // duration 동안 360도 회전

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            skillObj.transform.Rotate(Vector3.forward, -rotationSpeed * Time.deltaTime); // 반시계 방향 회전
            yield return null;
        }

        animator.Play("END");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length / animator.speed);

        // 회전이 끝난 후 프리팹 제거
        Destroy(skillObj);

        // 회전이 끝난 후 IDLE 애니메이션 재생
        Animator.Play("IDLE");
    }
}