using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Moq;
using Xunit;

namespace CoisasAFazer.Testes
{
    public class ObtemCategoriaPorIdExecute
    {
        [Fact(DisplayName = "Verifica se Id Existente por Categoria")]
        public void QuandoIdForExistenteObtemCategoriaPorIdUmaUnicaVez()
        {
            #region Arrange
            var idCategoria = 20;
            var comando = new ObtemCategoriaPorId(idCategoria);
            var mock = new Mock<IRepositorioTarefas>();
            var repo = mock.Object;
            var handler = new ObtemCategoriaPorIdHandler(repo);
            #endregion

            #region Act
            handler.Execute(comando);
            #endregion

            #region Assert
            mock.Verify(r => r.ObtemCategoriaPorId(idCategoria), Times.Once());
            #endregion
        }
    }
}
