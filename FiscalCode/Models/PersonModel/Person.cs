using System;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

using FiscalCode.Generic;
using FiscalCode.Formatter;

namespace FiscalCode.Models.PersonModel
{
    /// <summary>
    /// Model Info Persona
    /// </summary>
    public class Person
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
        public City City { get; set; }
        /// <summary>
        /// Provincia della città
        /// </summary>
        [NotMapped]
        public GenderType Gender { get; set; }
    }
}
