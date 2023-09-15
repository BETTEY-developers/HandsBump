using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HandsBump
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class MemberIdAttribute : Attribute
    {
        Type MemberType { get; set; }
        int Id { get; set; }
        public MemberIdAttribute(int id, Type type)
        {
            Id = id;
            MemberType = type;
        }

        public static void SetPropertyFromId(int id, object setvalue, object obj, Type type)
        {
            var prop = type.GetProperties().First((prop) =>
            {
                MemberIdAttribute? v = prop.GetCustomAttribute<MemberIdAttribute>();
                if (v == null)
                    return false;

                if (v.Id == id)
                    return true;
                else
                    return false;
            });

            prop.SetValue(obj, setvalue);
        }

        public static Type GetPropertyTypeFromId(int id, Type type)
        {
            var prop = type.GetProperties().First((prop) =>
            {
                MemberIdAttribute? v = prop.GetCustomAttribute<MemberIdAttribute>();
                if (v == null)
                    return false;

                if (v.Id == id)
                    return true;
                else
                    return false;
            });

            return prop.PropertyType;
        }
    }
}
