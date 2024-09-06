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
using ActivosFijos.Models.Vistas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ActivosFijos.Controllers
{
    [Authorize(AuthenticationSchemes = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin_web, gest_nom")]
    public class ctResponsablesController : Controller
    {
        private readonly repoResponsables responsableRepositorio;
        private readonly repoValidaciones validacionesRepositorio;
        private readonly repoCargos cargosRepositorio;
        private readonly repoAreasDeResponsabilidad areaRepositorio;
        private readonly aftContext _context;

        public ctResponsablesController(repoResponsables responsableRepositorio, repoValidaciones validacionesRepositorio, repoCargos cargosRepositorio, repoAreasDeResponsabilidad areaRepositorio, aftContext context)
        {
            this.responsableRepositorio = responsableRepositorio;
            this.validacionesRepositorio = validacionesRepositorio;
            this.cargosRepositorio = cargosRepositorio;
            this.areaRepositorio = areaRepositorio;
            _context = context;
        }

        // GET: ctResponsables
        public async Task<IActionResult> Responsables()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Lista de Responsables");
            return View("vResponsables");
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeResponsables()
        {
            try
            {
                List<Responsable> responsableList = await responsableRepositorio.ObtenerListadoResponsables();

                return PartialView("_vpTablaResponsables", responsableList);
            }
            catch (Exception ex)
            {                
                return PartialView("_vpTablaResponsables", new List<Responsable>());
            }            
        }

        // GET: ctResponsables/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var responsable = await _context.Responsables
                .Include(r => r.CargoNavigation)
                .FirstOrDefaultAsync(m => m.IdResponsable == id);
            if (responsable == null)
            {
                return NotFound();
            }

            return View(responsable);
        }

        // GET: ctResponsables/Create
        public IActionResult CrearResponsable()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Responsable");
            ViewData["respuesta"] = new clResponse<Responsable>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };

            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            ViewData["Cargos"] = new SelectList(_context.Cargos.OrderBy(c => c.Nombre), "IdCargo", "Nombre");
            ViewData["Areas"] = new SelectList(_context.AreaDeResponsabilidads.OrderBy(c => c.Nombre), "IdArea", "Nombre");
            return View("vCrearResponsable", new Responsable());
        }

        // POST: ctResponsables/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarResponsable([Bind("Nombre,Apellidos,Cargo,IdResponsable,AreaResponsabilidad,ResponsableDelArea,ResponsableDeEconomia")] Responsable responsable)
        {
            //string keyAreas = HttpContext.Request.Form["centroC"];
            //string[] areas = responsable.IdAreasResponsables;//.Split(',');
            var respuesta = new clResponse<bool> { SiSatisfactorio = false, Mensaje = string.Empty };
            #region Campo Nombre
            var iDictionaryReponsables = validacionesRepositorio.EspaciosEnBlanco(responsable.Nombre, "Nombre");
            if (iDictionaryReponsables.Count > 0)
            {
                foreach (var item in iDictionaryReponsables)
                    ModelState.AddModelError(item.Key, item.Value);
            }
            else if (responsableRepositorio.SiExisteNombreApellidosDelResponsable(responsable.Nombre, responsable.Apellidos))
            {
                ModelState.AddModelError("Nombre", "El nombre completo (Nombre y Apellidos) del responsable a insertar ya está en uso. Favor de rectificarlo");
            }
            #endregion
            #region Campo Apellidos
            iDictionaryReponsables = validacionesRepositorio.EspaciosEnBlanco(responsable.Apellidos, "Apellidos");
            if (iDictionaryReponsables.Count > 0)
            {
                foreach (var item in iDictionaryReponsables)
                    ModelState.AddModelError(item.Key, item.Value);
            }
            #endregion
            #region Campo Cargo
            iDictionaryReponsables = new Dictionary<string, string>();
            if ((String.IsNullOrEmpty(responsable.Cargo.ToString())) || (responsable.Cargo.Equals(Guid.Empty)))
            {
                ModelState.AddModelError("Cargo", "El cargo seleccionado para crear el Responsable no puede estar en blanco. Favor de rectificarlo");
            }
            else if (!cargosRepositorio.SiExisteElCargoConElID(responsable.Cargo))
            {
                ModelState.AddModelError("Cargo", "El cargo seleccionado para crear el Responsable no existe. Favor de rectificarlo");
            }
            #endregion
            #region Campo Area de Responsabilidad
            iDictionaryReponsables = new Dictionary<string, string>();
            if ((String.IsNullOrEmpty(responsable.AreaResponsabilidad.ToString())) || (responsable.AreaResponsabilidad.Equals(Guid.Empty)))
            {
                ModelState.AddModelError("AreaResponsabilidad", "El área seleccionada para crear el Responsable no puede estar en blanco. Favor de rectificarlo");
            }
            else if (!areaRepositorio.SiExisteElAreaConElID(responsable.AreaResponsabilidad))
            {
                ModelState.AddModelError("AreaResponsabilidad", "El área seleccionada para crear el Responsable no existe. Favor de rectificarlo");
            }
            #endregion

            if (ModelState.IsValid)
            {
                var mensaje = string.Empty;
                try
                {
                    responsable.IdResponsable = Guid.NewGuid();
                    var resultadoResponsable = await responsableRepositorio.InsertarResponsables(responsable).ConfigureAwait(false);
                    if (resultadoResponsable.SiSatisfactorio)
                    {
                        mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                        if (!String.IsNullOrEmpty(mensaje))
                            this.TempData["Mensaje"] = mensaje;
                        clGlobal.siNo = true;
                        clGlobal.objMensaje = resultadoResponsable.Mensaje;
                        return RedirectToAction(nameof(CrearResponsable));
                    }
                    else
                    {
                        mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoResponsable.Mensaje)}";
                        if (!String.IsNullOrEmpty(mensaje))
                            this.TempData["Mensaje"] = mensaje;
                        ViewData["respuesta"] = resultadoResponsable;
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear Responsable");
                        ViewData["Cargos"] = new SelectList(_context.Cargos.OrderBy(c => c.Nombre), "IdCargo", "Nombre", responsable.Cargo);
                        ViewData["Areas"] = new SelectList(_context.AreaDeResponsabilidads.OrderBy(c => c.Nombre), "IdArea", "Nombre", responsable.AreaResponsabilidad);
                        return View("vCrearResponsable", responsable);
                    }
                }
                catch (Exception ex)
                {
                    ViewData["respuesta"] = new clResponse<bool> { SiSatisfactorio = false, Mensaje = ex.Message };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Crear Responsable");
                    ViewData["Cargos"] = new SelectList(_context.Cargos.OrderBy(c => c.Nombre), "IdCargo", "Nombre", responsable.Cargo);
                    ViewData["Areas"] = new SelectList(_context.AreaDeResponsabilidads.OrderBy(c => c.Nombre), "IdArea", "Nombre", responsable.AreaResponsabilidad);
                    return View("vCrearResponsable", responsable);
                }
            }
            ViewData["Cargos"] = new SelectList(_context.Cargos.OrderBy(c => c.Nombre), "IdCargo", "Nombre", responsable.Cargo);
            ViewData["Areas"] = new SelectList(_context.AreaDeResponsabilidads.OrderBy(c => c.Nombre), "IdArea", "Nombre", responsable.AreaResponsabilidad);
            ViewData["respuesta"] = new clResponse<Responsable> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Responsable a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Responsable");
            return View("vCrearResponsable", responsable);
        }

        // GET: ctResponsables/Edit/5
        public async Task<IActionResult> EditarResponsable(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Responsable");
                ViewData["respuesta"] = new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "El objeto Responsable que se quiere editar es incorrecto o no existe." };
                return View("vEditarResponsable", new Responsable() { IdResponsable = Guid.Empty });
            }

            var responsable = await responsableRepositorio.SeleccionarResponsablesPorID(id).ConfigureAwait(false);
            if (responsable == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Responsable");
                ViewData["respuesta"] = new clResponse<Responsable>() { SiSatisfactorio = false, Mensaje = "El objeto Responsable se quiere editar no existe." };

                return View("vEditarResponsable", new Responsable() { IdResponsable = Guid.Empty });
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Responsable");
            ViewData["respuesta"] = new clResponse<Responsable>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };

            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            ViewData["Cargos"] = new SelectList(_context.Cargos.OrderBy(c => c.Nombre), "IdCargo", "Nombre");
            ViewData["Areas"] = new SelectList(_context.AreaDeResponsabilidads.OrderBy(c => c.Nombre), "IdArea", "Nombre");

            return View("vEditarResponsable", responsable);
        }

        // POST: ctResponsables/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarResponsable(Guid IdResponsable, [Bind("Nombre,Apellidos,Cargo,IdResponsable,AreaResponsabilidad,ResponsableDelArea,ResponsableDeEconomia")] Responsable responsable)
        {
            if (IdResponsable != responsable.IdResponsable)
            {
                ModelState.AddModelError("Nombre", "El REesponsable que se quiere editar es incorrecto.");
            }
            else
            {
                var respuesta = new clResponse<Responsable> { SiSatisfactorio = false, Mensaje = string.Empty };
                #region Campo Nombre
                var iDictionaryReponsables = validacionesRepositorio.EspaciosEnBlanco(responsable.Nombre, "Nombre");
                if (iDictionaryReponsables.Count > 0)
                {
                    foreach (var item in iDictionaryReponsables)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (responsableRepositorio.SiExisteNombreApellidosDelResponsableAModificar(responsable.Nombre, responsable.Apellidos, responsable.IdResponsable))
                {
                    ModelState.AddModelError("Nombre", "El nombre completo (Nombre y Apellidos) del responsable que se quiere editar ya está en uso. Favor de rectificarlo");
                }
                #endregion
                #region Campo Apellidos
                iDictionaryReponsables = validacionesRepositorio.EspaciosEnBlanco(responsable.Apellidos, "Apellidos");
                if (iDictionaryReponsables.Count > 0)
                {
                    foreach (var item in iDictionaryReponsables)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                #endregion
                #region Campo Cargo
                iDictionaryReponsables = new Dictionary<string, string>();
                if ((String.IsNullOrEmpty(responsable.Cargo.ToString())) || (responsable.Cargo.Equals(string.Empty)))
                {
                    ModelState.AddModelError("Cargo", "El cargo seleccionado para el Responsable a editar no puede estar en blanco. Favor de rectificarlo");
                }
                else if (!cargosRepositorio.SiExisteElCargoConElID(responsable.Cargo))
                {
                    ModelState.AddModelError("Cargo", "El cargo seleccionado para el Responsable a editar no existe. Favor de rectificarlo");
                }
                #endregion
                #region Campo Area de Responsabilidad
                iDictionaryReponsables = new Dictionary<string, string>();
                if ((String.IsNullOrEmpty(responsable.AreaResponsabilidad.ToString())) || (responsable.AreaResponsabilidad.Equals(Guid.Empty)))
                {
                    ModelState.AddModelError("AreaResponsabilidad", "El área seleccionada para el Responsable a editar no puede estar en blanco. Favor de rectificarlo");
                }
                else if (!areaRepositorio.SiExisteElAreaConElID(responsable.AreaResponsabilidad))
                {
                    ModelState.AddModelError("AreaResponsabilidad", "El área seleccionada para el Responsable a editar no existe. Favor de rectificarlo");
                }
                #endregion                                
            }

            if (ModelState.IsValid)
            {
                var mensaje = string.Empty;
                try
                {
                    var resultadoResponsable = await responsableRepositorio.ModificarResponsable(responsable).ConfigureAwait(false);
                    if (resultadoResponsable.SiSatisfactorio)
                    {
                        mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                        if (!String.IsNullOrEmpty(mensaje))
                            this.TempData["Mensaje"] = mensaje;
                        clGlobal.siNo = true;
                        clGlobal.objMensaje = resultadoResponsable.Mensaje;
                        return RedirectToAction(nameof(EditarResponsable), new { id = responsable.IdResponsable });
                    }
                    else
                    {
                        mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoResponsable.Mensaje)}";
                        if (!String.IsNullOrEmpty(mensaje))
                            this.TempData["Mensaje"] = mensaje;
                        ViewData["respuesta"] = resultadoResponsable;
                        ViewData["breadsC"] = LlenarBreadsCumbs("Editar Responsable");
                        ViewData["Cargos"] = new SelectList(_context.Cargos.OrderBy(c => c.Nombre), "IdCargo", "Nombre", responsable.Cargo);
                        ViewData["Areas"] = new SelectList(_context.AreaDeResponsabilidads.OrderBy(c => c.Nombre), "IdArea", "Nombre", responsable.AreaResponsabilidad);
                        return View("vEditarResponsable", responsable);
                    }
                }
                catch (Exception ex)
                {
                    ViewData["respuesta"] = new clResponse<bool> { SiSatisfactorio = false, Mensaje = ex.Message };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Editar Responsable");
                    ViewData["Cargos"] = new SelectList(_context.Cargos.OrderBy(c => c.Nombre), "IdCargo", "Nombre", responsable.Cargo);
                    ViewData["Areas"] = new SelectList(_context.AreaDeResponsabilidads.OrderBy(c => c.Nombre), "IdArea", "Nombre", responsable.AreaResponsabilidad);
                    return View("vEditarResponsable", responsable);
                }
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Responsable");
            ViewData["respuesta"] = new clResponse<Responsable> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Responsable a editar." };
            ViewData["Cargos"] = new SelectList(_context.Cargos.OrderBy(c => c.Nombre), "IdCargo", "Nombre", responsable.Cargo);
            ViewData["Areas"] = new SelectList(_context.AreaDeResponsabilidads.OrderBy(c => c.Nombre), "IdArea", "Nombre", responsable.AreaResponsabilidad);
                        
            return View("vEditarResponsable", responsable);
        }

        // GET: ctResponsables/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var responsable = await _context.Responsables
                .Include(r => r.CargoNavigation)
                .FirstOrDefaultAsync(m => m.IdResponsable == id);
            if (responsable == null)
            {
                return NotFound();
            }

            return View(responsable);
        }

        // POST: ctResponsables/Delete/5
        [HttpPost, ActionName("EliminarResponsable")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminarResponsable(Guid id)
        {
            try
            {
                var responsable = await responsableRepositorio.SeleccionarResponsablesPorID(id);
                clResponse<Responsable> resultado = new clResponse<Responsable>();
                resultado = await responsableRepositorio.EliminarResponsable(responsable);
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
                nombre = "Responsables",
                ruta = new List<string>() { "Responsables", "ctResponsables" }
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
