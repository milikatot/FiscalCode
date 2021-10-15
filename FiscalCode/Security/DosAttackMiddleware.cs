using System.Net;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;

namespace FiscalCode.Security
{
    /// <summary>
    /// Middleware per la gestione attacchi DOS 
    /// </summary>
    public class DosAttackMiddleware
    {
        #region Private fields  
        private static Dictionary<string, short> _IpAdresses = new();
        private static Stack<string> _Banned = new();
        private static Timer _Timer = CreateTimer();
        private static Timer _BannedTimer = CreateBanningTimer();
        #endregion

        /// <summary>
        /// Limite richieste, se supera il limite viene bannato l'IP
        /// </summary>
        private const int BANNED_REQUESTS = 10;
        /// <summary>
        /// Intervallo timer. 1 secondo
        /// </summary>
        private const int REDUCTION_INTERVAL = 1000;
        /// <summary>
        /// Tempo di attesa per essere s-bannato. 5 minuti
        /// </summary>
        private const int RELEASE_INTERVAL = 5 * 60 * 1000;   
        /// <summary>
        /// Richiesta corrente
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Middleware per la gestione attacchi DOS 
        /// </summary>
        /// <param name="next">Richiesta</param>
        public DosAttackMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string ip = httpContext.Connection.RemoteIpAddress.ToString();

            if (_Banned.Contains(ip))
                httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            CheckIpAddress(ip);
            await _next(httpContext);
        }


        /// <summary>    
        /// Controlla l'indirizzo IP nella collection e vieta la richiesta se necessario    
        /// </summary>    
        private static void CheckIpAddress(string ip)
        {
            if (!_IpAdresses.ContainsKey(ip))
                _IpAdresses[ip] = 1;
            else if (_IpAdresses[ip] == BANNED_REQUESTS)
            {
                _Banned.Push(ip);
                _IpAdresses.Remove(ip);
            }
            else
                _IpAdresses[ip]++;
        }
               
        #region Timers 

        /// <summary>    
        /// Crea il timer che sottrae una richiesta dalla collection _IpAddress.   
        /// </summary>    
        private static Timer CreateTimer()
        {
            Timer timer = GetTimer(REDUCTION_INTERVAL);
            timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
            return timer;
        }

        /// <summary>    
        /// Crea il timer che rimuove 1 indirizzo IP vietato ogni volta che è trascorso il tempo di attesa.
        /// </summary>    
        /// <returns>Oggetto timer</returns>    
        private static Timer CreateBanningTimer()
        {
            Timer timer = GetTimer(RELEASE_INTERVAL);

            timer.Elapsed += delegate {
                if (_Banned.Any()) 
                    _Banned.Pop();
            };

            return timer;
        }

        /// <summary>    
        /// Crea una semplice istanza del timer e la avvia.   
        /// </summary>    
        /// <param name="interval">Intervallo in millisecondi</param>    
        private static Timer GetTimer(int interval)
        {
            Timer timer = new();
            timer.Interval = interval;
            timer.Start();
            return timer;
        }

        /// <summary>    
        /// Elimina una richiesta dalla collection _IpAddress
        /// </summary>    
        private static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (string key in _IpAdresses.Keys.ToList())
                {
                    try
                    {
                        _IpAdresses[key]--;
                        if (_IpAdresses[key] == 0) 
                            _IpAdresses.Remove(key);
                    }
                    catch
                    {
                        // in caso di errori non faccio nulla a questo livello
                    }
                }
            }
            catch { }
        }
        #endregion
    }
}
