using Loja.Catalogo.Aplicacao.Services;
using Loja.Core.Comunicacao;
using Loja.Core.Message.Notificacoes;
using Loja.Venda.Aplicacao.Commands;
using Loja.Venda.Aplicacao.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Loja.WebApp.MVC.Controllers
{
    public class CarrinhoController : ControllerBase
    {
        private readonly IProdutoAppService _produtoAppService;
        private readonly IMediatorHandler _mediatorHandler;
        private readonly IPedidoQueries _pedidoQueries;

        public CarrinhoController(
            INotificationHandler<NotificacaoDominio> notifications,
            IProdutoAppService produtoAppService, 
            IMediatorHandler mediatorHandler,
            IPedidoQueries pedidoQueries
        ) : base(notifications, mediatorHandler)
        {
            _produtoAppService = produtoAppService;
            _mediatorHandler = mediatorHandler;
            _pedidoQueries = pedidoQueries;
        }

        [Route("meu-carrinho")]
        public async Task<IActionResult> Index()
        {
            return View(await _pedidoQueries.ObterCarrinhoCliente(ClienteId));
        }

        [HttpPost]
        [Route("meu-carrinho")]
        public async Task<IActionResult> AdicionarItem(Guid id, int quantidade)
        {
            var produto = await _produtoAppService.ObterPorId(id);

            if (produto == null) return BadRequest();

            if (produto.QuantidadeEstoque < quantidade)
            {
                TempData["Erro"] = "Produto com estoque insuficiente";

                return RedirectToAction("ProdutoDetalhe", "Vitrine", new { id });
            }

            var command = new AdicionarItemPedidoCommand(
                ClienteId, produto.Id, produto.Nome, quantidade, produto.Valor
            );
            
            await _mediatorHandler.EnviarComando(command);

            if (OperacaoValida())
            {
                return RedirectToAction("Index");
            }

            TempData["Erros"] = ObterMensagensErro();
            
            return RedirectToAction("ProdutoDetalhe", "Vitrine", new { id });
        }
    }
}