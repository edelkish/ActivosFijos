using ActivosFijos.Clases;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using ActivosFijos.Repositorios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> CrearLocal()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Local");
            ViewData["respuesta"] = new clResponse<Locale>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre");
            return View("vCrearLocal");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarLocal([Bind("Nombre,Descripcion,Cubiculo")] Locale local)
        {
            if (local != null)
            {
                var iDictionaryModel = validacionesRepositorio.EspaciosEnBlanco(local.Nombre, "Nombre");
                if (iDictionaryModel.Count == 0)
                {
                    if (localRepositorio.SiExisteNombreDelLocal(local.Nombre))
                    {
                        ModelState.AddModelError("Nombre", "El Nombre del local a insertar ya existe.");
                    }
                }
                else
                {
                    foreach (var item in iDictionaryModel)
                        ModelState.AddModelError(item.Key, item.Value);
                }

                if (local.Cubiculo == null)
                {
                    ModelState.AddModelError("Cubiculo", "El Cub\u00edculo es obligatorio.");
                }
                else if (!cubiculoRepositorio.CubiculoExists(local.Cubiculo.Value))
                {
                    ModelState.AddModelError("Cubiculo", "El Cub\u00edculo seleccionado no existe.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        local.IdLocal = Guid.NewGuid();
                        var resultadoLocal = await localRepositorio.InsertarLocal(local).ConfigureAwait(false);
                        if (resultadoLocal.SiSatisfactorio)
                        {
                            var mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoLocal.Mensaje;
                            return RedirectToAction(nameof(CrearLocal));
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoLocal;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Local");
                            ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre", local.Cubiculo);
                            return View("vCrearLocal", local);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<Locale> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear Local");
                        ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre", local.Cubiculo);
                        return View("vCrearLocal", local);
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Locale> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Local a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Local");
            ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre", local?.Cubiculo);
            return View("vCrearLocal", local);
        }
        public async Task<IActionResult> EditarLocal(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
                ViewData["respuesta"] = new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = "El objeto Local que se quiere editar es incorrecto o no existe." };
                ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre");
                return View("vEditarLocal", new Locale() { IdLocal = Guid.Empty });
            }
            var local = await localRepositorio.SeleccionarLocalesPorID(id).ConfigureAwait(false);
            if (local == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
                ViewData["respuesta"] = new clResponse<Locale>() { SiSatisfactorio = false, Mensaje = "El objeto Local que se quiere editar no existe." };
                ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre");
                return View("vEditarLocal", new Locale() { IdLocal = Guid.Empty });
            }
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
            ViewData["respuesta"] = new clResponse<Locale>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre", local.Cubiculo);
            return View("vEditarLocal", local);
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
                    ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre", local.Cubiculo);
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
                    ModelState.AddModelError("Nombre", "El Nombre del local a modificar ya existe.");
                }

                if (local.Cubiculo == null)
                {
                    ModelState.AddModelError("Cubiculo", "El Cub\u00edculo es obligatorio.");
                }
                else if (!cubiculoRepositorio.CubiculoExists(local.Cubiculo.Value))
                {
                    ModelState.AddModelError("Cubiculo", "El Cub\u00edculo seleccionado no existe.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var resultadoLocal = await localRepositorio.ModificarLocal(local).ConfigureAwait(false);
                        if (resultadoLocal.SiSatisfactorio)
                        {
                            clGlobal.siNo = resultadoLocal.SiSatisfactorio;
                            clGlobal.objMensaje = resultadoLocal.Mensaje;
                            return RedirectToAction(nameof(EditarLocal), new { id = local.IdLocal });
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoLocal;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
                            ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre", local.Cubiculo);
                            return View("vEditarLocal", local);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!localRepositorio.LocalExists(local.IdLocal))
                        {
                            ViewData["respuesta"] = new clResponse<Locale> { SiSatisfactorio = false, Mensaje = "El Local a editar no existe." };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
                            ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre", local.Cubiculo);
                            return View("vEditarLocal", local);
                        }
                        else
                        {
                            ViewData["respuesta"] = new clResponse<Locale> { SiSatisfactorio = false, Mensaje = ex.Message };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
                            ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre", local.Cubiculo);
                            return View("vEditarLocal", local);
                        }
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Locale> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Local a editar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Local");
            ViewData["IdCubiculo"] = new SelectList(await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false), "IdCubiculo", "Nombre", local?.Cubiculo);
            return View("vEditarLocal", local);
        }
        [HttpPost, ActionName("EliminarLocal")]
        public async Task<JsonResult> ConfirmarEliminarLocal(Guid id)
        {
            try
            {
                var local = await localRepositorio.SeleccionarLocalesPorID(id).ConfigureAwait(false);
                if (local == null)
                {
                    return Json(new { siNo = false, objMensaje = "El Local a eliminar no existe." });
                }

                if (await localRepositorio.SiTieneDependencias(id).ConfigureAwait(false))
                {
                    return Json(new { siNo = false, objMensaje = "No se puede eliminar el Local porque tiene puestos, activos fijos o historial asociado. Desasigne primero los elementos dependientes." });
                }

                var resultadoLocal = await localRepositorio.EliminarLocal(local).ConfigureAwait(false);
                return Json(new { siNo = resultadoLocal.SiSatisfactorio, objMensaje = resultadoLocal.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { siNo = false, objMensaje = ex.Message });
            }
        }
        private List<clBreadCumb> LlenarBreadsCumbs(string titulo)
        {
            List<clBreadCumb> prueba = new List<clBreadCumb>();
            clBreadCumb objBreadsCumbs = new clBreadCumb()
            {
                nombre = "Locales",
                ruta = new List<string>() { "Locales", "ctLocales" }
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
