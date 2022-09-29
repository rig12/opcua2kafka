using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace GPNA.OPCUA2Kafka.Extensions
{
    public static class SerializationExtensions
    {
        public static async Task<MemoryStream> ToMessagePackBuff(this object payload)
        {
            var ms = new MemoryStream();
            await MessagePackSerializer.SerializeAsync(ms, payload);
            return ms;
        }


        public static string GetMsgpModel(this Type self)
        {
            var sb = new StringBuilder("[MessagePackObject]\n");
            sb.AppendFormat("public class {0}\n{{\n", self.Name);

            foreach (var field in self.GetFields())
            {
                sb.AppendFormat("    public {0} {1};\n",
                    field.FieldType.Name,
                    field.Name);
            }

            foreach (var prop in self.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = prop.GetCustomAttribute(typeof(KeyAttribute));

                if (attr == null)
                    throw new FormatException($"The property {prop.Name} has no attribute MessagePack.KeyAttribute");

                var keyAttr = (KeyAttribute)attr;
                sb.Append($"[{attr}({keyAttr.IntKey})]\n");

                var propType = prop.PropertyType.IsGenericType &&
                               prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                    ? prop.PropertyType.GetGenericArguments()[0].Name + "?"
                    : prop.PropertyType.Name;


                sb.AppendFormat("    public {0} {1} {{{2}{3}}}\n",
                    propType,
                    prop.Name,
                    prop.CanRead ? " get;" : "",
                    prop.CanWrite ? " set; " : " ");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
