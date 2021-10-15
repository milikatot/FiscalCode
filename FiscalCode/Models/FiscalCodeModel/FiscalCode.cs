using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

using FiscalCode.Generic;
using FiscalCode.Models.PersonModel;

namespace FiscalCode.Models.FiscalCodeModel
{
    /// <summary>
    /// Classe per la gestione del codice fiscale
    /// </summary>
    public class FiscalCode
    {
        /// <summary>
        /// DB per il calcolo del codice fiscale
        /// </summary>
        private readonly DataSet _data;
        /// <summary>
        /// Codice Omocodia per la gestione dei codici fiscali doppi
        /// </summary>
        private readonly string _omocodiciRegex;
        /// <summary>
        /// Posizione codice Omocodia per la gestione dei codici fiscali doppi
        /// </summary>
        private readonly int[] _omocodiaPosition = { 14, 13, 12, 10, 9, 7, 6 };
        /// <summary>
        /// Lettere Mesi
        /// </summary>
        private readonly string _monthRegex;
        /// <summary>
        /// Classe per le funzioni per il calcolo del codice fiscale
        /// </summary>
        private readonly FunctionFiscalCode _function;

        /// <summary>
        /// Classe per la gestione del codice fiscale
        /// </summary>
        public FiscalCode()
        {
            // Lettura dati da xml
            _data = new();
            _data.ReadXml(@"Data\xml\data.xml");

            // instanzio classe per le funzioni del calcolo del codice fiscale
            _function = new(_data);

            // Espressione regolare per il codice Omocodia
            _omocodiciRegex = @"[0-9";
            DataTable omocodiaTable = _data.Tables["Omocodia"];
            for (int i = 0; i < omocodiaTable.Rows.Count; i++)
                _omocodiciRegex += omocodiaTable.Rows[i].Field<string>("Lettera");
            _omocodiciRegex += @"]";

            // Espressione regolare per i mesi
            _monthRegex = @"[";
            DataTable mesiTable = _data.Tables["Mesi"];
            for (int i = 0; i < mesiTable.Rows.Count; i++)
                _monthRegex += mesiTable.Rows[i].Field<string>("Lettera");
            _monthRegex += @"]";
        }

        /// <summary>
        /// Ritorna il codice fiscale a partire dai dati della persona
        /// </summary>
        /// <param name="person">Dati della persona</param>
        /// <returns>Ritorna il codice fiscale</returns>
        public string SelectCode(Person person)
        {
            // controllo dati della persona
            IsPersonCorrect(person);

            // calcolo il codice fiscale
            StringBuilder tmpCodice = new(16);

            // codice cognome 
            string aCodeSurname = _function.CalcCodeFromName(person.Surname, isSurname: true);
            tmpCodice.Append(aCodeSurname);

            // codice nome
            string aCodeName = _function.CalcCodeFromName(person.Name);
            tmpCodice.Append(aCodeName);

            // codice anno
            string aCodeYear = person.DateOfBirth.Year.ToString().Substring(2, 2);
            tmpCodice.Append(aCodeYear);

            // codice mese
            string aCodeMonth = _function.GetMonth(person.DateOfBirth.Month);
            tmpCodice.Append(aCodeMonth);

            // codice giorno            
            int codeDay = person.DateOfBirth.Day;
            if (person.Gender == GenderType.Female)
                codeDay += 40;
            tmpCodice.Append(codeDay.ToString("D2"));

            // codice città
            City city = _function.GetCity(person.City.Name, person.City.Province);
            tmpCodice.Append(city.Code);

            // carattere di controllo
            string cin = _function.GetCIN(tmpCodice.ToString());
            tmpCodice.Append(cin);

            return tmpCodice.ToString();
        }

        /// <summary>
        /// Controlla se i dati della persona sono corretti per il calcolo del codice fiscale
        /// </summary>
        /// <param name="person">Dati della persona</param>
        /// <returns></returns>
        private static void IsPersonCorrect(Person person)
        {
            if (string.IsNullOrWhiteSpace(person.Name))
                throw new ArgumentException("Nome non presente");

            if (string.IsNullOrWhiteSpace(person.Surname))
                throw new ArgumentException("Cognome non presente");

            if (DateTime.Compare(person.DateOfBirth, DateTime.MinValue) == 0)
                throw new ArgumentException("data di nascita non presente");

            if (person.City is null)
                throw new ArgumentException("Città/Provincia non presenti");

            if (string.IsNullOrWhiteSpace(person.City.Name) || string.IsNullOrWhiteSpace(person.City.Province))
                throw new ArgumentException("Città/Provincia non presenti");

        }

        /// <summary>
        /// Ritorna i dati della persona in base al codice fiscale
        /// </summary>
        /// <param name="fiscalCode">Codice fiscale da controllare</param>
        /// <returns></returns>
        public Person SelectPerson(string fiscalCode)
        {
            string cfNoOmocodiciRegex = @"^[A-Z]{6}\d{2}" + _monthRegex + @"\d{2}[A-Z]\d{3}[A-Z]";
            string cfRegex = @"^[A-Z]{6}" + _omocodiciRegex + "{2}" + _monthRegex + _omocodiciRegex + "{2}[A-Z]" + _omocodiciRegex + "{3}[A-Z]";

            string CF = fiscalCode.ToUpper();

            // Controllo se il codice fiscale è valido in base ai codici del mese e codice Omocodia
            if (!Regex.Match(CF, cfRegex).Success)
                throw new ArgumentException("Codice fiscale errato!");

            if (_function.GetCIN(CF.Substring(0, 15)) != CF.Substring(15, 1))
                throw new ArgumentException("Codice fiscale errato!");

            string aCodeNormalized;
            int aOmocodiaLevel;
            if (Regex.Match(CF, cfNoOmocodiciRegex).Success)
            {
                aCodeNormalized = CF;
                aOmocodiaLevel = 0;
            }
            else
            {
                // si tratta di un omocodice
                StringBuilder cfNormalized = new(CF);
                cfNormalized.Remove(15, 1);
                int omocodialevelRagg = 0;
                foreach (int i in _omocodiaPosition)
                {
                    if (char.IsLetter(cfNormalized[i]))
                    {
                        omocodialevelRagg++;
                        string tmpLettera = cfNormalized[i].ToString();
                        cfNormalized.Remove(i, 1);
                        cfNormalized.Insert(i, _function.GetOmocodice(tmpLettera));
                    }
                }
                cfNormalized.Append(_function.GetCIN(cfNormalized.ToString()));
                aOmocodiaLevel = omocodialevelRagg;
                aCodeNormalized = cfNormalized.ToString();
            }

            string surname = aCodeNormalized.Substring(0, 3);
            string name = aCodeNormalized.Substring(3, 3);
            DateTime dateOfirth = _function.GetDate(aCodeNormalized);

            GenderType gender = GenderType.Male;
            // il sesso viene calcolato dal giorno di nascita
            int numberDay = int.Parse(aCodeNormalized.Substring(9, 2));
            if (numberDay > 40)
                gender = GenderType.Female;

            City city = _function.GetCity(aCodeNormalized.Substring(11, 4));

            return new Person()
            {
                Name = name,
                Surname = surname,
                FiscalCode = CF,
                City = city,
                DateOfBirth = dateOfirth,
                Gender = gender
            };
        }

    }
}
