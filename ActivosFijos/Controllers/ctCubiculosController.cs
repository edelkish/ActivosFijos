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
    public class ctCubiculosController : Controller
    {
        private readonly repoCubiculos cubiculoRepositorio;
        private readonly repoValidaciones validacionesRepositorio;
        private readonly aftContext _context;
        public ctCubiculosController(repoCubiculos cubiculoRepositorio, repoValidaciones validacionesRepositorio, aftContext context)
        {
            this.cubiculoRepositorio = cubiculoRepositorio;
            this.validacionesRepositorio = validacionesRepositorio;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public IActionResult Cubiculos()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Gestión de cubículos");
            return View("vCubiculos");
        }
        [HttpPost]
        public async Task<IActionResult> ListadoDeCubiculos()
        {
            List<Cubiculo> cubiculosList = await cubiculoRepositorio.ObtenerListadoCubiculos().ConfigureAwait(false);
            
            return PartialView("_vpTablaCubiculos", cubiculosList);
        }
        public IActionResult CrearCubiculo()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear cubículos");
            ViewData["respuesta"] = new clResponse<Cubiculo>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vCrearCubiculo");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCubiculo([Bind("Nombre,Apodo,Descripcion,IdCubiculo")] Cubiculo cubiculo)
        {
            var respuesta = new clResponse<bool> { SiSatisfactorio = false, Mensaje = string.Empty };
            if (cubiculo != null)
            {
                var iDictionaryModel = validacionesRepositorio.EspaciosEnBlanco(cubiculo.Nombre, "Nombre");
                if (iDictionaryModel.Count == 0)
                {
                    if (cubiculoRepositorio.SiExisteNombreDelCubiculo(cubiculo.Nombre))
                    {
                        ModelState.AddModelError("Nombre", "El Nombre a insertar ya existe.");
                    }
                }
                else
                {
                    foreach (var item in iDictionaryModel)
                        ModelState.AddModelError(item.Key, item.Value);
                }
 

                iDictionaryModel.Clear();
                iDictionaryModel = validacionesRepositorio.EspaciosEnBlanco(cubiculo.Apodo, "Apodo");
                if (iDictionaryModel.Count == 0)
                {
                    if (cubiculoRepositorio.SiExisteApodoDelCubiculo(cubiculo.Apodo))
                    {
                        ModelState.AddModelError("Apodo", "El apodo del cubículo a insertar ya existe.");
                    }
                }
                else
                {
                    foreach (var item in iDictionaryModel)
                        ModelState.AddModelError(item.Key, item.Value);
                }

                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        cubiculo.IdCubiculo = Guid.NewGuid();
                        var resultadoCubiculo = await cubiculoRepositorio.InsertarCubiculo(cubiculo).ConfigureAwait(false);
                        if (resultadoCubiculo.SiSatisfactorio)
                        {
                            mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoCubiculo.Mensaje;
                            return RedirectToAction(nameof(CrearCubiculo));
                        }
                        else
                        {
                            mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoCubiculo.Mensaje)}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            ViewData["respuesta"] = resultadoCubiculo;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear cubículo");
                            return View("vCrearCubículo", cubiculo);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<bool> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear cubículo");
                        return View("vCrearCubiculo", cubiculo);
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Cubiculo> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Cubículo a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear cubículo");
            return View("vCrearCubiculo", cubiculo);
        }
        // GET: CCargos/Edit/5
        public async Task<IActionResult> EditarCubiculo(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar cubículo");
                ViewData["respuesta"] = new clResponse<Cubiculo>() { SiSatisfactorio = false, Mensaje = "El objeto Cubiculo que se quiere editar es incorrecto o no existe." };
                return View("vEditarCubiculo", new Cubiculo() { IdCubiculo = Guid.Empty });
            }
            var cubiculo = await cubiculoRepositorio.SeleccionarCubiculosPorID(id).ConfigureAwait(false);
            if (cubiculo == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar cubículo");
                ViewData["respuesta"] = new clResponse<Cubiculo>() { SiSatisfactorio = false, Mensaje = "El objeto Cubículo que se quiere editar no existe." };
                return View("vEditarCubiculo", new Cubiculo() { IdCubiculo = Guid.Empty });
            }
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar cubículo");
            ViewData["respuesta"] = new clResponse<Cubiculo>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vEditarCubiculo", cubiculo);
        }
        // POST: CCargos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarCubiculo(Guid IdCubiculo, [Bind("Nombre,Apodo,Descripcion,IdCubiculo")] Cubiculo cubiculo)
        {
            if (cubiculo != null)
            {
                if (IdCubiculo != cubiculo.IdCubiculo)
                {
                    ViewData["respuesta"] = new clResponse<Cubiculo> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Cubículo a editar." };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Editar cubículo");
                    return View("vEditarCubiculo", cubiculo);
                }
                var iDictionaryIntegrantes = validacionesRepositorio.EspaciosEnBlanco(cubiculo.Nombre, "Nombre");
                if (iDictionaryIntegrantes.Count > 0)
                {
                    foreach (var item in iDictionaryIntegrantes)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (cubiculoRepositorio.SiExisteNombreDelCubiculoAModificar(cubiculo.Nombre, cubiculo.IdCubiculo))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a insertar como modificación ya existe.");
                }

                iDictionaryIntegrantes.Clear();
                iDictionaryIntegrantes = validacionesRepositorio.EspaciosEnBlanco(cubiculo.Apodo, "Apodo");
                if (iDictionaryIntegrantes.Count > 0)
                {
                    foreach (var item in iDictionaryIntegrantes)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (cubiculoRepositorio.SiExisteApodoDelCubiculoAModificar(cubiculo.Apodo, cubiculo.IdCubiculo))
                {
                    ModelState.AddModelError("Apodo", "El apodo del cubículo a insertar como modificación ya existe.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var mensaje = string.Empty;
                        var resultadoCubiculo = await cubiculoRepositorio.ModificarCubiculo(cubiculo).ConfigureAwait(false);
                        if (resultadoCubiculo.SiSatisfactorio)
                        {
                            clGlobal.siNo = resultadoCubiculo.SiSatisfactorio;
                            clGlobal.objMensaje = resultadoCubiculo.Mensaje;
                            return RedirectToAction(nameof(EditarCubiculo), new { id = cubiculo.IdCubiculo });
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoCubiculo;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar cubículo");
                            return View("vEditarCubiculo", cubiculo);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!cubiculoRepositorio.CubiculoExists(cubiculo.IdCubiculo))
                        {
                            ViewData["respuesta"] = new clResponse<Cubiculo> { SiSatisfactorio = false, Mensaje = "El cubículo a editar no existe." };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar cubículo");
                            return View("vEditarCubiculo", cubiculo);
                        }
                        else
                        {
                            ViewData["respuesta"] = new clResponse<Cubiculo> { SiSatisfactorio = false, Mensaje = ex.Message };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar cubículo");
                            return View("vEditarCubiculo", cubiculo);
                        }
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Cubiculo> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Cubículo a editar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar cubículo");
            return View("vEditarCubiculo", cubiculo);
        }
        // GET: CCargos/Delete/5
        public async Task<IActionResult> EliminarCubiculo(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var cubiculo = await cubiculoRepositorio.SeleccionarCubiculosPorID(id).ConfigureAwait(false);
            if (cubiculo == null)
            {
                return NotFound();
            }
            return View(cubiculo);
        }
        [HttpPost, ActionName("EliminarCubiculo")]
        public async Task<JsonResult> ConfirmarEliminarCubiculo(Guid id)
        {
            try
            {
                var cubiculo = await cubiculoRepositorio.SeleccionarCubiculosPorID(id).ConfigureAwait(false);
                var resultadoCubiculo = await cubiculoRepositorio.EliminarCubiculo(cubiculo).ConfigureAwait(false);
                var result = new
                {
                    siNo = resultadoCubiculo.SiSatisfactorio,
                    objMensaje = resultadoCubiculo.Mensaje
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
                nombre = "Cubículos",
                ruta = new List<string>() { "Cubículos", "ctCubiculos" }
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
