using System;
using System.ComponentModel;

namespace KeePassHttp.Utils
{
    public static class EnumExtension
    {
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            string description = e.ToString();

            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(System.Globalization.CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (descriptionAttributes.Length > 0)
                        {
                            // we're only getting the first description we find
                            // others will be ignored
                            description = ((DescriptionAttribute)descriptionAttributes[0]).Description;
                        }
                        break;
                    }
                }
            }

            return description;
        }
    }
}
