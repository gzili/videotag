using System.Data;
using System.Globalization;
using Dapper;

namespace VideoTag.Server.SqlTypeHandlers;

public class DateTimeUtcHandler : SqlMapper.TypeHandler<DateTime>
{
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        // When writing new data, always force strict, max-precision ISO-8601
        parameter.Value = value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
    }

    public override DateTime Parse(object value)
    {
        if (value is string dateString)
        {
            // Highly optimized standard parsing that accepts any fractional length (0-7)
            return DateTime.Parse(
                dateString, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal
            );
        }

        if (value is DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        throw new DataException($"Cannot convert {value.GetType()} to UTC DateTime.");
    }
}
