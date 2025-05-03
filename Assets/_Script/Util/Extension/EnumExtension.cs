using System;
using System.Runtime.CompilerServices;

public static class EnumExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int ToInt<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
    {
        return *(int*)(&enumValue);
    }
}
