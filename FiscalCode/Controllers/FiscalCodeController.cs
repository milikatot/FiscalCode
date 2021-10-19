#region Using
using System;

using Microsoft.AspNetCore.Mvc;

using FiscalCode.Models.FiscalCode;
using FiscalCode.Models.PersonViewModel;
#endregion

namespace FiscalCode.Controllers
{
    /// <summary>
    /// Controller per le chiamate per il calcolo del codice fiscale
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FiscalCodeController : ControllerBase
    {
        /// <summary>
        /// Servizio per il calcolo del codice fiscale
        /// </summary>
        private readonly IFiscalCode service;

        /// <summary>
        /// Costruttore controller tabella "iva"
        /// </summary>
        /// <param name="pConfiguration">Parametro di configurazione controller</param>        
        public FiscalCodeController(IFiscalCode fiscalCodeService) 
        {
            service = fiscalCodeService;
        }

        /// <summary>
        /// Ritorna i dati della persona a partire dal codice fiscale o tessera sanitaria
        /// </summary>
        /// <param name="fiscalCode">Codice fiscale</param>
        /// <returns>Oggetto iva</returns>
        [HttpGet]
        public IActionResult Get([FromQuery] string fiscalCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fiscalCode))
                    throw new ArgumentException("Parametri errati");
                                
                PersonModel person = service.SelectPerson(fiscalCode);

                return Ok(person);
            }
            catch (Exception e)
            {
                return BadRequest(new { error = e.Message });
            }
        }

        /// <summary>
        /// Ritorna il codice fiscale della persona
        /// </summary>
        /// <param name="person">Persona corrente</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Post([FromBody] PersonModel person)
        {
            try
            {
                string fiscalCode = service.SelectCode(person);

                PersonModel result = new()
                {
                    Name = person.Name,
                    Surname = person.Surname,
                    FiscalCode = fiscalCode,
                    DateOfBirth = person.DateOfBirth,
                    City = person.City,
                    Gender = person.Gender
                };

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(new { error = e.Message });
            }
        }
    }
}
