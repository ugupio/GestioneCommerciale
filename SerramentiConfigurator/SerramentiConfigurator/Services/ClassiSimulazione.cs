using System.Collections.Generic;

namespace SerramentiConfigurator.Services
{
    public class RichiestaSimulazioneTest
    {
        public string CodiceFinituraScelta { get; set; }
        public decimal ScontoTrattativaProfili { get; set; }
        public decimal ScontoTrattativaAccessori { get; set; }
        public List<ProfiloSimulazioneInput> Profili { get; set; } = new();
        public List<AccessorioSimulazioneInput> Accessori { get; set; } = new();
    }

    public class ProfiloSimulazioneInput
    {
        public string CodiceArticolo { get; set; }
        public double MetriLineariTotali { get; set; }
        public int SerieSerramentoIdMock { get; set; }
        public double PesoAlMlMock { get; set; }
        public bool IsProfiloCommercialeMock { get; set; }
    }

    public class AccessorioSimulazioneInput
    {
        public string CodiceAccessorio { get; set; }
        public int Quantita { get; set; }
        public decimal PrezzoListinoUnitario { get; set; }
    }
}
