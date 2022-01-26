using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CoisasAFazer.Testes
{
    public class CadastrarTarefaHandlerExecute
    {
        [Fact(DisplayName = "Teste de Cadastro de Tarefas no DB")]
        public void DadaTarefaComInfoValidasDeveIncluirNoBD()
        {
            #region Arrange
            var comando = new CadastraTarefa("Estudar XUnit", new Categoria("Estudo"), new DateTime(2019, 12, 31));
            var mock = new Mock<ILogger<CadastraTarefaHandler>>();
            var options = new DbContextOptionsBuilder<DbTarefasContext>().UseInMemoryDatabase("DbTarefasContext").Options;
            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);
            var handler = new CadastraTarefaHandler(repo, mock.Object);
            #endregion

            #region Act
            handler.Execute(comando);
            #endregion

            #region Assert
            var tarefas = repo.ObtemTarefas(t => t.Titulo == "Estudar XUnit").FirstOrDefault();
            Assert.NotNull(tarefas);
            #endregion
        }

        [Fact(DisplayName = "Quando o IsSuccess for 'false' Dispara a Exception")]
        public void QuandoExceptionForLancadaResultadoIsSuccessDeveSerFalso()
        {
            #region Arrange
            var comando = new CadastraTarefa("Estudar XUnit", new Categoria("Estudo"), new DateTime(2019, 12, 31));
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            var mock = new Mock<IRepositorioTarefas>();
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(new Exception("Houve um Erro na Inclus�o de Tarefas"));
            var repo = mock.Object;
            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);
            #endregion

            #region Act
            CommandResult resultado = handler.Execute(comando);
            #endregion

            #region Assert
            Assert.False(resultado.IsSuccess);
            #endregion
        }

        // N�o funcionou - Nem passa na verifica��o
        // delegate void CapturaMensagemDeLog(LogLevel level, EventId eventId, object state, Exception exception, Func<object, Exception, string> function);

        [Fact(DisplayName = "Descobrindo o C�digo atrav�s de Testes")]
        public void DadaTarefaComInfoValidasDeveLogarOque()
        {
            #region Arrange
            var tituloEsperado = "Usar Moq para Aprofundar o Conhecimento da API";
            var comando = new CadastraTarefa(tituloEsperado, new Categoria("Estudo"), new DateTime(2019, 12, 31));
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            LogLevel levelCapturado = LogLevel.Error;
            string mensagemCapturada = string.Empty;
            // M�todo n�o funciona!
            //CapturaMensagemDeLog captura = (level, eventId, state, exception, function) =>
            //{
            //    levelCapturado = logLevel;
            //    mensagemCapturada = function(state, exception);
            //};
            mockLogger.Setup(l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<object, Exception, string>>((v, t) => true)
                )).Callback((IInvocation invocation) =>
                {
                    var logLevel = (LogLevel)invocation.Arguments[0];
                    var eventId = (EventId)invocation.Arguments[1];
                    var state = (IReadOnlyCollection<KeyValuePair<string, object>>)invocation.Arguments[2];
                    var exception = invocation.Arguments[3] as Exception;
                    var formatter = invocation.Arguments[4] as Delegate;
                    var formatterStr = formatter.DynamicInvoke(state, exception);

                    levelCapturado = logLevel;
                    mensagemCapturada = formatterStr.ToString();
                });
            var mock = new Mock<IRepositorioTarefas>();
            var repo = mock.Object;
            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);
            #endregion

            #region Act
            handler.Execute(comando);
            #endregion

            #region Assert
            Assert.Equal(LogLevel.Debug, levelCapturado); // Olhando as Informa��es descobrimos que � LogLevel.Debug
            //Assert.Equal("Persistindo a tarefa...", mensagemCapturada); // Olhando as Informa��es descobrimos que a Mensagem era ...
            Assert.Contains(tituloEsperado, mensagemCapturada);
            #endregion
        }

        [Fact(DisplayName = "Deve Logar a Exce��o para o Usu�rio")]
        public void QuandoExceptionForLancadaDeveLogarAMensagemDaExcecao()
        {
            #region Arrange
            var mensagem = "Houve um Erro na Inclus�o de Tarefas";
            var excecao = new Exception(mensagem);
            var comando = new CadastraTarefa("Estudar XUnit", new Categoria("Estudo"), new DateTime(2019, 12, 31));
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            var mock = new Mock<IRepositorioTarefas>();
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(excecao);
            var repo = mock.Object;
            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);
            #endregion

            #region Act
            CommandResult resultado = handler.Execute(comando);
            #endregion

            #region Assert
            mockLogger.Verify(l => l.Log(
                    LogLevel.Error, // N�vel de Log => LogError
                    It.IsAny<EventId>(), // Identificador do Evento
                    It.IsAny<object>(), // Objeto que Ser� Logado
                    excecao, // Exce��o que Ser� Loagada
                    It.Is<Func<object, Exception, string>>((v, t) => true) // Fun��o que converte Objeto + Exce��o em String
                ), Times.Once());
            #endregion
        }
    }
}
