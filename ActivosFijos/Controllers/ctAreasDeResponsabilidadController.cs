#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ActivosFijos.Models.EntidadesBD;
using ActivosFijos.Repositorios;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ActivosFijos.Controllers
{
    [Authorize(AuthenticationSchemes = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin_web, gest_nom")]
    public class ctAreasDeResponsabilidadController : Controller
    {
        private readonly aftContext _context;
        private readonly repoAreasDeResponsabilidad areasDeResponsabilidadRepositorio;
        private readonly repoValidaciones validacionesRepositorio;

        public ctAreasDeResponsabilidadController(aftContext context, repoAreasDeResponsabilidad areasDeResponsabilidadRepositorio, repoValidaciones validacionesRepositorio)
        {
            this.areasDeResponsabilidadRepositorio = areasDeResponsabilidadRepositorio;
            this.validacionesRepositorio = validacionesRepositorio;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: ctAreasDeResponsabilidad
        public IActionResult Areas()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Lista de Áreas de Responsabilidad");
            return View("vAreas");
        }

        [HttpPost]
        public async Task<IActionResult> TablaAreasDeResponsabilidad()
        {
            return PartialView("_vpTablaAreas", await areasDeResponsabilidadRepositorio.ListadoAreasDeResponsabilidad());
        }

        // GET: ctAreasDeResponsabilidad/Details/5
        public async Task<IActionResult> DetallesDeAreas(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var areaDeResponsabilidad = await areasDeResponsabilidadRepositorio.EncontrarAreaPorID(id);
            if (areaDeResponsabilidad == null)
            {
                return NotFound();
            }

            return View(areaDeResponsabilidad);
        }

        // GET: ctAreasDeResponsabilidad/Create
        public IActionResult CrearArea()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Área de Responsabilidad");
            ViewData["respuesta"] = new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };

            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vCrearArea");
        }

        // POST: ctAreasDeResponsabilidad/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarArea([Bind("idArea,Nombre,Descripcion")] AreaDeResponsabilidad areaDeResponsabilidad)
        {
            if (areaDeResponsabilidad != null)
            {
                var iDictionaryGrupos = validacionesRepositorio.EspaciosEnBlanco(areaDeResponsabilidad.Nombre, "Nombre");
                if (iDictionaryGrupos.Count > 0)
                {
                    foreach (var item in iDictionaryGrupos)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (areasDeResponsabilidadRepositorio.SiExisteNombreDelArea(areaDeResponsabilidad.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El Nombre del área a insertar ya existe.");
                }

                if (!string.IsNullOrEmpty(areaDeResponsabilidad.Descripcion))
                {
                    iDictionaryGrupos = validacionesRepositorio.EspaciosEnBlanco(areaDeResponsabilidad.Descripcion, "Descripcion");
                    if (iDictionaryGrupos.Count > 0)
                    {
                        foreach (var item in iDictionaryGrupos)
                            ModelState.AddModelError(item.Key, item.Value);
                    }
                }

                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        areaDeResponsabilidad.IdArea = Guid.NewGuid();
                        var resultadoArea = await areasDeResponsabilidadRepositorio.GuardarArea(areaDeResponsabilidad);
                        if (resultadoArea.SiSatisfactorio)
                        {
                            mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoArea.Mensaje;
                            return RedirectToAction(nameof(CrearArea));
                        }
                        else
                        {
                            mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoArea.Mensaje)}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            ViewData["respuesta"] = resultadoArea;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Área");
                            return View("vCrearArea", areaDeResponsabilidad);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<AreaDeResponsabilidad> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear Área");
                        return View("vCrearArea", areaDeResponsabilidad);
                    }
                }
            }

            ViewData["respuesta"] = new clResponse<AreaDeResponsabilidad> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Área a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Área");
            return View("vCrearArea", areaDeResponsabilidad);
        }

        // GET: ctAreasDeResponsabilidad/Edit/5
        public async Task<IActionResult> EditarArea(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Área");
                ViewData["respuesta"] = new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = "El objeto Área que se quiere editar es incorrecta o no existe." };
                return View("vEditarArea", new AreaDeResponsabilidad() { IdArea = Guid.Empty });
            }
            var areaResponsabilidad = await areasDeResponsabilidadRepositorio.EncontrarAreaPorID(id);
            if (areaResponsabilidad == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Área");
                ViewData["respuesta"] = new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = false, Mensaje = "El objeto Área que se quiere editar no existe." };
                return View("vEditarCargo", new AreaDeResponsabilidad() { IdArea = Guid.Empty });
            }
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Área");
            ViewData["respuesta"] = new clResponse<AreaDeResponsabilidad>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vEditarArea", areaResponsabilidad);
        }

        // POST: ctAreasDeResponsabilidad/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarArea(Guid IdArea, [Bind("IdArea,Nombre,Descripcion")] AreaDeResponsabilidad areaDeResponsabilidad)
        {
            if (areaDeResponsabilidad != null)
            {
                if (IdArea != areaDeResponsabilidad.IdArea)
                {
                    ViewData["respuesta"] = new clResponse<AreaDeResponsabilidad> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Área a editar." };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Editar Cargo");

                    return View("vEditarArea", areaDeResponsabilidad);
                }
                else
                {
                    var iDictionaryGrupos = validacionesRepositorio.EspaciosEnBlanco(areaDeResponsabilidad.Nombre, "Nombre");
                    if (iDictionaryGrupos.Count > 0)
                    {
                        foreach (var item in iDictionaryGrupos)
                            ModelState.AddModelError(item.Key, item.Value);
                    }
                    else if (areasDeResponsabilidadRepositorio.SiExisteNombreDelAreaEditada(areaDeResponsabilidad.Nombre, IdArea))
                    {
                        ModelState.AddModelError("Nombre", "El Nombre a insertar como modificación ya existe.");
                    }

                    if (!string.IsNullOrEmpty(areaDeResponsabilidad.Descripcion))
                    {
                        iDictionaryGrupos = validacionesRepositorio.EspaciosEnBlanco(areaDeResponsabilidad.Descripcion, "Descripcion");
                        if (iDictionaryGrupos.Count > 0)
                        {
                            foreach (var item in iDictionaryGrupos)
                                ModelState.AddModelError(item.Key, item.Value);
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        var resultadoArea = await areasDeResponsabilidadRepositorio.ModificarArea(areaDeResponsabilidad);
                        if (resultadoArea.SiSatisfactorio)
                        {
                            clGlobal.siNo = resultadoArea.SiSatisfactorio;
                            clGlobal.objMensaje = resultadoArea.Mensaje;
                            return RedirectToAction(nameof(EditarArea), new { id = areaDeResponsabilidad.IdArea });
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoArea;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Área");
                            return View("vEditarArea", areaDeResponsabilidad);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!areasDeResponsabilidadRepositorio.SiExisteElAreaConElID(areaDeResponsabilidad.IdArea))
                        {
                            ViewData["respuesta"] = new clResponse<AreaDeResponsabilidad> { SiSatisfactorio = false, Mensaje = "El  a editar no existe." };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Área");
                            return View("vEditarArea", areaDeResponsabilidad);
                        }
                        else
                        {
                            ViewData["respuesta"] = new clResponse<AreaDeResponsabilidad> { SiSatisfactorio = false, Mensaje = ex.Message };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Área");
                            return View("vEditarArea", areaDeResponsabilidad);
                        }
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<AreaDeResponsabilidad> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Área a editar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Área");
            return View("vEditarArea", areaDeResponsabilidad);
        }

        // GET: ctAreasDeResponsabilidad/Delete/5
        public async Task<IActionResult> EliminarArea(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var areaDeResponsabilidad = await areasDeResponsabilidadRepositorio.EncontrarAreaPorID(id);
            if (areaDeResponsabilidad == null)
            {
                return NotFound();
            }

            return View("vEliminarArea", areaDeResponsabilidad);
        }

        // POST: ctAreasDeResponsabilidad/Delete/5
        [HttpPost, ActionName("EliminarArea")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ConfirmarEliminacionDeArea(Guid id)
        {
            try
            {
                var areaDeResponsabilidad = await areasDeResponsabilidadRepositorio.EncontrarAreaPorID(id);
                var resultadoArea = await areasDeResponsabilidadRepositorio.EliminarArea(areaDeResponsabilidad);
                var result = new
                {
                    siNo = resultadoArea.SiSatisfactorio,
                    objMensaje = resultadoArea.Mensaje
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
                nombre = "Areas de Responsabilidad",
                ruta = new List<string>() { "Areas", "ctAreasDeResponsabilidad" }
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
