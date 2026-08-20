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
using System.Data;
using System.Diagnostics;
using ActivosFijos.Models.Auxiliar;
using System.Net;
using System.IO.Compression;
using ActivosFijos.Models.AuxBD;
using System.Text.Json;
using System.Text.Json.Serialization;
using OfficeOpenXml;

namespace ActivosFijos.Controllers
{
    public class ctUtilesYHerramientasController : Controller
    {
        private readonly aftContext _context;
        private readonly repoAreasDeResponsabilidad _repoAreas;
        private readonly repoResponsables _repoResponsables;
        private readonly repoCentrosDeCosto _repoCentroDeCostos;
        private readonly repoValidaciones _repoValidaciones;
        private readonly repoServcioActas _repoServcioActas;
        private readonly repoUtilesHerramientas _repoUTH;
        private readonly IWebHostEnvironment _env;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        private string SessionKeyMuestraUTH(int anno, int mes) => $"MuestraUTH_Pendiente_{anno}_{mes}";

        private (int mes, int anio) ObtenerMesAnioMuestreo()
        {
            int mes = DateTime.Today.Month - 1;
            int anio = mes == 0 ? DateTime.Today.Year - 1 : DateTime.Today.Year;
            if (mes == 0) mes = 12;
            return (mes, anio);
        }

        private class SelectListResponsable
        {
            public Guid IdResponsable { get; set; }
            public string NombreCompleto { get; set; }
        }

        public ctUtilesYHerramientasController(aftContext context, repoUtilesHerramientas repoUTH, repoAreasDeResponsabilidad repoAreas, repoResponsables repoResponsables,
            repoCentrosDeCosto repoCentroDeCostos, repoValidaciones repoValidaciones, repoServcioActas repoServcioActas, IWebHostEnvironment env)
        {
            _context = context;
            _repoAreas = repoAreas;
            _repoResponsables = repoResponsables;
            _repoCentroDeCostos = repoCentroDeCostos;
            _repoValidaciones = repoValidaciones;
            _repoServcioActas = repoServcioActas;
            _repoUTH = repoUTH;
            _env = env;
        }

        // GET: ctUtilesYHerramientas
        public async Task<IActionResult> UTH(Guid? area, Guid? responsable, DateTime? fecha, bool visualizarMovimiento = false)
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

            string directorNombre = string.Empty;
            if (director == null)
                directorNombre = "<<S/D>>";
            else 
                directorNombre = director.Nombre + " " + director.Apellidos;

            string responsArea = "<<S/D>>";

            if (area == null || area == Guid.Empty)
            {
                responsArea = directorNombre;
            }
            else
            {
                Responsable espEP = new Responsable();
                espEP = await _repoResponsables.SeleccionarResponsableEPDelArea(area ?? Guid.Empty);

                if (espEP != null)
                    responsArea = espEP.Nombre + " " + espEP.Apellidos;
            }

            ViewData["breadsC"] = LlenarBreadsCumbs("Listado de Utiles y Herramientas");

            ViewData["respuesta"] = new clResponse<mdActas>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vUTH", new mdActas() { Areas = areas, Responsables = responsables, ResponsableArea = responsArea, Fecha = fecha == null ? DateTime.Today : new DateTime(fecha.Value.Year, fecha.Value.Month, fecha.Value.Day), VisualizarMovimiento = visualizarMovimiento });
        }

        public async Task<IActionResult> UTHImportados(Guid? area, Guid? responsable, DateTime? fecha)
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

