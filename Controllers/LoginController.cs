using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LoginAspNetCoreEFCore.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
            {
                return Json(new { Msg = "Usuário Já Logado !!!" });
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Logar(string username, string senha)
        {
            var connection = new SqlConnection(GetStringConexao());
            SqlCommand sqlCommand = new SqlCommand(
                "SELECT * FROM usuarios WHERE Username=@username and senha=@senha", connection);
            sqlCommand.Parameters.AddWithValue("@username", username);
            sqlCommand.Parameters.AddWithValue("@senha", senha);

            await connection.OpenAsync();
            int qtd_registros = 0;
            using (SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection))
            {
                while (await sqlReader.ReadAsync())
                {
                    qtd_registros = qtd_registros + 1;
                }
            }

            await connection.OpenAsync();
            if (qtd_registros == 1)
                  {
                    using (SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                       while (await sqlReader.ReadAsync())
                         {
                           int usuarioId = sqlReader.GetInt32(0);
                           string usuario = sqlReader.GetString(1);
                           List<Claim> direitosAcesso = new List<Claim>
                              {
                                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                                new Claim(ClaimTypes.Name,usuario)
                              };

                        var userprincipal = new ClaimsPrincipal(new ClaimsIdentity(direitosAcesso, "Identity.Login"));
                          

                        await HttpContext.SignInAsync(
                                      CookieAuthenticationDefaults.AuthenticationScheme,
                                      new ClaimsPrincipal(userprincipal),
                                      new AuthenticationProperties
                                          {
                                            IsPersistent = true,
                                            ExpiresUtc = DateTime.Now.AddHours(1)
                                      });

                        return Json(new { msg = "Usuário : " + usuario + "  ID; " + usuarioId.ToString() });
                          }
                    }
             else if (qtd_registros == 0)
                   {
                    return Json(new { msg = "Ops ! Usuário Não encontrado..." });
                   }
             else 
                {
                    return Json(new { msg = "Ops ! Duplicidade de usuario e senha..." });
                }
            
            return Json(new { msg = "Ops ! Ocorreu erro de leitura de dados......." });
        }

        static string GetStringConexao()
            {
            string url = "Data Source=PC-ESCR;initial catalog=usuariosdb;user id=??????;password=???????;Integrated Security=True";

            return url;
            }

        public async Task<IActionResult> Logout()
        {
            System.Diagnostics.Debug.Print("entrei");
            if (User.Identity.IsAuthenticated)
            {
                System.Diagnostics.Debug.Print("entrei2");
                await HttpContext.SignOutAsync();
            }
            return RedirectToAction("Index", "Login");
        }
    }
}

