#region Using
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;
#endregion

namespace FiscalCode.Models.PersonViewModel
{
    /// <summary>
    /// Model Info Città
    /// </summary>
    public class CityModel
    {
        /// <summary>
        /// Codice comune contatto
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public string Code { get; set; }
        /// <summary>
        /// Comune contatto
        /// </summary>
        [NotMapped]
        public string Name { get; set; }
        /// <summary>
        /// Provincia della città
        /// </summary>
        [NotMapped]
        public string Province { get; set; }
    }
}
