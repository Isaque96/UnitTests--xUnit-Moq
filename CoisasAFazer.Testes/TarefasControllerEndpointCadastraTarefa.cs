using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Alura.CoisasAFazer.WebApp.Controllers;
using Alura.CoisasAFazer.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace CoisasAFazer.Testes
{
    public class TarefasControllerEndpointCadastraTarefa
    {
        [Fact(DisplayName = "Inserindo a Tarefa o Retorno deve ser o Status Code 200(OkResult)")]
        public void DadaTarefaComInformacoesValidasDeveRetornar200()
        {
            #region Arrange
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>().Object;
            var options = new DbContextOptionsBuilder<DbTarefasContext>().UseInMemoryDatabase("DbTarefas").Options;
            var contexto = new DbTarefasContext(options);
            contexto.Categorias.Add(new Categoria(20, "Estudo"));
            contexto.SaveChanges();
            var repo = new RepositorioTarefa(contexto);
            var controlador = new TarefasController(repo, mockLogger);
            var model = new CadastraTarefaVM
            {
                IdCategoria = 20,
                Titulo = "Estudar XUnit",
                Prazo = new DateTime(2019, 12, 31)
            };
            #endregion

            #region Act
            var retorno = controlador.EndpointCadastraTarefa(model);
            #endregion

            #region Assert
            Assert.IsType<OkResult>(retorno);
            #endregion
        }

        [Fact(DisplayName = "Exceção de Criação Retornando Status Code 500")]
        public void QuandoExcecaoForLancadaDeveRetornarStatusCode500()
        {
            #region Arrange
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>().Object;
            var mock = new Mock<IRepositorioTarefas>();
            mock.Setup(r => r.ObtemCategoriaPorId(20)).Returns(new Categoria(20, "Estudo"));
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(new Exception("Houve um Erro!"));
            var repo = mock.Object;
            var controlador = new TarefasController(repo, mockLogger);
            var model = new CadastraTarefaVM
            {
                IdCategoria = 20,
                Titulo = "Estudar XUnit",
                Prazo = new DateTime(2019, 12, 31)
            };
            #endregion

            #region Act
            var retorno = controlador.EndpointCadastraTarefa(model);
            #endregion

            #region Assert
            Assert.IsType<StatusCodeResult>(retorno);
            var statusCode = (retorno as StatusCodeResult).StatusCode;
            Assert.Equal(500, statusCode);
            #endregion
        }
    }
}
