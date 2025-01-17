using System;
using System.ComponentModel.DataAnnotations;

namespace Piipan.QueryTool
{
    public class DateOfBirthRangeAttribute : RangeAttribute
    {
        public DateOfBirthRangeAttribute(string minimumValue)
            : base(typeof(DateTime), minimumValue, DateTime.Now.ToShortDateString())
        {
        }
    }
}
