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
    public class ctCargosController : Controller
    {
        private readonly repoCargos cargoRepositorio;
        private readonly repoValidaciones validacionesRepositorio;
        private readonly aftContext _context;
        public ctCargosController(repoCargos cargoRepositorio, repoValidaciones validacionesRepositorio, aftContext context)
        {
            this.cargoRepositorio = cargoRepositorio;
            this.validacionesRepositorio = validacionesRepositorio;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public IActionResult Cargos()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Lista de Cargos");
            return View("vCargos");
        }
        [HttpPost]
        public async Task<IActionResult> ListadoDeCargos()
        {
            List<Cargo> cargosList = await cargoRepositorio.ObtenerListadoCargos().ConfigureAwait(false);
            
            return PartialView("_vpTablaCargos", cargosList);
        }
        // GET: CCargos/Details/5
        public async Task<IActionResult> DetallesCargos(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var cargos = await cargoRepositorio.SeleccionarCargosPorID(id).ConfigureAwait(false);
            if (cargos == null)
            {
                return NotFound();
            }
            return View(cargos);
        }
        // GET: CCargos/Create
        public IActionResult CrearCargo()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Cargo");
            ViewData["respuesta"] = new clResponse<Cargo>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vCrearCargo");
        }
        // POST: CCargos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCargo([Bind("Nombre,Descripcion,IdCargo")] Cargo cargos)
        {
            var respuesta = new clResponse<bool> { SiSatisfactorio = false, Mensaje = string.Empty };
            if (cargos != null)
            {
                var iDictionaryIntegrantes = validacionesRepositorio.EspaciosEnBlanco(cargos.Nombre, "Nombre");
                if (iDictionaryIntegrantes.Count > 0)
                {
                    foreach (var item in iDictionaryIntegrantes)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (cargoRepositorio.SiExisteNombreDelCargo(cargos.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a insertar ya existe.");
                }
                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        cargos.IdCargo = Guid.NewGuid();
                        var resultadoCargo = await cargoRepositorio.InsertarCargos(cargos).ConfigureAwait(false);
                        if (resultadoCargo.SiSatisfactorio)
                        {
                            mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoCargo.Mensaje;
                            return RedirectToAction(nameof(CrearCargo));
                        }
                        else
                        {
                            mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoCargo.Mensaje)}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            ViewData["respuesta"] = resultadoCargo;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Cargo");
                            return View("vCrearCargo", cargos);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<bool> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear Cargo");
                        return View("vCrearCargo", cargos);
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Cargo> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Cargo a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Cargo");
            return View("vCrearCargo", cargos);
        }
        // GET: CCargos/Edit/5
        public async Task<IActionResult> EditarCargo(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Cargo");
                ViewData["respuesta"] = new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "El objeto Cargo que se quiere editar es incorrecto o no existe." };
                return View("vEditarCargo", new Cargo() { IdCargo = Guid.Empty });
            }
            var cargos = await cargoRepositorio.SeleccionarCargosPorID(id).ConfigureAwait(false);
            if (cargos == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Cargo");
                ViewData["respuesta"] = new clResponse<Cargo>() { SiSatisfactorio = false, Mensaje = "El objeto Cargo se quiere editar no existe." };
                return View("vEditarCargo", new Cargo() { IdCargo = Guid.Empty });
            }
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Cargo");
            ViewData["respuesta"] = new clResponse<Cargo>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vEditarCargo", cargos);
        }
        // POST: CCargos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarCargo(Guid IdCargo, [Bind("Nombre,Descripcion,IdCargo")] Cargo cargos)
        {
            if (cargos != null)
            {
                if (IdCargo != cargos.IdCargo)
                {
                    ViewData["respuesta"] = new clResponse<Cargo> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Cargo a editar." };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Editar Cargo");
                    return View("vEditarCargo", cargos);
                }
                var iDictionaryIntegrantes = validacionesRepositorio.EspaciosEnBlanco(cargos.Nombre, "Nombre");
                if (iDictionaryIntegrantes.Count > 0)
                {
                    foreach (var item in iDictionaryIntegrantes)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (cargoRepositorio.SiExisteNombreDelCargoAModificar(cargos.Nombre, cargos.IdCargo))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a insertar como modificación ya existe.");
                }
                if (ModelState.IsValid)
                {
                    try
                    {
                        var mensaje = string.Empty;
                        var resultadoCargo = await cargoRepositorio.ModificarCargo(cargos).ConfigureAwait(false);
                        if (resultadoCargo.SiSatisfactorio)
                        {
                            clGlobal.siNo = resultadoCargo.SiSatisfactorio;
                            clGlobal.objMensaje = resultadoCargo.Mensaje;
                            return RedirectToAction(nameof(EditarCargo), new { id = cargos.IdCargo });
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoCargo;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Cargo");
                            return View("vEditarCargo", cargos);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!cargoRepositorio.CargosExists(cargos.IdCargo))
                        {
                            ViewData["respuesta"] = new clResponse<Cargo> { SiSatisfactorio = false, Mensaje = "El Cargo a editar no existe." };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Cargo");
                            return View("vEditarCargo", cargos);
                        }
                        else
                        {
                            ViewData["respuesta"] = new clResponse<Cargo> { SiSatisfactorio = false, Mensaje = ex.Message };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Cargo");
                            return View("vEditarCargo", cargos);
                        }
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Cargo> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Cargo a editar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Cargo");
            return View("vEditarCargo", cargos);
        }
        // GET: CCargos/Delete/5
        public async Task<IActionResult> EliminarCargo(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var cargos = await cargoRepositorio.SeleccionarCargosPorID(id).ConfigureAwait(false);
            if (cargos == null)
            {
                return NotFound();
            }
            return View(cargos);
        }
        [HttpPost, ActionName("EliminarCargo")]
        public async Task<JsonResult> ConfirmarEliminarCargo(Guid id)
        {
            try
            {
                var cargos = await cargoRepositorio.SeleccionarCargosPorID(id).ConfigureAwait(false);
                var resultadoCargo = await cargoRepositorio.EliminarCargo(cargos).ConfigureAwait(false);
                var result = new
                {
                    siNo = resultadoCargo.SiSatisfactorio,
                    objMensaje = resultadoCargo.Mensaje
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
                nombre = "Cargos",
                ruta = new List<string>() { "Cargos", "ctCargo" }
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
