using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FiscalCode.Generic
{
    /// <summary>
    /// Tipo Sesso
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GenderType
    {
        [EnumMember(Value = "M")]
        Male,
        [EnumMember(Value = "F")]
        Female
    }
}