            return View("vUTHImportados", new mdSubmayorErrores() { Areas = areas, Responsables = responsables, Fecha = fecha == null ? DateTime.Today : new DateTime(fecha.Value.Year, fecha.Value.Month, fecha.Value.Day) });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeResponsablesPorFecha(DateTime fecha, bool siMovimiento = false)
        {
            List<Responsable> responsables = siMovimiento ? await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(await _repoUTH.ListadoUTHConMovimientos(fecha)) : await _repoResponsables.ObtenerListadoResponsables(fecha, siMovimiento);
            List<SelectListItem> responsablesSelect = new List<SelectListItem>();
            responsablesSelect = responsables.Select(c => new SelectListItem() { Text = c.Nombre != "<<S/R>>" ? c.Nombre + " " + c.Apellidos : c.Nombre, Value = c.IdResponsable.ToString() }).OrderBy(c => c.Text).ToList();
            responsablesSelect.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });

            return PartialView("_vpResponsables", new mdActas() { Responsables = responsablesSelect });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeResponsablesUTHPorFechaYArea(DateTime fecha, string area, bool siMovimiento = false)
        {
            List<Responsable> responsables = siMovimiento ? await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(await _repoUTH.ListadoUTHConMovimientos(fecha), Guid.Parse(area)) : await _repoResponsables.ObtenerListadoResponsablesPorFechaYAreaUTH(fecha, Guid.Parse(area), siMovimiento);
            List<SelectListItem> responsablesSelect = new List<SelectListItem>();
            responsablesSelect = responsables.Select(c => new SelectListItem() { Text = c.Nombre != "<<S/R>>" ? c.Nombre + " " + c.Apellidos : c.Nombre, Value = c.IdResponsable.ToString() }).OrderBy(c => c.Text).ToList();
            responsablesSelect.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });

            return PartialView("_vpResponsables", new mdActas() { Responsables = responsablesSelect });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeResponsablesUTHPorFecha(DateTime fecha)
        {
            List<Responsable> responsables = await _repoResponsables.ObtenerListadoResponsablesUTH(fecha);
            List<SelectListItem> responsablesSelect = new List<SelectListItem>();
            responsablesSelect = responsables.Select(c => new SelectListItem() { Text = c.Nombre != "<<S/R>>" ? c.Nombre + " " + c.Apellidos : c.Nombre, Value = c.IdResponsable.ToString() }).ToList();
            responsablesSelect.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });

            return PartialView("_vpResponsablesUTH", new mdSubmayorErrores() { Responsables = responsablesSelect });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeAreasPorFecha(DateTime fecha, bool siMovimientos = false)
        {
            List<AreaDeResponsabilidad> areas = siMovimientos ? await _repoAreas.ObtenerListadoAreasSiMovimiento(await _repoUTH.ListadoUTHConMovimientos(fecha)) : await _repoAreas.ListadoAreasDeResponsabilidadUTH(fecha, siMovimientos);
            List<SelectListItem> areasSelect = new List<SelectListItem>();
            areasSelect = areas.Select(c => new SelectListItem() { Text = c.Nombre, Value = c.IdArea.ToString() }).ToList();
            areasSelect.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });

            return PartialView("_vpAreas", new mdActas() { Areas = areasSelect });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeAreasUTHPorFecha(DateTime fecha)
        {
            List<AreaDeResponsabilidad> areas = await _repoAreas.ListadoAreasDeResponsabilidadUTH(fecha);
            List<SelectListItem> areasSelect = new List<SelectListItem>();
            areasSelect = areas.Select(c => new SelectListItem() { Text = c.Nombre, Value = c.IdArea.ToString() }).ToList();
            areasSelect.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });

            return PartialView("_vpAreasUTH", new mdSubmayorErrores() { Areas = areasSelect });
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeUTHs(DateTime fecha, string area, string responsable, bool siMovimientos = false)
        {
            List<UtilesHerramienta> uthList = await _repoUTH.ListadoUTH(fecha, Guid.Parse(area), Guid.Parse(responsable));
            List<ExistenciaMovimientoUTH> existMovs = new List<ExistenciaMovimientoUTH>();
            existMovs = await _repoUTH.DeterminarSiExisteMovimientoDeUTH(fecha);

            if (siMovimientos)
                uthList = uthList.Where(c => existMovs.Any(ex => ex.Discriminante.Equals(string.Concat(c.Codigo, "|", c.Responsable, "|", c.CentroCosto, "|", c.AreaResponsabilidad)))).ToList();

            ViewData["ExistenciaDeMovimiento"] = existMovs;

            return PartialView("_vpTablaUTH", uthList.OrderBy(act => act.AreaResponsabilidadNavigation.Nombre).ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos).ThenBy(act => act.Codigo));
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeUTHsConMovimientos(DateTime fecha, string area, string responsable)
        {
            List<UTHConMovimientosParcial> uthList = await _repoUTH.ListadoUTHConMovimientos(fecha);
            List<UTHConMovimientos> uthMovlist = new List<UTHConMovimientos>();
            var query = uthList.AsQueryable();

            if (!Guid.Parse(area).Equals(Guid.Empty))
            {
                query = query.Where(c => c.Area.Equals(Guid.Parse(area)));
            }

            if (!Guid.Parse(responsable).Equals(Guid.Empty))
            {
                query = query.Where(c => c.Responsable.Equals(Guid.Parse(responsable)));
            }

            uthList = query.ToList();

            foreach (var uth in uthList)
            {
                if (uthMovlist.Count() == 0)
                {
                    Responsable resp = new Responsable();
                    resp = await _repoResponsables.SeleccionarResponsablesPorID(uth.Responsable);

                    CentroCosto centC = new CentroCosto();
                    centC = await _repoCentroDeCostos.EncontrarCentroCostoPorID(uth.CentroCosto);

                    AreaDeResponsabilidad areaResp = new AreaDeResponsabilidad();
                    areaResp = await _repoAreas.EncontrarAreaPorID(uth.Area);

                    UTHConMovimientos uthMov = new UTHConMovimientos()
                    {
                        Codigo = uth.Codigo,
                        Discriminante = uth.Discriminante,
                        Descripcion = uth.Descripcion,
                        Responsable = resp != null ? string.Concat(resp.Nombre, " ", resp.Apellidos) : "<<S/R>>",
                        idResponsable = uth.Responsable,
                        CentroCosto = centC != null ? centC.Nombre : "<<S/CC>>",
                        idCentroCosto = uth.CentroCosto,
                        Area = areaResp != null ? areaResp.Nombre : "<<S/A>>",
                        idArea = uth.Area,
                        Cantidad = uth.Cantidad,
                        CantidadAnterior = uth.CantidadAnterior
                    };

                    uthMovlist.Add(uthMov);
                }
                else if (uthMovlist.Count() > 0)
                {
                    UTHConMovimientos uthMov = new UTHConMovimientos();
                    if (uthMovlist.Exists(c => c.Discriminante.Equals(uth.Discriminante)))
                    {
                        uthMov = uthMovlist.Find(c => c.Discriminante.Equals(uth.Discriminante));
                        uthMovlist.Remove(uthMov);

                        if (uth.Cantidad > 0)
                            uthMov.Cantidad = uth.Cantidad;

                        if (uth.CantidadAnterior > 0)
                        {
                            uthMov.CantidadAnterior = uth.CantidadAnterior;
                        }

                        uthMovlist.Add(uthMov);
                    }
                    else
                    {
                        Responsable resp = new Responsable();
                        resp = await _repoResponsables.SeleccionarResponsablesPorID(uth.Responsable);

                        CentroCosto centC = new CentroCosto();
                        centC = await _repoCentroDeCostos.EncontrarCentroCostoPorID(uth.CentroCosto);

                        AreaDeResponsabilidad areaResp = new AreaDeResponsabilidad();
                        areaResp = await _repoAreas.EncontrarAreaPorID(uth.Area);

                        uthMov = new UTHConMovimientos()
                        {
                            Codigo = uth.Codigo,
                            Discriminante = uth.Discriminante,
                            Descripcion = uth.Descripcion,
                            Responsable = resp != null ? string.Concat(resp.Nombre, " ", resp.Apellidos) : "<<S/R>>",
                            idResponsable = uth.Responsable,
                            CentroCosto = centC != null ? centC.Nombre : "<<S/CC>>",
                            idCentroCosto = uth.CentroCosto,
                            Area = areaResp != null ? areaResp.Nombre : "<<S/A>>",
                            idArea = uth.Area,
                            Cantidad = uth.Cantidad,
                            CantidadAnterior = uth.CantidadAnterior
                        };

                        uthMovlist.Add(uthMov);
                    }
                }
            }

            ViewData["FechaSubmayor"] = fecha;

            return PartialView("_vpTablaUTHConMovimientos", uthMovlist.OrderBy(act => act.Area).ThenBy(act => act.Responsable).ThenBy(act => act.Codigo));
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeUTHImportados(DateTime fecha, string area, string responsable, bool siErrores = false)
        {
            List<UtilesHerramienta> uthList = await _repoUTH.ListadoUTH(fecha, Guid.Parse(area), Guid.Parse(responsable));

            if (siErrores)
                uthList = uthList
                .Where(act => act.ResponsableNavigation == null || act.ResponsableNavigation.Nombre == "<<S/R>>"
                    || act.AreaResponsabilidad == null || act.AreaResponsabilidadNavigation.Nombre.Equals("<<S/A>>")
                    || act.CentroCosto == null || act.CentroCostoNavigation.Nombre == "<<S/C>>")
                .OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                    .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                    .ThenBy(act => act.Codigo)
                .ToList();
            else
                uthList = uthList.OrderBy(act => act.AreaResponsabilidadNavigation.Nombre)
                   .ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos)
                   .ThenBy(act => act.Codigo)
               .ToList();

            mdSubmayorErrores subMayor = new mdSubmayorErrores()
            {
                Area = Guid.Parse(area),
                Fecha = fecha,
                Responsable = Guid.Parse(responsable),
                VisualizarErrores = siErrores
            };

            ViewData["SubMayor"] = subMayor;

            return PartialView("_vpTablaUTHImportados", uthList);
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

        [HttpPost]
        public IActionResult CargarDatosUTHResponsablesPorAreaDeResponsabilidad(string area)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            lista = _repoResponsables.ObtenerListadoResponsables(Guid.Parse(area)).Result.Select(res => new SelectListItem() { Text = res.Nombre == "<<S/R>>" ? res.Nombre : res.Nombre + " " + res.Apellidos, Value = res.IdResponsable.ToString(), Selected = false }).ToList();
            lista.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            return PartialView("_vpResponsablesUTH", new mdActas() { Responsables = lista });
        }

        [HttpPost]
        public async Task<IActionResult> CargarDatosUTHResponsablesPorAreaYFecha(string area, DateTime fecha)
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            List<UtilesHerramienta> utilesH = new List<UtilesHerramienta>();
            utilesH = await _repoUTH.ListadoUTH(fecha);
            utilesH = utilesH.Where(c => c.AreaResponsabilidad.Equals(Guid.Parse(area))).ToList();

            List<Responsable> responsables = new List<Responsable>();
            responsables = utilesH.Select(c => c.ResponsableNavigation).Distinct().ToList();

            lista = responsables.Select(res => new SelectListItem() { Text = res.Nombre == "<<S/R>>" ? res.Nombre : res.Nombre + " " + res.Apellidos, Value = res.IdResponsable.ToString(), Selected = false }).ToList();
            lista.Add(new SelectListItem() { Text = "Todos", Value = Guid.Empty.ToString(), Selected = true });
            return PartialView("_vpResponsablesUTH", new mdSubmayorErrores() { Responsables = lista });
        }

        [HttpPost]
        public async Task<IActionResult> CargarInfoUTHResponsableDelArea(string area)
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
        public async Task<IActionResult> GenerarActaUtilesHerramientas(mdActas model)
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
                return RedirectToAction(nameof(UTH), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha, visualizarMovimiento = model.VisualizarMovimiento });
            }
            else
            {
                try
                {
                    string nombreMes = repoServcioActas.GetNameMonth(model.Fecha.Month, model.Fecha.Year);

                    List<ExistenciaMovimientoUTH> existMovs = new List<ExistenciaMovimientoUTH>();
                    existMovs = await _repoUTH.DeterminarSiExisteMovimientoDeUTH(model.Fecha);

                    List<Responsable> responsables = new List<Responsable>();
                    List<AreaDeResponsabilidad> areasResponsabilidad = new List<AreaDeResponsabilidad>();

                    if (model.VisualizarMovimiento)
                    {
                        responsables = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existMovs, model.Fecha);
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
                                    return RedirectToAction(nameof(UTH), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha, visualizarMovimiento = model.VisualizarMovimiento });
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

                                        objFile = _repoServcioActas.ExportarUTHPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, responsableArea);
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

                                        objFile = _repoServcioActas.ExportarPorAreasUTHDocx(out result, out msg, Guid.Empty, item.IdArea, model.Fecha, responsableArea);
                                        var entry = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }

                                    objFile = _repoServcioActas.ExportarUTHADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
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
                                return RedirectToAction(nameof(UTH), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha });
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

                                    objFile = _repoServcioActas.ExportarUTHPorAreasIndividualDocx(out result, out msg, model.Responsable, model.Area, model.Fecha, responsableArea);
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
                                objFile = _repoServcioActas.ExportarUTHADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
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

                                            objFile = _repoServcioActas.ExportarUTHPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, model.ResponsableArea);
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

                                        objFile = _repoServcioActas.ExportarPorAreasUTHDocx(out result, out msg, Guid.Empty, areasResp.IdArea, model.Fecha, responsableArea);
                                        var entryEP = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entryEP.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }

                                        objFile = _repoServcioActas.ExportarUTHADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
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
                                    return RedirectToAction(nameof(UTH), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha, visualizarMovimiento = model.VisualizarMovimiento });
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

                                        objFile = _repoServcioActas.ExportarUTHPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, model.ResponsableArea);
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

                                    objFile = _repoServcioActas.ExportarPorAreasUTHDocx(out result, out msg, Guid.Empty, model.Area, model.Fecha, model.ResponsableArea);
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
                                    return RedirectToAction(nameof(UTH), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha, visualizarMovimiento = model.VisualizarMovimiento });
                                }

                                Responsable director = new Responsable();
                                director = _repoResponsables.ObtenerInfoDirector().Result;

                                Responsable responsable = new Responsable();
                                responsable = await _repoResponsables.SeleccionarResponsablesPorID(model.Responsable);

                                string nombreResponsable = responsable.Nombre + " " + responsable.Apellidos;

                                MemoryStream ms = new MemoryStream();
                                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                                {
                                    objFile = _repoServcioActas.ExportarUTHPorAreasIndividualDocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                    tituloDoc = nombreArea + " (" + nombreResponsable + ", " + nombreMes.Replace('/', '-') + ").docx";

                                    var entry = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entry.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }

                                    tituloDoc = nombreArea + " (" + nombreMes.Replace('/', '-') + ").docx";

                                    objFile = _repoServcioActas.ExportarPorAreasUTHDocx(out result, out msg, Guid.Empty, model.Area, model.Fecha, model.ResponsableArea);
                                    var entryEP = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entryEP.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }

                                    objFile = _repoServcioActas.ExportarUTHADocx(out result, out msg, Guid.Empty, Guid.Empty, model.Fecha, director.Nombre + " " + director.Apellidos);
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
                                    objFile = _repoServcioActas.ExportarUTHPorAreasIndividualDocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
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
                    return RedirectToAction(nameof(UTH), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha, visualizarMovimiento = model.VisualizarMovimiento });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerarActaUTH(mdActas model)
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
                List<UTHConMovimientosParcial> existenciaDeMovimientos = new List<UTHConMovimientosParcial>();
                existenciaDeMovimientos = await _repoUTH.ListadoUTHConMovimientos(model.Fecha);
                areasResponsabilidad = await _repoAreas.ObtenerListadoAreasSiMovimiento(existenciaDeMovimientos);

                if (model.Area == Guid.Empty)
                {
                    if (model.Responsable == Guid.Empty)
                    {
                        responsables = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos, true);

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
                                    respArea = await _repoResponsables.SeleccionarResponsableEPDelArea(item.AreaResponsabilidad??Guid.Empty);
                                    string responsableArea = respArea == null ? "<<S/D>>" : respArea.Nombre + " " + respArea.Apellidos;

                                    objFile = _repoServcioActas.ExportarUTHPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, responsableArea);
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

                                    objFile = _repoServcioActas.ExportarPorAreasUTHDocx(out result, out msg, Guid.Empty, item.IdArea, model.Fecha, responsableArea);
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

                                objFile = _repoServcioActas.ExportarUTHADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                if (objFile != null)
                                {
                                    var entryEP = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entryEP.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }
                                else
                                {
                                    clGlobal.objMensaje = $"Error al generar el acta general: {msg}";
                                }
                            }

                            return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material (Movimientos - {nombreMes.Replace('/', '-')}).zip");
                        }
                        else
                        {
                            clGlobal.siNo = false;
                            clGlobal.objMensaje = $"No existen movimientos en la fecha seleccionada.";
                            return RedirectToAction(nameof(UTH), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha });
                        }
                    }
                    else
                    {
                        responsables = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos.Where(ex => ex.Responsable.Equals(model.Responsable)).ToList(), true);
                        areasResponsabilidad = await _repoAreas.ObtenerListadoAreasSiMovimiento(existenciaDeMovimientos.Where(ex => ex.Responsable.Equals(model.Responsable)).ToList());
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


                                    respArea = await _repoResponsables.SeleccionarResponsableEPDelArea(item.AreaResponsabilidad ?? Guid.Empty);// item.AreaResponsabilidadNavigation.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();

                                    string responsableArea = respArea == null ? "<<S/D>>" : respArea.Nombre + " " + respArea.Apellidos;

                                    objFile = _repoServcioActas.ExportarUTHPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, responsableArea);
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

                                    objFile = _repoServcioActas.ExportarPorAreasUTHDocx(out result, out msg, Guid.Empty, item.IdArea, model.Fecha, responsableArea);
                                    var entry = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entry.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }

                                objFile = _repoServcioActas.ExportarUTHADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                if (objFile != null)
                                {
                                    var entryEP = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entryEP.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }
                                else
                                {
                                    clGlobal.objMensaje = $"Error al generar el acta general: {msg}";
                                }
                            }

                            return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material (Movimientos - {nombreMes.Replace('/', '-')}).zip");
                        }
                        else
                        {
                            clGlobal.siNo = false;
                            clGlobal.objMensaje = $"No existen movimientos en la fecha seleccionada.";
                            return RedirectToAction(nameof(DetallesDeMovimientosUTHs), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha });
                        }
                    }
                }
                else
                {
                    if (model.Responsable == Guid.Empty)
                    {
                        responsables = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos.Where(ex => ex.Area.Equals(model.Area)).ToList(), true);
                        areasResponsabilidad = await _repoAreas.ObtenerListadoAreasSiMovimiento(existenciaDeMovimientos.Where(ex => ex.Area.Equals(model.Area)).ToList());
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
                                    respArea = await _repoResponsables.SeleccionarResponsableEPDelArea(item.AreaResponsabilidad ?? Guid.Empty); //item.AreaResponsabilidadNavigation.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();
									string responsableArea = respArea == null ? "<<S/D>>" : respArea.Nombre + " " + respArea.Apellidos;

                                    objFile = _repoServcioActas.ExportarUTHPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, responsableArea);
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

                                    objFile = _repoServcioActas.ExportarPorAreasUTHDocx(out result, out msg, Guid.Empty, item.IdArea, model.Fecha, responsableArea);
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

                                objFile = _repoServcioActas.ExportarUTHADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                if (objFile != null)
                                {
                                    var entryEP = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entryEP.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }
                                else
                                {
                                    clGlobal.objMensaje = $"Error al generar el acta general: {msg}";
                                }
                            }

                            return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material (Movimientos - {nombreMes.Replace('/', '-')}).zip");
                        }
                        else
                        {
                            clGlobal.siNo = false;
                            clGlobal.objMensaje = $"No existen movimientos en la fecha seleccionada.";
                            return RedirectToAction(nameof(UTH), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha });
                        }
                    }
                    else
                    {
                        responsables = await _repoResponsables.ObtenerListadoResponsablesSiMovimiento(existenciaDeMovimientos
                            .Where(ex => (ex.Responsable.Equals(model.Responsable)) && (ex.Area.Equals(model.Area))).ToList(), true);
                        areasResponsabilidad = await _repoAreas.ObtenerListadoAreasSiMovimiento(existenciaDeMovimientos
                            .Where(ex => (ex.Responsable.Equals(model.Responsable)) && (ex.Area.Equals(model.Area))).ToList());
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
                                    respArea = await _repoResponsables.SeleccionarResponsableEPDelArea(item.AreaResponsabilidad ?? Guid.Empty); //item.AreaResponsabilidadNavigation.Responsables.Where(c => c.ResponsableDelArea).FirstOrDefault();
									string responsableArea = respArea == null ? "<<S/D>>" : respArea.Nombre + " " + respArea.Apellidos;

                                    objFile = _repoServcioActas.ExportarUTHPorAreasIndividualDocx(out result, out msg, item.IdResponsable, model.Area, model.Fecha, responsableArea);
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

                                    objFile = _repoServcioActas.ExportarPorAreasUTHDocx(out result, out msg, Guid.Empty, item.IdArea, model.Fecha, responsableArea);
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

                                objFile = _repoServcioActas.ExportarUTHADocx(out result, out msg, model.Responsable, model.Area, model.Fecha, model.ResponsableArea);
                                tituloDoc = $"ACTA RESP. MAT. ATI GNRAL ({nombreMes.Replace('/', '-')}).docx";

                                if (objFile != null)
                                {
                                    var entryEP = zip.CreateEntry(tituloDoc);

                                    using (var fileStream = new MemoryStream(objFile))
                                    using (var entryStream = entryEP.Open())
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }
                                else
                                {
                                    clGlobal.objMensaje = $"Error al generar el acta general: {msg}";
                                }
                            }

                            return File(ms.ToArray(), "application/zip", $"Actas Responsabilidad Material (Movimientos - {nombreMes.Replace('/', '-')}).zip");
                        }
                        else
                        {
                            clGlobal.siNo = false;
                            clGlobal.objMensaje = $"No existen movimientos en la fecha seleccionada.";
                            return RedirectToAction(nameof(DetallesDeMovimientosUTHs), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clGlobal.objMensaje = $"Ocurrió el siguiente error: {ex.Message}";
                return RedirectToAction(nameof(DetallesDeMovimientosUTHs), new { area = model.Area, responsable = model.Responsable, fecha = model.Fecha });
            }
        }

        public async Task<IActionResult> DetallesDeMovimientosUTHs(Guid? area, Guid? responsable, DateTime? fecha)
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

            return View("vDetallesDeMovimientosUTH", new mdActas() { Areas = areas, Responsables = responsables, Fecha = fecha == null ? DateTime.Today.AddMonths(-1) : new DateTime(fecha.Value.Year, fecha.Value.Month, fecha.Value.Day) });
        }

        [HttpPost]
        public async Task<IActionResult> MostrarDetallesMovimientosUTHs(DateTime fecha, string area, string responsable)
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
            Run r1 = new Run();
            Text t1 = new Text("Pellentesque ") { Space = SpaceProcessingModeValues.Preserve };
            // The Space attribute preserve white space before and after your text
            r1.Append(t1);
            p.Append(r1);

            // Run 2 - Bold
            Run r2 = new Run();
            RunProperties rp2 = new RunProperties();
            rp2.Bold = new Bold();
            // Always add properties first
            r2.Append(rp2);
            Text t2 = new Text("commodo ") { Space = SpaceProcessingModeValues.Preserve };
            r2.Append(t2);
            p.Append(r2);

            // Run 3
            Run r3 = new Run();
            Text t3 = new Text("rhoncus ") { Space = SpaceProcessingModeValues.Preserve };
            r3.Append(t3);
            p.Append(r3);

            // Run 4 – Italic
            Run r4 = new Run();
            RunProperties rp4 = new RunProperties();
            rp4.Italic = new Italic();
            // Always add properties first
            r4.Append(rp4);
            Text t4 = new Text("mauris") { Space = SpaceProcessingModeValues.Preserve };
            r4.Append(t4);
            p.Append(r4);

            // Run 5
            Run r5 = new Run();
            Text t5 = new Text(", sit ") { Space = SpaceProcessingModeValues.Preserve };
            r5.Append(t5);
            p.Append(r5);

            // Run 6 – Italic , bold and underlined
            Run r6 = new Run();
            RunProperties rp6 = new RunProperties();
            rp6.Italic = new Italic();
            rp6.Bold = new Bold();
            rp6.Underline = new Underline();
            // Always add properties first
            r6.Append(rp6);
            Text t6 = new Text("amet ") { Space = SpaceProcessingModeValues.Preserve };
            r6.Append(t6);
            p.Append(r6);

            // Run 7
            Run r7 = new Run();
            Text t7 = new Text("faucibus arcu ") { Space = SpaceProcessingModeValues.Preserve };
            r7.Append(t7);
            p.Append(r7);

            // Run 8 – Red color
            Run r8 = new Run();
            RunProperties rp8 = new RunProperties();
            rp8.Color = new Color() { Val = "FF0000" };
            // Always add properties first
            r8.Append(rp8);
            Text t8 = new Text("porttitor ") { Space = SpaceProcessingModeValues.Preserve };
            r8.Append(t8);
            p.Append(r8);

            // Run 9
            Run r9 = new Run();
            Text t9 = new Text("pharetra. Maecenas quis erat quis eros iaculis placerat ut at mauris.") { Space = SpaceProcessingModeValues.Preserve };
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
            Run r = new Run();
            Text t = new Text("Nam eu tortor ut mi euismod eleifend in ut ante. Donec a ligula ante. Sed rutrum ex quam. Nunc id mi ultricies, vestibulum sapien vel, posuere dui.") { Space = SpaceProcessingModeValues.Preserve };
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
                    Table table = new Table();

                    // Create a TableProperties object and specify its border information.
                    TableProperties tblProp = new TableProperties(
                        new TableBorders(
                            new TopBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Dashed),
                                Size = 24
                            },
                            new BottomBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Dashed),
                                Size = 24
                            },
                            new LeftBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Dashed),
                                Size = 24
                            },
                            new RightBorder()
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
                    tc1.Append(new Paragraph(new Run(new Text("some text"))));

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

        private async Task<List<mdMuestreoPendienteItemUTH>> ObtenerItemsUTH(int anio, int mes)
        {
            var uths = await _context.UtilesHerramientas
                .Include(u => u.AreaResponsabilidadNavigation)
                .Include(u => u.ResponsableNavigation)
                .Include(u => u.CentroCostoNavigation)
                .Where(u => u.FechaListadoUth.Year == anio && u.FechaListadoUth.Month == mes)
                .ToListAsync();

            return uths
                .Select(u => new mdMuestreoPendienteItemUTH
                {
                    IdUth = u.IdUth,
                    Codigo = u.Codigo,
                    Descripcion = u.Descripcion,
                    Responsable = u.Responsable,
                    NombreResponsable = u.ResponsableNavigation?.Nombre,
                    ApellidosResponsable = u.ResponsableNavigation?.Apellidos,
                    CentroCosto = u.CentroCosto,
                    NombreCentroCosto = u.CentroCostoNavigation?.Nombre,
                    AreaResponsabilidad = u.AreaResponsabilidad,
                    AreaNombre = u.AreaResponsabilidadNavigation?.Nombre,
                    Valor = u.ValorAdquisicion
                })
                .ToList();
        }

        public async Task<IActionResult> MuestreoDiezPorCientoUTH()
        {
            var (mes, anio) = ObtenerMesAnioMuestreo();

            List<mdMuestreoPendienteItemUTH> muestra = new List<mdMuestreoPendienteItemUTH>();
            bool muestraPendiente = false;
            bool yaExisteConfirmado = false;

            var sessionJson = HttpContext.Session.GetString(SessionKeyMuestraUTH(anio, mes));
            if (sessionJson != null)
            {
                var pendiente = JsonSerializer.Deserialize<mdMuestreoPendienteUTH>(sessionJson, _jsonOptions);
                if (pendiente?.Items.Count > 0)
                {
                    muestraPendiente = true;
                    muestra = pendiente.Items;
                }
            }
            else
            {
                yaExisteConfirmado = await _context.MuestreoMensualDiezPorCientoUTHs
                    .AnyAsync(m => m.Anno == anio && m.Mes == mes);

                if (yaExisteConfirmado)
                {
                    clGlobal.siNo = false;
                    clGlobal.objMensaje = $"Ya existe un muestreo confirmado para {mes}/{anio}.";
                }
            }

            var itemsDelMes = await ObtenerItemsUTH(anio, mes);

            int totalItemsDistintos = itemsDelMes.Count;
            int totalAreas = itemsDelMes.Select(i => i.AreaResponsabilidad).Distinct().Count();

            var areaNombreLookup = itemsDelMes
                .Where(i => i.AreaNombre != null)
                .GroupBy(i => i.AreaResponsabilidad)
                .ToDictionary(g => g.Key, g => g.First().AreaNombre);

            List<mdMuestreoResumenArea> resumenAreas;
            int limiteMuestra;
            int areasCubiertas;

            if (muestraPendiente)
            {
                resumenAreas = muestra
                    .GroupBy(m => m.AreaResponsabilidad)
                    .Select(g => new mdMuestreoResumenArea
                    {
                        AreaResponsabilidad = g.Key,
                        AreaNombre = areaNombreLookup.GetValueOrDefault(g.Key),
                        Cantidad = g.Count(),
                        Inventarios = string.Join(", ", g.Select(m => m.Codigo))
                    })
                    .OrderByDescending(r => r.Cantidad)
                    .ToList();

                limiteMuestra = muestra.Count;
                areasCubiertas = resumenAreas.Count;
            }
            else
            {
                resumenAreas = new List<mdMuestreoResumenArea>();
                limiteMuestra = 0;
                areasCubiertas = 0;
            }

            ViewData["totalItemsDistintos"] = totalItemsDistintos;
            ViewData["limiteMuestra"] = limiteMuestra;
            ViewData["totalAreas"] = totalAreas;
            ViewData["areasCubiertas"] = areasCubiertas;
            ViewData["resumenAreas"] = resumenAreas;
            ViewData["yaExisteMuestreo"] = muestra.Count > 0;
            ViewData["muestraPendiente"] = muestraPendiente;
            ViewData["yaExisteConfirmado"] = yaExisteConfirmado;
            ViewData["mesMuestreo"] = mes;
            ViewData["anioMuestreo"] = anio;

            ViewData["breadsC"] = LlenarBreadsCumbs("Muestreo 10% - Útiles y Herramientas");

            ViewData["respuesta"] = new clResponse<List<mdMuestreoPendienteItemUTH>>()
            {
                SiSatisfactorio = clGlobal.siNo,
                Mensaje = clGlobal.objMensaje
            };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vMuestreoDiezPorCientoUTH", muestra);
        }

        [HttpGet]
        public async Task<IActionResult> ConsultaMuestreoDiezPorCientoUTH(DateTime? fecha)
        {
            int mes = fecha?.Month ?? 0;
            int anio = fecha?.Year ?? 0;

            ViewData["mesMuestreo"] = mes;
            ViewData["anioMuestreo"] = anio;
            ViewData["breadsC"] = LlenarBreadsCumbs("Consulta Muestreo 10% - Útiles y Herramientas");

            if (fecha == null)
            {
                ViewData["yaExisteMuestreo"] = false;
                return View("vConsultaMuestreoDiezPorCientoUTH", new List<mdMuestreoPendienteItemUTH>());
            }

            var registros = await _context.MuestreoMensualDiezPorCientoUTHs
                .Where(m => m.Anno == anio && m.Mes == mes)
                .ToListAsync();

            var areaIds = registros.Select(m => m.AreaResponsabilidad).Distinct().ToList();
            var ccIds = registros.Where(m => m.CentroCosto != null).Select(m => m.CentroCosto.Value).Distinct().ToList();
            var respIds = registros.Where(m => m.Responsable != null).Select(m => m.Responsable.Value).Distinct().ToList();

            var areas = areaIds.Any()
                ? await _context.AreaDeResponsabilidads.Where(a => areaIds.Contains(a.IdArea)).ToDictionaryAsync(a => a.IdArea)
                : new Dictionary<Guid, AreaDeResponsabilidad>();
            var centros = ccIds.Any()
                ? await _context.CentroCostos.Where(c => ccIds.Contains(c.IdCentroCosto)).ToDictionaryAsync(c => c.IdCentroCosto)
                : new Dictionary<Guid, CentroCosto>();
            var responsables = respIds.Any()
                ? await _context.Responsables.Where(r => respIds.Contains(r.IdResponsable)).ToDictionaryAsync(r => r.IdResponsable)
                : new Dictionary<Guid, Responsable>();

            var muestra = registros.Select(r => new mdMuestreoPendienteItemUTH
            {
                IdUth = r.IdUth,
                Codigo = r.Codigo,
                Descripcion = r.Descripcion,
                Responsable = r.Responsable,
                NombreResponsable = r.Responsable != null && responsables.TryGetValue(r.Responsable.Value, out var resp) ? resp.Nombre : null,
                ApellidosResponsable = r.Responsable != null && responsables.TryGetValue(r.Responsable.Value, out var resp2) ? resp2.Apellidos : null,
                CentroCosto = r.CentroCosto,
                NombreCentroCosto = r.CentroCosto != null && centros.TryGetValue(r.CentroCosto.Value, out var cc) ? cc.Nombre : null,
                AreaResponsabilidad = r.AreaResponsabilidad,
                AreaNombre = areas.TryGetValue(r.AreaResponsabilidad, out var ar) ? ar.Nombre : null,
                Valor = r.Valor,
                SiExiste = r.SiExiste,
                Observaciones = r.Observaciones
            }).ToList();

            var itemsDelMes = await ObtenerItemsUTH(anio, mes);

            int totalItemsDistintos = itemsDelMes.Count;
            int limiteMuestra = (int)Math.Max(1, Math.Floor(totalItemsDistintos * 0.10));

            var areaTotals = itemsDelMes
                .GroupBy(i => i.AreaResponsabilidad)
                .Select(g => new { Area = g.Key, Count = g.Count(), Nombre = g.First().AreaNombre ?? "" })
                .OrderByDescending(x => x.Count)
                .ToList();

            int totalAreas = areaTotals.Count;

            var actualLookup = muestra
                .GroupBy(m => m.AreaResponsabilidad)
                .ToDictionary(g => g.Key, g => g.ToList());

            var cuotas = CalcularCuotasPorArea(
                areaTotals.Select(a => (a.Area, a.Count)).ToList(),
                limiteMuestra);

            var areaNombreLookup = areaTotals.ToDictionary(at => at.Area, at => at.Nombre);

            List<mdMuestreoResumenArea> resumenAreas = cuotas
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => new mdMuestreoResumenArea
                {
                    AreaResponsabilidad = kvp.Key,
                    AreaNombre = areaNombreLookup.ContainsKey(kvp.Key) ? areaNombreLookup[kvp.Key] : null,
                    Cantidad = kvp.Value,
                    Inventarios = actualLookup.ContainsKey(kvp.Key)
                        ? string.Join(", ", actualLookup[kvp.Key].Take(kvp.Value).Select(m => m.Codigo))
                        : ""
                })
                .OrderByDescending(r => r.Cantidad)
                .ToList();

            int areasCubiertas = resumenAreas.Count;

            var selectedIds = new HashSet<Guid>();
            foreach (var area in resumenAreas)
            {
                if (area.AreaResponsabilidad.HasValue && actualLookup.TryGetValue(area.AreaResponsabilidad.Value, out var itemsArea))
                {
                    foreach (var item in itemsArea.Take(area.Cantidad))
                        selectedIds.Add(item.IdUth);
                }
            }
            var muestraFiltrada = muestra.Where(m => selectedIds.Contains(m.IdUth)).ToList();

            ViewData["totalItemsDistintos"] = totalItemsDistintos;
            ViewData["limiteMuestra"] = limiteMuestra;
            ViewData["totalAreas"] = totalAreas;
            ViewData["areasCubiertas"] = areasCubiertas;
            ViewData["resumenAreas"] = resumenAreas;
            ViewData["yaExisteMuestreo"] = muestraFiltrada.Count > 0;

            var hoy = DateTime.Today;
            int mesEditable = hoy.Month == 1 ? 12 : hoy.Month - 1;
            int anioEditable = hoy.Month == 1 ? hoy.Year - 1 : hoy.Year;
            ViewData["editable"] = anio == anioEditable && mes == mesEditable && muestraFiltrada.Count > 0;

            return View("vConsultaMuestreoDiezPorCientoUTH", muestraFiltrada);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EjecutarMuestreoUTH()
        {
            var (mes, anio) = ObtenerMesAnioMuestreo();

            try
            {
                var itemsDelMes = await ObtenerItemsUTH(anio, mes);

                if (itemsDelMes.Count == 0)
                {
                    clGlobal.siNo = false;
                    clGlobal.objMensaje = $"No existen útiles y herramientas registrados en el período {mes}/{anio}. No es posible ejecutar el muestreo.";
                }
                else
                {
                    bool yaExisteBD = await _context.MuestreoMensualDiezPorCientoUTHs
                        .AnyAsync(m => m.Anno == anio && m.Mes == mes);

                    if (yaExisteBD)
                    {
                        clGlobal.siNo = false;
                        clGlobal.objMensaje = $"El muestreo de {mes}/{anio} ya fue ejecutado y confirmado anteriormente.";
                    }
                    else
                    {
                        var yaMuestreados = await _context.MuestreoMensualDiezPorCientoUTHs
                            .Where(m => m.Anno == anio)
                            .Select(m => m.IdUth)
                            .ToListAsync();

                        var excluidos = yaMuestreados.ToHashSet();

                        var itemsDisponibles = itemsDelMes
                            .Where(i => !excluidos.Contains(i.IdUth))
                            .ToList();

                        if (itemsDisponibles.Count == 0)
                        {
                            clGlobal.siNo = false;
                            clGlobal.objMensaje = $"Todos los útiles y herramientas del período {mes}/{anio} ya fueron muestreados anteriormente.";
                        }
                        else
                        {
                            int totalItemsDistintos = itemsDelMes.Count;
                            int limiteMuestra = (int)Math.Max(1, Math.Floor(totalItemsDistintos * 0.10));

                            var areaTotals = itemsDelMes
                                .GroupBy(i => i.AreaResponsabilidad)
                                .Select(g => new { Area = g.Key, Count = g.Count(), Nombre = g.First().AreaNombre ?? "" })
                                .OrderByDescending(x => x.Count)
                                .ToList();

                            var cuotas = CalcularCuotasPorArea(
                                areaTotals.Select(a => (a.Area, a.Count)).ToList(),
                                limiteMuestra);

                            var rnd = new Random();
                            var pendienteItems = new List<mdMuestreoPendienteItemUTH>();

                            foreach (var area in areaTotals)
                            {
                                int quota = cuotas.GetValueOrDefault(area.Area, 0);
                                if (quota <= 0) continue;

                                var seleccionados = itemsDisponibles
                                    .Where(i => i.AreaResponsabilidad == area.Area)
                                    .OrderBy(_ => rnd.Next())
                                    .Take(quota)
                                    .ToList();

                                foreach (var s in seleccionados)
                                {
                                    pendienteItems.Add(new mdMuestreoPendienteItemUTH
                                    {
                                        IdUth = s.IdUth,
                                        Codigo = s.Codigo,
                                        Descripcion = s.Descripcion,
                                        Responsable = s.Responsable,
                                        NombreResponsable = s.NombreResponsable,
                                        ApellidosResponsable = s.ApellidosResponsable,
                                        CentroCosto = s.CentroCosto,
                                        NombreCentroCosto = s.NombreCentroCosto,
                                        AreaResponsabilidad = s.AreaResponsabilidad,
                                        AreaNombre = s.AreaNombre,
                                        Valor = s.Valor,
                                        SiExiste = true
                                    });
                                }
                            }

                            var pendiente = new mdMuestreoPendienteUTH
                            {
                                Anno = anio,
                                Mes = mes,
                                Items = pendienteItems
                            };

                            var json = JsonSerializer.Serialize(pendiente, _jsonOptions);
                            HttpContext.Session.SetString(SessionKeyMuestraUTH(anio, mes), json);

                            clGlobal.siNo = true;
                            clGlobal.objMensaje = $"Muestreo de {mes}/{anio} completado exitosamente: {pendienteItems.Count} ítems seleccionados.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clGlobal.siNo = false;
                clGlobal.objMensaje = "Error al ejecutar el muestreo: " + ex.Message;
            }

            return RedirectToAction(nameof(MuestreoDiezPorCientoUTH));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarMuestreoUTH([FromBody] List<mdMuestreoConfirmacionUTH> items)
        {
            if (items == null || items.Count == 0)
                return Json(new { success = false, mensaje = "No se recibieron datos para confirmar." });

            try
            {
                var firstItem = items.First();
                var json = HttpContext.Session.GetString(SessionKeyMuestraUTH(firstItem.Anno, firstItem.Mes));
                if (json == null)
                    return Json(new { success = false, mensaje = "No hay datos de muestreo pendientes en la sesión." });

                var pendiente = JsonSerializer.Deserialize<mdMuestreoPendienteUTH>(json, _jsonOptions);
                if (pendiente == null || pendiente.Items.Count == 0)
                    return Json(new { success = false, mensaje = "No hay datos de muestreo pendientes en la sesión." });

                int anno = pendiente.Anno;
                int mes = pendiente.Mes;

                var existentes = await _context.MuestreoMensualDiezPorCientoUTHs
                    .Where(m => m.Anno == anno && m.Mes == mes)
                    .ToListAsync();

                if (existentes.Any())
                {
                    foreach (var record in existentes)
                    {
                        record.SiExiste = true;
                        record.Observaciones = null;
                    }
                }
                else
                {
                    var nuevos = pendiente.Items.Select(orig => new MuestreoMensualDiezPorCientoUTH
                    {
                        Anno = anno,
                        Mes = mes,
                        IdUth = orig.IdUth,
                        Codigo = orig.Codigo,
                        Responsable = orig.Responsable,
                        CentroCosto = orig.CentroCosto,
                        AreaResponsabilidad = orig.AreaResponsabilidad,
                        Descripcion = orig.Descripcion,
                        Valor = orig.Valor,
                        FechaMuestreo = DateTime.Now,
                        SiExiste = true,
                        Observaciones = null
                    }).ToList();

                    _context.MuestreoMensualDiezPorCientoUTHs.AddRange(nuevos);
                }

                await _context.SaveChangesAsync();
                HttpContext.Session.Remove(SessionKeyMuestraUTH(anno, mes));
                return Json(new { success = true, mensaje = "Muestreo confirmado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = "Error al confirmar el muestreo: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarSiExisteUTH(int anno, int mes, Guid idUth, bool siExiste, string? observaciones = null)
        {
            var json = HttpContext.Session.GetString(SessionKeyMuestraUTH(anno, mes));
            if (json != null)
            {
                var pendiente = JsonSerializer.Deserialize<mdMuestreoPendienteUTH>(json, _jsonOptions);
                if (pendiente != null)
                {
                    var item = pendiente.Items.FirstOrDefault(i => i.IdUth == idUth);
                    if (item != null)
                    {
                        item.SiExiste = siExiste;
                        item.Observaciones = observaciones;
                        HttpContext.Session.SetString(SessionKeyMuestraUTH(anno, mes), JsonSerializer.Serialize(pendiente, _jsonOptions));
                        return Json(new { success = true, siExiste, observaciones });
                    }
                }
            }

            var registro = await _context.MuestreoMensualDiezPorCientoUTHs
                .FirstOrDefaultAsync(m => m.Anno == anno && m.Mes == mes && m.IdUth == idUth);
            if (registro != null)
            {
                registro.SiExiste = siExiste;
                registro.Observaciones = observaciones;
                await _context.SaveChangesAsync();
                return Json(new { success = true, siExiste, observaciones });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarActaMuestreoUTH()
        {
            var (mes, anio) = ObtenerMesAnioMuestreo();

            var muestra = await _context.MuestreoMensualDiezPorCientoUTHs
                .Where(m => m.Anno == anio && m.Mes == mes)
                .ToListAsync();

            if (!muestra.Any())
            {
                TempData["Mensaje"] = "No hay datos de muestreo para exportar.";
                return RedirectToAction(nameof(ConsultaMuestreoDiezPorCientoUTH));
            }

            string templatePath = System.IO.Path.Combine(_env.WebRootPath, "plantillas", "PlantillaActaMuestreoUTH.xlsx");

            var ccIds = muestra.Where(m => m.CentroCosto != null).Select(m => m.CentroCosto.Value).Distinct().ToList();
            var centros = ccIds.Any()
                ? await _context.CentroCostos.Where(c => ccIds.Contains(c.IdCentroCosto)).ToDictionaryAsync(c => c.IdCentroCosto)
                : new Dictionary<Guid, CentroCosto>();

            var grupos = muestra
                .Where(m => m.CentroCosto != null)
                .GroupBy(m => m.CentroCosto.Value)
                .OrderBy(g => centros.TryGetValue(g.Key, out var cc) && cc.Nombre != null ? cc.Nombre : "ZZZ")
                .ToList();

            if (!grupos.Any())
            {
                TempData["Mensaje"] = "No hay centros de costo asociados a la muestra.";
                return RedirectToAction(nameof(ConsultaMuestreoDiezPorCientoUTH));
            }

            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var templateSheet = package.Workbook.Worksheets[0];
                string templateSheetName = templateSheet.Name;

                int areaIndex = 0;
                foreach (var grupo in grupos)
                {
                    var items = grupo.ToList();
                    string areaNombre = centros.TryGetValue(grupo.Key, out var cc) && cc.Nombre != null ? cc.Nombre : "SIN ÁREA";
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

                        ws.Cells[row, 2, row, 3].Merge = true;
                        ws.Cells[row, 2].Value = item.Codigo;

                        ws.Cells[row, 4, row, 7].Merge = true;
                        ws.Cells[row, 4].Value = item.Descripcion;

                        ws.Cells[row, 8].Value = item.Codigo;

                        decimal cup = item.Valor ?? 0;
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

                string fileName = $"MuestreoUTH10_{anio}_{mes:D2}.xlsx";
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarListadoCompletoUTH(DateTime fecha, string area, string responsable)
        {
            List<UtilesHerramienta> uthList = await _repoUTH.ListadoUTH(fecha, Guid.Parse(area), Guid.Parse(responsable));

            if (!uthList.Any())
            {
                TempData["Mensaje"] = "No hay datos para exportar.";
                return RedirectToAction(nameof(UTHImportados), new { fecha });
            }

            string templatePath = System.IO.Path.Combine(_env.WebRootPath, "plantillas", "PlantillaActaMuestreoUTH.xlsx");

            var ccIds = uthList.Where(u => u.CentroCosto != null).Select(u => u.CentroCosto.Value).Distinct().ToList();
            var centros = ccIds.Any()
                ? await _context.CentroCostos.Where(c => ccIds.Contains(c.IdCentroCosto)).ToDictionaryAsync(c => c.IdCentroCosto)
                : new Dictionary<Guid, CentroCosto>();

            var grupos = uthList
                .Where(u => u.CentroCosto != null)
                .GroupBy(u => u.CentroCosto.Value)
                .OrderBy(g => centros.TryGetValue(g.Key, out var cc) && cc.Nombre != null ? cc.Nombre : "ZZZ")
                .ToList();

            if (!grupos.Any())
            {
                TempData["Mensaje"] = "No hay centros de costo asociados al listado.";
                return RedirectToAction(nameof(UTHImportados), new { fecha });
            }

            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var templateSheet = package.Workbook.Worksheets[0];
                string templateSheetName = templateSheet.Name;

                int areaIndex = 0;
                foreach (var grupo in grupos)
                {
                    var items = grupo.ToList();
                    string areaNombre = centros.TryGetValue(grupo.Key, out var cc) && cc.Nombre != null ? cc.Nombre : "SIN ÁREA";
                    string sheetName = areaNombre.Length > 31 ? areaNombre[..31] : areaNombre;

                    ExcelWorksheet ws = package.Workbook.Worksheets.Copy(templateSheetName, sheetName);

                    ws.Cells["F2"].Value = "MINEM";
                    ws.Cells["F3"].Value = "Empresa de Tecnologías de la Información y Automática";
                    ws.Cells["F5"].Value = areaNombre;
                    ws.Cells["AA4"].Value = new DateTime(fecha.Year, fecha.Month, 1);
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

                        ws.Cells[row, 2, row, 3].Merge = true;
                        ws.Cells[row, 2].Value = item.Codigo;

                        ws.Cells[row, 4, row, 7].Merge = true;
                        ws.Cells[row, 4].Value = item.Descripcion;

                        ws.Cells[row, 8].Value = item.Codigo;

                        decimal cup = item.ValorContable ?? 0;
                        ws.Cells[row, 9, row, 10].Merge = true;
                        ws.Cells[row, 9].Value = cup;
                        totalCup += cup;

                        ws.Cells[row, 11, row, 13].Merge = true;
                        ws.Cells[row, 11].Value = 0;
                        ws.Cells[row, 14].Value = 1;
                        ws.Cells[row, 16, row, 17].Merge = true;
                        ws.Cells[row, 20, row, 21].Merge = true;
                        ws.Cells[row, 25, row, 30].Merge = true;
                        ws.Cells[row, 25].Value = item.ResponsableNavigation != null
                            ? $"{item.ResponsableNavigation.Nombre} {item.ResponsableNavigation.Apellidos}"
                            : "<<S/R>>";

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

                string fileName = $"ListadoCompletoUTH_{fecha.Year}_{fecha.Month:D2}.xlsx";
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PlanMuestreoAnualUTH(int? anio)
        {
            int year = anio ?? 0;

            ViewData["breadsC"] = LlenarBreadsCumbs("Planificación Muestreo Anual - Útiles y Herramientas");

            var model = new mdPlanMuestreoAnual { Anio = year };

            if (anio == null)
                return View("vPlanMuestreoAnualUTH", model);

            var uths = await _context.UtilesHerramientas
                .Include(u => u.AreaResponsabilidadNavigation)
                .Where(u => u.FechaListadoUth.Year == year - 1 && u.FechaListadoUth.Month == 12)
                .ToListAsync();

            var distinctItems = uths.ToList();

            int totalBase = distinctItems.Count;

            model.TotalActivosBase = totalBase;
            model.DiezPorCientoEsperado = (int)Math.Max(1, Math.Round(totalBase * 0.10));

            bool existePlan = await _context.PlanMuestreoAnualUTHs.AnyAsync(p => p.Anno == year);
            model.EstaGuardado = existePlan;

            var areasAgrupadas = distinctItems
                .GroupBy(u => u.AreaResponsabilidadNavigation?.Nombre ?? "S/A")
                .Select(g => new mdPlanMuestreoArea
                {
                    AreaId = g.First().AreaResponsabilidad,
                    NombreArea = g.Key,
                    CantidadCentros = g.Select(u => u.CentroCosto ?? Guid.Empty).Distinct().Count(),
                    Seleccionado = true
                })
                .OrderBy(a => a.NombreArea)
                .ToList();

            model.Areas = areasAgrupadas;

            return View("vPlanMuestreoAnualUTH", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlanMuestreoAnualUTH(int anio, List<string>? areasSeleccionadas)
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Planificación Muestreo Anual - Útiles y Herramientas");

            var selectedGuids = areasSeleccionadas?
                .Select(a => Guid.TryParse(a, out var g) ? (Guid?)g : null)
                .Where(g => g.HasValue)
                .Select(g => g.Value)
                .ToHashSet() ?? new HashSet<Guid>();

            var uthsDic = await _context.UtilesHerramientas
                .Include(u => u.CentroCostoNavigation)
                .Include(u => u.AreaResponsabilidadNavigation)
                .Where(u => u.FechaListadoUth.Year == anio - 1 && u.FechaListadoUth.Month == 12)
                .ToListAsync();

            var distinctItems = uthsDic.ToList();

            int totalBase = distinctItems.Count;

            var model = new mdPlanMuestreoAnual
            {
                Anio = anio,
                TotalActivosBase = totalBase,
                DiezPorCientoEsperado = (int)Math.Max(1, Math.Round(totalBase * 0.10)),
                Items = new List<mdPlanMuestreoAnualItem>()
            };

            var selectedNames = distinctItems
                .Where(u => u.AreaResponsabilidadNavigation != null && selectedGuids.Contains(u.AreaResponsabilidad))
                .Select(u => u.AreaResponsabilidadNavigation!.Nombre)
                .Distinct()
                .ToHashSet();

            var expandedGuids = distinctItems
                .Where(u => u.AreaResponsabilidadNavigation != null && selectedNames.Contains(u.AreaResponsabilidadNavigation.Nombre))
                .Select(u => u.AreaResponsabilidad)
                .Distinct()
                .ToHashSet();

            if (selectedGuids.Contains(Guid.Empty))
                expandedGuids.Add(Guid.Empty);

            var grupos = distinctItems
                .Where(u => expandedGuids.Contains(u.AreaResponsabilidad))
                .GroupBy(u => u.AreaResponsabilidad)
                .ToList();

            var areasInfo = new List<(Guid areaId, string nombreArea, int total, Guid centroCosto, string numCC, string nomCC)>();
            foreach (var grupo in grupos)
            {
                int total = grupo.Count();
                var first = grupo.First();
                var ccValido = grupo
                    .Where(u => u.CentroCostoNavigation != null && u.CentroCostoNavigation.Numero != null)
                    .Select(u => u.CentroCostoNavigation)
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

            model.EstaGuardado = await _context.PlanMuestreoAnualUTHs.AnyAsync(p => p.Anno == anio);

            ViewData["areasSeleccionadas"] = string.Join(",", expandedGuids);

            return View("vPlanMuestreoAnualUTH", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPlanMuestreoUTH(int anio, List<string>? areasSeleccionadas)
        {
            try
            {
                bool yaExiste = await _context.PlanMuestreoAnualUTHs.AnyAsync(p => p.Anno == anio);
                if (yaExiste)
                {
                    var viejos = await _context.PlanMuestreoAnualUTHs.Where(p => p.Anno == anio).ToListAsync();
                    _context.PlanMuestreoAnualUTHs.RemoveRange(viejos);
                    await _context.SaveChangesAsync();
                }

                var selectedAreas = areasSeleccionadas?
                    .Select(a => Guid.TryParse(a, out var g) ? (Guid?)g : null)
                    .Where(g => g.HasValue)
                    .Select(g => g.Value)
                    .ToHashSet() ?? new HashSet<Guid>();

                var uthsDic = await _context.UtilesHerramientas
                    .Include(u => u.CentroCostoNavigation)
                    .Include(u => u.AreaResponsabilidadNavigation)
                    .Where(u => u.FechaListadoUth.Year == anio - 1 && u.FechaListadoUth.Month == 12)
                    .ToListAsync();

                var distinctItems = uthsDic.ToList();

                var nombresSinCC = distinctItems
                    .Where(u => selectedAreas.Contains(u.AreaResponsabilidad))
                    .GroupBy(u => u.AreaResponsabilidadNavigation?.Nombre ?? "S/A")
                    .Where(g => !g.Any(u => u.CentroCostoNavigation != null && u.CentroCostoNavigation.Numero != null))
                    .Select(g => g.Key)
                    .ToList();

                if (nombresSinCC.Any())
                    return Json(new { success = false, mensaje = $"No se puede guardar: las siguientes áreas no tienen centro de costo asignado: {string.Join(", ", nombresSinCC)}. Asígneles un centro de costo e intente nuevamente." });

                var grupos = distinctItems
                    .Where(u => selectedAreas.Contains(u.AreaResponsabilidad))
                    .GroupBy(u => u.AreaResponsabilidad)
                    .ToList();

                var areasInfo = new List<(Guid areaId, string nombreArea, int total, Guid centroCosto, string numCC, string nomCC)>();
                foreach (var grupo in grupos)
                {
                    int total = grupo.Count();
                    var first = grupo.First();
                    var ccValido = grupo
                        .Where(u => u.CentroCostoNavigation != null && u.CentroCostoNavigation.Numero != null)
                        .Select(u => u.CentroCostoNavigation)
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
                var planes = items.Select(item => new PlanMuestreoAnualUTH
                {
                    Anno = anio,
                    CentroCosto = item.CentroCosto,
                    AreaResponsabilidad = item.AreaResponsabilidad,
                    TotalUtiles = item.TotalActivos,
                    Ene = item.Ene, Feb = item.Feb, Mar = item.Mar, Abr = item.Abr,
                    May = item.May, Jun = item.Jun, Jul = item.Jul, Ago = item.Ago,
                    Sep = item.Sep, Oct = item.Oct, Nov = item.Nov, Dic = item.Dic,
                    FechaCreacion = DateTime.Now
                }).ToList();

                if (!planes.Any())
                    return Json(new { success = false, mensaje = "No se encontraron útiles y herramientas en diciembre para generar la planificación con las áreas seleccionadas." });

                _context.PlanMuestreoAnualUTHs.AddRange(planes);
                await _context.SaveChangesAsync();

                return Json(new { success = true, mensaje = $"Planificación del año {anio} guardada correctamente ({planes.Count} áreas)." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = "Error al guardar la planificación: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerPlanMuestreoAnualUTH(int? anio)
        {
            int year = anio ?? 0;

            ViewData["breadsC"] = LlenarBreadsCumbs("Planificación Muestreo Anual - Útiles y Herramientas");

            var model = new mdPlanMuestreoAnual { Anio = year };

            try
            {
                if (anio == null)
                    return View("vVerPlanMuestreoAnualUTH", model);

                var uths = await _context.UtilesHerramientas
                    .Where(u => u.FechaListadoUth.Year == year - 1 && u.FechaListadoUth.Month == 12)
                    .ToListAsync();

                int totalBase = uths.Count;

                model.TotalActivosBase = totalBase;
                model.DiezPorCientoEsperado = (int)Math.Max(1, Math.Round(totalBase * 0.10));

                var planGuardado = await _context.PlanMuestreoAnualUTHs
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
                        model.Items.Add(MapearItemUTH(p, centros, areas));

                    model.Items = AgruparItemsPorArea(model.Items);
                    model.Totales = CalcularTotales(model.Items);
                }

                return View("vVerPlanMuestreoAnualUTH", model);
            }
            catch (Exception)
            {
                return View("vVerPlanMuestreoAnualUTH", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcelPlanMuestreoUTH(int anio)
        {
            var planGuardado = await _context.PlanMuestreoAnualUTHs
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

            var items = new List<mdPlanMuestreoAnualItem>();
            foreach (var p in planGuardado)
                items.Add(MapearItemUTH(p, centros, areas));

            items = AgruparItemsPorArea(items);
            var totales = CalcularTotales(items);

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("PlanMuestreo");

            ws.Cells[1, 1].Value = "Planificación de Conteo Anual de Útiles y Herramientas - " + anio;
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

            string fileName = $"PlanMuestreoAnualUTH_{anio}.xlsx";
            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private static mdPlanMuestreoAnualItem MapearItemUTH(PlanMuestreoAnualUTH p, Dictionary<Guid, CentroCosto> centros, Dictionary<Guid, AreaDeResponsabilidad> areas)
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

            return new mdPlanMuestreoAnualItem
            {
                CentroCosto = p.CentroCosto,
                NumeroCentroCosto = numeroCC ?? "S/A",
                NombreCentroCosto = nombreCC ?? "S/A",
                AreaResponsabilidad = p.AreaResponsabilidad,
                NombreArea = nombreArea ?? "S/A",
                TotalActivos = p.TotalUtiles,
                Ene = p.Ene, Feb = p.Feb, Mar = p.Mar, Abr = p.Abr,
                May = p.May, Jun = p.Jun, Jul = p.Jul, Ago = p.Ago,
                Sep = p.Sep, Oct = p.Oct, Nov = p.Nov, Dic = p.Dic
            };
        }

        private static Dictionary<Guid, int> CalcularCuotasPorArea(List<(Guid areaId, int total)> areas, int limite)
        {
            var cuotas = areas.ToDictionary(a => a.areaId, _ => 0);
            var ordenadas = areas.OrderByDescending(a => a.total).ToList();
            int grandTotal = areas.Sum(a => a.total);
            int restante = limite;

            foreach (var a in ordenadas)
            {
                if (restante <= 0) break;
                if (a.total >= 1) { cuotas[a.areaId] = 1; restante--; }
            }

            if (restante > 0 && grandTotal > 0)
            {
                var floors = new Dictionary<Guid, int>();
                var residuals = new Dictionary<Guid, double>();
                int sumaBase = 0;
                foreach (var a in ordenadas)
                {
                    double raw = (double)a.total * restante / grandTotal;
                    int baseVal = (int)Math.Floor(raw);
                    floors[a.areaId] = baseVal;
                    residuals[a.areaId] = raw - baseVal;
                    sumaBase += baseVal;
                }
                int rem = restante - sumaBase;
                if (rem > 0)
                {
                    foreach (var kvp in residuals.OrderByDescending(r => r.Value).ThenByDescending(r => floors[r.Key]).Take(rem))
                        floors[kvp.Key] = floors[kvp.Key] + 1;
                }
                foreach (var kvp in floors)
                    cuotas[kvp.Key] += kvp.Value;
            }

            return cuotas;
        }

        private static List<mdPlanMuestreoAnualItem> CalcularItemsConDistribucionGlobal(
            List<(Guid areaId, string nombreArea, int total, Guid centroCosto, string numCC, string nomCC)> areas)
        {
            int grandTotal = areas.Sum(a => a.total);
            if (grandTotal == 0) return new List<mdPlanMuestreoAnualItem>();

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
                var cuotas = CalcularCuotasPorArea(
                    areas.Select(a => (a.areaId, a.total)).ToList(),
                    monthTargets[m]);

                for (int i = 0; i < n; i++)
                    vals[i][m] = cuotas.GetValueOrDefault(areas[i].areaId, 0);
            }

            var items = new List<mdPlanMuestreoAnualItem>();
            for (int i = 0; i < n; i++)
            {
                items.Add(new mdPlanMuestreoAnualItem
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

        private static mdPlanMuestreoAnualItem CalcularTotales(List<mdPlanMuestreoAnualItem> items)
        {
            if (!items.Any())
                return null;

            return new mdPlanMuestreoAnualItem
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

        private static List<mdPlanMuestreoAnualItem> AgruparItemsPorArea(List<mdPlanMuestreoAnualItem> items)
        {
            return items
                .GroupBy(i => i.NombreArea ?? "S/A")
                .Select(g =>
                {
                    var rep = g.FirstOrDefault(i => i.NumeroCentroCosto != "S/A") ?? g.First();
                    return new mdPlanMuestreoAnualItem
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
