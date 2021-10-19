#region Using
using System;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

using FiscalCode.Generic;
using FiscalCode.Formatter;
#endregion

namespace FiscalCode.Models.PersonViewModel
{
    /// <summary>
    /// Model Info Persona
    /// </summary>
    public class PersonModel
    {
        /// <summary>
        /// Nome contatto
        /// </summary>
        [NotMapped]
        public string Name { get; set; }
        /// <summary>
        /// Cognome contatto
        /// </summary>
        [NotMapped]
        public string Surname { get; set; }
        /// <summary>
        /// Codice fiscale contatto
        /// </summary>
        [NotMapped]
        public string FiscalCode { get; set; }
        /// <summary>
        /// Data di nasciata contatto
        /// </summary>
        [NotMapped]
        [JsonConverter(typeof(DateFormatConverter), Consts.FORMAT_DATE_TO_STRING)]
        public DateTime DateOfBirth { get; set; }
        /// <summary>
        /// Comune contatto
        /// </summary>
        [NotMapped]
        public CityModel City { get; set; }
        /// <summary>
        /// Provincia della città
        /// </summary>
        [NotMapped]
        public GenderType Gender { get; set; }
    }
}
