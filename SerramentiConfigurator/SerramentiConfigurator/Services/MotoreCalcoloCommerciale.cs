using System;
using System.Collections.Generic;

namespace SerramentiConfigurator.Server.Services
{
    public class MotoreCalcoloCommerciale
    {
        public class OutputPreventivo
        {
            public decimal LordoProfiliAlKg { get; set; }
            public decimal NettoProfiliAlKg { get; set; }
            public decimal ContributoAllestimentoImpianto { get; set; }
            public double PesoTotaleAlluminioKg { get; set; }
            public decimal TotaleFatturatoProfili => NettoProfiliAlKg + ContributoAllestimentoImpianto;
        }

        public decimal DeterminaPrezzoGrezzoProfilo(bool isProfiloCommerciale, decimal prezzoGrezzoSeriePadre, decimal prezzoGrezzoRx450)
        {
            if (isProfiloCommerciale)
            {
                return prezzoGrezzoRx450 + 0.50m;
            }
            return prezzoGrezzoSeriePadre;
        }

        public OutputPreventivo CalcolaSezioneAlluminio(
            List<VoceProfiloInput> profili,
            decimal sovrapprezzoVerniciaturaAlKg,
            double sogliaPesoKg,
            decimal contributoSottoSoglia,
            decimal contributoSopraSoglia,
            decimal scontoTrattativaProfili,
            decimal prezzoGrezzoRx450)
        {
            var output = new OutputPreventivo();
            double pesoComplessivoStruttura = 0;

            if (profili == null)
                return output;

            foreach (var p in profili)
            {
                double metriTotali = p.MetriLineari;
                double pesoProfilo = metriTotali * p.PesoAlMl;
                pesoComplessivoStruttura += pesoProfilo;

                decimal prezzoGrezzoMetallo = DeterminaPrezzoGrezzoProfilo(p.IsProfiloCommerciale, p.PrezzoGrezzoSeriePadre, prezzoGrezzoRx450);
                decimal prezzoLordoAlKg = prezzoGrezzoMetallo + sovrapprezzoVerniciaturaAlKg;

                output.LordoProfiliAlKg += (decimal)pesoProfilo * prezzoLordoAlKg;
            }

            output.PesoTotaleAlluminioKg = pesoComplessivoStruttura;

            decimal fattoreSconto = 1 - (scontoTrattativaProfili / 100);
            output.NettoProfiliAlKg = Math.Round(output.LordoProfiliAlKg * fattoreSconto, 2);

            if (sogliaPesoKg > 0)
            {
                if (pesoComplessivoStruttura < sogliaPesoKg)
                {
                    output.ContributoAllestimentoImpianto = contributoSottoSoglia;
                }
                else
                {
                    output.ContributoAllestimentoImpianto = contributoSopraSoglia;
                }
            }

            return output;
        }
    }

    public class VoceProfiloInput
    {
        public double MetriLineari { get; set; }
        public double PesoAlMl { get; set; }
        public bool IsProfiloCommerciale { get; set; }
        public decimal PrezzoGrezzoSeriePadre { get; set; }
    }
}
