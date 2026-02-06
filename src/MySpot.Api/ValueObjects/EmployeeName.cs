using MySpot.Api.Exceptions;

namespace MySpot.Api.ValueObjects;

public sealed record EmployeeName
{
    public string Value { get; }
        
    public EmployeeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length is > 100 or < 3)
        {
            throw new InvalidFullNameException(value);
        }
            
        Value = value;
    }

    public static implicit operator EmployeeName(string value) => value is null ? null : new EmployeeName(value);

    public static implicit operator string(EmployeeName value) => value?.Value;

    public override string ToString() => Value;
}