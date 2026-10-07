#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using ActivosFijos.Models.Auxiliar;
using ActivosFijos.Repositorios;
using ActivosFijos.Clases;

namespace ActivosFijos.Controllers
{
    public class ctMovimientosController : Controller
    {
        private readonly aftContext _context;
        private readonly repoTrasladosAFT _repoTrasladosAFT;
        private readonly repoTipoMovimiento _repoTipoMovimiento;
        private readonly repoActivosFijos _repoActivosFijos;

        public ctMovimientosController(aftContext context, repoTrasladosAFT repoTrasladosAFT,
            repoTipoMovimiento repoTipoMovimiento, repoActivosFijos repoActivosFijos)
        {
            _context = context;
            _repoTrasladosAFT = repoTrasladosAFT;
            _repoTipoMovimiento = repoTipoMovimiento;
            _repoActivosFijos = repoActivosFijos;
        }

        // GET: ctMovimientos
        public async Task<IActionResult> RegistrarMovimientos(string fecha, Guid? id)
        {
            DateTime periodo = await _repoActivosFijos.UltimoSubmayor().ConfigureAwait(false);
            mdMovimientos model = await CargarModelo(periodo).ConfigureAwait(false);
            ViewData["UltimoSubmayor"] = periodo;
            ViewData["respuesta"] = new clResponse<TrasladosAft>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            bool siEditar = (id != null) && (id != Guid.Empty);
            if (siEditar)
            {
                TrasladosAft movimiento = await _repoTrasladosAFT.SeleccionarPorId(id.Value).ConfigureAwait(false);
                if (movimiento == null)
                {
                    ViewData["respuesta"] = new clResponse<TrasladosAft>() { SiSatisfactorio = false, Mensaje = "El movimiento que se quiere editar no existe." };
                    siEditar = false;
                }
                else
                {
                    model.Formulario = movimiento;
                }
            }
            ViewData["breadsC"] = LlenarBreadsCumbs(siEditar ? "Editar Movimiento" : "Registrar Movimientos");
            return View("vRegistrarMovimientos", model);
        }

