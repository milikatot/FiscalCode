using Newtonsoft.Json.Converters;

namespace FiscalCode.Formatter
{
    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
