using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Globalization;
using GestioneCommerciale.Models;

namespace GestioneCommerciale.Services
{


    public class PianificatoreService
    {
        private readonly string _apiKey = "pk.41a064476ff914f4e50c47e4f3cf9ebe";

        public async Task<(double lat, double lon)?> GeocodificaIndirizzo(string indirizzo)
        {
            try
            {
                using var client = new HttpClient();
                var builder = new UriBuilder("https://locationiq.com");
                var query = HttpUtility.ParseQueryString(builder.Query);
                query["key"] = _apiKey;
                query["q"] = $"{indirizzo}, Italy";
                query["format"] = "json";
                query["limit"] = "1";
                builder.Query = query.ToString();

                var resp = await client.GetAsync(builder.Uri);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var results = JsonSerializer.Deserialize<List<NominatimResult>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (results != null && results.Any())
                    {
                        double lat = double.Parse(results[0].Lat, CultureInfo.InvariantCulture);
                        double lon = double.Parse(results[0].Lon, CultureInfo.InvariantCulture);
                        return (lat, lon);
                    }
                }
            }
            catch { }
            return null;
        }

        public List<TappaGiro> CalcolaGiroOttimizzato(List<Cliente> clientiZona, double homeLat, double homeLon, bool rientroAPranzo, int durataVisitaMinuti)
        {
            var agenda = new List<TappaGiro>();
            DateTime orarioAttuale = DateTime.Today.AddHours(8).AddMinutes(30);
            double latAttuale = homeLat;
            double lonAttuale = homeLon;

            var rimanenti = clientiZona.Where(c => c.Lat.HasValue && c.Lon.HasValue).ToList();

            // Iniziamo dal più lontano da casa
            var prossimo = rimanenti
                .OrderByDescending(c => CalcolaDistanzaKM(homeLat, homeLon, (double)c.Lat, (double)c.Lon))
                .FirstOrDefault();

            while (prossimo != null)
            {
                double cLat = (double)prossimo.Lat;
                double cLon = (double)prossimo.Lon;

                // 1. CALCOLO VIAGGIO
                double dist = CalcolaDistanzaKM(latAttuale, lonAttuale, cLat, cLon);
                int minutiViaggio = (int)(dist * 0.9);
                if (minutiViaggio < 5 && dist > 0.1) minutiViaggio = 5;

                DateTime arrivoPrevisto = orarioAttuale.AddMinutes(minutiViaggio);
                DateTime fineVisitaPrevista = arrivoPrevisto.AddMinutes(durataVisitaMinuti);

                // 2. LOGICA PAUSA PRANZO (Inizio tra le 12 e le 14 o fine oltre le 12)
                if (fineVisitaPrevista.Hour >= 12 && fineVisitaPrevista.Hour < 14 || (fineVisitaPrevista.Hour == 12 && fineVisitaPrevista.Minute > 0))
                {
                    // Reset alle 14:00
                    DateTime ripartenzaPomeriggio = DateTime.Today.AddHours(14);

                    if (rientroAPranzo)
                    {
                        // Ti riporto a Piombino
                        latAttuale = homeLat;
                        lonAttuale = homeLon;

                        // Ricalcolo il viaggio partendo da casa verso il cliente che era "in sospeso"
                        double distDaCasa = CalcolaDistanzaKM(homeLat, homeLon, cLat, cLon);
                        dist = distDaCasa;
                        arrivoPrevisto = ripartenzaPomeriggio.AddMinutes((int)(dist * 0.9));
                        fineVisitaPrevista = arrivoPrevisto.AddMinutes(durataVisitaMinuti);
                    }
                    else
                    {
                        // Pausa sul posto: inizi alle 14:00
                        arrivoPrevisto = ripartenzaPomeriggio;
                        fineVisitaPrevista = arrivoPrevisto.AddMinutes(durataVisitaMinuti);
                    }
                }

                // 3. CALCOLO RIENTRO A CASA (Muro invalicabile delle 18:00)
                double distPerTornareACasa = CalcolaDistanzaKM(cLat, cLon, homeLat, homeLon);
                int minutiRitorno = (int)(distPerTornareACasa * 0.9); // Tempo stimato per tornare a Piombino

                // Calcoliamo l'orario in cui saresti effettivamente a casa dopo la visita
                DateTime orarioRientroACasa = fineVisitaPrevista.AddMinutes(minutiRitorno);

                // Se la visita finisce dopo le 18:00 OPPURE se arrivi a casa dopo le 18:00, questa tappa salta
                if (fineVisitaPrevista.Hour >= 18 || orarioRientroACasa.Hour >= 18 && (orarioRientroACasa.Minute > 0 || orarioRientroACasa.Hour > 18))
                {
                    // Questo cliente non può essere visitato oggi se vuoi essere a casa per le 18:00
                    break;
                }

                // 4. AGGIUNGI TAPPA
                agenda.Add(new TappaGiro
                {
                    IdCliente = prossimo.IdCliente,
                    RagSociale = prossimo.RagSociale,
                    Citta = prossimo.Citta_Legale,
                    OrarioArrivo = arrivoPrevisto,
                    OrarioPartenza = fineVisitaPrevista,
                    KmDaPuntoPrecedente = dist
                });

                // 5. AGGIORNA STATO
                orarioAttuale = fineVisitaPrevista;
                latAttuale = cLat;
                lonAttuale = cLon;
                rimanenti.Remove(prossimo);

                // CERCA IL PIÙ VICINO ALLA POSIZIONE ATTUALE
                prossimo = rimanenti
                    .OrderBy(c => CalcolaDistanzaKM(latAttuale, lonAttuale, (double)c.Lat, (double)c.Lon))
                    .FirstOrDefault();
            }
            return agenda;
        }


        // Formula Haversine precisa per i KM reali
        private double CalcolaDistanzaKM(double lat1, double lon1, double lat2, double lon2)
        {
            if (lat1 == 0 || lat2 == 0) return 0;

            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return 6371 * c;
        }

    }

    }
