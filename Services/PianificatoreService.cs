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

        public List<TappaGiro> CalcolaGiroOttimizzato(
    List<Cliente> clientiZona,
    double homeLat,
    double homeLon,
    bool rientroAPranzo,
    int durataVisitaMinuti,
    TimeSpan oraInizio,   // <--- NUOVO
    TimeSpan oraRientro   // <--- NUOVO
)
        {
            var agenda = new List<TappaGiro>();
            System.Diagnostics.Debug.WriteLine($"[Planner] CalcolaGiroOttimizzato start: clientiZona={clientiZona?.Count ?? 0}, home=({homeLat},{homeLon}), rientroAPranzo={rientroAPranzo}, durataVisitaMinuti={durataVisitaMinuti}, oraInizio={oraInizio}, oraRientro={oraRientro}");

            // inizializza stato
            DateTime orarioAttuale = DateTime.Today.Add(oraInizio);
            double latAttuale = homeLat;
            double lonAttuale = homeLon;

            var rimanenti = clientiZona.Where(c => c.Lat.HasValue && c.Lon.HasValue).ToList();
            System.Diagnostics.Debug.WriteLine($"[Planner] Clienti geocodificati validi: {rimanenti.Count}");

            DateTime limiteRientro = DateTime.Today.Add(oraRientro);
            DateTime lunchStart = DateTime.Today.AddHours(12);
            DateTime lunchEnd = DateTime.Today.AddHours(14);

            if (!rimanenti.Any()) return agenda;

            // distanza da casa per ordinamento monotono (lontano -> vicino)
            var distFromHome = rimanenti.ToDictionary(
                c => c,
                c => CalcolaDistanzaKM(homeLat, homeLon, c.Lat.Value, c.Lon.Value));

            // 1) ordina tutti i clienti per distanza da casa (decrescente) e usala come ordine definitivo
            var candidateOrder = rimanenti.OrderByDescending(c => distFromHome[c]).ToList();

            // 3) scheduling sull'ordine risultante
            orarioAttuale = DateTime.Today.Add(oraInizio);
            latAttuale = homeLat;
            lonAttuale = homeLon;
            int clientiNonSchedulati = 0;

            var remaining = candidateOrder.ToList();
            while (remaining.Any())
            {
                var cand = remaining[0];
                double cLat = cand.Lat.Value;
                double cLon = cand.Lon.Value;
                double dist = CalcolaDistanzaKM(latAttuale, lonAttuale, cLat, cLon);
                int minutiViaggio = (int)(dist * 0.9);
                if (minutiViaggio < 5 && dist > 0.1) minutiViaggio = 5;

                DateTime arrivoPrevisto = orarioAttuale.AddMinutes(minutiViaggio);
                DateTime fineVisitaPrevista = arrivoPrevisto.AddMinutes(durataVisitaMinuti);

                // se la visita si sovrappone alla pausa, effettuiamo prima la pausa e riordiniamo i rimanenti
                if (arrivoPrevisto < lunchEnd && fineVisitaPrevista > lunchStart)
                {
                    System.Diagnostics.Debug.WriteLine("[Planner] Pausa pranzo: riallineo rimanenti dopo le 14:00");
                    // aggiorniamo orario di ripartenza
                    if (rientroAPranzo)
                    {
                        orarioAttuale = lunchEnd;
                        latAttuale = homeLat;
                        lonAttuale = homeLon;
                    }
                    else
                    {
                        orarioAttuale = lunchEnd;
                        // latAttuale/lonAttuale rimangono quelli dell'ultima tappa prima della pausa
                    }

                    // riordiniamo i rimanenti in base alla distanza dalla posizione corrente per evitare riallontanamenti
                    remaining = remaining.OrderBy(x => CalcolaDistanzaKM(latAttuale, lonAttuale, x.Lat.Value, x.Lon.Value)).ToList();
                    continue; // ricalcola il primo candidato dopo la riord.
                }

                double distPerTornareACasa = CalcolaDistanzaKM(cLat, cLon, homeLat, homeLon);
                DateTime orarioRientroACasa = fineVisitaPrevista.AddMinutes((int)(distPerTornareACasa * 0.9));

                if (fineVisitaPrevista <= limiteRientro && orarioRientroACasa <= limiteRientro)
                {
                    System.Diagnostics.Debug.WriteLine($"[Planner] Aggiunto cliente {cand.IdCliente} ({cand.RagSociale}): arrivo={arrivoPrevisto:HH:mm}, partenza={fineVisitaPrevista:HH:mm}, km={dist:F2}");
                    agenda.Add(new TappaGiro
                    {
                        IdCliente = cand.IdCliente,
                        RagSociale = cand.RagSociale,
                        Citta = cand.Citta_Legale,
                        OrarioArrivo = arrivoPrevisto,
                        OrarioPartenza = fineVisitaPrevista,
                        KmDaPuntoPrecedente = Math.Round(dist, 2)
                    });

                    orarioAttuale = fineVisitaPrevista;
                    latAttuale = cLat;
                    lonAttuale = cLon;
                    remaining.RemoveAt(0);
                }
                else
                {
                    clientiNonSchedulati++;
                    System.Diagnostics.Debug.WriteLine($"[Planner] Cliente {cand.IdCliente} saltato per rientro (fineVisita={fineVisitaPrevista:HH:mm}, rientroCasa={orarioRientroACasa:HH:mm})");
                    remaining.RemoveAt(0);
                }
            }

            System.Diagnostics.Debug.WriteLine($"[Planner] CalcolaGiroOttimizzato end: tappe={agenda.Count}, clientiResidui={clientiNonSchedulati}");
            return agenda;
        }

        private List<Cliente> TwoOptOptimize(List<Cliente> route, double homeLat, double homeLon, int maxIterations)
        {
            if (route == null || route.Count < 3) return route;

            double bestDist = PathDistance(route, homeLat, homeLon);
            bool improved = true;
            int iter = 0;

            while (improved && iter < maxIterations)
            {
                improved = false;
                iter++;

                for (int i = 0; i < route.Count - 1; i++)
                {
                    for (int k = i + 1; k < route.Count; k++)
                    {
                        var newRoute = TwoOptSwap(route, i, k);
                        double newDist = PathDistance(newRoute, homeLat, homeLon);

                        if (newDist + 1e-6 < bestDist)
                        {
                            route = newRoute;
                            bestDist = newDist;
                            improved = true;
                            goto NEXT_ITERATION;
                        }
                    }
                }

            NEXT_ITERATION:;
            }

            return route;
        }

        private List<Cliente> TwoOptSwap(List<Cliente> route, int i, int k)
        {
            var newRoute = new List<Cliente>();

            for (int c = 0; c < i; c++) newRoute.Add(route[c]);
            for (int c = k; c >= i; c--) newRoute.Add(route[c]);
            for (int c = k + 1; c < route.Count; c++) newRoute.Add(route[c]);

            return newRoute;
        }

        private double PathDistance(List<Cliente> route, double homeLat, double homeLon)
        {
            double total = 0;
            double prevLat = homeLat;
            double prevLon = homeLon;

            foreach (var c in route)
            {
                total += CalcolaDistanzaKM(prevLat, prevLon, c.Lat.Value, c.Lon.Value);
                prevLat = c.Lat.Value;
                prevLon = c.Lon.Value;
            }

            total += CalcolaDistanzaKM(prevLat, prevLon, homeLat, homeLon);
            return total;
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
