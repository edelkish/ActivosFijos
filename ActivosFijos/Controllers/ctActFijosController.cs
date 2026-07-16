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
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Data;
using System.Diagnostics;
using ActivosFijos.Models.Auxiliar;
using System.Net;
using System.IO.Compression;
using System.Drawing;
using ActivosFijos.Models.AuxBD;
using DocumentFormat.OpenXml.Vml;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ActivosFijos.Controllers
{
    public class ctActFijosController : Controller
    {
        private readonly aftContext _context;
        private readonly repoActivosFijos _repoActivosFijos;
        private readonly repoAreasDeResponsabilidad _repoAreas;
        private readonly repoResponsables _repoResponsables;
        private readonly repoCentrosDeCosto _repoCentroDeCostos;
        private readonly repoValidaciones _repoValidaciones;
        private readonly repoServcioActas _repoServcioActas;
        private readonly repoClasificador _repoClasificador;
        private readonly IWebHostEnvironment _env;

        private class SelectListResponsable
        {
            public Guid IdResponsable { get; set; }
            public string NombreCompleto { get; set; }
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        private string SessionKeyMuestra(int anno, int mes) => $"MuestraPendiente_{anno}_{mes}";

        public ctActFijosController(aftContext context, repoActivosFijos repoActivosFijos, repoAreasDeResponsabilidad repoAreas, repoResponsables repoResponsables,
            repoCentrosDeCosto repoCentroDeCostos, repoValidaciones repoValidaciones, repoServcioActas repoServcioActas, repoClasificador repoClasificador,
            IWebHostEnvironment env)
        {
            _context = context;
            _repoActivosFijos = repoActivosFijos;
            _repoAreas = repoAreas;
            _repoResponsables = repoResponsables;
            _repoCentroDeCostos = repoCentroDeCostos;
            _repoValidaciones = repoValidaciones;
            _repoServcioActas = repoServcioActas;
            _repoClasificador = repoClasificador;
            _env = env;
        }

        // GET: ctActFijos
        public async Task<IActionResult> ActivosFijos(Guid? area, Guid? responsable, DateTime? fecha, bool visualizarMovimiento = false)
        {
            List<SelectListItem> areas = new List<SelectListItem>();
            List<SelectListItem> responsables = new List<SelectListItem>();

            DateTime fechaParametro = fecha == null ? DateTime.Today : fecha.Value;

            if (area == null || area == Guid.Empty)
            {
                areas = _repoAreas.ListadoAreasDeResponsabilidad(fechaParametro).Result.Select(ar => new SelectListItem() { Text = ar.Nombre, Value = ar.IdArea.ToString() }).ToList();
                areas.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            }
            else
            {
                areas = _repoAreas.ListadoAreasDeResponsabilidad(fechaParametro).Result.Select(ar => new SelectListItem() { Text = ar.Nombre, Value = ar.IdArea.ToString(), Selected = ar.IdArea.Equals(area) }).ToList();
                areas.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString() });
            }

            if (responsable == null || responsable == Guid.Empty)
            {
                responsables = _repoResponsables.ObtenerListadoResponsables(fechaParametro).Result.Select(res => new SelectListItem() { Text = res.Nombre != "<<S/R>>" ? res.Nombre + " " + res.Apellidos : res.Nombre, Value = res.IdResponsable.ToString() }).ToList();
                responsables.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            }
            else
            {
                responsables = _repoResponsables.ObtenerListadoResponsables(fechaParametro).Result.Select(res => new SelectListItem() { Text = res.Nombre != "<<S/R>>" ? res.Nombre + " " + res.Apellidos : res.Nombre, Value = res.IdResponsable.ToString(), Selected = res.IdResponsable.Equals(responsable) }).ToList();
                responsables.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString() });
            }

            Responsable director = new Responsable();
            director = await _repoResponsables.ObtenerInfoDirector();

            string responsArea = "<<S/D>>";

            if (area == null || area == Guid.Empty)
            {
                responsArea = director.Nombre + " " + director.Apellidos;
            }
            else
            {
                Responsable espEP = new Responsable();
                espEP = await _repoResponsables.SeleccionarResponsableEPDelArea(area ?? Guid.Empty);

                if (espEP != null)
                    responsArea = espEP.Nombre + " " + espEP.Apellidos;
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Listado de Activos Fijos");

            ViewData["respuesta"] = new clResponse<mdActas>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vActivosFijos", new mdActas() { Areas = areas, Responsables = responsables, ResponsableArea = responsArea, Fecha = fecha == null ? DateTime.Today : new DateTime(fecha.Value.Year, fecha.Value.Month, fecha.Value.Day), VisualizarMovimiento = visualizarMovimiento });
        }

        public async Task<IActionResult> ActivosFijosImportados(Guid? area, Guid? responsable, DateTime? fecha)
        {
            List<SelectListItem> areas = new List<SelectListItem>();
            List<SelectListItem> responsables = new List<SelectListItem>();

            DateTime fechaParametro = fecha == null ? DateTime.Today : fecha.Value;

            if (area == null || area == Guid.Empty)
            {
                areas = _repoAreas.ListadoAreasDeResponsabilidad(fechaParametro).Result.Select(ar => new SelectListItem() { Text = ar.Nombre, Value = ar.IdArea.ToString() }).ToList();
                areas.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            }
            else
            {
                areas = _repoAreas.ListadoAreasDeResponsabilidad(fechaParametro).Result.Select(ar => new SelectListItem() { Text = ar.Nombre, Value = ar.IdArea.ToString(), Selected = ar.IdArea.Equals(area) }).ToList();
                areas.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString() });
            }

            if (responsable == null || responsable == Guid.Empty)
            {
                responsables = _repoResponsables.ObtenerListadoResponsables(fechaParametro).Result.Select(res => new SelectListItem() { Text = res.Nombre != "<<S/R>>" ? res.Nombre + " " + res.Apellidos : res.Nombre, Value = res.IdResponsable.ToString() }).ToList();
                responsables.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            }
            else
            {
                responsables = _repoResponsables.ObtenerListadoResponsables(fechaParametro).Result.Select(res => new SelectListItem() { Text = res.Nombre != "<<S/R>>" ? res.Nombre + " " + res.Apellidos : res.Nombre, Value = res.IdResponsable.ToString(), Selected = res.IdResponsable.Equals(responsable) }).ToList();
                responsables.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString() });
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Activos Fijos Importados");

            ViewData["respuesta"] = new clResponse<mdSubmayorErrores>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vActivosFijosImportados", new mdSubmayorErrores() { Areas = areas, Responsables = responsables, Fecha = fecha == null ? DateTime.Today : new DateTime(fecha.Value.Year, fecha.Value.Month, fecha.Value.Day) });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeResponsablesPorFecha(DateTime fecha, bool siMovimiento = false)
        {
            List<Responsable> responsables = siMovimiento ? await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(await _repoActivosFijos.ListadoActivosFijosConMovimientos(fecha)) : await _repoResponsables.ObtenerListadoResponsables(fecha, siMovimiento);
            List<SelectListItem> responsablesSelect = new List<SelectListItem>();
            responsablesSelect = responsables.Select(c => new SelectListItem() { Text = c.Nombre != "<<S/R>>" ? c.Nombre + " " + c.Apellidos : c.Nombre, Value = c.IdResponsable.ToString() }).OrderBy(c => c.Text).ToList();
            responsablesSelect.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });

            return PartialView("_vpResponsables", new mdActas() { Responsables = responsablesSelect });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeResponsablesPorFechaYArea(DateTime fecha, string area, bool siMovimiento = false)
        {
            List<Responsable> responsables = siMovimiento ? await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(await _repoActivosFijos.ListadoActivosFijosConMovimientos(fecha), Guid.Parse(area)) : await _repoResponsables.ObtenerListadoResponsablesPorFechaYArea(fecha, Guid.Parse(area), siMovimiento);
            List<SelectListItem> responsablesSelect = new List<SelectListItem>();
            responsablesSelect = responsables.Select(c => new SelectListItem() { Text = c.Nombre != "<<S/R>>" ? c.Nombre + " " + c.Apellidos : c.Nombre, Value = c.IdResponsable.ToString() }).OrderBy(c => c.Text).ToList();
            responsablesSelect.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });

            return PartialView("_vpResponsables", new mdActas() { Responsables = responsablesSelect });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeResponsablesAFTPorFecha(DateTime fecha)
        {
            List<Responsable> responsables = await _repoResponsables.ObtenerListadoResponsables(fecha);
            List<SelectListItem> responsablesSelect = new List<SelectListItem>();
            responsablesSelect = responsables.Select(c => new SelectListItem() { Text = c.Nombre != "<<S/R>>" ? c.Nombre + " " + c.Apellidos : c.Nombre, Value = c.IdResponsable.ToString() }).ToList();
            responsablesSelect.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });

            return PartialView("_vpResponsablesAFT", new mdSubmayorErrores() { Responsables = responsablesSelect });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeAreasPorFecha(DateTime fecha, bool siMovimientos = false)
        {
            List<AreaDeResponsabilidad> areas = siMovimientos ? await _repoAreas.ObtenerListadoAreasSiMovimiento(await _repoActivosFijos.ListadoActivosFijosConMovimientos(fecha)) : await _repoAreas.ListadoAreasDeResponsabilidad(fecha, siMovimientos);
            List<SelectListItem> areasSelect = new List<SelectListItem>();
            areasSelect = areas.Select(c => new SelectListItem() { Text = c.Nombre, Value = c.IdArea.ToString() }).ToList();
            areasSelect.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });

            return PartialView("_vpAreas", new mdActas() { Areas = areasSelect });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeAreasAFTPorFecha(DateTime fecha)
        {
            List<AreaDeResponsabilidad> areas = await _repoAreas.ListadoAreasDeResponsabilidad(fecha);
            List<SelectListItem> areasSelect = new List<SelectListItem>();
            areasSelect = areas.Select(c => new SelectListItem() { Text = c.Nombre, Value = c.IdArea.ToString() }).ToList();
            areasSelect.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });

            return PartialView("_vpAreasAFT", new mdSubmayorErrores() { Areas = areasSelect });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeActivosFijos(DateTime fecha, string area, string responsable, bool siMovimientos = false)
        {
            List<ActFijo> actFijosList = await _repoActivosFijos.ListadoActivosFijos(fecha, Guid.Parse(area), Guid.Parse(responsable));
            List<ExistenciaMovimiento> existMovs = new List<ExistenciaMovimiento>();
            existMovs = await _repoActivosFijos.DeterminarSiExisteMovimientoDeActivosFijos(fecha);

            if (siMovimientos)
                actFijosList = actFijosList.Where(c => existMovs.Any(ex => ex.NoInventario.Equals(c.NoInventario))).ToList();

            ViewData["ExistenciaDeMovimiento"] = existMovs;

            return PartialView("_vpTablaActivosFijos", actFijosList.OrderBy(act => act.AreaResponsabilidadNavigation.Nombre).ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos).ThenBy(act => act.NoInventario));
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeActivosFijosConMovimientos(DateTime fecha, string area, string responsable)
        {
            List<ActivosFijosConMovimientos> actFijosList = await _repoActivosFijos.ListadoActivosFijosConMovimientos(fecha);
            if (!Guid.Parse(area).Equals(Guid.Empty))
            {
                actFijosList = actFijosList.Where(c => c.idAreaActual.Equals(Guid.Parse(area)) || c.idAreaAnterior.Equals(Guid.Parse(area))).ToList();
            }

            if (!Guid.Parse(responsable).Equals(Guid.Empty))
            {
                actFijosList = actFijosList.Where(c => c.idResponsableActual.Equals(Guid.Parse(responsable)) || c.idResponsableAnterior.Equals(Guid.Parse(responsable))).ToList();
            }

            ViewData["FechaSubmayor"] = fecha;

            return PartialView("_vpTablaActivosFijosConMovimientos", actFijosList.OrderBy(act => act.AreaActual).ThenBy(act => act.ResponsableActual).ThenBy(act => act.NoInventario));
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeActivosFijosImportados(DateTime fecha, string area, string responsable, bool siErrores = false)
        {
            List<ActFijo> actFijosList = await _repoActivosFijos.ListadoActivosFijos(fecha, Guid.Parse(area), Guid.Parse(responsable));

            if (siErrores)
                actFijosList = actFijosList
                .Where(act => act.ResponsableNavigation == null || act.ResponsableNavigation.Nombre == "<<S/R>>"
                    || act.AreaResponsabilidad == null || act.AreaResponsabilidadNavigation.Nombre.Equals("<<S/A>>")
                    || act.CentroCosto == null || act.CentroCostoNavigation.Nombre == "<<S/C>>")
                .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                    .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                    .ThenBy(act => act.NoInventario)
                .ToList();
            else
                actFijosList = actFijosList.OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                   .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                   .ThenBy(act => act.NoInventario)
               .ToList();

            mdSubmayorErrores subMayor = new mdSubmayorErrores()
            {
                Area = Guid.Parse(area),
                Fecha = fecha,
                Responsable = Guid.Parse(responsable),
                VisualizarErrores = siErrores
            };

            ViewData["SubMayor"] = subMayor;

            return PartialView("_vpTablaActivosFijosImportados", actFijosList);
        }

        // GET: ctActFijos/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actFijo = await _context.ActFijos
                .Include(a => a.AreaResponsabilidadNavigation)
                .Include(a => a.CentroCostoNavigation)
                .Include(a => a.ResponsableNavigation)
                .FirstOrDefaultAsync(m => m.NoInventario == id);
            if (actFijo == null)
            {
                return NotFound();
            }

            return View(actFijo);
        }

        // GET: ctActFijos/Create
        public IActionResult CrearActivoFijo()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Activo Fijo");
            ViewData["respuesta"] = new clResponse<ActFijo>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };

            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            IOrderedQueryable<AreaDeResponsabilidad> areas = _context.AreaDeResponsabilidads.OrderBy(c => c.Nombre);
            IOrderedQueryable<CentroCosto> centroCosto = _context.CentroCostos.OrderBy(c => c.Nombre);
            IOrderedQueryable<Responsable> responsables = _context.Responsables.Where(c => c.AreaResponsabilidad.Equals(areas.FirstOrDefault().IdArea)).OrderBy(c => c.Nombre);

            ViewData["AreaResponsabilidad"] = new SelectList(areas, "IdArea", "Nombre");
            ViewData["CentroCosto"] = new SelectList(centroCosto, "IdCentroCosto", "Nombre");
            ViewData["Responsable"] = new SelectList(responsables.Select(c => new SelectListResponsable() { IdResponsable = c.IdResponsable, NombreCompleto = c.Nombre == "<<S/R>>" ? c.Nombre : c.Nombre + " " + c.Apellidos }), "IdResponsable", "NombreCompleto");
            return View("vCrearActivoFijo", new ActFijo() { DepreciacionAcum = new Decimal(0.00), ValorContable = new Decimal(0.00) });
        }

        [HttpPost]
        public async Task<IActionResult> ListarResponsablesConElAreaID(Guid? idArea)
        {
            return PartialView("_vpSelectReponsablesActivosFijos");
        }

        // POST: ctActFijos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarActivoFijo([Bind("NoInventario,Descripcion,DepreciacionAcum,ValorContable,Responsable,CentroCosto,AreaResponsabilidad,Clasificador")] ActFijo actFijo)
        {
            if (actFijo != null)
            {
                var respuesta = new clResponse<bool> { SiSatisfactorio = false, Mensaje = string.Empty };
                #region Campo No. Inventario
                IDictionary<string, string> iDictionaryActivosFijos = new Dictionary<string, string>();
                if (actFijo.NoInventario <= 0)
                {
                    ModelState.AddModelError("NoInventario", "El No de Inventario a insertar es incorrecto. Favor de rectificarlo");
                }
                else if (_repoActivosFijos.SiExisteActivoFijoConNoInventario(actFijo.NoInventario))
                {
                    ModelState.AddModelError("NoInventario", "El No de Inventario a insertar ya existe. Favor de rectificarlo");
                }
                #endregion
                #region Campo Descripción
                iDictionaryActivosFijos = _repoValidaciones.EspaciosEnBlanco(actFijo.Descripcion, "Descripcion");
                if (iDictionaryActivosFijos.Count > 0)
                {
                    foreach (var item in iDictionaryActivosFijos)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                #endregion
                #region Campo Responsable
                iDictionaryActivosFijos = new Dictionary<string, string>();
                if ((String.IsNullOrEmpty(actFijo.Responsable.ToString())) || (actFijo.Responsable.Equals(Guid.Empty)))
                {
                    ModelState.AddModelError("Responsable", "El responsable seleccionado para crear el Activo Fijo no puede estar en blanco. Favor de rectificarlo");
                }
                else if (!_repoResponsables.SiExisteElReponsableConElID(actFijo.Responsable))
                {
                    ModelState.AddModelError("Responsable", "El responsable seleccionado para crear el Activo Fijo no existe. Favor de rectificarlo");
                }
                #endregion
                #region Campo Area de Responsabilidad
                iDictionaryActivosFijos = new Dictionary<string, string>();
                if ((String.IsNullOrEmpty(actFijo.AreaResponsabilidad.ToString())) || (actFijo.AreaResponsabilidad.Equals(Guid.Empty)))
                {
                    ModelState.AddModelError("AreaResponsabilidad", "El área seleccionada para crear el Activo Fijo no puede estar en blanco. Favor de rectificarlo");
                }
                else if (!_repoAreas.SiExisteElAreaConElID(actFijo.AreaResponsabilidad))
                {
                    ModelState.AddModelError("AreaResponsabilidad", "El área seleccionada para crear el Activo Fijo no existe. Favor de rectificarlo");
                }
                #endregion
                #region Campo Centro de Costo
                iDictionaryActivosFijos = new Dictionary<string, string>();
                if ((String.IsNullOrEmpty(actFijo.CentroCosto.ToString())) || (actFijo.CentroCosto.Equals(Guid.Empty)))
                {
                    ModelState.AddModelError("CentroCosto", "El centro de costo seleccionado para crear el Activo Fijo no puede estar en blanco. Favor de rectificarlo");
                }
                else if (!_repoCentroDeCostos.SiExisteElCentroCostoConElID(actFijo.CentroCosto))
                {
                    ModelState.AddModelError("CentroCosto", "El centro de costo seleccionado para crear el Activo Fijo no existe. Favor de rectificarlo");
                }
                #endregion

                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        actFijo.NoInventario = actFijo.NoInventario;
                        var resultadoActivoFijo = await _repoActivosFijos.GuardarActivoFijo(actFijo);
                        if (resultadoActivoFijo.SiSatisfactorio)
                        {
                            mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoActivoFijo.Mensaje;
                            return RedirectToAction(nameof(CrearActivoFijo));
                        }
                        else
                        {
                            mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoActivoFijo.Mensaje)}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            ViewData["respuesta"] = resultadoActivoFijo;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Activos Fijos");
                            ViewData["AreaResponsabilidad"] = new SelectList(_context.AreaDeResponsabilidads, "IdArea", "Nombre", actFijo.AreaResponsabilidad);
                            ViewData["CentroCosto"] = new SelectList(_context.CentroCostos, "IdCentroCosto", "Nombre", actFijo.CentroCosto);
                            ViewData["Responsable"] = new SelectList(_context.Responsables.Select(c => new SelectListResponsable() { IdResponsable = c.IdResponsable, NombreCompleto = c.Nombre + " " + c.Apellidos }), "IdResponsable", "NombreCompleto", actFijo.Responsable);
                            return View("vCrearActivoFijo", actFijo);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<ActFijo> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear Activos Fijos");
                        ViewData["AreaResponsabilidad"] = new SelectList(_context.AreaDeResponsabilidads, "IdArea", "Nombre", actFijo.AreaResponsabilidad);
                        ViewData["CentroCosto"] = new SelectList(_context.CentroCostos, "IdCentroCosto", "Nombre", actFijo.CentroCosto);
                        ViewData["Responsable"] = new SelectList(_context.Responsables.Select(c => new SelectListResponsable() { IdResponsable = c.IdResponsable, NombreCompleto = c.Nombre + " " + c.Apellidos }), "IdResponsable", "NombreCompleto", actFijo.Responsable);
                        return View("vCrearActivoFijo", actFijo);
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<ActFijo> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Activo Fijo a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Activos Fijos");
            ViewData["AreaResponsabilidad"] = new SelectList(_context.AreaDeResponsabilidads, "IdArea", "Nombre", actFijo.AreaResponsabilidad);
            ViewData["CentroCosto"] = new SelectList(_context.CentroCostos, "IdCentroCosto", "Nombre", actFijo.CentroCosto);
            ViewData["Responsable"] = new SelectList(_context.Responsables.Select(c => new SelectListResponsable() { IdResponsable = c.IdResponsable, NombreCompleto = c.Nombre + " " + c.Apellidos }), "IdResponsable", "NombreCompleto", actFijo.Responsable);
            return View("vCrearActivoFijo", actFijo);
        }

        // GET: ctActFijos/Edit/5
        [Route("ctActFijos/EditarActivoFijo/{id}/{fecha}")]
        public async Task<IActionResult> EditarActivoFijo(long? id, DateTime fecha)
        {
            if (id == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Activos Fijos");
                ViewData["respuesta"] = new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El objeto Activo Fijo que se quiere editar es incorrecto o no existe." };
                return View("vEditarActivoFijo", new ActFijo() { NoInventario = new long() });
            }

            var activoFijo = await _repoActivosFijos.EncontrarActivoFijoID(id, fecha);
            if (activoFijo == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Activos Fijos");
                ViewData["respuesta"] = new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El objeto Activo Fijo se quiere editar no existe." };

                return View("vEditarActivoFijo", new ActFijo() { NoInventario = new long() });
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Activos Fijos");
            ViewData["respuesta"] = new clResponse<ActFijo>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };

            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            ViewData["AreaResponsabilidad"] = _repoAreas.ListadoAreasDeResponsabilidad(activoFijo.FechaSubmayor).Result.Select(ar => new SelectListItem() { Text = ar.Nombre, Value = ar.IdArea.ToString(), Selected = ar.IdArea.Equals(activoFijo.AreaResponsabilidad) }).ToList();//new SelectList(_context.AreaDeResponsabilidads, "IdArea", "Nombre", activoFijo.AreaResponsabilidad);
            ViewData["CentroCosto"] = new SelectList(_context.CentroCostos, "IdCentroCosto", "Nombre", activoFijo.CentroCosto);
            ViewData["Responsable"] = _repoResponsables.ObtenerListadoResponsables(activoFijo.FechaSubmayor).Result.Select(res => new SelectListItem() { Text = res.Nombre != "<<S/R>>" ? res.Nombre + " " + res.Apellidos : res.Nombre, Value = res.IdResponsable.ToString(), Selected = res.IdResponsable.Equals(activoFijo.Responsable) }).ToList();//new SelectList(_context.Responsables.Select(c => new SelectListResponsable() { IdResponsable = c.IdResponsable, NombreCompleto = c.Nombre == "<<S/R>>" ? c.Nombre : c.Nombre + " " + c.Apellidos }), "IdResponsable", "NombreCompleto", activoFijo.Responsable);
            return View("vEditarActivoFijo", activoFijo);
        }

        public async Task<IActionResult> EditarActivoFijoImportado(long? id, mdSubmayorErrores submayor)
        {
            if (id == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Activos Fijos");
                ViewData["respuesta"] = new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El objeto Activo Fijo que se quiere editar es incorrecto o no existe." };
                return View("vEditarActivoFijo", new ActFijo() { NoInventario = new long() });
            }

            var activoFijo = await _repoActivosFijos.EncontrarActivoFijoID(id);
            if (activoFijo == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Activos Fijos");
                ViewData["respuesta"] = new clResponse<ActFijo>() { SiSatisfactorio = false, Mensaje = "El objeto Activo Fijo se quiere editar no existe." };

                return View("vEditarActivoFijo", new ActFijo() { NoInventario = new long() });
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Activos Fijos");
            ViewData["respuesta"] = new clResponse<ActFijo>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };

            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            ViewData["AreaResponsabilidad"] = _repoAreas.ListadoAreasDeResponsabilidad(activoFijo.FechaSubmayor).Result.Select(ar => new SelectListItem() { Text = ar.Nombre, Value = ar.IdArea.ToString(), Selected = ar.IdArea.Equals(activoFijo.AreaResponsabilidad) }).ToList();//new SelectList(_context.AreaDeResponsabilidads, "IdArea", "Nombre", activoFijo.AreaResponsabilidad);
            ViewData["CentroCosto"] = new SelectList(_context.CentroCostos, "IdCentroCosto", "Nombre", activoFijo.CentroCosto);
            ViewData["Responsable"] = _repoResponsables.ObtenerListadoResponsables(activoFijo.FechaSubmayor).Result.Select(res => new SelectListItem() { Text = res.Nombre != "<<S/R>>" ? res.Nombre + " " + res.Apellidos : res.Nombre, Value = res.IdResponsable.ToString(), Selected = res.IdResponsable.Equals(activoFijo.Responsable) }).ToList();//new SelectList(_context.Responsables.Select(c => new SelectListResponsable() { IdResponsable = c.IdResponsable, NombreCompleto = c.Nombre == "<<S/R>>" ? c.Nombre : c.Nombre + " " + c.Apellidos }), "IdResponsable", "NombreCompleto", activoFijo.Responsable);
            return View("vEditarActivoFijo", activoFijo);
        }

        // POST: ctActFijos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarActivoFijo(long noInventario, [Bind("NoInventario,Descripcion,DepreciacionAcum,ValorContable,Responsable,CentroCosto,AreaResponsabilidad,FechaSubmayor")] ActFijo actFijo)
        {
            if ((actFijo != null) && (noInventario == actFijo.NoInventario))
            {
                #region Campo No. Inventario
                IDictionary<string, string> iDictionaryActivosFijos = new Dictionary<string, string>();
                if (actFijo.NoInventario <= 0)
                {
                    ModelState.AddModelError("NoInventario", "El No de Inventario a insertar como modificación es incorrecto. Favor de rectificarlo");
                }
                else if (_repoActivosFijos.SiExisteActivoFijoConNoInventarioEditado(actFijo.NoInventario))
                {
                    ModelState.AddModelError("NoInventario", "El No de Inventario a insertar como modificación ya existe. Favor de rectificarlo");
                }
                #endregion
                #region Campo Descripción
                iDictionaryActivosFijos = _repoValidaciones.EspaciosEnBlanco(actFijo.Descripcion, "Descripcion");
                if (iDictionaryActivosFijos.Count > 0)
                {
                    foreach (var item in iDictionaryActivosFijos)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                #endregion
                #region Campo Responsable
                iDictionaryActivosFijos = new Dictionary<string, string>();
                if ((String.IsNullOrEmpty(actFijo.Responsable.ToString())) || (actFijo.Responsable.Equals(Guid.Empty)))
                {
                    ModelState.AddModelError("Responsable", "El responsable seleccionado para modificar el Activo Fijo no puede estar en blanco. Favor de rectificarlo");
                }
                else if (!_repoResponsables.SiExisteElReponsableConElID(actFijo.Responsable))
                {
                    ModelState.AddModelError("Responsable", "El responsable seleccionado para modificar el Activo Fijo no existe. Favor de rectificarlo");
                }
                #endregion
                #region Campo Area de Responsabilidad
                iDictionaryActivosFijos = new Dictionary<string, string>();
                if ((String.IsNullOrEmpty(actFijo.AreaResponsabilidad.ToString())) || (actFijo.AreaResponsabilidad.Equals(Guid.Empty)))
                {
                    ModelState.AddModelError("AreaResponsabilidad", "El área seleccionada para modificar el Activo Fijo no puede estar en blanco. Favor de rectificarlo");
                }
                else if (!_repoAreas.SiExisteElAreaConElID(actFijo.AreaResponsabilidad))
                {
                    ModelState.AddModelError("AreaResponsabilidad", "El área seleccionada para modificar el Activo Fijo no existe. Favor de rectificarlo");
                }
                #endregion
                #region Campo Centro de Costo
                iDictionaryActivosFijos = new Dictionary<string, string>();
                if ((String.IsNullOrEmpty(actFijo.CentroCosto.ToString())) || (actFijo.CentroCosto.Equals(Guid.Empty)))
                {
                    ModelState.AddModelError("CentroCosto", "El centro de costo seleccionado para modificar el Activo Fijo no puede estar en blanco. Favor de rectificarlo");
                }
                else if (!_repoCentroDeCostos.SiExisteElCentroCostoConElID(actFijo.CentroCosto))
                {
                    ModelState.AddModelError("CentroCosto", "El centro de costo seleccionado para modificar el Activo Fijo no existe. Favor de rectificarlo");
                }
                #endregion

                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        var resultadoActivoFijo = await _repoActivosFijos.ModificarActivoFijo(actFijo);
                        if (resultadoActivoFijo.SiSatisfactorio)
                        {
                            mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoActivoFijo.Mensaje;
                            return RedirectToAction(nameof(EditarActivoFijo), new { id = actFijo.NoInventario, fecha = actFijo.FechaSubmayor.ToString("yyyy-MM-dd") });
                        }
                        else
                        {
                            mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoActivoFijo.Mensaje)}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            ViewData["respuesta"] = resultadoActivoFijo;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Activos Fijos");
                            ViewData["AreaResponsabilidad"] = new SelectList(_context.AreaDeResponsabilidads, "IdArea", "Nombre", actFijo.AreaResponsabilidad);
                            ViewData["CentroCosto"] = new SelectList(_context.CentroCostos, "IdCentroCosto", "Nombre", actFijo.CentroCosto);
                            ViewData["Responsable"] = new SelectList(_context.Responsables.Select(c => new SelectListResponsable() { IdResponsable = c.IdResponsable, NombreCompleto = c.Nombre == "<<S/R>>" ? c.Nombre : c.Nombre + " " + c.Apellidos }), "IdResponsable", "NombreCompleto", actFijo.Responsable);
                            return View("vEditarActivoFijo", actFijo);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<ActFijo> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Editar Activos Fijos");
                        ViewData["AreaResponsabilidad"] = new SelectList(_context.AreaDeResponsabilidads, "IdArea", "Nombre", actFijo.AreaResponsabilidad);
                        ViewData["CentroCosto"] = new SelectList(_context.CentroCostos, "IdCentroCosto", "Nombre", actFijo.CentroCosto);
                        ViewData["Responsable"] = new SelectList(_context.Responsables.Select(c => new SelectListResponsable() { IdResponsable = c.IdResponsable, NombreCompleto = c.Nombre == "<<S/R>>" ? c.Nombre : c.Nombre + " " + c.Apellidos }), "IdResponsable", "NombreCompleto", actFijo.Responsable);
                        return View("vEditarActivoFijo", actFijo);
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<ActFijo> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Activo Fijo a modificar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Activos Fijos");
            ViewData["AreaResponsabilidad"] = new SelectList(_context.AreaDeResponsabilidads, "IdArea", "Nombre", actFijo.AreaResponsabilidad);
            ViewData["CentroCosto"] = new SelectList(_context.CentroCostos, "IdCentroCosto", "Nombre", actFijo.CentroCosto);
            ViewData["Responsable"] = new SelectList(_context.Responsables.Select(c => new SelectListResponsable() { IdResponsable = c.IdResponsable, NombreCompleto = c.Nombre == "<<S/R>>" ? c.Nombre : c.Nombre + " " + c.Apellidos }), "IdResponsable", "NombreCompleto", actFijo.Responsable);
            return View("vEditarActivoFijo", actFijo);
        }

        // POST: ctActFijos/Delete/5
        [HttpPost, ActionName("EliminarActivosFijos")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminarActivosFijos(long id)
        {
            try
            {
                var activoFijo = await _repoActivosFijos.EncontrarActivoFijoID(id);
                clResponse<ActFijo> resultado = new clResponse<ActFijo>();
                resultado = await _repoActivosFijos.EliminarActivoFijo(activoFijo);
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

        [HttpPost]
        public IActionResult CargarDatosResponsablesPorAreaDeResponsabilidad(string area)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            lista = _repoResponsables.ObtenerListadoResponsables(Guid.Parse(area)).Result.Select(res => new SelectListItem() { Text = res.Nombre == "<<S/R>>" ? res.Nombre : res.Nombre + " " + res.Apellidos, Value = res.IdResponsable.ToString(), Selected = false }).ToList();
            lista.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            return PartialView("_vpResponsables", new mdActas() { Responsables = lista });
        }

        [HttpPost]
        public async Task<IActionResult> CargarDatosResponsablesPorAreaYFecha(string area, DateTime fecha)
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            List<ActFijo> activosFijos = new List<ActFijo>();
            activosFijos = await _repoActivosFijos.ListadoActivosFijos(fecha);
            activosFijos = activosFijos.Where(c => c.AreaResponsabilidad.Equals(Guid.Parse(area))).ToList();

            List<Responsable> responsables = new List<Responsable>();
            responsables = activosFijos.Select(c => c.ResponsableNavigation).Distinct().ToList();

            lista = responsables.Select(res => new SelectListItem() { Text = res.Nombre == "<<S/R>>" ? res.Nombre : res.Nombre + " " + res.Apellidos, Value = res.IdResponsable.ToString(), Selected = false }).ToList();
            lista.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            return PartialView("_vpResponsablesAFT", new mdSubmayorErrores() { Responsables = lista });
        }

        [HttpPost]
        public async Task<IActionResult> CargarInfoResponsableDelArea(string area)
        {
            Responsable responsable = new Responsable();
            if (Guid.Parse(area) == Guid.Empty)
            {
                responsable = await _repoResponsables.ObtenerInfoDirector();
                ViewData["ResponsableDelArea"] = responsable.Nombre + " " + responsable.Apellidos;
            }
            else
            {
                responsable = _repoResponsables.ObtenerListadoResponsables(Guid.Parse(area)).Result.Where(res => res.ResponsableDelArea == true).FirstOrDefault();
                ViewData["ResponsableDelArea"] = responsable == null ? "<<Sin responsable>>" : responsable.Nombre + " " + responsable.Apellidos;
            }

            return PartialView("_vpResponsableDelArea", new mdActas() { ResponsableArea = responsable == null ? "<<Sin responsable>>" : responsable.Nombre + " " + responsable.Apellidos });
        }

        [HttpPost]
        public async Task<IActionResult> GenerarActa(mdActas model)
        {
            bool result;
            string msg;
            byte[] objFile = null;

            string tituloDoc = string.Empty;

            Responsable respSeleccionado = new Responsable();
            respSeleccionado = await _repoResponsables.SeleccionarResponsablesPorID(model.Responsable);

            if (respSeleccionado != null && respSeleccionado.Nombre.Equals("<<S/R>>"))
            {
                clGlobal.siNo = false;
                clGlobal.objMensaje = $"No se pueden generar actas cuando el responsable seleccionado es {_repoResponsables.SeleccionarResponsablesPorID(model.Responsable).Result.Nombre}.";
                return RedirectToAction(nameof(ActivosFijos), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha, visualizarMovimiento = model.VisualizarMovimiento });
            }
            else
            {
                try
                {
                    string nombreMes = repoServcioActas.GetNameMonth(model.Fecha.Month, model.Fecha.Year);

                    List<ExistenciaMovimiento> existMovs = new List<ExistenciaMovimiento>();
                    existMovs = await _repoActivosFijos.DeterminarSiExisteMovimientoDeActivosFijos(model.Fecha);

                    List<Responsable> responsables = new List<Responsable>();
                    List<AreaDeResponsabilidad> areasResponsabilidad = new List<AreaDeResponsabilidad>();

                    if (model.VisualizarMovimiento)
                    {
                        List<ExistenciaMovimiento> existenciaMovimientos = new List<ExistenciaMovimiento>();
                        existenciaMovimientos = _repoActivosFijos.DeterminarSiExisteMovimientoDeActivosFijos(model.Fecha).Result;
                        responsables = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaMovimientos, model.Fecha);
                        areasResponsabilidad = responsables.Select(c => c.AreaResponsabilidadNavigation).Distinct().ToList();
                    }
                    else
                    {
                        responsables = await _repoResponsables.ObtenerListadoResponsables(model.Area);
                    }

                    if (model.Area == Guid.Empty)
                    {
                        if (model.VisualizarMovimiento)
                        {
                            if (!model.Responsable.Equals(Guid.Empty))
                            {
                                if (!responsables.Any(c => c.IdResponsable.Equals(model.Responsable)))
                                {
                                    Responsable resp = new Responsable();
                                    resp = _repoResponsables.SeleccionarResponsablesPorID(model.Responsable).Result;

                                    string nombreResp = resp == null ? "<<S/D>>" : resp.Nombre + " " + resp.Apellidos;

                                    clGlobal.siNo = false;
                                    clGlobal.objMensaje = $"No existen movimientos para el responsable {nombreResp} en la fecha seleccionada.";
                                    return RedirectToAction(nameof(ActivosFijos), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha, visualizarMovimiento = model.VisualizarMovimiento });
                                }
                            }

                            if (responsables.Count > 0)
                            {
                                MemoryStream ms = new MemoryStream();
                                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                                {
                                    foreach (var item in responsables)
                                    {
                                        string nombreArea = item.AreaResponsabilidadNavigation.Nombre;
                                        string nombreResponsable = item.Nombre + " " + item.Apellidos;
                                        tituloDoc = nombreArea + " (" + nombreResponsable + ", " + nombreMes.Replace('/', '-') + ").docx";

                                        Responsable respArea = new Responsable();
                                        respArea = item.AreaResponsabilidadNavigation.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();
                                        string responsableArea = respArea == null ? "<<S/D>>" : respArea.Nombre + " " + respArea.Apellidos;

                                        objFile = _repoServcioActas.ExportarPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, responsableArea);
                                        var entry = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }

                                    foreach (var item in areasResponsabilidad)
                                    {
                                        tituloDoc = item.Nombre + " (" + nombreMes.Replace('/', '-') + ").docx";
                                        Responsable responArea = item.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();

                                        string responsableArea = String.Empty;

                                        if (responArea != null)
                                            responsableArea = responArea.Nombre + " " + responArea.Apellidos;
                                        else
                                        {
                                            responsableArea = "<<S/D>>";
                                        }

                                        objFile = _repoServcioActas.ExportarPorAreasDocx(out result, out msg, Guid.Empty, item.IdArea, model.Fecha, responsableArea);
                                        var entry = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }

                                    objFile = _repoServcioActas.ExportarADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                    tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                    var entryEP = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entryEP.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }

                                return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material (Movimientos - {nombreMes.Replace('/', '-')}).zip");
                            }
                            else
                            {
                                clGlobal.siNo = false;
                                clGlobal.objMensaje = $"No existen movimientos en la fecha seleccionada.";
                                return RedirectToAction(nameof(ActivosFijos), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha });
                            }
                        }
                        else
                        {
                            if (!model.Responsable.Equals(Guid.Empty))
                            {
                                MemoryStream ms = new MemoryStream();
                                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                                {
                                    Responsable responsable = new Responsable();
                                    responsable = _repoResponsables.SeleccionarResponsablesPorID(model.Responsable).Result;
                                    string nombreArea = responsable.AreaResponsabilidadNavigation.Nombre;
                                    string nombreResponsable = responsable == null ? "<<S/D>>" : responsable.Nombre + " " + responsable.Apellidos;
                                    tituloDoc = nombreArea + " (" + nombreResponsable + ", " + nombreMes.Replace('/', '-') + ").docx";

                                    Responsable respArea = new Responsable();
                                    respArea = _repoResponsables.SeleccionarResponsableEPDelArea(responsable.AreaResponsabilidad ?? Guid.Empty).Result;
                                    string responsableArea = respArea == null ? "<<S/D>>" : respArea.Nombre + " " + respArea.Apellidos;

                                    objFile = _repoServcioActas.ExportarPorAreasIndividualDocx(out result, out msg, model.Responsable, model.Area, model.Fecha, responsableArea);
                                    var entry = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entry.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }

                                return File(objFile, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", tituloDoc);
                            }
                            else
                            {
                                objFile = _repoServcioActas.ExportarADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                return File(objFile, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", tituloDoc);
                            }
                        }
                    }
                    else
                    {
                        string nombreArea = _repoAreas.EncontrarAreaPorID(model.Area).Result.Nombre;
                        if (model.Responsable == Guid.Empty)
                        {
                            if (model.VisualizarMovimiento)
                            {
                                if (areasResponsabilidad.Count() > 0 && areasResponsabilidad.Any(c => c.IdArea.Equals(model.Area)))
                                {
                                    responsables = responsables.Where(c => c.AreaResponsabilidad.Equals(model.Area)).ToList();

                                    MemoryStream ms = new MemoryStream();
                                    using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                                    {
                                        foreach (var item in responsables)
                                        {
                                            string nombreResponsable = item.Nombre + " " + item.Apellidos;
                                            tituloDoc = nombreArea + " (" + nombreResponsable + ", " + nombreMes.Replace('/', '-') + ").docx";

                                            objFile = _repoServcioActas.ExportarPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, model.ResponsableArea);
                                            var entry = zip.CreateEntry(tituloDoc);

                                            using (var fileStream = new MemoryStream(objFile))
                                            using (var entryStream = entry.Open())
                                            {
                                                fileStream.CopyTo(entryStream);
                                            }
                                        }

                                        AreaDeResponsabilidad areasResp = new AreaDeResponsabilidad();
                                        areasResp = areasResponsabilidad.Where(c => c.IdArea.Equals(model.Area)).FirstOrDefault();

                                        tituloDoc = areasResp.Nombre + " (" + nombreMes.Replace('/', '-') + ").docx";
                                        Responsable responArea = areasResp.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();

                                        string responsableArea = String.Empty;

                                        if (responArea != null)
                                            responsableArea = responArea.Nombre + " " + responArea.Apellidos;
                                        else
                                        {
                                            responsableArea = "<<S/D>>";
                                        }

                                        objFile = _repoServcioActas.ExportarPorAreasDocx(out result, out msg, Guid.Empty, areasResp.IdArea, model.Fecha, responsableArea);
                                        var entryEP = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entryEP.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }

                                        objFile = _repoServcioActas.ExportarADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                        tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                        var entryGen = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entryGen.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }

                                    return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material (Movimientos - {nombreMes.Replace('/', '-')}).zip");
                                }
                                else
                                {
                                    clGlobal.siNo = false;
                                    clGlobal.objMensaje = $"No existen movimientos para el área {nombreArea} en la fecha seleccionada.";
                                    return RedirectToAction(nameof(ActivosFijos), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha, visualizarMovimiento = model.VisualizarMovimiento });
                                }
                            }
                            else
                            {
                                MemoryStream ms = new MemoryStream();
                                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                                {
                                    foreach (var item in responsables)
                                    {
                                        string nombreResponsable = item.Nombre + " " + item.Apellidos;
                                        tituloDoc = nombreArea + " (" + nombreResponsable + ", " + nombreMes.Replace('/', '-') + ").docx";

                                        objFile = _repoServcioActas.ExportarPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, model.ResponsableArea);
                                        if (objFile != null)
                                        {
                                            var entry = zip.CreateEntry(tituloDoc);

                                            using (var fileStream = new MemoryStream(objFile))
                                            using (var entryStream = entry.Open())
                                            {
                                                fileStream.CopyTo(entryStream);
                                            }
                                        }
                                    }

                                    tituloDoc = nombreArea + " (" + nombreMes.Replace('/', '-') + ").docx";

                                    objFile = _repoServcioActas.ExportarPorAreasDocx(out result, out msg, Guid.Empty, model.Area, model.Fecha, model.ResponsableArea);
                                    var entryEP = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entryEP.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }

                                return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material ({nombreArea} - {nombreMes}).zip"); //"application/vnd.openxmlformats-officedocument.wordprocessingml.document", tituloDoc);
                            }
                        }
                        else
                        {
                            if (model.VisualizarMovimiento)
                            {
                                if (!responsables.Any(c => c.IdResponsable.Equals(model.Responsable)))
                                {
                                    Responsable esp = new Responsable();
                                    esp = _repoResponsables.SeleccionarResponsablesPorID(model.Responsable).Result;

                                    clGlobal.siNo = false;
                                    clGlobal.objMensaje = $"No existen movimientos en el área {nombreArea} para el responsable {esp.Nombre + " " + esp.Apellidos} en la fecha seleccionada.";
                                    return RedirectToAction(nameof(ActivosFijos), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha, visualizarMovimiento = model.VisualizarMovimiento });
                                }

                                Responsable director = new Responsable();
                                director = _repoResponsables.ObtenerInfoDirector().Result;

                                Responsable responsable = new Responsable();
                                responsable = await _repoResponsables.SeleccionarResponsablesPorID(model.Responsable);

                                string nombreResponsable = responsable.Nombre + " " + responsable.Apellidos;

                                MemoryStream ms = new MemoryStream();
                                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                                {
                                    objFile = _repoServcioActas.ExportarPorAreasIndividualDocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                    tituloDoc = nombreArea + " (" + nombreResponsable + ", " + nombreMes.Replace('/', '-') + ").docx";

                                    var entry = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entry.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }

                                    tituloDoc = nombreArea + " (" + nombreMes.Replace('/', '-') + ").docx";

                                    objFile = _repoServcioActas.ExportarPorAreasDocx(out result, out msg, Guid.Empty, model.Area, model.Fecha, model.ResponsableArea);
                                    var entryEP = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entryEP.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }

                                    objFile = _repoServcioActas.ExportarADocx(out result, out msg, Guid.Empty, Guid.Empty, model.Fecha, director.Nombre + " " + director.Apellidos);
                                    tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                    entryEP = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entryEP.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }

                                return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material ({nombreArea} - {nombreMes}).zip");
                            }
                            else
                            {
                                Responsable responsable = new Responsable();
                                responsable = await _repoResponsables.SeleccionarResponsablesPorID(model.Responsable);

                                string nombreResponsable = responsable.Nombre + " " + responsable.Apellidos;

                                MemoryStream ms = new MemoryStream();
                                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                                {
                                    objFile = _repoServcioActas.ExportarPorAreasIndividualDocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                    tituloDoc = nombreArea + " (" + nombreResponsable + ", " + nombreMes.Replace('/', '-') + ").docx";

                                    var entry = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entry.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }

                                return File(objFile, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", tituloDoc);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    clGlobal.siNo = false;
                    clGlobal.objMensaje = $"Ocurrió el siguiente error: {ex.Message}";
                    return RedirectToAction(nameof(ActivosFijos), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha, visualizarMovimiento = model.VisualizarMovimiento });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerarActaAFT(mdActas model)
        {
            bool result;
            string msg;
            byte[] objFile = null;

            string tituloDoc = string.Empty;

            try
            {
                string nombreMes = repoServcioActas.GetNameMonth(model.Fecha.Month, model.Fecha.Year);

                List<Responsable> responsables = new List<Responsable>();
                List<AreaDeResponsabilidad> areasResponsabilidad = new List<AreaDeResponsabilidad>();
                List<ActivosFijosConMovimientos> existenciaDeMovimientos = new List<ActivosFijosConMovimientos>();
                existenciaDeMovimientos = await _repoActivosFijos.ListadoActivosFijosConMovimientos(model.Fecha);
                areasResponsabilidad = await _repoAreas.ObtenerListadoAreasSiMovimiento(existenciaDeMovimientos);

                if (model.Area == Guid.Empty)
                {
                    if (model.Responsable == Guid.Empty)
                    {
                        List<Responsable> responsablesActuales = new List<Responsable>();
                        List<Responsable> responsablesAnteriores = new List<Responsable>();
                        responsablesActuales = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos, true);
                        responsablesAnteriores = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos, false);
                        responsables = responsablesActuales.Union(responsablesAnteriores).ToList();
                        if (responsables.Count > 0)
                        {
                            MemoryStream ms = new MemoryStream();
                            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                            {
                                foreach (var item in responsables)
                                {
                                    string nombreArea = item.AreaResponsabilidadNavigation.Nombre;
                                    string nombreResponsable = item.Nombre + " " + item.Apellidos;
                                    tituloDoc = nombreArea + " (" + nombreResponsable + ", " + nombreMes.Replace('/', '-') + ").docx";

                                    Responsable respArea = new Responsable();
                                    respArea = item.AreaResponsabilidadNavigation.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();
                                    string responsableArea = respArea == null ? "<<S/D>>" : respArea.Nombre + " " + respArea.Apellidos;

                                    objFile = _repoServcioActas.ExportarPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, responsableArea);
                                    if (objFile != null)
                                    {
                                        var entry = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }
                                }

                                foreach (var item in areasResponsabilidad)
                                {
                                    tituloDoc = item.Nombre + " (" + nombreMes.Replace('/', '-') + ").docx";
                                    Responsable responArea = await _repoResponsables.SeleccionarResponsableEPDelArea(item.IdArea);// item.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();

                                    string responsableArea = String.Empty;

                                    if (responArea != null)
                                        responsableArea = responArea.Nombre + " " + responArea.Apellidos;
                                    else
                                    {
                                        responsableArea = "<<S/D>>";
                                    }

                                    objFile = _repoServcioActas.ExportarPorAreasDocx(out result, out msg, Guid.Empty, item.IdArea, model.Fecha, responsableArea);
                                    if (objFile != null)
                                    {
                                        var entry = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }
                                }

                                objFile = _repoServcioActas.ExportarADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                var entryEP = zip.CreateEntry(tituloDoc);

                                using (var fileStream = new MemoryStream(objFile))
                                using (var entryStream = entryEP.Open())
                                {
                                    fileStream.CopyTo(entryStream);
                                }
                            }

                            return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material (Movimientos - {nombreMes.Replace('/', '-')}).zip");
                        }
                        else
                        {
                            clGlobal.siNo = false;
                            clGlobal.objMensaje = $"No existen movimientos en la fecha seleccionada.";
                            return RedirectToAction(nameof(ActivosFijos), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha });
                        }
                    }
                    else
                    {
                        List<Responsable> responsablesActuales = new List<Responsable>();
                        List<Responsable> responsablesAnteriores = new List<Responsable>();
                        responsablesActuales = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos.Where(ex => ex.idResponsableActual.Equals(model.Responsable) || ex.idResponsableAnterior.Equals(model.Responsable)).ToList(), true);
                        responsablesAnteriores = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos.Where(ex => ex.idResponsableActual.Equals(model.Responsable) || ex.idResponsableAnterior.Equals(model.Responsable)).ToList(), false);
                        responsables = responsablesActuales.Union(responsablesAnteriores).ToList();
                        areasResponsabilidad = await _repoAreas.ObtenerListadoAreasSiMovimiento(existenciaDeMovimientos.Where(ex => ex.idResponsableActual.Equals(model.Responsable) || ex.idResponsableAnterior.Equals(model.Responsable)).ToList());
                        if (responsables.Count > 0)
                        {
                            MemoryStream ms = new MemoryStream();
                            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                            {
                                Guid areaActiva = Guid.Empty;
                                foreach (var item in responsables.Where(c => c.IdResponsable.Equals(model.Responsable)))
                                {
                                    string nombreArea = String.Empty;

                                    Responsable respArea = new Responsable();
                                    respArea = await _repoResponsables.SeleccionarResponsablePorArea(areaActiva, model.Responsable);

                                    if (respArea == null)
                                    {
                                        respArea = new Responsable();
                                        respArea = await _repoResponsables.SeleccionarResponsablesPorID(model.Responsable);
                                        nombreArea = respArea.AreaResponsabilidadNavigation.Nombre;
                                    }
                                    else
                                    {
                                        nombreArea = item.AreaResponsabilidadNavigation.Nombre;
                                    }


                                    string nombreResponsable = item.Nombre + " " + item.Apellidos;
                                    tituloDoc = nombreArea + " (" + nombreResponsable + ", " + nombreMes.Replace('/', '-') + ").docx";

                                    areaActiva = item.AreaResponsabilidadNavigation.IdArea;


                                    respArea = item.AreaResponsabilidadNavigation.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();

                                    string responsableArea = respArea == null ? "<<S/D>>" : respArea.Nombre + " " + respArea.Apellidos;

                                    objFile = _repoServcioActas.ExportarPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, responsableArea);
                                    if (objFile != null)
                                    {
                                        var entry = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }
                                }

                                foreach (var item in areasResponsabilidad)
                                {
                                    tituloDoc = item.Nombre + " (" + nombreMes.Replace('/', '-') + ").docx";
                                    Responsable responArea = item.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();

                                    string responsableArea = String.Empty;

                                    if (responArea != null)
                                        responsableArea = responArea.Nombre + " " + responArea.Apellidos;
                                    else
                                    {
                                        responsableArea = "<<S/D>>";
                                    }

                                    objFile = _repoServcioActas.ExportarPorAreasDocx(out result, out msg, Guid.Empty, item.IdArea, model.Fecha, responsableArea);
                                    var entry = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entry.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }

                                objFile = _repoServcioActas.ExportarADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                var entryEP = zip.CreateEntry(tituloDoc);

                                using (var fileStream = new MemoryStream(objFile))
                                using (var entryStream = entryEP.Open())
                                {
                                    fileStream.CopyTo(entryStream);
                                }
                            }

                            return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material (Movimientos - {nombreMes.Replace('/', '-')}).zip");
                        }
                        else
                        {
                            clGlobal.siNo = false;
                            clGlobal.objMensaje = $"No existen movimientos en la fecha seleccionada.";
                            return RedirectToAction(nameof(ActivosFijos), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha });
                        }
                    }
                }
                else
                {
                    if (model.Responsable == Guid.Empty)
                    {
                        List<Responsable> responsablesActuales = new List<Responsable>();
                        List<Responsable> responsablesAnteriores = new List<Responsable>();
                        responsablesActuales = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos.Where(ex => ex.idAreaAnterior.Equals(model.Area) || ex.idAreaActual.Equals(model.Area)).ToList(), true);
                        responsablesAnteriores = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos.Where(ex => ex.idAreaAnterior.Equals(model.Area) || ex.idAreaActual.Equals(model.Area)).ToList(), false);
                        responsables = responsablesActuales.Union(responsablesAnteriores).ToList();
                        areasResponsabilidad = await _repoAreas.ObtenerListadoAreasSiMovimiento(existenciaDeMovimientos.Where(ex => ex.idAreaActual.Equals(model.Area) || ex.idAreaAnterior.Equals(model.Area)).ToList());
                        if (responsables.Count > 0)
                        {
                            MemoryStream ms = new MemoryStream();
                            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                            {
                                foreach (var item in responsables)
                                {
                                    string nombreArea = item.AreaResponsabilidadNavigation.Nombre;
                                    string nombreResponsable = item.Nombre + " " + item.Apellidos;
                                    tituloDoc = nombreArea + " (" + nombreResponsable + ", " + nombreMes.Replace('/', '-') + ").docx";

                                    Responsable respArea = new Responsable();
                                    respArea = item.AreaResponsabilidadNavigation.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();
                                    string responsableArea = respArea == null ? "<<S/D>>" : respArea.Nombre + " " + respArea.Apellidos;

                                    objFile = _repoServcioActas.ExportarPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, responsableArea);
                                    if (objFile != null)
                                    {
                                        var entry = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }
                                }

                                foreach (var item in areasResponsabilidad.Where(c => c.IdArea.Equals(model.Area)))
                                {
                                    tituloDoc = item.Nombre + " (" + nombreMes.Replace('/', '-') + ").docx";
                                    Responsable responArea = item.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();

                                    string responsableArea = String.Empty;

                                    if (responArea != null)
                                        responsableArea = responArea.Nombre + " " + responArea.Apellidos;
                                    else
                                    {
                                        responsableArea = "<<S/D>>";
                                    }

                                    objFile = _repoServcioActas.ExportarPorAreasDocx(out result, out msg, Guid.Empty, item.IdArea, model.Fecha, responsableArea);
                                    if (objFile != null)
                                    {
                                        var entry = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }
                                }

                                objFile = _repoServcioActas.ExportarADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                var entryEP = zip.CreateEntry(tituloDoc);

                                using (var fileStream = new MemoryStream(objFile))
                                using (var entryStream = entryEP.Open())
                                {
                                    fileStream.CopyTo(entryStream);
                                }
                            }

                            return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material (Movimientos - {nombreMes.Replace('/', '-')}).zip");
                        }
                        else
                        {
                            clGlobal.siNo = false;
                            clGlobal.objMensaje = $"No existen movimientos en la fecha seleccionada.";
                            return RedirectToAction(nameof(ActivosFijos), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha });
                        }
                    }
                    else
                    {
                        List<Responsable> responsablesActuales = new List<Responsable>();
                        List<Responsable> responsablesAnteriores = new List<Responsable>();
                        responsablesActuales = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos
                            .Where(ex => (ex.idResponsableActual.Equals(model.Responsable) || ex.idResponsableAnterior.Equals(model.Responsable))
                            && (ex.idAreaActual.Equals(model.Area) || ex.idAreaAnterior.Equals(model.Area))).ToList(), true);
                        responsablesAnteriores = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos
                            .Where(ex => (ex.idResponsableActual.Equals(model.Responsable) || ex.idResponsableAnterior.Equals(model.Responsable))
                            && (ex.idAreaActual.Equals(model.Area) || ex.idAreaAnterior.Equals(model.Area))).ToList(), false);
                        responsables = responsablesActuales.Union(responsablesAnteriores).ToList();
                        areasResponsabilidad = await _repoAreas.ObtenerListadoAreasSiMovimiento(existenciaDeMovimientos
                            .Where(ex => (ex.idResponsableActual.Equals(model.Responsable) || ex.idResponsableAnterior.Equals(model.Responsable))
                            && (ex.idAreaActual.Equals(model.Area) || ex.idAreaAnterior.Equals(model.Area))).ToList());
                        if (responsables.Count > 0)
                        {
                            MemoryStream ms = new MemoryStream();
                            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                            {
                                foreach (var item in responsables.Where(c => c.IdResponsable.Equals(model.Responsable)))
                                {
                                    string nombreArea = item.AreaResponsabilidadNavigation.Nombre;
                                    string nombreResponsable = item.Nombre + " " + item.Apellidos;
                                    tituloDoc = nombreArea + " (" + nombreResponsable + ", " + nombreMes.Replace('/', '-') + ").docx";

                                    Responsable respArea = new Responsable();
                                    respArea = item.AreaResponsabilidadNavigation.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();
                                    string responsableArea = respArea == null ? "<<S/D>>" : respArea.Nombre + " " + respArea.Apellidos;

                                    objFile = _repoServcioActas.ExportarPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, responsableArea);
                                    if (objFile != null)
                                    {
                                        var entry = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }
                                }

                                foreach (var item in areasResponsabilidad.Where(c => c.IdArea.Equals(model.Area)))
                                {
                                    tituloDoc = item.Nombre + " (" + nombreMes.Replace('/', '-') + ").docx";
                                    Responsable responArea = item.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();

                                    string responsableArea = String.Empty;

                                    if (responArea != null)
                                        responsableArea = responArea.Nombre + " " + responArea.Apellidos;
                                    else
                                    {
                                        responsableArea = "<<S/D>>";
                                    }

                                    objFile = _repoServcioActas.ExportarPorAreasDocx(out result, out msg, Guid.Empty, item.IdArea, model.Fecha, responsableArea);
                                    if (objFile != null)
                                    {
                                        var entry = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }
                                }

                                objFile = _repoServcioActas.ExportarADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                var entryEP = zip.CreateEntry(tituloDoc);

                                using (var fileStream = new MemoryStream(objFile))
                                using (var entryStream = entryEP.Open())
                                {
                                    fileStream.CopyTo(entryStream);
                                }
                            }

                            return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material (Movimientos - {nombreMes.Replace('/', '-')}).zip");
                        }
                        else
                        {
                            clGlobal.siNo = false;
                            clGlobal.objMensaje = $"No existen movimientos en la fecha seleccionada.";
                            return RedirectToAction(nameof(ActivosFijos), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> DetallesDeMovimientos(Guid? area, Guid? responsable, DateTime? fecha)
        {
            List<SelectListItem> areas = new List<SelectListItem>();
            List<SelectListItem> responsables = new List<SelectListItem>();

            DateTime fechaParametro = fecha == null ? DateTime.Today : fecha.Value;

            if (area == null || area == Guid.Empty)
            {
                areas = _repoAreas.ListadoAreasDeResponsabilidad(fechaParametro).Result.Select(ar => new SelectListItem() { Text = ar.Nombre, Value = ar.IdArea.ToString() }).ToList();
                areas.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            }
            else
            {
                areas = _repoAreas.ListadoAreasDeResponsabilidad(fechaParametro).Result.Select(ar => new SelectListItem() { Text = ar.Nombre, Value = ar.IdArea.ToString(), Selected = ar.IdArea.Equals(area) }).ToList();
                areas.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString() });
            }

            if (responsable == null || responsable == Guid.Empty)
            {
                responsables = _repoResponsables.ObtenerListadoResponsables(fechaParametro).Result.Select(res => new SelectListItem() { Text = res.Nombre != "<<S/R>>" ? res.Nombre + " " + res.Apellidos : res.Nombre, Value = res.IdResponsable.ToString() }).ToList();
                responsables.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            }
            else
            {
                responsables = _repoResponsables.ObtenerListadoResponsables(fechaParametro).Result.Select(res => new SelectListItem() { Text = res.Nombre != "<<S/R>>" ? res.Nombre + " " + res.Apellidos : res.Nombre, Value = res.IdResponsable.ToString(), Selected = res.IdResponsable.Equals(responsable) }).ToList();
                responsables.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString() });
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Listado de Movimientos");

            ViewData["respuesta"] = new clResponse<mdActas>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vDetallesDeMovimientos", new mdActas() { Areas = areas, Responsables = responsables, Fecha = fecha == null ? DateTime.Today.AddMonths(-1) : new DateTime(fecha.Value.Year, fecha.Value.Month, fecha.Value.Day) });
        }

        [HttpPost]
        public async Task<IActionResult> MostrarDetallesMovimientos(DateTime fecha, string area, string responsable)
        {
            var forms = HttpContext.Request.Form;
            DateTime fechaMov = new DateTime();

            if (!DateTime.TryParse(forms["fechaMov"].ToString(), out fechaMov))
            {

            }

            return View("vDetallesDeMovimientos");
        }

        private Paragraph CreateAParagraphWithStyles(Paragraph p)
        {
            // Run 1
            DocumentFormat.OpenXml.Wordprocessing.Run r1 = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.Text t1 = new DocumentFormat.OpenXml.Wordprocessing.Text("Pellentesque ") { Space = SpaceProcessingModeValues.Preserve };
            // The Space attribute preserve white space before and after your text
            r1.Append(t1);
            p.Append(r1);

            // Run 2 - Bold
            DocumentFormat.OpenXml.Wordprocessing.Run r2 = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.RunProperties rp2 = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();
            rp2.Bold = new DocumentFormat.OpenXml.Wordprocessing.Bold();
            // Always add properties first
            r2.Append(rp2);
            DocumentFormat.OpenXml.Wordprocessing.Text t2 = new DocumentFormat.OpenXml.Wordprocessing.Text("commodo ") { Space = SpaceProcessingModeValues.Preserve };
            r2.Append(t2);
            p.Append(r2);

            // Run 3
            DocumentFormat.OpenXml.Wordprocessing.Run r3 = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.Text t3 = new DocumentFormat.OpenXml.Wordprocessing.Text("rhoncus ") { Space = SpaceProcessingModeValues.Preserve };
            r3.Append(t3);
            p.Append(r3);

            // Run 4 – Italic
            DocumentFormat.OpenXml.Wordprocessing.Run r4 = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.RunProperties rp4 = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();
            rp4.Italic = new DocumentFormat.OpenXml.Wordprocessing.Italic();
            // Always add properties first
            r4.Append(rp4);
            DocumentFormat.OpenXml.Wordprocessing.Text t4 = new DocumentFormat.OpenXml.Wordprocessing.Text("mauris") { Space = SpaceProcessingModeValues.Preserve };
            r4.Append(t4);
            p.Append(r4);

            // Run 5
            DocumentFormat.OpenXml.Wordprocessing.Run r5 = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.Text t5 = new DocumentFormat.OpenXml.Wordprocessing.Text(", sit ") { Space = SpaceProcessingModeValues.Preserve };
            r5.Append(t5);
            p.Append(r5);

            // Run 6 – Italic , bold and underlined
            DocumentFormat.OpenXml.Wordprocessing.Run r6 = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.RunProperties rp6 = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();
            rp6.Italic = new DocumentFormat.OpenXml.Wordprocessing.Italic();
            rp6.Bold = new DocumentFormat.OpenXml.Wordprocessing.Bold();
            rp6.Underline = new DocumentFormat.OpenXml.Wordprocessing.Underline();
            // Always add properties first
            r6.Append(rp6);
            DocumentFormat.OpenXml.Wordprocessing.Text t6 = new DocumentFormat.OpenXml.Wordprocessing.Text("amet ") { Space = SpaceProcessingModeValues.Preserve };
            r6.Append(t6);
            p.Append(r6);

            // Run 7
            DocumentFormat.OpenXml.Wordprocessing.Run r7 = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.Text t7 = new DocumentFormat.OpenXml.Wordprocessing.Text("faucibus arcu ") { Space = SpaceProcessingModeValues.Preserve };
            r7.Append(t7);
            p.Append(r7);

            // Run 8 – Red color
            DocumentFormat.OpenXml.Wordprocessing.Run r8 = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.RunProperties rp8 = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();
            rp8.Color = new DocumentFormat.OpenXml.Wordprocessing.Color() { Val = "FF0000" };
            // Always add properties first
            r8.Append(rp8);
            DocumentFormat.OpenXml.Wordprocessing.Text t8 = new DocumentFormat.OpenXml.Wordprocessing.Text("porttitor ") { Space = SpaceProcessingModeValues.Preserve };
            r8.Append(t8);
            p.Append(r8);

            // Run 9
            DocumentFormat.OpenXml.Wordprocessing.Run r9 = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.Text t9 = new DocumentFormat.OpenXml.Wordprocessing.Text("pharetra. Maecenas quis erat quis eros iaculis placerat ut at mauris.") { Space = SpaceProcessingModeValues.Preserve };
            r9.Append(t9);
            p.Append(r9);
            // Add your paragraph to docx body

            return p;
        }

        private Paragraph CreateAParagraphWithJustification(Paragraph p)
        {
            ParagraphProperties pp = new ParagraphProperties();
            pp.Justification = new Justification() { Val = JustificationValues.Center };
            // Add paragraph properties to your paragraph
            p.Append(pp);
            // Run
            DocumentFormat.OpenXml.Wordprocessing.Run r = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.Text t = new DocumentFormat.OpenXml.Wordprocessing.Text("Nam eu tortor ut mi euismod eleifend in ut ante. Donec a ligula ante. Sed rutrum ex quam. Nunc id mi ultricies, vestibulum sapien vel, posuere dui.") { Space = SpaceProcessingModeValues.Preserve };
            r.Append(t);
            p.Append(r);

            return p;
        }

        // Apply a style to a paragraph.
        public static void AddStyleToDoc(MainDocumentPart mainPart, string styleid, string stylename, StyleRunProperties styleRunProperties)
        {

            // Get the Styles part for this document.
            StyleDefinitionsPart part =
                mainPart.StyleDefinitionsPart;

            // If the Styles part does not exist, add it and then add the style.
            if (part == null)
            {
                part = AddStylesPartToPackage(mainPart);
                AddNewStyle(part, styleid, stylename, styleRunProperties);
            }
            else
            {
                // If the style is not in the document, add it.
                if (IsStyleIdInDocument(mainPart, styleid) != true)
                {
                    // No match on styleid, so let's try style name.
                    string styleidFromName = GetStyleIdFromStyleName(mainPart, stylename);
                    if (styleidFromName == null)
                    {
                        AddNewStyle(part, styleid, stylename, styleRunProperties);
                    }
                    else
                        styleid = styleidFromName;
                }
            }

        }

        // Return styleid that matches the styleName, or null when there's no match.
        private static string GetStyleIdFromStyleName(MainDocumentPart mainPart, string styleName)
        {
            StyleDefinitionsPart stylePart = mainPart.StyleDefinitionsPart;
            string styleId = stylePart.Styles.Descendants<StyleName>()
                .Where(s => s.Val.Value.Equals(styleName) &&
                    (((Style)s.Parent).Type == StyleValues.Paragraph))
                .Select(n => ((Style)n.Parent).StyleId).FirstOrDefault();
            return styleId;
        }

        // Add a StylesDefinitionsPart to the document.  Returns a reference to it.
        public static StyleDefinitionsPart AddStylesPartToPackage(MainDocumentPart mainPart)
        {
            StyleDefinitionsPart part;
            part = mainPart.AddNewPart<StyleDefinitionsPart>();
            DocumentFormat.OpenXml.Wordprocessing.Styles root = new DocumentFormat.OpenXml.Wordprocessing.Styles();
            root.Save(part);
            return part;
        }

        public static bool IsStyleIdInDocument(MainDocumentPart mainPart, string styleid)
        {
            // Get access to the Styles element for this document.
            DocumentFormat.OpenXml.Wordprocessing.Styles s = mainPart.StyleDefinitionsPart.Styles;

            // Check that there are styles and how many.
            int n = s.Elements<DocumentFormat.OpenXml.Wordprocessing.Style>().Count();
            if (n == 0)
                return false;

            // Look for a match on styleid.
            DocumentFormat.OpenXml.Wordprocessing.Style style = s.Elements<DocumentFormat.OpenXml.Wordprocessing.Style>()
                .Where(st => (st.StyleId == styleid) && (st.Type == StyleValues.Paragraph))
                .FirstOrDefault();
            if (style == null)
                return false;

            return true;
        }

        // Create a new style with the specified styleid and stylename and add it to the specified style definitions part.
        private static void AddNewStyle(StyleDefinitionsPart styleDefinitionsPart, string styleid, string stylename, StyleRunProperties styleRunProperties)
        {
            // Get access to the root element of the styles part.
            DocumentFormat.OpenXml.Wordprocessing.Styles styles = styleDefinitionsPart.Styles;

            // Create a new paragraph style and specify some of the properties.
            DocumentFormat.OpenXml.Wordprocessing.Style style = new DocumentFormat.OpenXml.Wordprocessing.Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = styleid,
                CustomStyle = true
            };
            style.Append(new StyleName() { Val = stylename });
            style.Append(new BasedOn() { Val = "Normal" });
            style.Append(new NextParagraphStyle() { Val = "Normal" });
            style.Append(new UIPriority() { Val = 900 });

            // Create the StyleRunProperties object and specify some of the run properties.


            // Add the run properties to the style.
            // --- Here we use the OuterXml. If you are using the same var twice, you will get an error. So to be sure just insert the xml and you will get through the error.
            style.Append(styleRunProperties);

            // Add the style to the styles part.
            styles.Append(style);
        }

        // Insert a table into a word processing document.
        private void CreateTable(string fileName)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Use the file name and path passed in as an argument 
                // to open an existing Word 2007 document.

                using (WordprocessingDocument doc
                = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
                {
                    // Create an empty table.
                    DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();

                    // Create a TableProperties object and specify its border information.
                    TableProperties tblProp = new TableProperties(
                        new TableBorders(
                            new DocumentFormat.OpenXml.Wordprocessing.TopBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Dashed),
                                Size = 24
                            },
                            new DocumentFormat.OpenXml.Wordprocessing.BottomBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Dashed),
                                Size = 24
                            },
                            new DocumentFormat.OpenXml.Wordprocessing.LeftBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Dashed),
                                Size = 24
                            },
                            new DocumentFormat.OpenXml.Wordprocessing.RightBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Dashed),
                                Size = 24
                            },
                            new InsideHorizontalBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Dashed),
                                Size = 24
                            },
                            new InsideVerticalBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Dashed),
                                Size = 24
                            }
                        )
                    );

                    // Append the TableProperties object to the empty table.
                    table.AppendChild<TableProperties>(tblProp);

                    // Create a row.
                    TableRow tr = new TableRow();

                    // Create a cell.
                    TableCell tc1 = new TableCell();

                    // Specify the width property of the table cell.
                    tc1.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));

                    // Specify the table cell content.
                    tc1.Append(new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("some text"))));

                    // Append the table cell to the table row.
                    tr.Append(tc1);

                    // Create a second table cell by copying the OuterXml value of the first table cell.
                    TableCell tc2 = new TableCell(tc1.OuterXml);

                    // Append the table cell to the table row.
                    tr.Append(tc2);

                    // Append the table row to the table.
                    table.Append(tr);

                    // Append the table to the document.
                    doc.MainDocumentPart.Document.Body.Append(table);
                }
            }
        }

        private List<clBreadCumb> LlenarBreadsCumbs(string titulo)
        {
            List<clBreadCumb> prueba = new List<clBreadCumb>();
            clBreadCumb objBreadsCumbs = new clBreadCumb()
            {
                nombre = "Activos Fijos",
                ruta = new List<string>() { "ActivosFijos", "ctActFijos" }
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

        public async Task<IActionResult> MovimientosDelAFT()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Movimientos de Activos Fijos");

            ViewData["respuesta"] = new clResponse<mdMovimientosAFT>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vMovimientosActivosFijos", await _repoActivosFijos.ListadoActivosFijos());
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeMovimientosDelAFT(long noInventario)
        {
            try
            {
                List<ActivosFijosMovimientos> actFijosList = await _repoActivosFijos.ListadoActivosFijosMovimientos(noInventario);

                return PartialView("_vpTablaActivosFijosMovimientos", actFijosList);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<IActionResult> MovimientosDelAFTPorResponsable()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Movimientos de Activos Fijos");

            ViewData["respuesta"] = new clResponse<mdMovimientosAFT>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vMovimientosActivosFijosPorResponsable", await _repoResponsables.ObtenerListadoResponsables());
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeMovimientosDelAFTPorResponsable(string responsable)
        {
            try
            {
                List<ActivosFijosMovimientosResponsables> actFijosList = await _repoActivosFijos.ListadoActivosFijosMovimientos(Guid.Parse(responsable));

                return PartialView("_vpTablaActivosFijosMovimientosPorResponsable", actFijosList);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<IActionResult> Transferencias()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Transferencias de Activos Fijos");

            ViewData["respuesta"] = new clResponse<mdMovimientosAFT>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vTransferenciasActivosFijos", await _repoActivosFijos.ListadoActivosFijos());
        }

        public async Task<IActionResult> Traslados()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Traslados de Activos Fijos");

            ViewData["respuesta"] = new clResponse<mdMovimientosAFT>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vTrasladosActivosFijos", await _repoActivosFijos.ListadoActivosFijos());
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeTrasladosDeAFT()
        {
            try
            {
                List<ActivosFijosMovimientos> actFijosList = new List<ActivosFijosMovimientos>();

                return PartialView("_vpTablaActivosFijosMovimientos", actFijosList);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<IActionResult> ClasificacionAFT()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Clasificar Activos Fijos");

            ViewData["respuesta"] = new clResponse<mdActas>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vClasificarAFT");
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeClasificadores()
        {
            List<Clasificador> clasificadorList = await _repoClasificador.ObtenerListadoClasificador().ConfigureAwait(false);

            return PartialView("_vpTablaClasificador", clasificadorList);
        }

        [HttpPost]
        public async Task<JsonResult> ListadoDeAFTSinClasificar()
        {
            List<ActivosFijosSinClasificar> clasificadorList = await _repoActivosFijos.ListadoActivosFijosSinClasificar().ConfigureAwait(false);

            var result = new
            {
                TotalRows = clasificadorList.Count(),
                Rows = clasificadorList
            };
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> ListadoDeAFTClasificados()
        {
            List<ActivosFijosClasificados> clasificadorList = await _repoActivosFijos.ListadoActivosFijosClasificados().ConfigureAwait(false);

            var result = new
            {
                TotalRows = clasificadorList.Count(),
                Rows = clasificadorList
            };
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> GuardarAFTClasificados(string indexes, string referencia)
        {
            try
            {
                List<ActFijo> actFijos = new List<ActFijo>();
                DateTime fechaSubmayor = await _repoActivosFijos.UltimoSubmayor();
                actFijos = await _repoActivosFijos.ListadoActivosFijos(fechaSubmayor).ConfigureAwait(false);
                var result = new
                {
                    siSatisfactorio = false,
                    problema = ""
                };

                clResponse<ActFijo> response = new clResponse<ActFijo>();

                foreach (var item in indexes.Split('|'))
                {
                    ActFijo aft = new ActFijo();
                    aft = actFijos.Find(a => a.NoInventario.Equals(long.Parse(item.ToString())));

                    if (aft != null)
                    {
                        aft.Clasificador = Guid.Parse(referencia);

                        await _repoActivosFijos.ModificarActivoFijo(aft);

                    }
                }

                result = new
                {
                    siSatisfactorio = true,
                    problema = "SIN PROBLEMAS"
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new
                {
                    siSatisfactorio = false,
                    problema = ex.Message
                };

                return Json(result);
            }
        }

        [HttpPost]
        public async Task<JsonResult> GuardarAFTDesclasificados(string indexes)
        {
            try
            {
                List<ActFijo> actFijos = new List<ActFijo>();
                DateTime fechaSubmayor = await _repoActivosFijos.UltimoSubmayor();
                actFijos = await _repoActivosFijos.ListadoActivosFijos(fechaSubmayor).ConfigureAwait(false);
                var result = new
                {
                    siSatisfactorio = false,
                    problema = ""
                };

                clResponse<ActFijo> response = new clResponse<ActFijo>();

                foreach (var item in indexes.Split('|'))
                {
                    ActFijo aft = new ActFijo();
                    aft = actFijos.Find(a => a.NoInventario.Equals(long.Parse(item.ToString())));

                    if (aft != null)
                    {
                        aft.Clasificador = await _repoClasificador.ObtenerReferenciaSinClasificar();

                        await _repoActivosFijos.ModificarActivoFijo(aft);

                    }
                }

                result = new
                {
                    siSatisfactorio = true,
                    problema = "SIN PROBLEMAS"
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new
                {
                    siSatisfactorio = false,
                    problema = ex.Message
                };

                return Json(result);
            }
        }

        public async Task<IActionResult> ExportarAFTClasificados(Guid? centroCosto, Guid? clasificador)
        {
            List<SelectListItem> centrosDeCostos = new List<SelectListItem>();
            List<SelectListItem> clasificadores = new List<SelectListItem>();

            if (centroCosto == null || centroCosto == Guid.Empty)
            {
                centrosDeCostos = _repoCentroDeCostos.ListadoCentroDeCosto().Result.Select(ar => new SelectListItem() { Text = ar.Nombre, Value = ar.IdCentroCosto.ToString() }).ToList();
                centrosDeCostos.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            }
            else
            {
                centrosDeCostos = _repoCentroDeCostos.ListadoCentroDeCosto().Result.Select(ar => new SelectListItem() { Text = ar.Nombre, Value = ar.IdCentroCosto.ToString(), Selected = ar.IdCentroCosto.Equals(centroCosto) }).ToList();
                centrosDeCostos.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString() });
            }

            if (clasificador == null || clasificador == Guid.Empty)
            {
                clasificadores = _repoClasificador.ObtenerListadoClasificador().Result.Select(res => new SelectListItem() { Text = res.Nombre, Value = res.IdClasificador.ToString() }).ToList();
                clasificadores.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            }
            else
            {
                clasificadores = _repoClasificador.ObtenerListadoClasificador().Result.Select(res => new SelectListItem() { Text = res.Nombre, Value = res.IdClasificador.ToString(), Selected = res.IdClasificador.Equals(clasificador) }).ToList();
                clasificadores.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString() });
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Exportar Activos Fijos Clasificados");

            ViewData["respuesta"] = new clResponse<mdExportarAFTClasificados>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vExportarAFTClasificados", new mdExportarAFTClasificados() { CentrosDeCostos = centrosDeCostos, Clasificadores = clasificadores });
        }

        [HttpPost]
        public async Task<IActionResult> CargarTablaAFTClasificados(string centroCosto, string clasificador)
        {
            List<ActivosFijosClasificados> aftClasificados = new List<ActivosFijosClasificados>();
            if ((centroCosto == null || Guid.Parse(centroCosto) == Guid.Empty) && (clasificador == null || Guid.Parse(clasificador) == Guid.Empty))
            {
                aftClasificados = await _repoActivosFijos.ListadoActivosFijosClasificados();
            }
            else if ((centroCosto != null && Guid.Parse(centroCosto) != Guid.Empty) && (clasificador == null || Guid.Parse(clasificador) == Guid.Empty))
            {
                aftClasificados = await _repoActivosFijos.ListadoActivosFijosClasificados(Guid.Parse(centroCosto));
            }
            else if ((centroCosto == null || Guid.Parse(centroCosto) == Guid.Empty) && (clasificador != null && Guid.Parse(clasificador) != Guid.Empty))
            {
                aftClasificados = await _repoActivosFijos.ListadoActivosFijosClasificados(Guid.Parse(clasificador), true);
            }
            else if ((centroCosto != null && Guid.Parse(centroCosto) != Guid.Empty) && (clasificador != null && Guid.Parse(clasificador) != Guid.Empty))
            {
                aftClasificados = await _repoActivosFijos.ListadoActivosFijosClasificados(Guid.Parse(centroCosto), Guid.Parse(clasificador));
            }

            return PartialView("_vpTablaExportarAFTClasificados", aftClasificados);
        }

        [HttpPost]
        public async Task<IActionResult> CargarTablaClasificadoresTotales(string centroCosto)
        {
            List<ClasificadoresTotales> clasifTotales = new List<ClasificadoresTotales>();
            if (centroCosto == null || Guid.Parse(centroCosto) == Guid.Empty)
            {
                clasifTotales = await _repoClasificador.ListadoClasificadoresTotales();
            }
            else if (centroCosto != null && Guid.Parse(centroCosto) != Guid.Empty)
            {
                clasifTotales = await _repoClasificador.ListadoClasificadoresTotales(Guid.Parse(centroCosto));
            }

            return PartialView("_vpTablaClasificadoresTotales", clasifTotales);
        }

        //[HttpPost]
        //public async Task<IActionResult> ExportarExcelAFTClasificados()
        //{
        //    string valorInput = Request.Form["valorInput"];
        //    return File(await ExportarExcelExistenciasEPPlus(valorInput), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        //}

        public async Task<IActionResult> ExportarExcelClasificadoresTotalesEPPlus(string centroCosto)
        {
            try
            {
                string titulo = "";

                List<ClasificadoresTotales> clasifTotales = new List<ClasificadoresTotales>();
                if (centroCosto == null || Guid.Parse(centroCosto) == Guid.Empty)
                {
                    clasifTotales = await _repoClasificador.ListadoClasificadoresTotales();
                    titulo = "TODOS";
                }
                else if (centroCosto != null && Guid.Parse(centroCosto) != Guid.Empty)
                {
                    clasifTotales = await _repoClasificador.ListadoClasificadoresTotales(Guid.Parse(centroCosto));
                    CentroCosto cc = new CentroCosto();
                    cc = await _repoCentroDeCostos.EncontrarCentroCostoPorID(Guid.Parse(centroCosto));
                    titulo = cc.Nombre ?? "S/N";
                }

                string excelName = "";
                var fecha = "(" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString() + " " + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + ")";
                excelName = "Clasificadores (" + titulo + ") " + fecha + ".xlsx";

                if (clasifTotales.Count > 0)
                {
                    var exportbytes = ExporttoExcel<ClasificadoresTotales>(clasifTotales, excelName);
                    return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
                }
                else
                {
                    TempData["Message"] = "No Data to Export";
                    return View("vExportarAFTClasificados");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                TempData["Message"] = "No Data to Export";
                return View("vExportarAFTClasificados");
            }
        }

        private byte[] ExporttoExcel<T>(List<T> table, string filename)
        {
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet workSheet = pack.Workbook.Worksheets.Add(filename);
            workSheet.View.ShowGridLines = false;
            workSheet.Cells["B2"].Value = "Clasificador";
            workSheet.Cells["C2"].Value = "Cantidad";
            workSheet.Cells["B2:C2"].Style.Font.Bold = true;

            workSheet.Column(2).Width =40;
            workSheet.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
            workSheet.Column(2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            workSheet.Column(2).Style.WrapText = true;

            workSheet.Column(3).Width = 15;
            workSheet.Column(3).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
            workSheet.Column(3).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            workSheet.Cells["B2"].LoadFromCollection(table, true, OfficeOpenXml.Table.TableStyles.Light1);
            return pack.GetAsByteArray();
        }

        [HttpGet]
        public async Task<IActionResult> MuestreoDiezPorCiento()
        {
            int mes = DateTime.Today.Month - 1;
            int anio = mes == 0 ? DateTime.Today.Year - 1 : DateTime.Today.Year;
            if (mes == 0) mes = 12;

            List<MuestreoMensualDiezPorCiento> muestra;
            bool muestraPendiente = false;
            bool yaExisteConfirmado = false;

            var sessionJson = HttpContext.Session.GetString(SessionKeyMuestra(anio, mes));
            if (sessionJson != null)
            {
                var pendiente = JsonSerializer.Deserialize<mdMuestreoPendiente>(sessionJson, _jsonOptions);
                if (pendiente?.Items.Count > 0)
                {
                    muestraPendiente = true;
                    muestra = pendiente.Items.Select(i => new MuestreoMensualDiezPorCiento
                    {
                        Anno = pendiente.Anno,
                        Mes = pendiente.Mes,
                        NoInventario = i.NoInventario,
                        SubNumero = i.SubNumero,
                        FechaSubmayor = i.FechaSubmayor,
                        Descripcion = i.Descripcion,
                        CentroCosto = i.CentroCosto,
                        SiExiste = i.SiExiste,
                        Observaciones = i.Observaciones,
                        FechaMuestreo = DateTime.Now,
                        ActFijoNavigation = new ActFijo
                        {
                            NoInventario = i.NoInventario,
                            SubNumero = i.SubNumero,
                            FechaSubmayor = i.FechaSubmayor,
                            AreaResponsabilidad = i.AreaResponsabilidad,
                            ResponsableNavigation = i.NombreResponsable != null ? new Responsable
                            {
                                Nombre = i.NombreResponsable,
                                Apellidos = i.ApellidosResponsable
                            } : null
                        },
                        CentroCostoNavigation = i.NombreCentroCosto != null ? new CentroCosto
                        {
                            Nombre = i.NombreCentroCosto
                        } : null
                    }).ToList();
                }
                else
                {
                    muestra = new List<MuestreoMensualDiezPorCiento>();
                }
            }
            else
            {
                yaExisteConfirmado = await _context.MuestreoMensualDiezPorCientos
                    .AnyAsync(m => m.Anno == anio && m.Mes == mes);

                if (yaExisteConfirmado)
                {
                    clGlobal.siNo = false;
                    clGlobal.objMensaje = $"Ya existe un muestreo confirmado para {mes}/{anio}.";
                }

                muestra = new List<MuestreoMensualDiezPorCiento>();
            }

            var activosDelMes = await _context.ActFijos
                .Where(a => a.FechaSubmayor.Year == anio && a.FechaSubmayor.Month == mes)
                .Select(a => new { a.NoInventario, a.SubNumero, a.AreaResponsabilidad, AreaNombre = a.AreaResponsabilidadNavigation != null ? a.AreaResponsabilidadNavigation.Nombre : "" })
                .Distinct()
                .ToListAsync();

            int totalActivosDistintos = activosDelMes.Count;
            int totalAreas = activosDelMes
                .Where(a => a.AreaResponsabilidad != null)
                .Select(a => a.AreaResponsabilidad)
                .Distinct()
                .Count();

            var areaNombreLookup = activosDelMes
                .Where(a => a.AreaResponsabilidad != null)
                .GroupBy(a => a.AreaResponsabilidad)
                .ToDictionary(g => g.Key, g => g.First().AreaNombre ?? "");

            List<Models.Auxiliar.mdMuestreoResumenArea> resumenAreas;
            int limiteMuestra;
            int areasCubiertas;
            List<MuestreoMensualDiezPorCiento> muestraFiltrada;

            if (muestraPendiente)
            {
                resumenAreas = muestra
                    .Where(m => m.ActFijoNavigation?.AreaResponsabilidad != null)
                    .GroupBy(m => m.ActFijoNavigation.AreaResponsabilidad)
                    .Select(g => new Models.Auxiliar.mdMuestreoResumenArea
                    {
                        AreaResponsabilidad = g.Key,
                        AreaNombre = areaNombreLookup.GetValueOrDefault(g.Key) ?? "",
                        Cantidad = g.Count(),
                        Inventarios = string.Join(", ", g.Select(m => m.NoInventario))
                    })
                    .OrderByDescending(r => r.Cantidad)
                    .ToList();

                limiteMuestra = muestra.Count;
                areasCubiertas = resumenAreas.Count;
                muestraFiltrada = muestra;
            }
            else
            {
                resumenAreas = new List<Models.Auxiliar.mdMuestreoResumenArea>();
                limiteMuestra = 0;
                areasCubiertas = 0;
                muestraFiltrada = new List<MuestreoMensualDiezPorCiento>();
            }

            ViewData["totalActivosDistintos"] = totalActivosDistintos;
            ViewData["limiteMuestra"] = limiteMuestra;
            ViewData["totalAreas"] = totalAreas;
            ViewData["areasCubiertas"] = areasCubiertas;
            ViewData["resumenAreas"] = resumenAreas;
            ViewData["yaExisteMuestreo"] = muestraFiltrada.Count > 0;
            ViewData["muestraPendiente"] = muestraPendiente;
            ViewData["yaExisteConfirmado"] = yaExisteConfirmado;
            ViewData["mesMuestreo"] = mes;
            ViewData["anioMuestreo"] = anio;

            ViewData["breadsC"] = LlenarBreadsCumbs("Muestreo 10%");

            ViewData["respuesta"] = new clResponse<List<MuestreoMensualDiezPorCiento>>()
            {
                SiSatisfactorio = clGlobal.siNo,
                Mensaje = clGlobal.objMensaje
            };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vMuestreoDiezPorCiento", muestraFiltrada);
        }

        [HttpGet]
        public async Task<IActionResult> ConsultaMuestreoDiezPorCiento(DateTime? fecha)
        {
            int mes = fecha?.Month ?? 0;
            int anio = fecha?.Year ?? 0;

            ViewData["mesMuestreo"] = mes;
            ViewData["anioMuestreo"] = anio;
            ViewData["breadsC"] = LlenarBreadsCumbs("Consulta Muestreo 10%");

            if (fecha == null)
            {
                ViewData["yaExisteMuestreo"] = false;
                return View("vConsultaMuestreoDiezPorCiento", new List<MuestreoMensualDiezPorCiento>());
            }

            var muestra = await _context.MuestreoMensualDiezPorCientos
                .Include(m => m.ActFijoNavigation).ThenInclude(a => a.AreaResponsabilidadNavigation)
                .Include(m => m.ActFijoNavigation).ThenInclude(a => a.ResponsableNavigation)
                .Include(m => m.CentroCostoNavigation)
                .Where(m => m.Anno == anio && m.Mes == mes)
                .ToListAsync();

            var activosDelMes = await _context.ActFijos
                .Where(a => a.FechaSubmayor.Year == anio && a.FechaSubmayor.Month == mes)
                .Select(a => new { a.NoInventario, a.SubNumero, a.AreaResponsabilidad, AreaNombre = a.AreaResponsabilidadNavigation != null ? a.AreaResponsabilidadNavigation.Nombre : "" })
                .Distinct()
                .ToListAsync();

            int totalActivosDistintos = activosDelMes.Count;
            int limiteMuestra = (int)Math.Max(1, Math.Floor(totalActivosDistintos * 0.10));

            var areaTotals = activosDelMes
                .Where(a => a.AreaResponsabilidad != null)
                .GroupBy(a => a.AreaResponsabilidad)
                .Select(g => new { Area = g.Key, Count = g.Count(), Nombre = g.First().AreaNombre ?? "" })
                .OrderByDescending(x => x.Count)
                .ToList();

            int totalAreas = areaTotals.Count;

            var actualLookup = muestra
                .Where(m => m.ActFijoNavigation?.AreaResponsabilidad != null)
                .GroupBy(m => m.ActFijoNavigation.AreaResponsabilidad)
                .ToDictionary(g => g.Key, g => g.ToList());

            int totalMuestra = limiteMuestra;
            var expected = new Dictionary<Guid?, int>();
            var residuals = new Dictionary<Guid?, double>();
            int sumaBase = 0;
            foreach (var at in areaTotals)
            {
                double raw = (double)at.Count * totalMuestra / totalActivosDistintos;
                int baseVal = (int)Math.Floor(raw);
                expected[at.Area] = baseVal;
                residuals[at.Area] = raw - baseVal;
                sumaBase += baseVal;
            }
            int rem = totalMuestra - sumaBase;
            if (rem > 0)
            {
                foreach (var kvp in residuals.OrderByDescending(r => r.Value).ThenByDescending(r => expected[r.Key]).Take(rem))
                {
                    expected[kvp.Key] = expected[kvp.Key] + 1;
                }
            }

            var areaNombreLookup = areaTotals.ToDictionary(at => at.Area, at => at.Nombre);

            List<Models.Auxiliar.mdMuestreoResumenArea> resumenAreas = expected
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => new Models.Auxiliar.mdMuestreoResumenArea
                {
                    AreaResponsabilidad = kvp.Key,
                    AreaNombre = areaNombreLookup.ContainsKey(kvp.Key) ? areaNombreLookup[kvp.Key] : "",
                    Cantidad = kvp.Value,
                    Inventarios = actualLookup.ContainsKey(kvp.Key)
                        ? string.Join(", ", actualLookup[kvp.Key].Take(kvp.Value).Select(m => m.NoInventario))
                        : ""
                })
                .OrderByDescending(r => r.Cantidad)
                .ToList();

            int areasCubiertas = resumenAreas.Count;

            var selectedKeys = new HashSet<(long NoInventario, int SubNumero, DateTime FechaSubmayor)>();
            foreach (var area in resumenAreas)
            {
                if (actualLookup.TryGetValue(area.AreaResponsabilidad, out var assets))
                {
                    foreach (var asset in assets.Take(area.Cantidad))
                        selectedKeys.Add((asset.NoInventario, asset.SubNumero, asset.FechaSubmayor));
                }
            }
            var muestraFiltrada = muestra.Where(m => selectedKeys.Contains((m.NoInventario, m.SubNumero, m.FechaSubmayor))).ToList();

            ViewData["totalActivosDistintos"] = totalActivosDistintos;
            ViewData["limiteMuestra"] = limiteMuestra;
            ViewData["totalAreas"] = totalAreas;
            ViewData["areasCubiertas"] = areasCubiertas;
            ViewData["resumenAreas"] = resumenAreas;
            ViewData["yaExisteMuestreo"] = muestraFiltrada.Count > 0;

            var hoy = DateTime.Today;
            int mesEditable = hoy.Month == 1 ? 12 : hoy.Month - 1;
            int anioEditable = hoy.Month == 1 ? hoy.Year - 1 : hoy.Year;
            ViewData["editable"] = anio == anioEditable && mes == mesEditable && muestraFiltrada.Count > 0;

            return View("vConsultaMuestreoDiezPorCiento", muestraFiltrada);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EjecutarMuestreo()
        {
            int mes = DateTime.Today.Month - 1;
            int anio = mes == 0 ? DateTime.Today.Year - 1 : DateTime.Today.Year;
            if (mes == 0) mes = 12;

            try
            {
                var activosDelMes = await _context.ActFijos
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Include(a => a.ResponsableNavigation)
                    .Include(a => a.CentroCostoNavigation)
                    .Where(a => a.FechaSubmayor.Year == anio && a.FechaSubmayor.Month == mes)
                    .ToListAsync();

                if (activosDelMes.Count == 0)
                {
                    clGlobal.siNo = false;
                    clGlobal.objMensaje = $"No existen activos fijos registrados en el submayor del período {mes}/{anio}. No es posible ejecutar el muestreo.";
                }
                else
                {
                    bool yaExisteBD = await _context.MuestreoMensualDiezPorCientos
                        .AnyAsync(m => m.Anno == anio && m.Mes == mes);

                    if (yaExisteBD)
                    {
                        clGlobal.siNo = false;
                        clGlobal.objMensaje = $"El muestreo de {mes}/{anio} ya fue ejecutado y confirmado anteriormente.";
                    }
                    else
                    {
                        var yaMuestreados = await _context.MuestreoMensualDiezPorCientos
                            .Where(m => m.Anno == anio)
                            .Select(m => new { m.NoInventario, m.SubNumero, m.FechaSubmayor })
                            .ToListAsync();

                        var excluidos = yaMuestreados
                            .Select(m => (m.NoInventario, m.SubNumero, m.FechaSubmayor))
                            .ToHashSet();

                        var activosDisponibles = activosDelMes
                            .Where(a => !excluidos.Contains((a.NoInventario, a.SubNumero, a.FechaSubmayor)))
                            .ToList();

                        if (activosDisponibles.Count == 0)
                        {
                            clGlobal.siNo = false;
                            clGlobal.objMensaje = $"Todos los activos del período {mes}/{anio} ya fueron muestreados anteriormente.";
                        }
                        else
                        {
                            int totalActivosDistintos = activosDelMes.Count;
                            int limiteMuestra = (int)Math.Max(1, Math.Floor(totalActivosDistintos * 0.10));

                            var areaTotals = activosDelMes
                                .Where(a => a.AreaResponsabilidad != null)
                                .GroupBy(a => a.AreaResponsabilidad)
                                .Select(g => new { Area = g.Key, Count = g.Count(), Nombre = g.First().AreaResponsabilidadNavigation != null ? g.First().AreaResponsabilidadNavigation.Nombre : "" })
                                .OrderByDescending(x => x.Count)
                                .ToList();

                            var expected = new Dictionary<Guid?, int>();
                            var residuals = new Dictionary<Guid?, double>();
                            int sumaBase = 0;
                            foreach (var at in areaTotals)
                            {
                                double raw = (double)at.Count * limiteMuestra / totalActivosDistintos;
                                int baseVal = (int)Math.Floor(raw);
                                expected[at.Area] = baseVal;
                                residuals[at.Area] = raw - baseVal;
                                sumaBase += baseVal;
                            }
                            int rem = limiteMuestra - sumaBase;
                            if (rem > 0)
                            {
                                foreach (var kvp in residuals.OrderByDescending(r => r.Value).ThenByDescending(r => expected[r.Key]).Take(rem))
                                {
                                    expected[kvp.Key] = expected[kvp.Key] + 1;
                                }
                            }

                            var rnd = new Random();
                            var pendienteItems = new List<mdMuestreoPendienteItem>();

                            foreach (var area in areaTotals)
                            {
                                int quota = expected.GetValueOrDefault(area.Area, 0);
                                if (quota <= 0) continue;

                                var seleccionados = activosDisponibles
                                    .Where(a => a.AreaResponsabilidad == area.Area)
                                    .OrderBy(_ => rnd.Next())
                                    .Take(quota)
                                    .ToList();

                            foreach (var a in seleccionados)
                            {
                                pendienteItems.Add(new mdMuestreoPendienteItem
                                {
                                    NoInventario = a.NoInventario,
                                    SubNumero = a.SubNumero,
                                    FechaSubmayor = a.FechaSubmayor,
                                    Descripcion = a.Descripcion,
                                    CentroCosto = a.CentroCosto,
                                    NombreCentroCosto = a.CentroCostoNavigation?.Nombre,
                                    AreaResponsabilidad = a.AreaResponsabilidad,
                                    AreaNombre = a.AreaResponsabilidadNavigation?.Nombre,
                                    NombreResponsable = a.ResponsableNavigation?.Nombre,
                                    ApellidosResponsable = a.ResponsableNavigation?.Apellidos,
                                    SiExiste = true
                                });
                            }
                        }

                        var pendiente = new mdMuestreoPendiente
                        {
                            Anno = anio,
                            Mes = mes,
                            Items = pendienteItems
                        };

                        var json = JsonSerializer.Serialize(pendiente, _jsonOptions);
                        HttpContext.Session.SetString(SessionKeyMuestra(anio, mes), json);

                        clGlobal.siNo = true;
                        clGlobal.objMensaje = $"Muestreo de {mes}/{anio} completado exitosamente: {pendienteItems.Count} activos seleccionados.";
                    }
                    }
                }
            }
            catch (Exception ex)
            {
                clGlobal.siNo = false;
                clGlobal.objMensaje = "Error al ejecutar el muestreo: " + ex.Message;
            }

            return RedirectToAction(nameof(MuestreoDiezPorCiento));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarActaMuestreo()
        {
            int mes = DateTime.Today.Month - 1;
            int anio = mes == 0 ? DateTime.Today.Year - 1 : DateTime.Today.Year;
            if (mes == 0) mes = 12;

            var muestra = await _context.MuestreoMensualDiezPorCientos
                .Include(m => m.ActFijoNavigation)
                .Include(m => m.CentroCostoNavigation)
                .Where(m => m.Anno == anio && m.Mes == mes)
                .ToListAsync();

            if (!muestra.Any())
            {
                TempData["Mensaje"] = "No hay datos de muestreo para exportar.";
                return RedirectToAction(nameof(ConsultaMuestreoDiezPorCiento));
            }

            string templatePath = System.IO.Path.Combine(_env.WebRootPath, "plantillas", "PlantillaActaMuestreo.xlsx");

            var grupos = muestra
                .Where(m => m.CentroCosto != null)
                .GroupBy(m => m.CentroCosto.Value)
                .OrderBy(g => g.First().CentroCostoNavigation?.Nombre ?? "ZZZ")
                .ToList();

            if (!grupos.Any())
            {
                TempData["Mensaje"] = "No hay centros de costo asociados a la muestra.";
                return RedirectToAction(nameof(ConsultaMuestreoDiezPorCiento));
            }

            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var templateSheet = package.Workbook.Worksheets[0];
                string templateSheetName = templateSheet.Name;

                int areaIndex = 0;
                foreach (var grupo in grupos)
                {
                    var items = grupo.ToList();
                    string areaNombre = items.First().CentroCostoNavigation?.Nombre ?? "SIN ÁREA";
                    string sheetName = areaNombre.Length > 31 ? areaNombre[..31] : areaNombre;

                    ExcelWorksheet ws = package.Workbook.Worksheets.Copy(templateSheetName, sheetName);

                    ws.Cells["F2"].Value = "MINEM";
                    ws.Cells["F3"].Value = "Empresa de Tecnologías de la Información y Automática";
                    ws.Cells["F5"].Value = areaNombre;
                    ws.Cells["AA4"].Value = new DateTime(anio, mes, 1);
                    ws.Cells["AA5"].Value = ++areaIndex;
                    ws.Cells["AB6"].Value = 1;
                    ws.Column(4).Width = 20;

                    if (items.Count > 1)
                        ws.InsertRow(10, items.Count - 1, 9);

                    for (int i = ws.MergedCells.Count - 1; i >= 0; i--)
                    {
                        var addr = ws.MergedCells[i];
                        if (addr != null && new ExcelAddress(addr).Start.Row >= 9)
                            ws.Cells[addr].Merge = false;
                    }

                    decimal totalCup = 0;
                    for (int i = 0; i < items.Count; i++)
                    {
                        int row = 9 + i;
                        var item = items[i];

                        string noInv = item.SubNumero > 0
                            ? $"{item.NoInventario}-{item.SubNumero}"
                            : item.NoInventario.ToString();

                        ws.Cells[row, 2, row, 3].Merge = true;
                        ws.Cells[row, 2].Value = noInv;

                        ws.Cells[row, 4, row, 7].Merge = true;
                        ws.Cells[row, 4].Value = item.Descripcion;

                        ws.Cells[row, 8].Value = item.NoInventario;

                        decimal cup = item.ActFijoNavigation?.ValorAdquisicion ?? 0;
                        ws.Cells[row, 9, row, 10].Merge = true;
                        ws.Cells[row, 9].Value = cup;
                        totalCup += cup;

                        ws.Cells[row, 11, row, 13].Merge = true;
                        ws.Cells[row, 11].Value = 0;
                        ws.Cells[row, 14].Value = 1;
                        ws.Cells[row, 16, row, 17].Merge = true;
                        ws.Cells[row, 20, row, 21].Merge = true;
                        ws.Cells[row, 25, row, 30].Merge = true;

                        ws.Row(row).CustomHeight = false;
                    }

                    ws.Cells[9, 1, 9 + items.Count - 1, 31].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    int subTotalRow = 9 + items.Count;
                    ws.Cells[subTotalRow, 9, subTotalRow, 11].Merge = true;
                    ws.Cells[subTotalRow, 12, subTotalRow, 17].Merge = true;
                    ws.Cells[subTotalRow, 19, subTotalRow, 25].Merge = true;
                    ws.Cells[subTotalRow, 9].Value = "SUB-TOTAL:";
                    ws.Cells[subTotalRow, 12].Value = totalCup;
                    ws.Cells[subTotalRow, 19].Value = 0;
                    ws.Cells[subTotalRow, 28].Value = items.Count;

                    int totalRow = 10 + items.Count;
                    ws.Cells[totalRow, 9, totalRow, 11].Merge = true;
                    ws.Cells[totalRow, 12, totalRow, 17].Merge = true;
                    ws.Cells[totalRow, 19, totalRow, 25].Merge = true;
                    ws.Cells[totalRow, 9].Value = "TOTAL:";
                    ws.Cells[totalRow, 12].Value = totalCup;
                    ws.Cells[totalRow, 19].Value = 0;
                    ws.Cells[totalRow, 28].Value = items.Count;

                    ws.Cells[9, 1, totalRow, 31].Style.Font.Size = 8;
                    ws.Cells[9, 4, totalRow, 7].Style.WrapText = true;

                    int footerStart = 11 + items.Count;

                    double cm = 1 / 2.54;
                    ws.PrinterSettings.TopMargin = 1.91 * cm;
                    ws.PrinterSettings.BottomMargin = 1.91 * cm;
                    ws.PrinterSettings.LeftMargin = 0.64 * cm;
                    ws.PrinterSettings.RightMargin = 0.64 * cm;
                    ws.PrinterSettings.HeaderMargin = 0.76 * cm;
                    ws.PrinterSettings.FooterMargin = 0.76 * cm;
                    double pageH = 792 - (1.91 * cm * 72) * 2 - 99;
                    double page1 = pageH - 99;
                    double pageN = pageH;
                    double contentH = items.Count * 10 + 81;
                    int blankRows = 0;
                    if (contentH <= page1)
                        blankRows = (int)Math.Floor((page1 - contentH) / 10);
                    else if (contentH > page1)
                    {
                        double lastPg = (contentH - page1) % pageN;
                        if (lastPg > 0)
                            blankRows = (int)Math.Floor((pageN - lastPg) / 10);
                    }
                    if (blankRows > 0)
                    {
                        ws.InsertRow(footerStart, blankRows);
                        for (int j = 0; j < blankRows; j++)
                            ws.Row(footerStart + j).Height = 10;
                        footerStart += blankRows;
                    }

                    for (int f = 0; f < 4; f++)
                    {
                        ws.Cells[footerStart + f, 3, footerStart + f, 4].Merge = true;
                        ws.Cells[footerStart + f, 13, footerStart + f, 24].Merge = true;
                    }
                }

                package.Workbook.Worksheets.Delete(templateSheetName);

                var ms = new MemoryStream();
                package.SaveAs(ms);
                ms.Position = 0;

                string fileName = $"Muestreo10_{anio}_{mes:D2}.xlsx";
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarMuestreo([FromBody] List<Models.Auxiliar.mdMuestreoConfirmacion> items)
        {
            if (items == null || items.Count == 0)
                return Json(new { success = false, mensaje = "No se recibieron datos para confirmar." });

            try
            {
                var firstItem = items.First();
                var json = HttpContext.Session.GetString(SessionKeyMuestra(firstItem.Anno, firstItem.Mes));
                if (json == null)
                    return Json(new { success = false, mensaje = "No hay datos de muestreo pendientes en la sesión." });

                var pendiente = JsonSerializer.Deserialize<mdMuestreoPendiente>(json, _jsonOptions);
                if (pendiente == null || pendiente.Items.Count == 0)
                    return Json(new { success = false, mensaje = "No hay datos de muestreo pendientes en la sesión." });

                int anno = pendiente.Anno;
                int mes = pendiente.Mes;

                var existingRecords = await _context.MuestreoMensualDiezPorCientos
                    .Where(m => m.Anno == anno && m.Mes == mes)
                    .ToListAsync();

                if (existingRecords.Any())
                {
                    foreach (var record in existingRecords)
                    {
                        record.SiExiste = true;
                        record.Observaciones = null;
                    }
                }
                else
                {
                    var nuevos = pendiente.Items.Select(i => new MuestreoMensualDiezPorCiento
                    {
                        Anno = anno,
                        Mes = mes,
                        NoInventario = i.NoInventario,
                        SubNumero = i.SubNumero,
                        FechaSubmayor = i.FechaSubmayor,
                        Descripcion = i.Descripcion,
                        CentroCosto = i.CentroCosto,
                        FechaMuestreo = DateTime.Now,
                        SiExiste = true,
                        Observaciones = null
                    }).ToList();

                    _context.MuestreoMensualDiezPorCientos.AddRange(nuevos);
                }

                await _context.SaveChangesAsync();
                HttpContext.Session.Remove(SessionKeyMuestra(anno, mes));
                return Json(new { success = true, mensaje = "Muestreo confirmado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = "Error al confirmar el muestreo: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarSiExiste(int anno, int mes, long noInventario, int subNumero, DateTime fechaSubmayor, bool siExiste, string? observaciones = null)
        {
            var json = HttpContext.Session.GetString(SessionKeyMuestra(anno, mes));
            if (json != null)
            {
                var pendiente = JsonSerializer.Deserialize<mdMuestreoPendiente>(json, _jsonOptions);
                if (pendiente != null)
                {
                    var item = pendiente.Items.FirstOrDefault(i =>
                        i.NoInventario == noInventario && i.SubNumero == subNumero && i.FechaSubmayor == fechaSubmayor);

                    if (item != null)
                    {
                        item.SiExiste = siExiste;
                        item.Observaciones = observaciones;
                        HttpContext.Session.SetString(SessionKeyMuestra(anno, mes), JsonSerializer.Serialize(pendiente, _jsonOptions));
                        return Json(new { success = true, siExiste, observaciones });
                    }
                }
            }

            var registro = await _context.MuestreoMensualDiezPorCientos
                .FindAsync(anno, mes, noInventario, subNumero, fechaSubmayor);
            if (registro != null)
            {
                registro.SiExiste = siExiste;
                registro.Observaciones = observaciones;
                await _context.SaveChangesAsync();
                return Json(new { success = true, siExiste, observaciones });
            }

            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> PlanMuestreoAnual(int? anio)
        {
            int year = anio ?? 0;

            ViewData["breadsC"] = LlenarBreadsCumbs("Planificación Muestreo Anual");

            var model = new Models.Auxiliar.mdPlanMuestreoAnual { Anio = year };

            if (anio == null)
                return View("vPlanMuestreoAnual", model);

            int totalBase = await _context.ActFijos
                .Where(a => a.FechaSubmayor.Year == year - 1 && a.FechaSubmayor.Month == 12)
                .Select(a => new { a.NoInventario, a.SubNumero })
                .Distinct()
                .CountAsync();

            model.TotalActivosBase = totalBase;
            model.DiezPorCientoEsperado = (int)Math.Max(1, Math.Round(totalBase * 0.10));

            bool existePlan = await _context.PlanMuestreoAnuals.AnyAsync(p => p.Anno == year);
            model.EstaGuardado = existePlan;

            var activos = await _context.ActFijos
                .Include(a => a.AreaResponsabilidadNavigation)
                .Where(a => a.FechaSubmayor.Year == year - 1 && a.FechaSubmayor.Month == 12)
                .ToListAsync();

            var areasAgrupadas = activos
                .GroupBy(a => a.AreaResponsabilidadNavigation?.Nombre ?? "S/A")
                .Select(g => new Models.Auxiliar.mdPlanMuestreoArea
                {
                    AreaId = g.First().AreaResponsabilidad ?? Guid.Empty,
                    NombreArea = g.Key,
                    CantidadCentros = g.Select(a => a.CentroCosto ?? Guid.Empty).Distinct().Count(),
                    Seleccionado = true
                })
                .OrderBy(a => a.NombreArea)
                .ToList();

            model.Areas = areasAgrupadas;

            return View("vPlanMuestreoAnual", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlanMuestreoAnual(int anio, List<string>? areasSeleccionadas)
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Planificación Muestreo Anual");

            var selectedGuids = areasSeleccionadas?
                .Select(a => Guid.TryParse(a, out var g) ? (Guid?)g : null)
                .Where(g => g.HasValue)
                .Select(g => g.Value)
                .ToHashSet() ?? new HashSet<Guid>();

            int totalBase = await _context.ActFijos
                .Where(a => a.FechaSubmayor.Year == anio - 1 && a.FechaSubmayor.Month == 12)
                .Select(a => new { a.NoInventario, a.SubNumero })
                .Distinct()
                .CountAsync();

            var model = new Models.Auxiliar.mdPlanMuestreoAnual
            {
                Anio = anio,
                TotalActivosBase = totalBase,
                DiezPorCientoEsperado = (int)Math.Max(1, Math.Round(totalBase * 0.10)),
                Items = new List<Models.Auxiliar.mdPlanMuestreoAnualItem>()
            };

            var activosDic = await _context.ActFijos
                .Include(a => a.CentroCostoNavigation)
                .Include(a => a.AreaResponsabilidadNavigation)
                .Where(a => a.FechaSubmayor.Year == anio - 1 && a.FechaSubmayor.Month == 12)
                .ToListAsync();

            var selectedNames = activosDic
                .Where(a => a.AreaResponsabilidadNavigation != null
                    && selectedGuids.Contains(a.AreaResponsabilidad ?? Guid.Empty))
                .Select(a => a.AreaResponsabilidadNavigation!.Nombre)
                .Distinct()
                .ToHashSet();

            var expandedGuids = activosDic
                .Where(a => a.AreaResponsabilidadNavigation != null
                    && selectedNames.Contains(a.AreaResponsabilidadNavigation.Nombre))
                .Select(a => a.AreaResponsabilidad ?? Guid.Empty)
                .Distinct()
                .ToHashSet();

            if (selectedGuids.Contains(Guid.Empty))
                expandedGuids.Add(Guid.Empty);

            var grupos = activosDic
                .Where(a => expandedGuids.Contains(a.AreaResponsabilidad ?? Guid.Empty))
                .GroupBy(a => a.AreaResponsabilidad ?? Guid.Empty)
                .ToList();

            var areasInfo = new List<(Guid areaId, string nombreArea, int total, Guid centroCosto, string numCC, string nomCC)>();
            foreach (var grupo in grupos)
            {
                int total = grupo.Select(a => new { a.NoInventario, a.SubNumero }).Distinct().Count();
                var first = grupo.First();
                var ccValido = grupo
                    .Where(a => a.CentroCostoNavigation != null && a.CentroCostoNavigation.Numero != null)
                    .Select(a => a.CentroCostoNavigation)
                    .FirstOrDefault();

                areasInfo.Add((
                    grupo.Key,
                    first.AreaResponsabilidadNavigation?.Nombre ?? "S/A",
                    total,
                    ccValido?.IdCentroCosto ?? Guid.Empty,
                    ccValido?.Numero?.ToString() ?? "S/A",
                    ccValido?.Nombre ?? "S/A"
                ));
            }

            model.Items = CalcularItemsConDistribucionGlobal(areasInfo);
            model.Totales = CalcularTotales(model.Items);

            var areasSinCC = model.Items
                .Where(i => i.NumeroCentroCosto == "S/A")
                .Select(i => i.NombreArea)
                .ToList();
            model.HayAreasSinCentroCosto = areasSinCC.Any();
            model.AreasSinCentroCosto = areasSinCC;

            model.EstaGuardado = await _context.PlanMuestreoAnuals.AnyAsync(p => p.Anno == anio);

            ViewData["areasSeleccionadas"] = string.Join(",", expandedGuids);

            return View("vPlanMuestreoAnual", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPlanMuestreo(int anio, List<string>? areasSeleccionadas)
        {
            try
            {
                bool yaExiste = await _context.PlanMuestreoAnuals.AnyAsync(p => p.Anno == anio);
                if (yaExiste)
                {
                    var viejos = await _context.PlanMuestreoAnuals.Where(p => p.Anno == anio).ToListAsync();
                    _context.PlanMuestreoAnuals.RemoveRange(viejos);
                    await _context.SaveChangesAsync();
                }

                var selectedAreas = areasSeleccionadas?
                    .Select(a => Guid.TryParse(a, out var g) ? (Guid?)g : null)
                    .Where(g => g.HasValue)
                    .Select(g => g.Value)
                    .ToHashSet() ?? new HashSet<Guid>();

                var activosDic = await _context.ActFijos
                    .Include(a => a.CentroCostoNavigation)
                    .Include(a => a.AreaResponsabilidadNavigation)
                    .Where(a => a.FechaSubmayor.Year == anio - 1 && a.FechaSubmayor.Month == 12)
                    .ToListAsync();

                var nombresSinCC = activosDic
                    .Where(a => selectedAreas.Contains(a.AreaResponsabilidad ?? Guid.Empty))
                    .GroupBy(a => a.AreaResponsabilidadNavigation?.Nombre ?? "S/A")
                    .Where(g => !g.Any(a => a.CentroCostoNavigation != null && a.CentroCostoNavigation.Numero != null))
                    .Select(g => g.Key)
                    .ToList();

                if (nombresSinCC.Any())
                    return Json(new { success = false, mensaje = $"No se puede guardar: las siguientes áreas no tienen centro de costo asignado: {string.Join(", ", nombresSinCC)}. Asígneles un centro de costo e intente nuevamente." });

                var grupos = activosDic
                    .Where(a => selectedAreas.Contains(a.AreaResponsabilidad ?? Guid.Empty))
                    .GroupBy(a => a.AreaResponsabilidad ?? Guid.Empty)
                    .ToList();

                var areasInfo = new List<(Guid areaId, string nombreArea, int total, Guid centroCosto, string numCC, string nomCC)>();
                foreach (var grupo in grupos)
                {
                    int total = grupo.Select(a => new { a.NoInventario, a.SubNumero }).Distinct().Count();
                    var first = grupo.First();
                    var ccValido = grupo
                        .Where(a => a.CentroCostoNavigation != null && a.CentroCostoNavigation.Numero != null)
                        .Select(a => a.CentroCostoNavigation)
                        .FirstOrDefault();

                    areasInfo.Add((
                        grupo.Key,
                        first.AreaResponsabilidadNavigation?.Nombre ?? "S/A",
                        total,
                        ccValido?.IdCentroCosto ?? Guid.Empty,
                        ccValido?.Numero?.ToString() ?? "S/A",
                        ccValido?.Nombre ?? "S/A"
                    ));
                }

                var items = CalcularItemsConDistribucionGlobal(areasInfo);
                var planes = items.Select(item => new PlanMuestreoAnual
                {
                    Anno = anio,
                    CentroCosto = item.CentroCosto,
                    AreaResponsabilidad = item.AreaResponsabilidad,
                    TotalActivos = item.TotalActivos,
                    Ene = item.Ene, Feb = item.Feb, Mar = item.Mar, Abr = item.Abr,
                    May = item.May, Jun = item.Jun, Jul = item.Jul, Ago = item.Ago,
                    Sep = item.Sep, Oct = item.Oct, Nov = item.Nov, Dic = item.Dic,
                    FechaCreacion = DateTime.Now
                }).ToList();

                if (!planes.Any())
                    return Json(new { success = false, mensaje = "No se encontraron activos en diciembre para generar la planificación con las áreas seleccionadas." });

                _context.PlanMuestreoAnuals.AddRange(planes);
                await _context.SaveChangesAsync();

                return Json(new { success = true, mensaje = $"Planificación del año {anio} guardada correctamente ({planes.Count} áreas)." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = "Error al guardar la planificación: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerPlanMuestreoAnual(int? anio)
        {
            int year = anio ?? 0;

            ViewData["breadsC"] = LlenarBreadsCumbs("Planificación Muestreo Anual");

            var model = new Models.Auxiliar.mdPlanMuestreoAnual { Anio = year };

            try
            {
                if (anio == null)
                return View("vVerPlanMuestreoAnual", model);

                int totalBase = await _context.ActFijos
                    .Where(a => a.FechaSubmayor.Year == year - 1 && a.FechaSubmayor.Month == 12)
                    .Select(a => new { a.NoInventario, a.SubNumero })
                    .Distinct()
                    .CountAsync();

                model.TotalActivosBase = totalBase;
                model.DiezPorCientoEsperado = (int)Math.Max(1, Math.Round(totalBase * 0.10));

                var planGuardado = await _context.PlanMuestreoAnuals
                    .Where(p => p.Anno == year)
                    .OrderBy(p => p.AreaResponsabilidad).ThenBy(p => p.CentroCosto)
                    .ToListAsync();

                if (planGuardado.Any())
                {
                    model.EstaGuardado = true;

                    var centroIds = planGuardado.Select(p => p.CentroCosto).Where(id => id != Guid.Empty).Distinct().ToList();
                    var areaIds = planGuardado.Select(p => p.AreaResponsabilidad).Where(id => id != Guid.Empty).Distinct().ToList();

                    var centros = centroIds.Any()
                        ? await _context.CentroCostos.Where(c => centroIds.Contains(c.IdCentroCosto)).ToDictionaryAsync(c => c.IdCentroCosto)
                        : new Dictionary<Guid, CentroCosto>();
                    var areas = areaIds.Any()
                        ? await _context.AreaDeResponsabilidads.Where(a => areaIds.Contains(a.IdArea)).ToDictionaryAsync(a => a.IdArea)
                        : new Dictionary<Guid, AreaDeResponsabilidad>();

                    foreach (var p in planGuardado)
                        model.Items.Add(MapearItem(p, centros, areas));

                    model.Items = AgruparItemsPorArea(model.Items);
                    model.Totales = CalcularTotales(model.Items);
                }

                return View("vVerPlanMuestreoAnual", model);
            }
            catch (Exception ex)
            {
                return View("vVerPlanMuestreoAnual", model);
            }

            
        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcelPlanMuestreo(int anio)
        {
            var planGuardado = await _context.PlanMuestreoAnuals
                .Where(p => p.Anno == anio)
                .OrderBy(p => p.AreaResponsabilidad).ThenBy(p => p.CentroCosto)
                .ToListAsync();

            if (!planGuardado.Any())
                return NotFound();

            var centroIds = planGuardado.Select(p => p.CentroCosto).Where(id => id != Guid.Empty).Distinct().ToList();
            var areaIds = planGuardado.Select(p => p.AreaResponsabilidad).Where(id => id != Guid.Empty).Distinct().ToList();

            var centros = centroIds.Any()
                ? await _context.CentroCostos.Where(c => centroIds.Contains(c.IdCentroCosto)).ToDictionaryAsync(c => c.IdCentroCosto)
                : new Dictionary<Guid, CentroCosto>();
            var areas = areaIds.Any()
                ? await _context.AreaDeResponsabilidads.Where(a => areaIds.Contains(a.IdArea)).ToDictionaryAsync(a => a.IdArea)
                : new Dictionary<Guid, AreaDeResponsabilidad>();

            var items = new List<Models.Auxiliar.mdPlanMuestreoAnualItem>();
            foreach (var p in planGuardado)
                items.Add(MapearItem(p, centros, areas));

            items = AgruparItemsPorArea(items);
            var totales = CalcularTotales(items);

            using var package = new OfficeOpenXml.ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("PlanMuestreo");

            ws.Cells[1, 1].Value = "Planificación Muestreo Anual - " + anio;
            ws.Cells[1, 1, 1, 15].Merge = true;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;
            ws.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            string[] headers = { "Área", "Centro Costo (#)", "Total", "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[3, i + 1].Value = headers[i];
                ws.Cells[3, i + 1].Style.Font.Bold = true;
            }

            var headerRange = ws.Cells[3, 1, 3, 15];
            headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(68, 77, 88));

            int row = 4;
            foreach (var item in items)
            {
                ws.Cells[row, 1].Value = item.NombreArea;
                ws.Cells[row, 2].Value = item.NumeroCentroCosto;
                ws.Cells[row, 3].Value = item.TotalActivos;
                ws.Cells[row, 4].Value = item.Ene;
                ws.Cells[row, 5].Value = item.Feb;
                ws.Cells[row, 6].Value = item.Mar;
                ws.Cells[row, 7].Value = item.Abr;
                ws.Cells[row, 8].Value = item.May;
                ws.Cells[row, 9].Value = item.Jun;
                ws.Cells[row, 10].Value = item.Jul;
                ws.Cells[row, 11].Value = item.Ago;
                ws.Cells[row, 12].Value = item.Sep;
                ws.Cells[row, 13].Value = item.Oct;
                ws.Cells[row, 14].Value = item.Nov;
                ws.Cells[row, 15].Value = item.Dic;
                row++;
            }

            if (totales != null)
            {
                ws.Cells[row, 1].Value = totales.NombreArea;
                ws.Cells[row, 1].Style.Font.Bold = true;
                ws.Cells[row, 3].Value = totales.TotalActivos;
                ws.Cells[row, 4].Value = totales.Ene;
                ws.Cells[row, 5].Value = totales.Feb;
                ws.Cells[row, 6].Value = totales.Mar;
                ws.Cells[row, 7].Value = totales.Abr;
                ws.Cells[row, 8].Value = totales.May;
                ws.Cells[row, 9].Value = totales.Jun;
                ws.Cells[row, 10].Value = totales.Jul;
                ws.Cells[row, 11].Value = totales.Ago;
                ws.Cells[row, 12].Value = totales.Sep;
                ws.Cells[row, 13].Value = totales.Oct;
                ws.Cells[row, 14].Value = totales.Nov;
                ws.Cells[row, 15].Value = totales.Dic;

                var footerRange = ws.Cells[row, 1, row, 15];
                footerRange.Style.Font.Bold = true;
                footerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                footerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(176, 176, 176));
            }

            ws.PrinterSettings.Orientation = eOrientation.Landscape;
            ws.PrinterSettings.FitToPage = true;
            ws.PrinterSettings.FitToWidth = 1;
            ws.PrinterSettings.FitToHeight = 0;

            ws.Cells[1, 1, row, 15].AutoFitColumns();

            string fileName = $"PlanMuestreoAnual_{anio}.xlsx";
            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private Models.Auxiliar.mdPlanMuestreoAnualItem MapearItem(PlanMuestreoAnual p, Dictionary<Guid, CentroCosto> centros, Dictionary<Guid, AreaDeResponsabilidad> areas)
        {
            string? numeroCC = null;
            string? nombreCC = null;
            string? nombreArea = null;

            if (p.CentroCosto != Guid.Empty && centros.TryGetValue(p.CentroCosto, out var cc))
            {
                numeroCC = cc.Numero?.ToString();
                nombreCC = cc.Nombre;
            }
            if (p.AreaResponsabilidad != Guid.Empty && areas.TryGetValue(p.AreaResponsabilidad, out var ar))
            {
                nombreArea = ar.Nombre;
            }

            return new Models.Auxiliar.mdPlanMuestreoAnualItem
            {
                CentroCosto = p.CentroCosto,
                NumeroCentroCosto = numeroCC ?? "S/A",
                NombreCentroCosto = nombreCC ?? "S/A",
                AreaResponsabilidad = p.AreaResponsabilidad,
                NombreArea = nombreArea ?? "S/A",
                TotalActivos = p.TotalActivos,
                Ene = p.Ene, Feb = p.Feb, Mar = p.Mar, Abr = p.Abr,
                May = p.May, Jun = p.Jun, Jul = p.Jul, Ago = p.Ago,
                Sep = p.Sep, Oct = p.Oct, Nov = p.Nov, Dic = p.Dic
            };
        }

        private static List<Models.Auxiliar.mdPlanMuestreoAnualItem> CalcularItemsConDistribucionGlobal(
            List<(Guid areaId, string nombreArea, int total, Guid centroCosto, string numCC, string nomCC)> areas)
        {
            int grandTotal = areas.Sum(a => a.total);
            if (grandTotal == 0) return new List<Models.Auxiliar.mdPlanMuestreoAnualItem>();

            int globalBase = Math.Max(1, (int)Math.Floor(grandTotal * 0.10));
            int globalRem = grandTotal - globalBase * 10;

            int[] monthTargets = new int[10];
            for (int m = 0; m < 10; m++)
                monthTargets[m] = globalBase + (m < globalRem ? 1 : 0);

            int n = areas.Count;
            int[][] vals = new int[n][];
            for (int i = 0; i < n; i++)
                vals[i] = new int[10];

            for (int m = 0; m < 10; m++)
            {
                int[] floors = new int[n];
                long[] remainders = new long[n];
                int totalFloor = 0;

                for (int i = 0; i < n; i++)
                {
                    long prod = (long)areas[i].total * monthTargets[m];
                    floors[i] = (int)(prod / grandTotal);
                    remainders[i] = prod - floors[i] * grandTotal;
                    totalFloor += floors[i];
                }

                int rem = monthTargets[m] - totalFloor;
                if (rem > 0)
                {
                    var topAreas = Enumerable.Range(0, n)
                        .OrderByDescending(i => remainders[i])
                        .Take(rem)
                        .ToList();

                    foreach (int i in topAreas)
                        floors[i]++;
                }

                for (int i = 0; i < n; i++)
                    vals[i][m] = floors[i];
            }

            var items = new List<Models.Auxiliar.mdPlanMuestreoAnualItem>();
            for (int i = 0; i < n; i++)
            {
                items.Add(new Models.Auxiliar.mdPlanMuestreoAnualItem
                {
                    CentroCosto = areas[i].centroCosto,
                    NumeroCentroCosto = areas[i].numCC,
                    NombreCentroCosto = areas[i].nomCC,
                    AreaResponsabilidad = areas[i].areaId,
                    NombreArea = areas[i].nombreArea,
                    TotalActivos = areas[i].total,
                    Ene = vals[i][0], Feb = vals[i][1], Mar = vals[i][2],
                    Abr = vals[i][3], May = vals[i][4], Jun = areas[i].total,
                    Jul = vals[i][5], Ago = vals[i][6], Sep = vals[i][7],
                    Oct = vals[i][8], Nov = vals[i][9], Dic = areas[i].total
                });
            }

            return items;
        }

        private static Models.Auxiliar.mdPlanMuestreoAnualItem CalcularTotales(List<Models.Auxiliar.mdPlanMuestreoAnualItem> items)
        {
            if (!items.Any())
                return null;

            return new Models.Auxiliar.mdPlanMuestreoAnualItem
            {
                CentroCosto = Guid.Empty,
                NumeroCentroCosto = null,
                NombreCentroCosto = null,
                AreaResponsabilidad = Guid.Empty,
                NombreArea = "TOTALES",
                TotalActivos = items.Sum(i => i.TotalActivos),
                Ene = items.Sum(i => i.Ene),
                Feb = items.Sum(i => i.Feb),
                Mar = items.Sum(i => i.Mar),
                Abr = items.Sum(i => i.Abr),
                May = items.Sum(i => i.May),
                Jun = items.Sum(i => i.Jun),
                Jul = items.Sum(i => i.Jul),
                Ago = items.Sum(i => i.Ago),
                Sep = items.Sum(i => i.Sep),
                Oct = items.Sum(i => i.Oct),
                Nov = items.Sum(i => i.Nov),
                Dic = items.Sum(i => i.Dic)
            };
        }

        private static List<Models.Auxiliar.mdPlanMuestreoAnualItem> AgruparItemsPorArea(List<Models.Auxiliar.mdPlanMuestreoAnualItem> items)
        {
            return items
                .GroupBy(i => i.NombreArea ?? "S/A")
                .Select(g =>
                {
                    var rep = g.FirstOrDefault(i => i.NumeroCentroCosto != "S/A") ?? g.First();
                    return new Models.Auxiliar.mdPlanMuestreoAnualItem
                    {
                        CentroCosto = rep.CentroCosto,
                        NumeroCentroCosto = rep.NumeroCentroCosto,
                        NombreCentroCosto = rep.NombreCentroCosto,
                        AreaResponsabilidad = rep.AreaResponsabilidad,
                        NombreArea = g.Key,
                        TotalActivos = g.Sum(i => i.TotalActivos),
                        Ene = g.Sum(i => i.Ene),
                        Feb = g.Sum(i => i.Feb),
                        Mar = g.Sum(i => i.Mar),
                        Abr = g.Sum(i => i.Abr),
                        May = g.Sum(i => i.May),
                        Jun = g.Sum(i => i.Jun),
                        Jul = g.Sum(i => i.Jul),
                        Ago = g.Sum(i => i.Ago),
                        Sep = g.Sum(i => i.Sep),
                        Oct = g.Sum(i => i.Oct),
                        Nov = g.Sum(i => i.Nov),
                        Dic = g.Sum(i => i.Dic)
                    };
                })
                .OrderBy(i => i.NombreArea)
                .ToList();
        }
    }
}
