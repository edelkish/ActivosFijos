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
    [Authorize(AuthenticationSchemes = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin_web, gest_nom")]
    public class ctTipoMovimientoController : Controller
    {
        private readonly aftContext _context;
        private readonly repoTipoMovimiento _repoTipoMovimiento;
        private readonly repoValidaciones _validacionesRepositorio;

        public ctTipoMovimientoController(aftContext context, repoTipoMovimiento repoTipoMovimiento, repoValidaciones validacionesRepositorio)
        {
            _context = context;
            _repoTipoMovimiento = repoTipoMovimiento;
            _validacionesRepositorio = validacionesRepositorio;
        }

        // GET: ctTipoMovimiento
        public async Task<IActionResult> TiposDeMovimientos()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Listado de Tipos de Movimiento");
            return View("vTiposDeMovimientos");
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeTiposDeMovimientos()
        {
            List<TipoMovimientoAFT> tipoMovimientoList = await _repoTipoMovimiento.ObtenerListadoDeTiposMovimiento().ConfigureAwait(false);
            return PartialView("_vpTablaTiposDeMovimientos", tipoMovimientoList);
        }

        // GET: ctTipoMovimiento/Crear
        public IActionResult CrearTipoMovimiento()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Tipo de Movimiento");
            ViewData["respuesta"] = new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vCrearTipoMovimiento");
        }

        // POST: ctTipoMovimiento/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarTipoMovimiento([Bind("IdTipoMovimientoAFT,Nombre,Descripcion,Orden,SiActivo")] TipoMovimientoAFT tipoMovimiento)
        {
            var respuesta = new clResponse<bool> { SiSatisfactorio = false, Mensaje = string.Empty };
            if (tipoMovimiento != null)
            {
                var iDictionaryTipoMovimiento = _validacionesRepositorio.EspaciosEnBlanco(tipoMovimiento.Nombre, "Nombre");
                if (iDictionaryTipoMovimiento.Count > 0)
                {
                    foreach (var item in iDictionaryTipoMovimiento)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (_context.TipoMovimientoAFTs.Any(t => t.Nombre == tipoMovimiento.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a insertar ya existe.");
                }

                iDictionaryTipoMovimiento = _validacionesRepositorio.EspaciosEnBlanco(tipoMovimiento.Orden.ToString(), "Orden");
                if (iDictionaryTipoMovimiento.Count > 0)
                {
                    foreach (var item in iDictionaryTipoMovimiento)
                        ModelState.AddModelError(item.Key, item.Value);
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        tipoMovimiento.IdTipoMovimientoAFT = Guid.NewGuid();
                        var resultadoTipoMovimiento = await _repoTipoMovimiento.InsertarTipoMovimiento(tipoMovimiento).ConfigureAwait(false);
                        if (resultadoTipoMovimiento.SiSatisfactorio)
                        {
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoTipoMovimiento.Mensaje;
                            return RedirectToAction(nameof(CrearTipoMovimiento));
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoTipoMovimiento;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Tipo de Movimiento");
                            return View("vCrearTipoMovimiento", tipoMovimiento);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<TipoMovimientoAFT> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear Tipo de Movimiento");
                        return View("vCrearTipoMovimiento", tipoMovimiento);
                    }
                }
            }

            ViewData["respuesta"] = new clResponse<TipoMovimientoAFT> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Tipo de Movimiento a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Tipo de Movimiento");
            return View("vCrearTipoMovimiento", tipoMovimiento);
        }

        // GET: ctTipoMovimiento/Editar/5
        public async Task<IActionResult> EditarTipoMovimiento(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Tipo de Movimiento");
                ViewData["respuesta"] = new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = false, Mensaje = "El Tipo de Movimiento que se quiere editar es incorrecto o no existe." };
                return View("vEditarTipoMovimiento", new TipoMovimientoAFT() { IdTipoMovimientoAFT = Guid.Empty });
            }

            var tipoMovimiento = await _repoTipoMovimiento.SeleccionarTipoMovimientoPorID(id).ConfigureAwait(false);
            if (tipoMovimiento == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Tipo de Movimiento");
                ViewData["respuesta"] = new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = false, Mensaje = "El Tipo de Movimiento que se quiere editar no existe." };
                return View("vEditarTipoMovimiento", new TipoMovimientoAFT() { IdTipoMovimientoAFT = Guid.Empty });
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Tipo de Movimiento");
            ViewData["respuesta"] = new clResponse<TipoMovimientoAFT>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vEditarTipoMovimiento", tipoMovimiento);
        }

        // POST: ctTipoMovimiento/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarTipoMovimiento(Guid idTipoMovimientoAFT, [Bind("IdTipoMovimientoAFT,Nombre,Descripcion,Orden,SiActivo")] TipoMovimientoAFT tipoMovimiento)
        {
            if (tipoMovimiento != null)
            {
                if (idTipoMovimientoAFT != tipoMovimiento.IdTipoMovimientoAFT)
                {
                    ViewData["respuesta"] = new clResponse<TipoMovimientoAFT> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Tipo de Movimiento a editar." };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Editar Tipo de Movimiento");
                    return View("vEditarTipoMovimiento", tipoMovimiento);
                }

                var iDictionaryTipoMovimiento = _validacionesRepositorio.EspaciosEnBlanco(tipoMovimiento.Nombre, "Nombre");
                if (iDictionaryTipoMovimiento.Count > 0)
                {
                    foreach (var item in iDictionaryTipoMovimiento)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (_context.TipoMovimientoAFTs.Any(t => t.Nombre == tipoMovimiento.Nombre && t.IdTipoMovimientoAFT != tipoMovimiento.IdTipoMovimientoAFT))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a guardar como modificación ya existe.");
                }

                iDictionaryTipoMovimiento = _validacionesRepositorio.EspaciosEnBlanco(tipoMovimiento.Orden.ToString(), "Orden");
                if (iDictionaryTipoMovimiento.Count > 0)
                {
                    foreach (var item in iDictionaryTipoMovimiento)
                        ModelState.AddModelError(item.Key, item.Value);
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var resultadoTipoMovimiento = await _repoTipoMovimiento.ModificarTipoMovimiento(tipoMovimiento).ConfigureAwait(false);
                        if (resultadoTipoMovimiento.SiSatisfactorio)
                        {
                            clGlobal.siNo = resultadoTipoMovimiento.SiSatisfactorio;
                            clGlobal.objMensaje = resultadoTipoMovimiento.Mensaje;
                            return RedirectToAction(nameof(EditarTipoMovimiento), new { id = tipoMovimiento.IdTipoMovimientoAFT });
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoTipoMovimiento;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Tipo de Movimiento");
                            return View("vEditarTipoMovimiento", tipoMovimiento);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!_repoTipoMovimiento.TipoMovimientoExists(tipoMovimiento.IdTipoMovimientoAFT))
                        {
                            ViewData["respuesta"] = new clResponse<TipoMovimientoAFT> { SiSatisfactorio = false, Mensaje = "El Tipo de Movimiento a editar no existe." };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Tipo de Movimiento");
                            return View("vEditarTipoMovimiento", tipoMovimiento);
                        }
                        else
                        {
                            ViewData["respuesta"] = new clResponse<TipoMovimientoAFT> { SiSatisfactorio = false, Mensaje = ex.Message };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Tipo de Movimiento");
                            return View("vEditarTipoMovimiento", tipoMovimiento);
                        }
                    }
                }
            }

            ViewData["respuesta"] = new clResponse<TipoMovimientoAFT> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Tipo de Movimiento a editar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Tipo de Movimiento");
            return View("vEditarTipoMovimiento", tipoMovimiento);
        }

        // POST: ctTipoMovimiento/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ConfirmarEliminarTipoMovimiento(Guid id)
        {
            try
            {
                var tipoMovimiento = await _repoTipoMovimiento.SeleccionarTipoMovimientoPorID(id);
                var resultadoTipoMovimiento = await _repoTipoMovimiento.EliminarTipoMovimiento(tipoMovimiento);
                var result = new
                {
                    siNo = resultadoTipoMovimiento.SiSatisfactorio,
                    objMensaje = resultadoTipoMovimiento.Mensaje
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
                nombre = "Tipos de Movimiento",
                ruta = new List<string>() { "Tipos de Movimiento", "ctTipoMovimiento" }
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