        // POST: ctMovimientos/GuardarMovimiento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarMovimiento(
            [Bind("Id,NoInventario,SubNumero,TipoMovimiento,Responsable,Motivo")] TrasladosAft movimiento,
            string desde, string hasta, string fecha)
        {
            DateTime periodo = await _repoActivosFijos.UltimoSubmayor().ConfigureAwait(false);
            bool siNuevo = (movimiento == null) || (movimiento.Id == Guid.Empty);

            if (movimiento != null)
            {
                DateTime fechaDesde;
                if (string.IsNullOrWhiteSpace(desde) || !DateTime.TryParseExact(desde, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaDesde))
                {
                    ModelState.AddModelError("Desde", "Debe indicar la fecha de inicio del movimiento.");
                }
                else
                {
                    movimiento.Desde = fechaDesde;

                    if (!string.IsNullOrWhiteSpace(hasta))
                    {
                        DateTime fechaHasta;
                        if (!DateTime.TryParseExact(hasta, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaHasta))
                        {
                            ModelState.AddModelError("Hasta", "La fecha final indicada no es v\u00e1lida.");
                        }
                        else if (fechaHasta < fechaDesde)
                        {
                            ModelState.AddModelError("Hasta", "La fecha final no puede ser anterior a la inicial.");
                        }
                        else
                        {
                            movimiento.Hasta = fechaHasta;
                        }
                    }
                }

                if (movimiento.NoInventario <= 0)
                {
                    ModelState.AddModelError("NoInventario", "Debe indicar el No. de Inventario.");
                }
                else if (!await _context.ActFijos.AnyAsync(a => (a.NoInventario == movimiento.NoInventario) && (a.SubNumero == movimiento.SubNumero) && a.FechaSubmayor == periodo).ConfigureAwait(false))
                {
                    ModelState.AddModelError("NoInventario", "El Activo Fijo indicado no existe.");
                }
                else if (await _repoTrasladosAFT.ExisteMismoDia(movimiento.NoInventario, movimiento.SubNumero, movimiento.Desde, siNuevo ? null : movimiento.Id).ConfigureAwait(false))
                {
                    ModelState.AddModelError("Desde", "Ya existe un movimiento declarado para ese Activo Fijo en esa fecha.");
                }

                if ((movimiento.TipoMovimiento == Guid.Empty) || !await _context.TipoMovimientoAFTs.AnyAsync(t => (t.IdTipoMovimientoAFT == movimiento.TipoMovimiento) && t.SiActivo).ConfigureAwait(false))
                {
                    ModelState.AddModelError("TipoMovimiento", "Debe seleccionar el tipo de movimiento.");
                }

                if (movimiento.Responsable == Guid.Empty)
                {
                    ModelState.AddModelError("Responsable", "Debe seleccionar el responsable de destino.");
                }
                else
                {
                    Responsable responsable = await _context.Responsables
                        .FirstOrDefaultAsync(r => r.IdResponsable == movimiento.Responsable).ConfigureAwait(false);

                    if (responsable == null)
                    {
                        ModelState.AddModelError("Responsable", "El responsable de destino no existe.");
                    }
                    else if (responsable.AreaResponsabilidad == null)
                    {
                        ModelState.AddModelError("Responsable", "El responsable de destino no tiene \u00e1rea asignada.");
                    }
                    else
                    {
                        movimiento.AreaDestino = responsable.AreaResponsabilidad.Value;
                    }
                }

                if (ModelState.IsValid)
                {
                    clResponse<TrasladosAft> respuesta = await _repoTrasladosAFT.Guardar(movimiento).ConfigureAwait(false);
                    clGlobal.siNo = respuesta.SiSatisfactorio;
                    clGlobal.objMensaje = respuesta.Mensaje;
                    return RedirectToAction(nameof(RegistrarMovimientos));
                }
            }

            mdMovimientos model = await CargarModelo(periodo).ConfigureAwait(false);
            if (movimiento != null)
            {
                model.Formulario = movimiento;
            }

            ViewData["UltimoSubmayor"] = periodo;
            ViewData["respuesta"] = new clResponse<TrasladosAft>() { SiSatisfactorio = false, Mensaje = "Persisten errores en el movimiento a guardar." };
            ViewData["breadsC"] = LlenarBreadsCumbs(siNuevo ? "Registrar Movimientos" : "Editar Movimiento");
            return View("vRegistrarMovimientos", model);
        }

        // POST: ctMovimientos/EliminarMovimiento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarMovimiento(Guid id, string fecha)
        {
            clResponse<int> respuesta = await _repoTrasladosAFT.Eliminar(id).ConfigureAwait(false);
            clGlobal.siNo = respuesta.SiSatisfactorio;
            clGlobal.objMensaje = respuesta.Mensaje;
            return RedirectToAction(nameof(RegistrarMovimientos));
        }

        private async Task<mdMovimientos> CargarModelo(DateTime periodo)
        {
            DateTime inicio = new DateTime(periodo.Year, periodo.Month, 1);

            mdMovimientos model = new mdMovimientos();
            model.Fecha = inicio;
            model.Movimientos = await _repoTrasladosAFT.ObtenerPorMes(inicio).ConfigureAwait(false);
            model.Tipos = await _repoTipoMovimiento.ObtenerListadoDeTiposMovimientoActivos().ConfigureAwait(false);
            model.Responsables = await _context.Responsables
                .Include(r => r.AreaResponsabilidadNavigation)
                .Where(r => r.AreaResponsabilidad != null)
                .OrderBy(r => r.Apellidos)
                .ThenBy(r => r.Nombre)
                .ToListAsync().ConfigureAwait(false);
            model.Formulario.Desde = inicio;
            return model;
        }

        private static DateTime? ParsearPeriodo(string fecha)
        {
            DateTime periodo;
            if (DateTime.TryParseExact(fecha, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out periodo))
            {
                return periodo;
            }
            return null;
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
