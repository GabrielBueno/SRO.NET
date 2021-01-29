using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using RestSharp;

namespace CorreiosSRO {

    /// <summary>
    /// Classe que agrupa métodos de acesso à API SRO dos Correios
    /// </summary>
    public class CorreiosSRO {
        private readonly string _apiUrl = "http://webservice.correios.com.br:80/service/rastro";

        public enum EnumTipo { Lista }
        public enum EnumResultado { Todos, Ultimo, Primeiro }
        public enum EnumLingua { Portugues, Ingles, Espanhol }

        public string Usuario { get; set; }
        public string Senha { get; set; }
        public EnumTipo Tipo { get; set; }
        public EnumResultado Resultado { get; set; }
        public EnumLingua Lingua { get; set; }

        /// <summary>
        /// Construtor que inicializa as credenciais para o acesso à API.
        /// 
        /// Este construtor também inicializa as configurações dos parâmetros da consulta
        /// de rastreio, como no construtor vazio
        /// </summary>
        /// <param name="usuario">Usuário do SRO Correios</param>
        /// <param name="senha">Senha do SRO Correios</param>
        public CorreiosSRO(string usuario, string senha) : this() {
            Credenciais(usuario, senha);
        }

        /// <summary>
        /// Construtor vazio.
        /// Os parâmetros da consulta do rastreio serão inicializados com seus valores padrão.
        /// 
        /// Tipo: Lista (L)
        /// Resultado: Todos (T)
        /// Língua: Português (101)
        /// </summary>
        public CorreiosSRO() {
            ParametrosRastreio(
                tipo:      EnumTipo.Lista, 
                resultado: EnumResultado.Todos, 
                lingua:    EnumLingua.Portugues
            );
        }

        /// <summary>
        /// Altera as credenciais de acesso à API SRO
        /// </summary>
        /// <param name="usuario">Usuário do SRO</param>
        /// <param name="senha">Senha do SRO</param>
        /// <returns>A própria instância na qual o método foi executado</returns>
        public CorreiosSRO Credenciais(string usuario, string senha) {
            Usuario = usuario;
            Senha   = senha;

            return this;
        }

        /// <summary>
        /// Altera os parâmetros da consulta de rastreio
        /// </summary>
        /// <param name="tipo">O tipo da consulta</param>
        /// <param name="resultado">O tipo de resultado desejado</param>
        /// <param name="lingua">A língua cuja resposta deve estar</param>
        /// <returns>A própria instância na qual o método foi executado</returns>
        public CorreiosSRO ParametrosRastreio(EnumTipo tipo, EnumResultado resultado, EnumLingua lingua) {
            Tipo      = tipo;
            Resultado = resultado;
            Lingua    = lingua;

            return this;
        }

        /// <summary>
        /// Executa a ação "buscaEventos" na API SRO em um código específico
        /// </summary>
        /// <param name="codigo">O código de rastreio</param>
        /// <returns>O objeto contendo a resposta da API SRO</returns>
        public SROResponse BuscaEventos(string codigo)
            => DoSRORequest("buscaEventos", codigo);

        /// <summary>
        /// Executa a ação "buscaEventos" na API SRO em um código específico, de forma assíncrona
        /// </summary>
        /// <param name="codigo">O código de rastreio</param>
        /// <returns>O objeto contendo a resposta da API SRO</returns>
        public Task<SROResponse> BuscaEventosAsync(string codigo)
            => DoSRORequestAsync("buscaEventos", codigo);

        /// <summary>
        /// Executa a ação "buscaEventosLista" na API SRO em um código específico, de forma assíncrona
        /// </summary>
        /// <param name="codigo">O código de rastreio</param>
        /// <returns>O objeto contendo a resposta da API SRO</returns>
        public SROResponse BuscaEventosLista(string codigo)
            => DoSRORequest("buscaEventosLista", codigo);

        /// <summary>
        /// Executa a ação "buscaEventosLista" na API SRO em um código específico, de forma assíncrona
        /// </summary>
        /// <param name="codigo">O código de rastreio</param>
        /// <returns>O objeto contendo a resposta da API SRO</returns>
        public Task<SROResponse> BuscaEventosListaAsync(string codigo)
            => DoSRORequestAsync("buscaEventosLista", codigo);

