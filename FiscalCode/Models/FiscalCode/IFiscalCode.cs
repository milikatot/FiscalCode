using FiscalCode.Models.PersonViewModel;

namespace FiscalCode.Models.FiscalCode
{
    /// <summary>
    /// Interfaccia per la gestione del codice fiscale
    /// </summary>
    public interface IFiscalCode
    {
        /// <summary>
        /// Ritorna il codice fiscale a partire dai dati della persona
        /// </summary>
        /// <param name="person">Dati della persona</param>
        /// <returns>Ritorna il codice fiscale</returns>
        public string SelectCode(PersonModel person);


        /// <summary>
        /// Ritorna i dati della persona in base al codice fiscale
        /// </summary>
        /// <param name="fiscalCode">Codice fiscale da controllare</param>
        /// <returns></returns>
        public PersonModel SelectPerson(string fiscalCode);

    }
}
