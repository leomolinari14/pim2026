using System.ComponentModel.DataAnnotations;

namespace PimBurguer.Api.Dtos
{
    public class ItemSolicitado
    {
        [Range(1, int.MaxValue)]
        public int ProdutoId
        {
            get; set;
        }

        [Range(1, 20)]
        public int Quantidade
        {
            get; set;
        }
    }
}
