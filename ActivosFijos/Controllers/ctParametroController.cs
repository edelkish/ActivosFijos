#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using ActivosFijos.Repositorios;
using ActivosFijos.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ActivosFijos.Controllers
{
    [Authorize(AuthenticationSchemes = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin_web, gest_config")]
    public class ctParametroController : Controller
    {
        private readonly aftContext _context;
        private readonly repoParametro _repoParametro;
        private readonly repoValidaciones _validacionesRepositorio;

        public ctParametroController(aftContext context, repoParametro repoParametro, repoValidaciones validacionesRepositorio)
        {
            _context = context;
            _repoParametro = repoParametro;
            _validacionesRepositorio = validacionesRepositorio;
        }

        // GET: ctParametro
        public IActionResult Parametros()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Listado de Parámetros");
            return View("vParametros");
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeParametros()
        {
            List<Parametro> parametroList = await _repoParametro.ObtenerListadoDeParametros().ConfigureAwait(false);

            return PartialView("_vpTablaParametros", parametroList);
        }

        // GET: ctParametro/Crear
        public IActionResult CrearParametro()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Parámetro");
            ViewData["respuesta"] = new clResponse<Parametro>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vCrearParametro");
        }

        // POST: ctParametro/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarParametro([Bind("IdParametro,Nombre,Valor,Grupo")] Parametro parametro)
        {
            var respuesta = new clResponse<bool> { SiSatisfactorio = false, Mensaje = string.Empty };
            if (parametro != null)
            {
                var iDictionaryParametro = _validacionesRepositorio.EspaciosEnBlanco(parametro.Nombre, "Nombre");
                if (iDictionaryParametro.Count > 0)
                {
                    foreach (var item in iDictionaryParametro)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (_repoParametro.SiExisteNombreDelParametro(parametro.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a insertar ya existe.");
                }

                iDictionaryParametro = _validacionesRepositorio.EspaciosEnBlanco(parametro.Grupo, "Grupo");
                if (iDictionaryParametro.Count > 0)
                {
                    foreach (var item in iDictionaryParametro)
                        ModelState.AddModelError(item.Key, item.Value);
                }

                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        parametro.IdParametro = Guid.NewGuid();
                        var resultadoParametro = await _repoParametro.InsertarParametro(parametro).ConfigureAwait(false);
                        if (resultadoParametro.SiSatisfactorio)
                        {
                            mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoParametro.Mensaje;
                            return RedirectToAction(nameof(CrearParametro));
                        }
                        else
                        {
                            mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoParametro.Mensaje)}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            ViewData["respuesta"] = resultadoParametro;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Parámetro");
                            return View("vCrearParametro", parametro);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<Parametro> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear Parámetro");
                        return View("vCrearParametro", parametro);
                    }
                }
            }

            ViewData["respuesta"] = new clResponse<Parametro> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Parámetro a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Parámetro");
            return View("vCrearParametro", parametro);
        }

        // GET: ctParametro/Editar/5
        public async Task<IActionResult> EditarParametro(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Parámetro");
                ViewData["respuesta"] = new clResponse<Parametro>() { SiSatisfactorio = false, Mensaje = "El objeto Parámetro que se quiere editar es incorrecto o no existe." };
                return View("vEditarParametro", new Parametro() { IdParametro = Guid.Empty });
            }

            var parametro = await _repoParametro.SeleccionarParametroPorID(id).ConfigureAwait(false);
            if (parametro == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Parámetro");
                ViewData["respuesta"] = new clResponse<Parametro>() { SiSatisfactorio = false, Mensaje = "El objeto Parámetro que se quiere editar no existe." };

                return View("vEditarParametro", new Parametro() { IdParametro = Guid.Empty });
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Parámetro");
            ViewData["respuesta"] = new clResponse<Parametro>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };

            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vEditarParametro", parametro);
        }

        // POST: ctParametro/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarParametro(Guid idParametro, [Bind("IdParametro,Nombre,Valor,Grupo")] Parametro parametro)
        {
            if (parametro != null)
            {
                if (idParametro != parametro.IdParametro)
                {
                    ViewData["respuesta"] = new clResponse<Parametro> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Parámetro a editar." };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Editar Parámetro");
                    return View("vEditarParametro", parametro);
                }
                var iDictionaryParametro = _validacionesRepositorio.EspaciosEnBlanco(parametro.Nombre, "Nombre");
                if (iDictionaryParametro.Count > 0)
                {
                    foreach (var item in iDictionaryParametro)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (_repoParametro.SiExisteNombreDelParametroAModificar(parametro.Nombre, parametro.IdParametro))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a guardar como modificación ya existe.");
                }

                iDictionaryParametro = _validacionesRepositorio.EspaciosEnBlanco(parametro.Grupo, "Grupo");
                if (iDictionaryParametro.Count > 0)
                {
                    foreach (var item in iDictionaryParametro)
                        ModelState.AddModelError(item.Key, item.Value);
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var mensaje = string.Empty;
                        var resultadoParametro = await _repoParametro.ModificarParametro(parametro).ConfigureAwait(false);
                        if (resultadoParametro.SiSatisfactorio)
                        {
                            clGlobal.siNo = resultadoParametro.SiSatisfactorio;
                            clGlobal.objMensaje = resultadoParametro.Mensaje;
                            return RedirectToAction(nameof(EditarParametro), new { id = parametro.IdParametro });
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoParametro;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Parámetro");
                            return View("vEditarParametro", parametro);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!_repoParametro.ParametroExists(parametro.IdParametro))
                        {
                            ViewData["respuesta"] = new clResponse<Parametro> { SiSatisfactorio = false, Mensaje = "El Parámetro a editar no existe." };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Parámetro");
                            return View("vEditarParametro", parametro);
                        }
                        else
                        {
                            ViewData["respuesta"] = new clResponse<Parametro> { SiSatisfactorio = false, Mensaje = ex.Message };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Parámetro");
                            return View("vEditarParametro", parametro);
                        }
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Parametro> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Parámetro a editar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Parámetro");
            return View("vEditarParametro", parametro);
        }

        // POST: ctParametro/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ConfirmarEliminarParametro(Guid id)
        {
            try
            {
                var parametro = await _repoParametro.SeleccionarParametroPorID(id);
                var resultadoParametro = await _repoParametro.EliminarParametro(parametro);
                var result = new
                {
                    siNo = resultadoParametro.SiSatisfactorio,
                    objMensaje = resultadoParametro.Mensaje
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new
                {
                    siNo = false,
                    objMensaje = ex.Message
                };
                return Json(result);
            }
        }

        private List<clBreadCumb> LlenarBreadsCumbs(string titulo)
        {
            List<clBreadCumb> prueba = new List<clBreadCumb>();
            clBreadCumb objBreadsCumbs = new clBreadCumb()
            {
                nombre = "Parámetros",
                ruta = new List<string>() { "Parámetros", "ctParametro" }
            };
            prueba.Add(objBreadsCumbs);
            objBreadsCumbs = new clBreadCumb()
            {
                nombre = titulo,
                ruta = new List<string>()
            };
            prueba.Add(objBreadsCumbs);
            return prueba;
        }
    }
}