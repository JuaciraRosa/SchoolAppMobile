using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AppSchoolMaui.Helpers
{
    public static class HttpErrors
    {
        public static string ToUserMessage(this HttpRequestException ex)
        {
            return ex.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Sessão expirada ou não autenticado. Faça login novamente.",
                HttpStatusCode.Forbidden => "Sem permissões para esta ação.",
                HttpStatusCode.NotFound => "Recurso não encontrado.",
                HttpStatusCode.BadRequest => "Pedido inválido. Verifique os dados.",
                HttpStatusCode.RequestTimeout => "A ligação demorou demasiado tempo.",
                HttpStatusCode.InternalServerError => "Erro no servidor. Tente novamente em instantes.",
                _ => "Falha de rede. Verifique a ligação e tente novamente."
            };
        }
    }
}
