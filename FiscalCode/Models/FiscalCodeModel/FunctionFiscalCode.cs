using System;
using System.Data;
using System.Linq;
using System.Text;

using FiscalCode.Models.PersonModel;

namespace FiscalCode.Models.FiscalCodeModel
{
    /// <summary>
    /// Classe per la gestione delle funzioni per il calcolo del codice fiscale
    /// </summary>
    public class FunctionFiscalCode
    {
        /// <summary>
        /// DB per il calcolo del codice fiscale
        /// </summary>
        private readonly DataSet _data;        
        /// <summary>
        /// Vocali per il calcolo
        /// </summary>
        private readonly string _vowel = "AEIOU";
        /// <summary>
        /// Consonanti per il calcolo
        /// </summary>
        private readonly string _consonant = "BCDFGHJKLMNPQRSTVWXYZ";

        /// <summary>
        /// Costruttore per la gestione del codice fiscale
        /// </summary>
        public FunctionFiscalCode(DataSet data) => _data = data;

        /// <summary>
        /// Funzione per il check del carattere di controllo
        /// </summary>
        /// <param name="partialCode">Codice fiscale senza carattere di controllo</param>
        /// <returns></returns>
        public string GetCIN(string partialCode)
        {
            int somma = 0;
            for (int i = 0; i < partialCode.Length; i++)
            {
                string tableName;
                if ((i + 1) % 2 == 1)
                    tableName = "CINDispari";
                else
                    tableName = "CINPari";

                somma += _data.Tables[tableName].Select($"Carattere = '{partialCode[i]}'")[0].Field<int>("Valore");
            }
            return _data.Tables["CINResto"].Select($"Resto = {(somma % 26)}")[0].Field<string>("Valore");
        }

        /// <summary>
        /// Restituisce il codice Omocodia in base alla lettera
        /// </summary>
        /// <param name="letter">Lettera </param>
        /// <returns></returns>
        public int GetOmocodice(string letter)
        {
            DataRow[] foundRows = _data.Tables["Omocodia"].Select($"Lettera = '{letter}'");

            if (!foundRows.Any())
                throw new Exception($"Lettera {letter} non trovata");

            return foundRows[0].Field<int>("Cifra");
        }

        /// <summary>
        /// Restituisce la lettera in base al codice Omocodia
        /// </summary>
        /// <param name="number">Codice Omocodia</param>
        /// <returns></returns>
        public string GetOmocodice(int number)
        {
            DataRow[] foundRows = _data.Tables["Omocodia"].Select($"Cifra = {number}");

            if (!foundRows.Any())
                throw new Exception($"Codice {number} non trovato");

            return foundRows[0].Field<string>("Lettera");
        }

        /// <summary>
        /// Ritorna la data di nascita a partiee dal codice fiscale normalizzato
        /// </summary>
        /// <param name="fiscalCodeNormalized">Codice fiscale normalizzato</param>
        /// <returns></returns>
        public DateTime GetDate(string fiscalCodeNormalized)
        {
            int aYear = int.Parse(fiscalCodeNormalized.Substring(6, 2));

            int thisYear = DateTime.Now.Year;
            thisYear -= ((thisYear / 100) * 100);

            if (aYear > thisYear)
                aYear += 1900;
            else
                aYear += 2000;

            int aMonth = GetMonth(fiscalCodeNormalized.Substring(8, 1));
            int aDay = int.Parse(fiscalCodeNormalized.Substring(9, 2));

            if (aDay > 40)
                aDay -= 40; // Donna tolgo 40

            return new DateTime(aYear, aMonth, aDay);
        }

        /// <summary>
        /// Converte il codice nel numero di mese
        /// </summary>
        /// <param name="code">Codice del comune</param>
        /// <returns></returns>
        public int GetMonth(string code)
        {
            DataRow[] foundRows = _data.Tables["Mesi"].Select($"Lettera = '{code}'");

            if (!foundRows.Any())
                throw new ArgumentException("codice mese non trovato");

            DataRow meseRow = foundRows[0];

            return meseRow.Field<int>("Mese");
        }

        /// <summary>
        /// Restituisce il codice a partire dal mese
        /// </summary>
        /// <param name="month">Mese in numero</param>
        /// <returns></returns>
        public string GetMonth(int month)
        {
            DataRow[] foundRows = _data.Tables["Mesi"].Select($"Mese = {month}");

            if (!foundRows.Any())
                throw new ArgumentException("Mese non trovato");

            DataRow monthRow = foundRows[0];

            return monthRow.Field<string>("Lettera");
        }

        /// <summary>
        /// Calcola il codice del nome/cognome a partire da esso. 
        /// </summary>
        /// <param name="name">nome/cognome da calcolare</param>
        /// <param name="isSurname"(Opzionale) se true effettua il calcolo per il cognome</param>
        /// <returns>Ritorna il codice di 3 cifre</returns>
        public string CalcCodeFromName(string name, bool isSurname = false)
        {
            bool consonantRemove = false;
            StringBuilder tmpCode = new(4);

            foreach (char c in name.ToUpper())
                if (_consonant.Contains(c))
                {
                    tmpCode.Append(c);

                    if (!consonantRemove && !isSurname && tmpCode.Length == 4)
                    {
                        tmpCode.Remove(1, 1);
                        consonantRemove = true;
                    }
                }

            if (tmpCode.Length > 3)
                tmpCode.Remove(3, tmpCode.Length - 3);

            if (tmpCode.Length < 3)
                foreach (char c in name.ToUpper())
                {
                    if (_vowel.IndexOf(c) >= 0)
                        tmpCode.Append(c);

                    if (tmpCode.Length == 3)
                        break;
                }

            if (tmpCode.Length < 3)
            {
                int missingChars = 3 - tmpCode.Length;
                tmpCode.Append(new string('X', missingChars));
            }

            return tmpCode.ToString();
        }

        /// <summary>
        /// Ritorna la città a partire dal codice del comune
        /// </summary>
        /// <param name="codeCity">Codice comune</param>
        /// <returns>I dati della Città</returns>
        public City GetCity(string codeCity)
        {
            DataRow[] foundRows = _data.Tables["Comuni"].Select($"Codice = '{codeCity}'");

            if (!foundRows.Any())
                throw new ArgumentException("Codice città non trovato");

            return GetCity(cityRow: foundRows[0]);
        }

        /// <summary>
        /// Ritorna la città a partire dal comune/provincia
        /// </summary>
        /// <param name="city">Comune</param>
        /// <param name="province">Porvincia</param>
        /// <returns>I dati della Città</returns>
        public City GetCity(string city, string province)
        {
            string aCity = city.Replace("'", "''");
            string aProvince = province.Replace("'", "''");

            DataRow[] foundRows = _data.Tables["Comuni"].Select($"Nome = '{aCity}' And Provincia ='{aProvince}'");

            if (!foundRows.Any())
                throw new ArgumentException("Città o provincia non trovati");

            return GetCity(cityRow: foundRows[0]);
        }

        /// <summary>
        /// Ritorna la città a partire dalla row del db
        /// </summary>
        /// <param name="cityRow">Data row del db</param>
        /// <returns></returns>
        private static City GetCity(DataRow cityRow)
        {
            return new()
            {
                Code = cityRow.Field<string>("Codice"),
                Name = cityRow.Field<string>("Nome"),
                Province = cityRow.Field<string>("Provincia")
            };
        }
    }
}
