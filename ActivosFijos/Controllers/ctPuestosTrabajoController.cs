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
    public class ctPuestosTrabajoController : Controller
    {
        private readonly repoPuestosTrabajo puestoTrabajoRepositorio;
        private readonly repoLocales localRepositorio;
        private readonly repoValidaciones validacionesRepositorio;
        private readonly aftContext _context;
        public ctPuestosTrabajoController(repoPuestosTrabajo puestoTrabajoRepositorio, repoLocales localRepositorio, repoValidaciones validacionesRepositorio, aftContext context)
        {
            this.puestoTrabajoRepositorio = puestoTrabajoRepositorio;
            this.localRepositorio = localRepositorio;
            this.validacionesRepositorio = validacionesRepositorio;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public IActionResult PuestosTrabajo()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Lista de puestos de trabajo");
            return View("vPuestosTrabajo");
        }
        [HttpPost]
        public async Task<IActionResult> ListadoDePuestos()
        {
            List<PuestoTrabajo> puestosList = await puestoTrabajoRepositorio.ObtenerListadoPuestos().ConfigureAwait(false);
            return PartialView("_vpTablaPuestosTrabajo", puestosList);
        }
        public async Task<IActionResult> CrearPuestoTrabajo()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear puesto de trabajo");
            ViewData["respuesta"] = new clResponse<PuestoTrabajo>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            ViewData["IdLocal"] = new SelectList(await localRepositorio.ObtenerListadoLocales().ConfigureAwait(false), "IdLocal", "Nombre");
            return View("vCrearPuestoTrabajo");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPuestoTrabajo([Bind("Nombre,Descripcion,IdLocal,IdPuesto")] PuestoTrabajo puestoTrabajo)
        {
            if (puestoTrabajo != null)
            {
                var iDictionaryModel = validacionesRepositorio.EspaciosEnBlanco(puestoTrabajo.Nombre, "Nombre");
                if (iDictionaryModel.Count == 0)
                {
                    if (puestoTrabajoRepositorio.SiExisteNombreDelPuesto(puestoTrabajo.Nombre, puestoTrabajo.IdLocal))
                    {
                        ModelState.AddModelError("Nombre", "El Nombre del puesto de trabajo a insertar ya existe en ese local.");
                    }
                }
                else
                {
                    foreach (var item in iDictionaryModel)
                        ModelState.AddModelError(item.Key, item.Value);
                }

                if (!localRepositorio.LocalExists(puestoTrabajo.IdLocal))
                {
                    ModelState.AddModelError("IdLocal", "El Local seleccionado para el puesto de trabajo no existe.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        puestoTrabajo.IdPuesto = Guid.NewGuid();
                        var resultado = await puestoTrabajoRepositorio.InsertarPuestoTrabajo(puestoTrabajo).ConfigureAwait(false);
                        if (resultado.SiSatisfactorio)
                        {
                            var mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultado.Mensaje;
                            return RedirectToAction(nameof(CrearPuestoTrabajo));
                        }
                        else
                        {
                            ViewData["respuesta"] = resultado;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear puesto de trabajo");
                            ViewData["IdLocal"] = new SelectList(await localRepositorio.ObtenerListadoLocales().ConfigureAwait(false), "IdLocal", "Nombre", puestoTrabajo.IdLocal);
                            return View("vCrearPuestoTrabajo", puestoTrabajo);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<PuestoTrabajo> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear puesto de trabajo");
                        ViewData["IdLocal"] = new SelectList(await localRepositorio.ObtenerListadoLocales().ConfigureAwait(false), "IdLocal", "Nombre", puestoTrabajo.IdLocal);
                        return View("vCrearPuestoTrabajo", puestoTrabajo);
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<PuestoTrabajo> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Puesto de Trabajo a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear puesto de trabajo");
            ViewData["IdLocal"] = new SelectList(await localRepositorio.ObtenerListadoLocales().ConfigureAwait(false), "IdLocal", "Nombre", puestoTrabajo != null ? puestoTrabajo.IdLocal : Guid.Empty);
            return View("vCrearPuestoTrabajo", puestoTrabajo);
        }
        public async Task<IActionResult> EditarPuestoTrabajo(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar puesto de trabajo");
                ViewData["respuesta"] = new clResponse<PuestoTrabajo>() { SiSatisfactorio = false, Mensaje = "El objeto Puesto de Trabajo que se quiere editar es incorrecto o no existe." };
                return View("vEditarPuestoTrabajo", new PuestoTrabajo() { IdPuesto = Guid.Empty });
            }
            var puestoTrabajo = await puestoTrabajoRepositorio.SeleccionarPuestoPorID(id).ConfigureAwait(false);
            if (puestoTrabajo == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar puesto de trabajo");
                ViewData["respuesta"] = new clResponse<PuestoTrabajo>() { SiSatisfactorio = false, Mensaje = "El objeto Puesto de Trabajo que se quiere editar no existe." };
                return View("vEditarPuestoTrabajo", new PuestoTrabajo() { IdPuesto = Guid.Empty });
            }
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar puesto de trabajo");
            ViewData["respuesta"] = new clResponse<PuestoTrabajo>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            ViewData["IdLocal"] = new SelectList(await localRepositorio.ObtenerListadoLocales().ConfigureAwait(false), "IdLocal", "Nombre", puestoTrabajo.IdLocal);
            return View("vEditarPuestoTrabajo", puestoTrabajo);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarPuestoTrabajo(Guid IdPuesto, [Bind("Nombre,Descripcion,IdLocal,IdPuesto")] PuestoTrabajo puestoTrabajo)
        {
            if (puestoTrabajo != null)
            {
                if (IdPuesto != puestoTrabajo.IdPuesto)
                {
                    ViewData["respuesta"] = new clResponse<PuestoTrabajo> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Puesto de Trabajo a editar." };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Editar puesto de trabajo");
                    ViewData["IdLocal"] = new SelectList(await localRepositorio.ObtenerListadoLocales().ConfigureAwait(false), "IdLocal", "Nombre", puestoTrabajo.IdLocal);
                    return View("vEditarPuestoTrabajo", puestoTrabajo);
                }
                var iDictionaryIntegrantes = validacionesRepositorio.EspaciosEnBlanco(puestoTrabajo.Nombre, "Nombre");
                if (iDictionaryIntegrantes.Count > 0)
                {
                    foreach (var item in iDictionaryIntegrantes)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (puestoTrabajoRepositorio.SiExisteNombreDelPuestoAModificar(puestoTrabajo.Nombre, puestoTrabajo.IdLocal, puestoTrabajo.IdPuesto))
                {
                    ModelState.AddModelError("Nombre", "El Nombre del puesto de trabajo a insertar como modificación ya existe en ese local.");
                }

                if (!localRepositorio.LocalExists(puestoTrabajo.IdLocal))
                {
                    ModelState.AddModelError("IdLocal", "El Local seleccionado para el puesto de trabajo no existe.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var resultado = await puestoTrabajoRepositorio.ModificarPuestoTrabajo(puestoTrabajo).ConfigureAwait(false);
                        if (resultado.SiSatisfactorio)
                        {
                            clGlobal.siNo = resultado.SiSatisfactorio;
                            clGlobal.objMensaje = resultado.Mensaje;
                            return RedirectToAction(nameof(EditarPuestoTrabajo), new { id = puestoTrabajo.IdPuesto });
                        }
                        else
                        {
                            ViewData["respuesta"] = resultado;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar puesto de trabajo");
                            ViewData["IdLocal"] = new SelectList(await localRepositorio.ObtenerListadoLocales().ConfigureAwait(false), "IdLocal", "Nombre", puestoTrabajo.IdLocal);
                            return View("vEditarPuestoTrabajo", puestoTrabajo);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!puestoTrabajoRepositorio.PuestoTrabajoExists(puestoTrabajo.IdPuesto))
                        {
                            ViewData["respuesta"] = new clResponse<PuestoTrabajo> { SiSatisfactorio = false, Mensaje = "El Puesto de Trabajo a editar no existe." };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar puesto de trabajo");
                            ViewData["IdLocal"] = new SelectList(await localRepositorio.ObtenerListadoLocales().ConfigureAwait(false), "IdLocal", "Nombre", puestoTrabajo.IdLocal);
                            return View("vEditarPuestoTrabajo", puestoTrabajo);
                        }
                        else
                        {
                            ViewData["respuesta"] = new clResponse<PuestoTrabajo> { SiSatisfactorio = false, Mensaje = ex.Message };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar puesto de trabajo");
                            ViewData["IdLocal"] = new SelectList(await localRepositorio.ObtenerListadoLocales().ConfigureAwait(false), "IdLocal", "Nombre", puestoTrabajo.IdLocal);
                            return View("vEditarPuestoTrabajo", puestoTrabajo);
                        }
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<PuestoTrabajo> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Puesto de Trabajo a editar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar puesto de trabajo");
            ViewData["IdLocal"] = new SelectList(await localRepositorio.ObtenerListadoLocales().ConfigureAwait(false), "IdLocal", "Nombre", puestoTrabajo != null ? puestoTrabajo.IdLocal : Guid.Empty);
            return View("vEditarPuestoTrabajo", puestoTrabajo);
        }
        [HttpPost, ActionName("EliminarPuestoTrabajo")]
        public async Task<JsonResult> ConfirmarEliminarPuestoTrabajo(Guid id)
        {
            try
            {
                var puestoTrabajo = await puestoTrabajoRepositorio.SeleccionarPuestoPorID(id).ConfigureAwait(false);
                var resultado = await puestoTrabajoRepositorio.EliminarPuestoTrabajo(puestoTrabajo).ConfigureAwait(false);
                var result = new
                {
                    siNo = resultado.SiSatisfactorio,
                    objMensaje = resultado.Mensaje
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
                nombre = "Puestos de trabajo",
                ruta = new List<string>() { "Puestos de trabajo", "ctPuestosTrabajo" }
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