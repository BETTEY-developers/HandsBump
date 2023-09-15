namespace HandsBumpPresetCreater;

internal class SwitchHelper
{
    public class CaseBlock<T>
    {
        public T Target { get; set; }
        public bool IsDefault { get; set; } = false;
        public Action<object> Action { get; set; }
    }
    public static void Switch<T>(T obj, params CaseBlock<T>[] cases) where T : class
    {
        foreach(var caseb in cases)
        {
            if(caseb.IsDefault || caseb.Target.Equals(obj))
            {
                caseb.Action(obj);
                return;
            }
        }
    }

    public static CaseBlock<T> Case<T>(T target,Action<object> action)
    {
        return new CaseBlock<T>()
        {
            Action = action,
            Target = target
        };
    }

    public static CaseBlock<T> Default<T>(Action<object> action)
    {
        return new CaseBlock<T>()
        {
            Action = action,
            IsDefault = true
        };
    }
}
