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

        private class SelectListResponsable
        {
            public Guid IdResponsable { get; set; }
            public string NombreCompleto { get; set; }
        }

        public ctUtilesYHerramientasController(aftContext context, repoUtilesHerramientas repoUTH, repoAreasDeResponsabilidad repoAreas, repoResponsables repoResponsables,
            repoCentrosDeCosto repoCentroDeCostos, repoValidaciones repoValidaciones, repoServcioActas repoServcioActas)
        {
            _context = context;
            _repoAreas = repoAreas;
            _repoResponsables = repoResponsables;
            _repoCentroDeCostos = repoCentroDeCostos;
            _repoValidaciones = repoValidaciones;
            _repoServcioActas = repoServcioActas;
            _repoUTH = repoUTH;
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
            List<AreaDeResponsabilidad> areas = siMovimientos ? await _repoAreas.ObtenerListadoAreasSiMovimientoUTH(await _repoUTH.ListadoUTHConMovimientos(fecha)) : await _repoAreas.ListadoAreasDeResponsabilidadUTH(fecha, siMovimientos);
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
            List<ExistenciaMovimiento> existMovs = new List<ExistenciaMovimiento>();
            //existMovs = await _repoUTH.DeterminarSiExisteMovimientoDeUTH(fecha);

            //if (siMovimientos)
            //actFijosList = actFijosList.Where(c => existMovs.Any(ex => ex.NoInventario.Equals(c.NoInventario))).ToList();

            //ViewData["ExistenciaDeMovimiento"] = existMovs;

            return PartialView("_vpTablaActivosFijos", uthList.OrderBy(act => act.AreaResponsabilidadNavigation.Nombre).ThenBy(act => act.ResponsableNavigation.Nombre + " " + act.ResponsableNavigation.Apellidos).ThenBy(act => act.Descripcion));
        }

        [HttpPost]
        public async Task<IActionResult> ListadoDeUTHsConMovimientos(DateTime fecha, string area, string responsable)
        {
            List<UTHConMovimientosParcial> uthList = await _repoUTH.ListadoUTHConMovimientos(fecha);
            List<UTHConMovimientos> uthMovlist = new List<UTHConMovimientos>();

            if (!Guid.Parse(area).Equals(Guid.Empty))
            {
                uthList = uthList.Where(c => c.Area.Equals(Guid.Parse(area))).ToList();
            }

            if (!Guid.Parse(responsable).Equals(Guid.Empty))
            {
                uthList = uthList.Where(c => c.Responsable.Equals(Guid.Parse(responsable))).ToList();
            }

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
                } else if (uthMovlist.Count() > 0)
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
                    } else
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

                    List<ExistenciaMovimientoUTH> existMovs = new List<ExistenciaMovimientoUTH>();
                    existMovs = await _repoUTH.DeterminarSiExisteMovimientoDeUTH(model.Fecha);

                    List<Responsable> responsables = new List<Responsable>();
                    List<AreaDeResponsabilidad> areasResponsabilidad = new List<AreaDeResponsabilidad>();

                    if (model.VisualizarMovimiento)
                    {
                        List<ExistenciaMovimientoUTH> existenciaMovimientos = new List<ExistenciaMovimientoUTH>();
                        existenciaMovimientos = _repoUTH.DeterminarSiExisteMovimientoDeUTH(model.Fecha).Result;
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
                                        var entry = zip.CreateEntry(tituloDoc);

                                        using (var fileStream = new MemoryStream(objFile))
                                        using (var entryStream = entry.Open())
                                        {
                                            fileStream.CopyTo(entryStream);
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
                    return RedirectToAction("Index");
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
                List<ActivosFijosConMovimientos> existenciaDeMovimientos = new List<ActivosFijosConMovimientos>();
                //existenciaDeMovimientos = await _repoUTH.ListadoActivosFijosConMovimientos(model.Fecha);
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

            return View("vDetallesDeMovimientosUTH", new mdActas() { Areas = areas, Responsables = responsables, Fecha = fecha == null ? DateTime.Today : new DateTime(fecha.Value.Year, fecha.Value.Month, fecha.Value.Day) });
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
    }
}
