#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class ctConfiguracionController : Controller
    {
        private readonly aftContext _context;
        private readonly repoConfiguracion _repoConfiguracion;
        private readonly repoValidaciones _validacionesRepositorio;

        public ctConfiguracionController(aftContext context, repoConfiguracion repoConfiguracion, repoValidaciones validacionesRepositorio)
        {
            _context = context;
            _repoConfiguracion = repoConfiguracion;
            _validacionesRepositorio = validacionesRepositorio;
        }

        // GET: ctConfiguracion
        public async Task<IActionResult> Configuraciones()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Listado de Configuraciones");
            return View("vConfiguraciones");
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeConfiguraciones()
        {
            List<Configuracion> configList = await _repoConfiguracion.ObtenerListadoDeConfiguraciones().ConfigureAwait(false);

            return PartialView("_vpTablaConfiguraciones", configList);
        }

        // GET: ctConfiguracion/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configuracion = await _context.Configuracions
                .FirstOrDefaultAsync(m => m.IdConfiguracion == id);
            if (configuracion == null)
            {
                return NotFound();
            }

            return View(configuracion);
        }

        // GET: ctConfiguracion/Create
        public IActionResult CrearConfiguracion()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Configuración");
            ViewData["respuesta"] = new clResponse<Configuracion>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vCrearConfiguracion");
        }

        // POST: ctConfiguracion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracion([Bind("IdConfiguracion,Nombre,Columna,Fila")] Configuracion configuracion)
        {
            var respuesta = new clResponse<bool> { SiSatisfactorio = false, Mensaje = string.Empty };
            if (configuracion != null)
            {
                var iDictionaryConfiguracion = _validacionesRepositorio.EspaciosEnBlanco(configuracion.Nombre, "Nombre");
                if (iDictionaryConfiguracion.Count > 0)
                {
                    foreach (var item in iDictionaryConfiguracion)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (_repoConfiguracion.SiExisteNombreDeLaConfiguracion(configuracion.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a insertar ya existe.");
                }

                iDictionaryConfiguracion = _validacionesRepositorio.EspaciosEnBlanco(configuracion.Columna, "Columna");
                if (iDictionaryConfiguracion.Count > 0)
                {
                    foreach (var item in iDictionaryConfiguracion)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (_repoConfiguracion.SiExisteValorDeLaColumna(configuracion.Columna))
                {
                    ModelState.AddModelError("Columna", "El valor de la Columna a insertar ya existe.");
                }

                iDictionaryConfiguracion = _validacionesRepositorio.EspaciosEnBlanco(configuracion.Fila.ToString(), "Fila");
                if (iDictionaryConfiguracion.Count > 0)
                {
                    foreach (var item in iDictionaryConfiguracion)
                        ModelState.AddModelError(item.Key, item.Value);
                } else
                {
                    iDictionaryConfiguracion = _validacionesRepositorio.VerificarCampoFila(configuracion.Fila, "Fila");
                    if (iDictionaryConfiguracion.Count > 0)
                    {
                        foreach (var item in iDictionaryConfiguracion)
                            ModelState.AddModelError(item.Key, item.Value);
                    }
                }                

                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        configuracion.IdConfiguracion = Guid.NewGuid();
                        var resultadoCargo = await _repoConfiguracion.InsertarConfiguracion(configuracion).ConfigureAwait(false);
                        if (resultadoCargo.SiSatisfactorio)
                        {
                            mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoCargo.Mensaje;
                            return RedirectToAction(nameof(CrearConfiguracion));
                        }
                        else
                        {
                            mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoCargo.Mensaje)}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            ViewData["respuesta"] = resultadoCargo;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Configuración");
                            return View("vCrearConfiguracion", configuracion);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<bool> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear Configuración");
                        return View("vCrearConfigruacion", configuracion);
                    }
                }
            }

            ViewData["respuesta"] = new clResponse<Configuracion> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Configuración a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Configuración");
            return View("vCrearConfiguracion", configuracion);
        }

        // GET: ctConfiguracion/Edit/5
        public async Task<IActionResult> EditarConfiguracion(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Configuración");
                ViewData["respuesta"] = new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "El objeto Configuración que se quiere editar es incorrecto o no existe." };
                return View("vEditarResponsable", new Configuracion() { IdConfiguracion = Guid.Empty });
            }

            var configuracion = await _repoConfiguracion.SeleccionarConfiguracionPorID(id).ConfigureAwait(false);
            if (configuracion == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Configuración");
                ViewData["respuesta"] = new clResponse<Configuracion>() { SiSatisfactorio = false, Mensaje = "El objeto Configuración que se quiere editar no existe." };

                return View("vEditarConfiguracion", new Configuracion() { IdConfiguracion = Guid.Empty });
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Configuración");
            ViewData["respuesta"] = new clResponse<Configuracion>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };

            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vEditarConfiguracion", configuracion);
        }

        // POST: ctConfiguracion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarConfiguracion(Guid idConfiguracion, [Bind("IdConfiguracion,Nombre,Columna,Fila")] Configuracion configuracion)
        {
            if (configuracion != null)
            {
                if (idConfiguracion != configuracion.IdConfiguracion)
                {
                    ViewData["respuesta"] = new clResponse<Configuracion> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Configuración a editar." };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Editar Configuración");
                    return View("vEditarConfiguracion", configuracion);
                }
                var iDictionaryConfiguracion = _validacionesRepositorio.EspaciosEnBlanco(configuracion.Nombre, "Nombre");
                if (iDictionaryConfiguracion.Count > 0)
                {
                    foreach (var item in iDictionaryConfiguracion)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (_repoConfiguracion.SiExisteNombreDeLaConfiguracionAModificar(configuracion.Nombre, configuracion.IdConfiguracion))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a guardar como modificación ya existe.");
                }

                iDictionaryConfiguracion = _validacionesRepositorio.EspaciosEnBlanco(configuracion.Columna, "Columna");
                if (iDictionaryConfiguracion.Count > 0)
                {
                    foreach (var item in iDictionaryConfiguracion)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (_repoConfiguracion.SiExisteValorDeLaColumnaAModificar(configuracion.Columna, configuracion.IdConfiguracion))
                {
                    ModelState.AddModelError("Columna", "El valor de la Columna a guardar como modificación ya existe.");
                }

                iDictionaryConfiguracion = _validacionesRepositorio.EspaciosEnBlanco(configuracion.Fila.ToString(), "Fila");
                if (iDictionaryConfiguracion.Count > 0)
                {
                    foreach (var item in iDictionaryConfiguracion)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else
                {
                    iDictionaryConfiguracion = _validacionesRepositorio.VerificarCampoFila(configuracion.Fila, "Fila");
                    if (iDictionaryConfiguracion.Count > 0)
                    {
                        foreach (var item in iDictionaryConfiguracion)
                            ModelState.AddModelError(item.Key, item.Value);
                    }
                }
                if (ModelState.IsValid)
                {
                    try
                    {
                        var mensaje = string.Empty;
                        var resultadoConfiguracion = await _repoConfiguracion.ModificarConfiguracion(configuracion).ConfigureAwait(false);
                        if (resultadoConfiguracion.SiSatisfactorio)
                        {
                            clGlobal.siNo = resultadoConfiguracion.SiSatisfactorio;
                            clGlobal.objMensaje = resultadoConfiguracion.Mensaje;
                            return RedirectToAction(nameof(EditarConfiguracion), new { id = configuracion.IdConfiguracion });
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoConfiguracion;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Configuración");
                            return View("vEditarConfiguracion", configuracion);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!_repoConfiguracion.ConfiguracionExists(configuracion.IdConfiguracion))
                        {
                            ViewData["respuesta"] = new clResponse<Configuracion> { SiSatisfactorio = false, Mensaje = "La Configuración a editar no existe." };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Configuración");
                            return View("vEditarConfiguracion", configuracion);
                        }
                        else
                        {
                            ViewData["respuesta"] = new clResponse<Configuracion> { SiSatisfactorio = false, Mensaje = ex.Message };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Configuración");
                            return View("vEditarConfiguracion", configuracion);
                        }
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Configuracion> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Configuración a editar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Configuración");
            return View("vEditarConfiguracion", configuracion);
        }

        // GET: ctConfiguracion/Delete/5
        public async Task<IActionResult> EliminarConfiguracion(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configuracion = await _repoConfiguracion.SeleccionarConfiguracionPorID(id);
            if (configuracion == null)
            {
                return NotFound();
            }

            return View(configuracion);
        }

        // POST: ctConfiguracion/Delete/5
        [HttpPost, ActionName("EliminarConfiguracion")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ConfirmarEliminarConfiguracion(Guid id)
        {
            try
            {
                var configuracion = await _repoConfiguracion.SeleccionarConfiguracionPorID(id);
                var resultadoConfiguracion = await _repoConfiguracion.EliminarConfiguracion(configuracion);
                var result = new
                {
                    siNo = resultadoConfiguracion.SiSatisfactorio,
                    objMensaje = resultadoConfiguracion.Mensaje
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
                nombre = "Configuraciones",
                ruta = new List<string>() { "Configuraciones", "ctConfiguracion" }
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
