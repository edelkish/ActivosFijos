using ActivosFijos.Models.Vistas;
using ActivosFijos.Repositorios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ActivosFijos.Controllers
{
    public class ctAutenticacionController : Controller
    {
        private readonly IHttpContextAccessor _iHttpContextAccessor;
        private string returnUrl = "";
        private readonly repoUsuarios _usuarios;

        public ctAutenticacionController(IHttpContextAccessor iHttpContextAccessor, repoUsuarios usuarios)
        {
            _iHttpContextAccessor = iHttpContextAccessor;
            _usuarios = usuarios;
        }
        // GET: ctAutenticacionController
        public ActionResult Autenticacion(string? returnUrl = "")
        {
            this.returnUrl = returnUrl ?? "";
            ViewBag.ReturnUrl = this.returnUrl;

            return View("vAutenticacion");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Autenticarse(mdLogin loginModel, string? returnUrl = "")
        {
            if (ModelState.IsValid)
            {
                var iDictionaryAutenticacion = await _usuarios.AutenticarUsuarioPassword(loginModel, returnUrl);
                if (iDictionaryAutenticacion.Count > 0)
                {
                    foreach (var item in iDictionaryAutenticacion)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else
                {
                    var usuario = await _usuarios.ObtenerDatosPersonal(loginModel.nombreUsuario);
                    var claims = new List<Claim>() {
                        new Claim(ClaimTypes.Name, usuario.Usuario1),
                        new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString())
                    };
                    var permisos = usuario.IdPermisos;
                    foreach (var item in permisos)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, item.Identificador, item.Nombre));
                    }                    
                    claims.Add(new Claim(ClaimTypes.UserData, usuario.Nombre + " " + usuario.Apellidos));

                    var identity = new ClaimsIdentity(claims, "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync("Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        if (Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);
                        else
                            return RedirectToAction("Inicio", "ctInicio");
                    }
                    return RedirectToAction("Inicio", "ctInicio");
                }
            }

            this.returnUrl = returnUrl ?? "";
            ViewBag.ReturnUrl = this.returnUrl;

            return View("vAutenticacion", loginModel);
        }

        [Authorize(AuthenticationSchemes = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Desloguearse()
        {
            await HttpContext.SignOutAsync("Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Autenticacion");
        }
    }
}
