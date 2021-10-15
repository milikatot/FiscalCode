using System;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

using FiscalCode.Models.PersonModel;

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
        /// Costruttore controller tabella "iva"
        /// </summary>
        /// <param name="pConfiguration">Parametro di configurazione controller</param>        
        public FiscalCodeController(IConfiguration pConfiguration) { }

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

                Models.FiscalCodeModel.FiscalCode calcFiscalCode = new();
                
                Person person = calcFiscalCode.SelectPerson(fiscalCode);

                return new OkObjectResult(person);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { error = e.Message });
            }
        }

        /// <summary>
        /// Ritorna il codice fiscale della persona
        /// </summary>
        /// <param name="pPerson">Persona corrente</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Post([FromBody] Person pPerson)
        {
            try
            {
                Models.FiscalCodeModel.FiscalCode calcFiscalCode = new();

                string fiscalCode = calcFiscalCode.SelectCode(pPerson);

                Person result = new()
                {
                    Name = pPerson.Name,
                    Surname = pPerson.Surname,
                    FiscalCode = fiscalCode,
                    DateOfBirth = pPerson.DateOfBirth,
                    City = pPerson.City,
                    Gender = pPerson.Gender
                };

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(new { error = e.Message });
            }
        }
    }
}
