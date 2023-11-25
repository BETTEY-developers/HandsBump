using EUtility.ConsoleEx.Message;
using EUtility.StringEx.StringExtension;
using System.Reflection;
using System.Text;
using static System.Console;

namespace HandsBump
{
    internal partial class Program
    {
        static T Setting<T>(Func<object, Type, object> OtherTypeProc, Func<object, Type, string> OtherTypeToString = null, T Default = default) where T : class, new()
        {
            MessageOutputer message = new MessageOutputer()
            {
                new MessageUnit()
                {
                    Title = "↑",
                    Description = "上一个属性"
                },
                new MessageUnit()
                {
                    Title = "↓",
                    Description = "下一个属性"
                },
                new MessageUnit()
                {
                    Title = "Other",
                    Description = "设置属性值"
                },
                new MessageUnit()
                {
                    Title = "ESC",
                    Description = "退出并返回设置的结果"
                }
            };
            T result = Default == default ? new T() : Default;
            int select = 0, index = 0, selectcurleft = 0;
            PropertyInfo selectprop = default;
            while (true)
            {
                Clear();
                WriteLogo();
                message.Write();
                index = 0;
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (index == select)
                    {
                        BackgroundColor = ConsoleColor.White;
                        ForegroundColor = ConsoleColor.Black;
                        string wout = "";
                        if (new List<Type>() { typeof(int), typeof(string) }.Contains(prop.PropertyType))
                            wout = $"{prop.Name,-50}{prop.GetValue(result)}";
                        else
                            wout = $"{prop.Name,-50}{OtherTypeToString(prop.GetValue(result), prop.PropertyType)}";
                        WriteLine($"{wout}{new string(' ', WindowWidth - wout.GetStringInConsoleGridWidth())}");
                        ResetColor();
                        selectprop = prop;
                        selectcurleft = wout.GetStringInConsoleGridWidth();
                    }
                    else
                    {
                        string wout = "";
                        if (new List<Type>() { typeof(int), typeof(string) }.Contains(prop.PropertyType))
                            wout = $"{prop.Name,-50}{prop.GetValue(result)}";
                        else
                            wout = $"{prop.Name,-50}{OtherTypeToString(prop.GetValue(result), prop.PropertyType)}";
                        WriteLine($"{wout}{new string(' ', WindowWidth - wout.GetStringInConsoleGridWidth())}");
                    }
                    index++;
                }
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                        select++;
                        if (select == typeof(T).GetProperties().Length)
                        {
                            select = typeof(T).GetProperties().Length - 1;
                        }
                        break;
                    case ConsoleKey.UpArrow:
                        select--;
                        if (select == -1)
                        {
                            select = 0;
                        }
                        break;
                    case ConsoleKey.Escape:
                        return result;
                    default:
                        SwitchHelper.Switch<Type>(
                            selectprop.PropertyType,
                            SwitchHelper.Case<Type>(typeof(int), (_) =>
                            {
                                switch (key.Key)
                                {
                                    case ConsoleKey.Backspace:
                                        selectprop.SetValue(result, (int)((int)(selectprop.GetValue(result) ?? 0) / 10));
                                        break;
                                    case ConsoleKey.Add:
                                        selectprop.SetValue(result, (int)(selectprop.GetValue(result) ?? 0) + 1);
                                        break;
                                    case ConsoleKey.Subtract:
                                        selectprop.SetValue(result, (int)(selectprop.GetValue(result) ?? 0) - 1);
                                        break;
                                    default:
                                        if (key.Key >= ConsoleKey.D0 && key.Key <= ConsoleKey.D9 ||
                                        key.Key >= ConsoleKey.NumPad0 && key.Key <= ConsoleKey.NumPad9)
                                            selectprop.SetValue(result, (int)(selectprop.GetValue(result) ?? 0) * 10 + (key.KeyChar - 48));
                                        break;
                                }
                            }),
                            SwitchHelper.Case<Type>(typeof(string), (_) =>
                            {
                                switch (key.Key)
                                {
                                    case ConsoleKey.Backspace:
                                        if (((string)selectprop.GetValue(result)).Length == 0)
                                            break;
                                        selectprop.SetValue(result, new string(((string)selectprop.GetValue(result)).ToCharArray()[..^1]));
                                        break;
                                    default:
                                        if (key.KeyChar < 32)
                                            return;
                                        var string2list = ((string)selectprop.GetValue(result) ?? "").ToList();
                                        string2list.Add(key.KeyChar);
                                        StringBuilder sb = new StringBuilder();
                                        string2list.ForEach(x => sb.Append(x));
                                        selectprop.SetValue(result, sb.ToString());
                                        break;
                                }
                            }),
                            SwitchHelper.Default<Type>((_) =>
                            {
                                selectprop.SetValue(result, OtherTypeProc(selectprop.GetValue(result), selectprop.PropertyType));
                            })
                        );
                        break;
                }
            }
        }
    }
}
