namespace HandsBumpPresetCreater;

using EUtility.ConsoleEx.Message;
using EUtility.StringEx.StringExtension;
using HandsBump;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;
using System.Text;

using static System.Console;
internal class Program
{
    public static string sihuo =
        "The sun is always shines." +
        "And the sunshine is always around us." +
        "Forever.";

    static void WL()
    {
        WriteLine("Hands Bump! Preset Creater  (Copyright) Eliamrity Team, Amlight 2023");
        WriteLine();
    }

    static T Setting<T>(Func<object,Type,object> OtherTypeProc, Func<object, Type, string> OtherTypeToString = null,T Default = default) where T : class, new()
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
        T result = Default == default?new T() : Default;
        int select = 0, index = 0, selectcurleft = 0;
        PropertyInfo selectprop = default;
        while(true)
        {
            Clear();
            WL();
            message.Write();
            index = 0;
            foreach (var prop in typeof(T).GetProperties())
            {
                if(index == select)
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
                        wout = $"{prop.Name,-50}{OtherTypeToString(prop.GetValue(result),prop.PropertyType)}";
                    WriteLine($"{wout}{new string(' ', WindowWidth - wout.GetStringInConsoleGridWidth())}");
                }
                index++;
            }
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.DownArrow:
                    select++;
                    if(select == typeof(T).GetProperties().Length)
                    {
                        select = typeof(T).GetProperties().Length - 1;
                    }
                    break;
                case ConsoleKey.UpArrow:
                    select--;
                    if(select == -1)
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
                                    var string2list = ((string)selectprop.GetValue(result)??"").ToList();
                                    string2list.Add((key.KeyChar>=48)?key.KeyChar:'\0');
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
        
                    
        return result;
    }

    static void Main(string[] args)
    {
        WL();
        CursorVisible = false;
        var pre = Setting<Preset>((prop, type) =>
        {
            var list = (List<Preset.Player>)Convert.ChangeType(prop, type) ?? new List<Preset.Player>();
            while (true)
            {
                Clear();
                WL();
                Dictionary<string, string> items = new();
                int index = 0;
                foreach (var item in list)
                {
                    items.Add($"第{index}位玩家设置", $"第{index}位玩家设置");
                    index++;
                }
                items.Add("添加", "添加一个玩家设置");
                items.Add("退出", "退出");
                int select = Menu.WriteLargerMenu(items);
                if (select == items.Count - 1)
                {
                    return list;
                }
                else if (select == items.Count - 1 - 1)
                {
                    list.Add(new Preset.Player());
                    list[select] = Setting((prop1, _) => prop1, Default: list[select]);
                }
                else
                {
                    list[select] = Setting((prop1, _) => prop1, Default: list[select]);
                }
            }
        }, OtherTypeToString: (obj, type) =>
        {
            List<Preset.Player> player = obj as List<Preset.Player>??new();
            StringBuilder sb = new();
            for(int i = 0; i < Math.Min(5,player.Count); i++)
            {
                sb.Append($"{player[i].Target,-3}");
            }
            return sb.ToString();
        });
        Clear();
        WL();
        var select = Menu.WriteMenu(new Dictionary<string, string>()
        {
            { "保存", "将预设保存到文件" },
            { "保存并通信", "将预设保存到文件并传输预设到HandsBump!" }
        });
        if(select == 0)
        {
            string json = System.Text.Json.JsonSerializer.Serialize<Preset>(pre);
            List<byte> bin = new(Encoding.Default.GetBytes(json));
            List<string> bytesstr = new();
            bin.ForEach(x=>bytesstr.Add(x.ToString()));
            string content = string.Join('#', bytesstr);;
            string vaildname = pre.PresetName;
            foreach(char novaild in Path.GetInvalidFileNameChars())
            {
                vaildname = vaildname.Replace(novaild, '-');
            }
            StreamWriter sw = new(vaildname+".pre");
            sw.WriteLine(content);
            sw.Close();
        }
        else
        {
            //NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream()
        }
    }
}