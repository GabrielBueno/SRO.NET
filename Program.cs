using System;

namespace CorreiosSRO {
    class Program {
        static void Main(string[] args) {
            var correios = new CorreiosSRO("ECT", "SRO");

            correios.BuscaEventos("ON440549803BR");
        }
    }
}
