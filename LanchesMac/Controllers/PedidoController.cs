using LanchesMac.Models;
using LanchesMac.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanchesMac.Controllers
{
    public class PedidoController : Controller
    {
        private readonly CarrinhoCompra _carrinhoCompra;
        private readonly IPedidoRepository _pedidoRepository;

        public PedidoController(CarrinhoCompra carrinhoCompra, IPedidoRepository pedidoRepository)
        {
            _carrinhoCompra = carrinhoCompra;
            _pedidoRepository = pedidoRepository;
        }

        [HttpGet] // não é necessário colocar [HttpGet], pois é inferido. [HttpPost] é obrigatório.
        [Authorize]
        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Checkout(Pedido pedido)
        {
            int totalItensPedido = 0;
            decimal precoTotalPedido = 0.0m;

            //primeiro precisamos obter os itens do carrinho de compras do cliente
            List<CarrinhoCompraItem> items = _carrinhoCompra.GetCarrinhoCompraItens();
            _carrinhoCompra.CarrinhoCompraItens = items;

            if(_carrinhoCompra.CarrinhoCompraItens.Count == 0)
            {
                ModelState.AddModelError("", "Seu carrinho está vazio. Que tal incluir algum produto?");
            }

            // calcular o total de itens e o total do pedido
            foreach(var item in items)
            {
                totalItensPedido += item.Quantidade;
                precoTotalPedido += (item.Quantidade * item.Lanche.Preco);
            }

            // atribuindo os valores obtidos ao pedido
            pedido.TotalItensPedido = totalItensPedido;
            pedido.PedidoTotal = precoTotalPedido;

            // validar os dados do pedido
            if(ModelState.IsValid)
            {
                // criar o pedido e os detalhes
                _pedidoRepository.CriarPedido(pedido);

                //definir mensagens ao cliente
                ViewBag.CheckoutCompletoMensagem = "Obrigado pelo seu pedido :)";
                ViewBag.TotalPedido = _carrinhoCompra.GetCarrinhoCompraTotal();

                // limpando o carrinho de compras
                _carrinhoCompra.LimparCarrinho();

                //exibindo a view com dados do cliente e do pedido
                return View("~/Views/Pedido/CheckoutCompleto.cshtml", pedido);
            }
            return View(pedido);
        }
    }
}
