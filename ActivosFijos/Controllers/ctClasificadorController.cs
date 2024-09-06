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
    public class ctClasificadorController : Controller
    {
        private readonly repoClasificador clasificadorRepositorio;
        private readonly repoValidaciones validacionesRepositorio;
        private readonly aftContext _context;
        public ctClasificadorController(repoClasificador clasificadorRepositorio, repoValidaciones validacionesRepositorio, aftContext context)
        {
            this.clasificadorRepositorio = clasificadorRepositorio;
            this.validacionesRepositorio = validacionesRepositorio;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public IActionResult Clasificadores()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Lista de Clasificadores");
            return View("vClasificador");
        }
        [HttpPost]
        public async Task<IActionResult> ListadoDeClasificadores()
        {
            List<Clasificador> clasificadorList = await clasificadorRepositorio.ObtenerListadoClasificador().ConfigureAwait(false);
            
            return PartialView("_vpTablaClasificador", clasificadorList);
        }

        public async Task<IActionResult> DetallesClasificador(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var clasificador = await clasificadorRepositorio.SeleccionarClasificadorPorID(id).ConfigureAwait(false);
            if (clasificador == null)
            {
                return NotFound();
            }
            return View(clasificador);
        }

        public IActionResult CrearClasificador()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Clasificador");
            ViewData["respuesta"] = new clResponse<Clasificador>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vCrearClasificador");
        }
        // POST: CCargos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarClasificador([Bind("Nombre,Descripcion,IdClasificador")] Clasificador clasificador)
        {
            var respuesta = new clResponse<bool> { SiSatisfactorio = false, Mensaje = string.Empty };
            if (clasificador != null)
            {
                var iDictionaryIntegrantes = validacionesRepositorio.EspaciosEnBlanco(clasificador.Nombre, "Nombre");
                if (iDictionaryIntegrantes.Count > 0)
                {
                    foreach (var item in iDictionaryIntegrantes)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (clasificadorRepositorio.SiExisteNombreDelClasificador(clasificador.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a insertar ya existe.");
                }
                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        clasificador.IdClasificador = Guid.NewGuid();
                        var resultadoClasificador = await clasificadorRepositorio.InsertarClasificador(clasificador).ConfigureAwait(false);
                        if (resultadoClasificador.SiSatisfactorio)
                        {
                            mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoClasificador.Mensaje;
                            return RedirectToAction(nameof(CrearClasificador));
                        }
                        else
                        {
                            mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoClasificador.Mensaje)}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            ViewData["respuesta"] = resultadoClasificador;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Clasificador");
                            return View("vCrearClasificador", clasificador);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<bool> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear Clasificador");
                        return View("vCrearClasificador", clasificador);
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Clasificador> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Clasificador a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Clasificador");
            return View("vCrearClasificador", clasificador);
        }
        // GET: CCargos/Edit/5
        public async Task<IActionResult> EditarClasificador(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Clasificador");
                ViewData["respuesta"] = new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El objeto Clasificador que se quiere editar es incorrecto o no existe." };
                return View("vEditarClasificador", new Clasificador() { IdClasificador = Guid.Empty });
            }
            var clasificador = await clasificadorRepositorio.SeleccionarClasificadorPorID(id).ConfigureAwait(false);
            if (clasificador == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Clasificador");
                ViewData["respuesta"] = new clResponse<Clasificador>() { SiSatisfactorio = false, Mensaje = "El objeto Clasificador se quiere editar no existe." };
                return View("vEditarClasificador", new Clasificador() { IdClasificador = Guid.Empty });
            }
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Clasificador");
            ViewData["respuesta"] = new clResponse<Clasificador>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vEditarClasificador", clasificador);
        }
        // POST: CCargos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarClasificador(Guid IdClasificador, [Bind("Nombre,Descripcion,IdClasificador")] Clasificador clasificador)
        {
            if (clasificador != null)
            {
                if (IdClasificador != clasificador.IdClasificador)
                {
                    ViewData["respuesta"] = new clResponse<Clasificador> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Clasificador a editar." };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Editar Clasificador");
                    return View("vEditarClasificador", clasificador);
                }
                var iDictionaryIntegrantes = validacionesRepositorio.EspaciosEnBlanco(clasificador.Nombre, "Nombre");
                if (iDictionaryIntegrantes.Count > 0)
                {
                    foreach (var item in iDictionaryIntegrantes)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (clasificadorRepositorio.SiExisteNombreDelClasificadorAModificar(clasificador.Nombre, clasificador.IdClasificador))
                {
                    ModelState.AddModelError("Nombre", "El Nombre a insertar como modificación ya existe.");
                }
                if (ModelState.IsValid)
                {
                    try
                    {
                        var mensaje = string.Empty;
                        var resultadoClasificador = await clasificadorRepositorio.ModificarClasificador(clasificador).ConfigureAwait(false);
                        if (resultadoClasificador.SiSatisfactorio)
                        {
                            clGlobal.siNo = resultadoClasificador.SiSatisfactorio;
                            clGlobal.objMensaje = resultadoClasificador.Mensaje;
                            return RedirectToAction(nameof(EditarClasificador), new { id = clasificador.IdClasificador });
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoClasificador;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Clasificador");
                            return View("vEditarClasificador", clasificador);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!clasificadorRepositorio.ClasificadorsExists(clasificador.IdClasificador))
                        {
                            ViewData["respuesta"] = new clResponse<Clasificador> { SiSatisfactorio = false, Mensaje = "El Clasificador a editar no existe." };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Clasificador");
                            return View("vEditarClasificador", clasificador);
                        }
                        else
                        {
                            ViewData["respuesta"] = new clResponse<Clasificador> { SiSatisfactorio = false, Mensaje = ex.Message };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Clasificador");
                            return View("vEditarClasificador", clasificador);
                        }
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<Clasificador> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Clasificador a editar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Clasificador");
            return View("vEditarClasificador", clasificador);
        }
        // GET: CCargos/Delete/5
        public async Task<IActionResult> EliminarClasificador(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var cargos = await clasificadorRepositorio.SeleccionarClasificadorPorID(id).ConfigureAwait(false);
            if (cargos == null)
            {
                return NotFound();
            }
            return View(cargos);
        }
        [HttpPost, ActionName("EliminarClasificador")]
        public async Task<JsonResult> ConfirmarEliminarClasificador(Guid id)
        {
            try
            {
                var clasificador = await clasificadorRepositorio.SeleccionarClasificadorPorID(id).ConfigureAwait(false);
                var resultadoCargo = await clasificadorRepositorio.EliminarClasificador(clasificador).ConfigureAwait(false);
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
                nombre = "Clasificadores",
                ruta = new List<string>() { "Clasificador", "ctClasificador" }
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
