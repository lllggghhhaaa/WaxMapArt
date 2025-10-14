using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WaxMapArt.Web.Components.Components;

public class SelectOption<T>
{
    public T Value { get; set; } = default!;
    public string Text { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public bool IsDisabled { get; set; }
    
    public static List<SelectOption<T>> FromList<TSource>(
        IEnumerable<TSource> source,
        Func<TSource, T> valueSelector,
        Func<TSource, string> textSelector,
        Func<TSource, bool>? selectedSelector = null,
        Func<TSource, bool>? disabledSelector = null)
    {
        return source.Select(item => new SelectOption<T>
        {
            Value = valueSelector(item),
            Text = textSelector(item),
            IsSelected = selectedSelector?.Invoke(item) ?? false,
            IsDisabled = disabledSelector?.Invoke(item) ?? false
        }).ToList();
    }
    
    public static List<SelectOption<string>> FromStrings(params string[] values)
    {
        return values.Select(v => new SelectOption<string>
        {
            Value = v,
            Text = v,
            IsSelected = false,
            IsDisabled = false
        }).ToList();
    }
    
    public static List<SelectOption<int>> FromNumbers(params int[] values)
    {
        return values.Select(v => new SelectOption<int>
        {
            Value = v,
            Text = v.ToString(),
            IsSelected = false,
            IsDisabled = false
        }).ToList();
    }
    
    public override string ToString() => Text;

    public override bool Equals(object? obj)
    {
        if (obj is SelectOption<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }
}

public static class SelectOption
{
    public static List<SelectOption<TEnum>> FromEnum<TEnum>() where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Select(e => new SelectOption<TEnum>
            {
                Value = e,
                Text = GetEnumDisplayName(e),
                IsSelected = false,
                IsDisabled = false
            })
            .ToList();
    }
    
    private static string GetEnumDisplayName<TEnum>(TEnum enumValue) where TEnum : Enum
    {
        var field = typeof(TEnum).GetField(enumValue.ToString());
        if (field == null) return ConvertCamelCaseToReadable(enumValue.ToString());
        
        if (field.GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() is DisplayAttribute displayAttribute && !string.IsNullOrEmpty(displayAttribute.Name))
            return displayAttribute.Name;

        if (field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() is DescriptionAttribute descriptionAttribute && !string.IsNullOrEmpty(descriptionAttribute.Description))
            return descriptionAttribute.Description;

        return ConvertCamelCaseToReadable(enumValue.ToString());
    }
    
    private static string ConvertCamelCaseToReadable(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
            
        var result = System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1");
        return result.Trim();
    }
}