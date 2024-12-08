using Google.Protobuf.Protocol;
public static class Content
{
    public static string ConvertSpecialOptions(string option)
    {
        if (System.Enum.TryParse(option, out ItemOptionType optionType))
        {
            switch (optionType)
            {
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
                case ItemOptionType.Avoid:
                    return "회피율";
                case ItemOptionType.Accuracy:
                    return "명중률";
                case ItemOptionType.HpRegen:
                    return "체력 회복";
                case ItemOptionType.Heal:
                    return "회복량";
                case ItemOptionType.Up:
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
}