        /// <summary>
        /// Constrói e executa uma requisição para a API SRO com uma ação e um código
        /// de rastreio específicos
        /// </summary>
        /// <param name="acao">A ação da API que será executada</param>
        /// <param name="codigo">O código de rastreio</param>
        /// <returns>O objeto contendo a resposta da API</returns>
        private SROResponse DoSRORequest(string acao, string codigo) {
            var xml               = XML(acao, codigo);
            var (client, request) = SRORequest(xml);

            var response = client.Post<SROResponse>(request);

            return response.Data;
        }

        /// <summary>
        /// Constrói e executa, de forma assíncrona, uma requisição para a API SRO com uma ação e um código
        /// de rastreio específicos
        /// </summary>
        /// <param name="acao">A ação da API que será executada</param>
        /// <param name="codigo">O código de rastreio</param>
        /// <returns>O objeto contendo a resposta da API</returns>
        private Task<SROResponse> DoSRORequestAsync(string acao, string codigo) {
            var xml               = XML(acao, codigo);
            var (client, request) = SRORequest(xml);

            return client.PostAsync<SROResponse>(request);
        }

        /// <summary>
        /// Constrói uma requisição para a API SRO
        /// </summary>
        /// <param name="xml">XML que representa o corpo da requisição</param>
        /// <returns>Tupla (client, request) com, respectivamente, o RestClient e o RestRequest construídos</returns>
        private (RestClient, RestRequest) SRORequest(XElement xml) {
            var client  = new RestClient(_apiUrl);
            var request = new RestRequest();

            request.AddHeader("Content-Type", "text/xml");
            request.AddHeader("Accept",       "text/xml");

            request.AddParameter("text/xml", xml.ToString(), "text/xml", ParameterType.RequestBody);

            return (client, request);
        }

        /// <summary>
        /// Constrói o XML do corpo de uma requisição para a API SRO,
        /// com uma ação e um código de rastreio específico
        /// </summary>
        /// <param name="acao">A ação da API SRO que será executada</param>
        /// <param name="codigo">O código de rastreio</param>
        /// <returns>Objeto XElement contendo o nó raiz do corpo</returns>
        private XElement XML(string acao, string codigo) {
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace res     = "http://resource.webservice.correios.com.br/";

            var root = new XElement(soapenv+"Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                new XAttribute(XNamespace.Xmlns + "res",     res),

                new XElement(soapenv+"Header"),
                new XElement(soapenv+"Body",
                    new XElement(res+acao,
                        new XElement("usuario",   Usuario),
                        new XElement("senha",     Senha),
                        new XElement("tipo",      Str(Tipo)),
                        new XElement("resultado", Str(Resultado)),
                        new XElement("lingua",    Str(Lingua)),
                        new XElement("objetos",   codigo)
                    )
                )
            );

            return root;
        }

        /// <summary>
        /// Converte um EnumTipo para a string correspondente na definição da API SRO
        /// </summary>
        /// <param name="tipo">Valor que será convertido</param>
        /// <returns>O valor convertido</returns>
        private string Str(EnumTipo tipo) {
            switch (tipo) {
                case EnumTipo.Lista:
                    return "L";
            }

            return "L";
        }

        /// <summary>
        /// Converte um EnumResultado para a string correspondente na definição da API SRO
        /// </summary>
        /// <param name="tipo">Valor que será convertido</param>
        /// <returns>O valor convertido</returns>
        private string Str(EnumResultado resultado) {
            switch (resultado) {
                case EnumResultado.Primeiro:
                    return "P";

                case EnumResultado.Ultimo:
                    return "U";

                case EnumResultado.Todos:
                    return "T";
            }

            return "T";
        }

        /// <summary>
        /// Converte um EnumLingua para a string correspondente na definição da API SRO
        /// </summary>
        /// <param name="tipo">Valor que será convertido</param>
        /// <returns>O valor convertido</returns>
        private string Str(EnumLingua lingua) {
            switch (lingua) {
                case EnumLingua.Portugues:
                    return "101";

                case EnumLingua.Ingles:
                    return "102";

                case EnumLingua.Espanhol:
                    return "103";
            }

            return "101";
        }
    }
}