using Google.Protobuf.Protocol;
using UnityEngine;
public static class Content
{
    public static string ConvertSpecialOptions(string option)
    {
        string key = option.Split('_')[0];
        if (System.Enum.TryParse(key, out ItemOptionType optionType))
        {
            switch (optionType)
            {
                case ItemOptionType.Damage:
                    return "공격력";
                case ItemOptionType.Attack:
                    return "공격력";
                case ItemOptionType.Defense:
                    return "방어력";
                case ItemOptionType.DefenseMulti:
                    return "방어력";
                case ItemOptionType.CriticalChance:
                    return "치명타 확률";
                case ItemOptionType.CriticalDamage:
                    return "치명타 피해";
                case ItemOptionType.AttackSpeed:
                    return "공격 속도";
                case ItemOptionType.Speed:
                    return "이동 속도";
                case ItemOptionType.Hp:
                    return "체력";
                case ItemOptionType.HpMulti:
                    return "체력";
                case ItemOptionType.Avoid:
                    return "회피";
                case ItemOptionType.Accuracy:
                    return "적중";
                case ItemOptionType.HpRegen:
                    return "체력 회복";                
                case ItemOptionType.Heal:
                    return "회복량";
                case ItemOptionType.Up:
                    return "미지력";
                case ItemOptionType.UpMulti:
                    return "미지력";
                case ItemOptionType.UpRegen:
                    return "미지력 회복";
                case ItemOptionType.Skill:
                    return "특수기술";
                default:
                    return option;
            }
        }
        return option;
    }
    public static string ConvertSpecialOptionsValue(string option, string value)
    {
        string key = option.Split('_')[0];
        if (System.Enum.TryParse(key, out ItemOptionType optionType))
        {
            switch (optionType)
            {
                case ItemOptionType.Attack:
                    return $"{value}%";
                case ItemOptionType.DefenseMulti:
                    return $"{value}%";
                case ItemOptionType.HpMulti:
                    return $"{value}%";
                case ItemOptionType.UpMulti:
                    return $"{value}%";
                case ItemOptionType.CriticalChance:
                    return $"{value}%";
                case ItemOptionType.CriticalDamage:
                    return $"{value}%";
                default:
                    return value;
            }
        }
        return value;
    }
    public static string ConvertGrade(Grade grade)
    {
        switch (grade)
        {
            case Grade.Normal:
                return "노말";
            case Grade.Rare:
                return "레어";
            case Grade.Elite:
                return "엘리트";
            case Grade.Epic:
                return "에픽";
            case Grade.Uncharted:
                return "미지";
            default:
                return "노말";
        }
    }
    public static Color GetGradeColor(Grade grade)
    {
        switch (grade)
        {
            case Grade.Normal:
                return Color.white;
            case Grade.Rare:
                return new Color(90 / 255f, 107 / 255f, 255 / 255f);
            case Grade.Elite:
                return new Color(200 / 255f, 255 / 255f, 45 / 255f);
            case Grade.Epic:
                return new Color(0 / 255f, 255 / 255f, 200 / 255f);
            case Grade.Uncharted:
                return new Color(255 / 255f, 0 / 255f, 173 / 255f);
            default:
                return Color.white;
        }
    }

    public static Color GetColorByIndex(int index)
    {
        switch (index % 4)
        {
            case 0:
                return Content.GetGradeColor(Grade.Normal);
            case 1:
                return Content.GetGradeColor(Grade.Rare);
            case 2:
                return Content.GetGradeColor(Grade.Elite);
            case 3:
                return Content.GetGradeColor(Grade.Epic);
            default:
                return Content.GetGradeColor(Grade.Normal);
        }
    }

    public static Color GetEnhanceColor(int rank)
    {
        if (rank == 0)
            return Color.white;

        Color startColor;
        Color endColor;
        float t;

        if (rank >= 1 && rank <= 5)
        {
            startColor = Color.white;
            endColor = GetGradeColor(Grade.Rare);
            t = (rank - 1) / 4f;
        }
        else if (rank >= 6 && rank <= 10)
        {
            startColor = GetGradeColor(Grade.Rare);
            endColor = GetGradeColor(Grade.Elite);
            t = (rank - 6) / 4f;
        }
        else if (rank >= 11 && rank <= 15)
        {
            startColor = GetGradeColor(Grade.Elite);
            endColor = GetGradeColor(Grade.Epic);
            t = (rank - 11) / 4f;
        }
        else // rank >= 16 && rank <= 20
        {
            startColor = GetGradeColor(Grade.Epic);
            endColor = GetGradeColor(Grade.Uncharted);
            t = (rank - 16) / 4f;
        }

        return Color.Lerp(startColor, endColor, t);
    }
}