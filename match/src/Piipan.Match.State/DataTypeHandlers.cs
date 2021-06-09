using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;

namespace Piipan.Match.State.DataTypeHandlers
{
    /// <summary>
    /// Custom handlers for converting sql columns to C# properties
    /// Converts sql arrays of datetimes into C# Lists of DateTimes
    /// </summary>
    /// <remarks>
    /// Used when configuring Dapper as SqlMapper.AddTypeHandler(new DateTimeListHandler());
    /// </remarks>
    public class DateTimeListHandler : SqlMapper.TypeHandler<List<DateTime>>
    {
        public override List<DateTime> Parse(object value)
        {
            DateTime[] typedValue = (DateTime[])value;
            return typedValue?.ToList();
        }

        public override void SetValue(IDbDataParameter parameter, List<DateTime> value)
        {
            parameter.Value = value; // no need to convert to DateTime[] in this direction
        }
    }
}
