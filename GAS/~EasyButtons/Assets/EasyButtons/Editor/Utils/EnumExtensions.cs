namespace EasyButtons.Editor.Utils
{
    using System;

    public static class EnumExtensions
    {
        public static bool ContainsFlag<TEnum>(this TEnum thisEnum, TEnum flag) where TEnum : Enum
        {
            Type enumType = typeof(TEnum);

            if (!enumType.IsEnum)
                throw new ArgumentException("TEnum must be an enumerated type.");

            ulong thisValue = Convert.ToUInt64(thisEnum);
            ulong flagValue = Convert.ToUInt64(flag);

            return (thisValue & flagValue) != 0;
        }
    }
}
