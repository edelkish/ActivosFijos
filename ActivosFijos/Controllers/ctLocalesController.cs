using ActivosFijos.Clases;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using ActivosFijos.Repositorios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Controllers
{
    [Authorize(AuthenticationSchemes = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin_web, gest_nom")]
    public class ctLocalesController : Controller
    {
        private readonly repoCubiculos cubiculoRepositorio;
        private readonly repoLocales localRepositorio;
        private readonly repoValidaciones validacionesRepositorio;
        private readonly aftContext _context;
        public ctLocalesController(repoCubiculos cubiculoRepositorio, repoLocales localRepositorio, repoValidaciones validacionesRepositorio, aftContext context)
        {
            this.cubiculoRepositorio = cubiculoRepositorio;
            this.localRepositorio = localRepositorio;
            this.validacionesRepositorio = validacionesRepositorio;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public IActionResult Locales()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Lista de Locales");
            return View("vLocales");
        }
        [HttpPost]
        public async Task<IActionResult> ListadoDeLocales()
        {
            List<Locale> localesList = await localRepositorio.ObtenerListadoLocales().ConfigureAwait(false);
            
            return PartialView("_vpTablaLocales", localesList);
        }

        public async Task<IActionResult> DetallesLocal(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var local = await localRepositorio.SeleccionarLocalesPorID(id).ConfigureAwait(false);
            if (local == null)
            {
                return NotFound();
            }
            return View(local);
        }

        public IActionResult CrearLocal()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Local");
            ViewData["respuesta"] = new clResponse<Locale>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vCrearLocal");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCargo([Bind("Nombre,Descripcion,IdLocal")] Locale local)
        {
            var respuesta = new clResponse<bool> { SiSatisfactorio = false, Mensaje = string.Empty };
            if (local != null)
            {
                var iDictionaryIntegrantes = validacionesRepositorio.EspaciosEnBlanco(local.Nombre, "Nombre");
                if (iDictionaryIntegrantes.Count > 0)
                {
                    foreach (var item in iDictionaryIntegrantes)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (localRepositorio.SiExisteNombreDelLocal(local.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a insertar ya existe.");
                }
                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        local.IdLocal = Guid.NewGuid();
                        var resultadoLocal = await localRepositorio.InsertarLocal(local).ConfigureAwait(false);
                        if (resultadoLocal.SiSatisfactorio)
                        {
                            mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoLocal.Mensaje;
                            return RedirectToAction(nameof(CrearLocal));
                        }
                        else
                        {
                            mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoLocal.Mensaje)}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            ViewData["respuesta"] = resultadoLocal;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Local");
                            return View("vCrearLocal", local);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<bool> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear Local");
                        return View("vCrearLocal", local);
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Locale> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Local a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Local");
            return View("vCrearLocal", local);
        }

        public async Task<IActionResult> EditarLocal(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
                ViewData["respuesta"] = new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = "El objeto Local que se quiere editar es incorrecto o no existe." };
                return View("vEditarLocal", new Cargo() { IdCargo = Guid.Empty });
            }
            var local = await localRepositorio.SeleccionarLocalesPorID(id).ConfigureAwait(false);
            if (local == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
                ViewData["respuesta"] = new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = "El objeto Local se quiere editar no existe." };
                return View("vEditarLocal", new Locale() { IdLocal = Guid.Empty });
            }
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Cargo");
            ViewData["respuesta"] = new clResponse<Cargo>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vEditarCargo", local);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarLocal(Guid IdLocal, [Bind("IdLocal,Nombre,Descripcion,Cubiculo")] Locale local)
        {
            if (local != null)
            {
                if (IdLocal != local.IdLocal)
                {
                    ViewData["respuesta"] = new clResponse<Locale> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Local a editar." };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
                    return View("vEditarLocal", local);
                }
                var iDictionaryError = validacionesRepositorio.EspaciosEnBlanco(local.Nombre, "Nombre");
                if (iDictionaryError.Count > 0)
                {
                    foreach (var item in iDictionaryError)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (localRepositorio.SiExisteNombreDelLocalAModificar(local.Nombre, local.IdLocal))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a insertar como modificación ya existe.");
                }

                if (!cubiculoRepositorio.CubiculoExists(local.Cubiculo??Guid.Empty))
                {
                    ModelState.AddModelError("Cubiculo", "El Cubículo seleccionado para el local a insertar como modificación no existe.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var mensaje = string.Empty;
                        var resultadoCargo = await localRepositorio.ModificarLocal(local).ConfigureAwait(false);
                        if (resultadoCargo.SiSatisfactorio)
                        {
                            clGlobal.siNo = resultadoCargo.SiSatisfactorio;
                            clGlobal.objMensaje = resultadoCargo.Mensaje;
                            return RedirectToAction(nameof(EditarLocal), new { id = local.IdLocal });
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoCargo;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
                            return View("vEditarLocal", local);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!localRepositorio.LocalExists(local.IdLocal))
                        {
                            ViewData["respuesta"] = new clResponse<Locale> { SiSatisfactorio = false, Mensaje = "El Local a editar no existe." };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
                            return View("vEditarLocal", local);
                        }
                        else
                        {
                            ViewData["respuesta"] = new clResponse<Locale> { SiSatisfactorio = false, Mensaje = ex.Message };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
                            return View("vEditarLocal", local);
                        }
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Locale> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Local a editar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
            return View("vEditarLocal", local);
        }
        // GET: CCargos/Delete/5
        public async Task<IActionResult> EliminarLocal(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var local = await localRepositorio.SeleccionarLocalesPorID(id).ConfigureAwait(false);
            if (local == null)
            {
                return NotFound();
            }
            return View(local);
        }
        [HttpPost, ActionName("EliminarLocal")]
        public async Task<JsonResult> ConfirmarEliminarLocal(Guid id)
        {
            try
            {
                var local = await localRepositorio.SeleccionarLocalesPorID(id).ConfigureAwait(false);
                var resultadoLocal = await localRepositorio.EliminarLocal(local).ConfigureAwait(false);
                var result = new
                {
                    siNo = resultadoLocal.SiSatisfactorio,
                    objMensaje = resultadoLocal.Mensaje
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
                nombre = "Locales",
                ruta = new List<string>() { "Locales", "ctLocal" }
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
