#nullable disable
using ActivosFijos.Clases;
using ActivosFijos.Models.Auxiliar;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using ActivosFijos.Repositorios;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace ActivosFijos.Controllers
{
    public class ctBajasActivoController : Controller
    {
        private readonly aftContext _context;
        private readonly repoBajasActivo _repoBajas;
        private readonly IWebHostEnvironment _env;

        public ctBajasActivoController(aftContext context, repoBajasActivo repoBajas, IWebHostEnvironment env)
        {
            _context = context;
            _repoBajas = repoBajas;
            _env = env;
        }

        public IActionResult Index()
        {
            var fechas = ObtenerFechasComparacion();
            ViewBag.FechaOrigen = fechas.fechaOrigen;
            ViewBag.FechaDestino = fechas.fechaDestino;
            return View("vBajasActivo");
        }

        [HttpPost]
        public async Task<IActionResult> BuscarActivosBaja()
        {
            var fechas = ObtenerFechasComparacion();

            var activosBaja = await _repoBajas.ObtenerActivosBajaPeriodo(fechas.fechaOrigen, fechas.fechaDestino);

            var items = activosBaja.Select(a => new mdBajaActivoItem
            {
                NoInventario = a.NoInventario,
                SubNumero = a.SubNumero,
                Descripcion = a.Descripcion,
                Responsable = a.Responsable,
                Area = a.Area,
                ValorContable = a.ValorContable,
                ValorAdquisicion = a.ValorAdquisicion,
                FechaSubmayor = a.FechaSubmayor,
                MotivoBajaId = 0,
                Seleccionado = false
            }).ToList();

            foreach (var item in items)
            {
                var existente = await _repoBajas.ObtenerBajaExistente(item.NoInventario, item.SubNumero, item.FechaSubmayor);
                if (existente.MotivoBajaId.HasValue)
                {
                    item.MotivoBajaId = existente.MotivoBajaId.Value;
                    item.Observaciones = existente.Observaciones;
                    item.Seleccionado = true;
                }
            }

            ViewBag.MotivosBaja = await _repoBajas.ObtenerMotivosBaja();
            ViewBag.FechaOrigen = fechas.fechaOrigen;
            ViewBag.FechaDestino = fechas.fechaDestino;

            return PartialView("_vpActivosBaja", items);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarBajas(List<mdBajaActivoItem> bajas, int? anio)
        {
            DateTime fechaOrigen;
            if (anio.HasValue)
                fechaOrigen = new DateTime(anio.Value - 1, 12, DateTime.DaysInMonth(anio.Value - 1, 12));
            else
                fechaOrigen = ObtenerFechasComparacion().fechaOrigen;

            var bajasSeleccionadas = bajas.Where(b => b.Seleccionado && b.MotivoBajaId > 0).ToList();

            if (!bajasSeleccionadas.Any())
            {
                TempData["Mensaje"] = "Debe seleccionar al menos un activo con un motivo de baja.";
                return RedirectToAction(nameof(Index));
            }

            var registros = bajasSeleccionadas.Select(b => new BajaActivo
            {
                NoInventario = b.NoInventario,
                SubNumero = b.SubNumero,
                FechaSubmayor = fechaOrigen,
                MotivoBajaId = b.MotivoBajaId,
                Observaciones = b.Observaciones,
                UsuarioId = 1,
                FechaRegistro = DateTime.Now
            }).ToList();

            var resultado = await _repoBajas.RegistrarBajasMasivas(registros);

            clGlobal.siNo = resultado.SiSatisfactorio;
            clGlobal.objMensaje = resultado.Mensaje;

            if (anio.HasValue)
                return RedirectToAction(nameof(BajasPorAnio));
            else
                return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportarExcel()
        {
            var fechas = ObtenerFechasComparacion();

            var activosBaja = await _repoBajas.ObtenerActivosBajaPeriodo(fechas.fechaOrigen, fechas.fechaDestino);
            var motivos = await _repoBajas.ObtenerMotivosBaja();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Bajas");

            ws.Cells[1, 1].Value = "Reporte de Bajas - Activos Fijos";
            ws.Cells[1, 1, 1, 8].Merge = true;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;
            ws.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            ws.Cells[2, 1].Value = $"Período Origen: {fechas.fechaOrigen:MMMM yyyy} | Período Destino: {fechas.fechaDestino:MMMM yyyy}";
            ws.Cells[2, 1, 2, 8].Merge = true;
            ws.Cells[2, 1].Style.Font.Size = 11;

            string[] headers = { "No. Inventario", "Sub Nº", "Descripción", "Responsable", "Área", "Valor Contable", "Valor Adquisición", "Motivo Baja" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[4, i + 1].Value = headers[i];
                ws.Cells[4, i + 1].Style.Font.Bold = true;
            }

            var headerRange = ws.Cells[4, 1, 4, 8];
            headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(68, 77, 88));

            int row = 5;
            foreach (var activo in activosBaja)
            {
                var motivoRegistrado = await _repoBajas.ObtenerMotivoBajaExistente(activo.NoInventario, activo.SubNumero, activo.FechaSubmayor);
                string motivoNombre = "";
                if (motivoRegistrado.HasValue)
                {
                    var motivo = motivos.FirstOrDefault(m => m.IdMotivoBaja == motivoRegistrado.Value);
                    motivoNombre = motivo?.Nombre ?? "";
                }

                ws.Cells[row, 1].Value = activo.NoInventario;
                ws.Cells[row, 2].Value = activo.SubNumero;
                ws.Cells[row, 3].Value = activo.Descripcion;
                ws.Cells[row, 4].Value = activo.Responsable;
                ws.Cells[row, 5].Value = activo.Area;
                ws.Cells[row, 6].Value = activo.ValorContable;
                ws.Cells[row, 7].Value = activo.ValorAdquisicion;
                ws.Cells[row, 8].Value = motivoNombre;

                ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";

                row++;
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Bajas_{fechas.fechaOrigen:yyyyMM}_{fechas.fechaDestino:yyyyMM}.xlsx");
        }

        // ===================== Bajas por Año Seleccionado =====================

        private async Task<DateTime> ObtenerFechaDestino(int anio)
        {
            if (anio >= DateTime.Today.Year)
            {
                var ultimaFecha = await _repoBajas.ObtenerUltimaFechaSubmayorPorAnio(anio);
                if (ultimaFecha.HasValue)
                    return ultimaFecha.Value;
            }
            return new DateTime(anio, 12, DateTime.DaysInMonth(anio, 12));
        }

        public async Task<IActionResult> BajasPorAnio()
        {
            int anioActual = DateTime.Today.Year;
            ViewBag.AnioSeleccionado = anioActual;
            ViewBag.FechaOrigen = new DateTime(anioActual - 1, 12, DateTime.DaysInMonth(anioActual - 1, 12));
            ViewBag.FechaDestino = await ObtenerFechaDestino(anioActual);
            return View("vBajasPorAnio");
        }

        [HttpPost]
        public async Task<IActionResult> BuscarBajasPorAnio(int anio)
        {
            DateTime fechaOrigen = new DateTime(anio - 1, 12, DateTime.DaysInMonth(anio - 1, 12));
            DateTime fechaDestino = await ObtenerFechaDestino(anio);

            var activosBaja = await _repoBajas.ObtenerActivosBajaPeriodo(fechaOrigen, fechaDestino);

            var items = activosBaja.Select(a => new mdBajaActivoItem
            {
                NoInventario = a.NoInventario,
                SubNumero = a.SubNumero,
                Descripcion = a.Descripcion,
                Responsable = a.Responsable,
                Area = a.Area,
                ValorContable = a.ValorContable,
                ValorAdquisicion = a.ValorAdquisicion,
                FechaSubmayor = a.FechaSubmayor,
                MotivoBajaId = 0,
                Seleccionado = false
            }).ToList();

            foreach (var item in items)
            {
                var existente = await _repoBajas.ObtenerBajaExistente(item.NoInventario, item.SubNumero, item.FechaSubmayor);
                if (existente.MotivoBajaId.HasValue)
                {
                    item.MotivoBajaId = existente.MotivoBajaId.Value;
                    item.Observaciones = existente.Observaciones;
                    item.Seleccionado = true;
                }
            }

            ViewBag.MotivosBaja = await _repoBajas.ObtenerMotivosBaja();
            ViewBag.FechaOrigen = fechaOrigen;
            ViewBag.FechaDestino = fechaDestino;
            ViewBag.AnioSeleccionado = anio;

            return PartialView("_vpActivosBajaPorAnio", items);
        }

        public async Task<IActionResult> ExportarExcelPorAnio(int anio)
        {
            DateTime fechaOrigen = new DateTime(anio - 1, 12, DateTime.DaysInMonth(anio - 1, 12));
            DateTime fechaDestino = await ObtenerFechaDestino(anio);

            var activosBaja = await _repoBajas.ObtenerActivosBajaPeriodo(fechaOrigen, fechaDestino);

            var filtrados = new List<Models.AuxBD.ActivosBaja>();
            foreach (var activo in activosBaja)
            {
                var motivoRegistrado = await _repoBajas.ObtenerMotivoBajaExistente(activo.NoInventario, activo.SubNumero, activo.FechaSubmayor);
                if (motivoRegistrado.HasValue && motivoRegistrado.Value == 1)
                {
                    filtrados.Add(activo);
                }
            }

            if (!filtrados.Any())
            {
                TempData["Mensaje"] = "No hay activos con motivo 'Baja Pendiente Materias Primas' para exportar.";
                return RedirectToAction(nameof(BajasPorAnio));
            }

            string templatePath = System.IO.Path.Combine(_env.WebRootPath, "plantillas", "PlantillaActaMuestreo.xlsx");

            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var templateSheet = package.Workbook.Worksheets[0];
                string templateSheetName = templateSheet.Name;

                ExcelWorksheet ws = package.Workbook.Worksheets.Copy(templateSheetName, "Bajas MP");

                ws.Cells["F2"].Value = "MINEM";
                ws.Cells["F3"].Value = "Empresa de Tecnologías de la Información y Automática";
                ws.Cells["F5"].Value = "Baja Pendiente Materias Primas";
                ws.Cells["AA4"].Value = new DateTime(anio, 12, 1);
                ws.Cells["AA5"].Value = 1;
                ws.Cells["AB6"].Value = 1;
                ws.Column(4).Width = 20;
                ws.Column(8).Hidden = true;
                ws.Column(9).Hidden = true;
                ws.Column(10).Hidden = true;
                ws.Column(11).Hidden = true;
                ws.Column(12).Hidden = true;
                ws.Column(13).Hidden = true;

                if (filtrados.Count > 1)
                    ws.InsertRow(10, filtrados.Count - 1, 9);

                for (int i = ws.MergedCells.Count - 1; i >= 0; i--)
                {
                    var addr = ws.MergedCells[i];
                    if (addr != null && new ExcelAddress(addr).Start.Row >= 9)
                        ws.Cells[addr].Merge = false;
                }

                for (int i = 0; i < filtrados.Count; i++)
                {
                    int row = 9 + i;
                    var activo = filtrados[i];

                    string noInv = activo.SubNumero > 0
                        ? $"{activo.NoInventario}-{activo.SubNumero}"
                        : activo.NoInventario.ToString();

                    ws.Cells[row, 2, row, 3].Merge = true;
                    ws.Cells[row, 2].Value = noInv;

                    ws.Cells[row, 4, row, 7].Merge = true;
                    ws.Cells[row, 4].Value = activo.Descripcion;

                    ws.Cells[row, 14].Value = 1;
                    ws.Cells[row, 16, row, 17].Merge = true;
                    ws.Cells[row, 20, row, 21].Merge = true;
                    ws.Cells[row, 25, row, 30].Merge = true;

                    ws.Row(row).CustomHeight = false;
                }

                int lastDataRow = 9 + filtrados.Count - 1;
                var thin = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                var cantRange = ws.Cells[9, 14, lastDataRow, 14];
                cantRange.Style.Border.Left.Style = thin;
                cantRange.Style.Border.Right.Style = thin;
                cantRange.Style.Border.Top.Style = thin;
                cantRange.Style.Border.Bottom.Style = thin;

                var siRange = ws.Cells[9, 16, lastDataRow, 17];
                siRange.Style.Border.Left.Style = thin;
                siRange.Style.Border.Right.Style = thin;
                siRange.Style.Border.Top.Style = thin;
                siRange.Style.Border.Bottom.Style = thin;

                var noRange = ws.Cells[9, 20, lastDataRow, 21];
                noRange.Style.Border.Left.Style = thin;
                noRange.Style.Border.Right.Style = thin;
                noRange.Style.Border.Top.Style = thin;
                noRange.Style.Border.Bottom.Style = thin;

                ws.Cells[9, 1, lastDataRow, 31].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                int subTotalRow = 9 + filtrados.Count;
                int totalRow = 10 + filtrados.Count;

                ws.Cells[subTotalRow, 1, subTotalRow, 31].Clear();
                ws.Cells[totalRow, 1, totalRow, 31].Clear();

                ws.Cells[totalRow, 9, totalRow, 27].Merge = true;
                ws.Cells[totalRow, 9].Value = "Total: " + filtrados.Count;
                ws.Cells[totalRow, 9].Style.Font.Bold = true;

                ws.Cells[9, 1, totalRow, 31].Style.Font.Size = 8;
                ws.Cells[9, 4, totalRow, 7].Style.WrapText = true;

                int footerStart = 11 + filtrados.Count;

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
                double contentH = filtrados.Count * 10 + 81;
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

                package.Workbook.Worksheets.Delete(templateSheetName);

                var ms = new MemoryStream();
                package.SaveAs(ms);
                ms.Position = 0;

                string fileName = $"BajasMP_{anio}.xlsx";
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        private (DateTime fechaOrigen, DateTime fechaDestino) ObtenerFechasComparacion()
        {
            int anioActual = DateTime.Today.Year;
            int mesActual = DateTime.Today.Month;

            // Origen: último mes del año anterior (diciembre)
            DateTime fechaOrigen = new DateTime(anioActual - 1, 12, DateTime.DaysInMonth(anioActual - 1, 12));

            // Destino: mes actual - 1
            int mesDestino = mesActual - 1;
            int anioDestino = anioActual;
            if (mesDestino < 1)
            {
                mesDestino = 12;
                anioDestino = anioActual - 1;
            }
            DateTime fechaDestino = new DateTime(anioDestino, mesDestino, DateTime.DaysInMonth(anioDestino, mesDestino));

            return (fechaOrigen, fechaDestino);
        }

        private List<clBreadCumb> LlenarBreadsCumbs(string titulo)
        {
            List<clBreadCumb> breads = new List<clBreadCumb>();
            clBreadCumb objBreadsCumbs = new clBreadCumb()
            {
                nombre = "Activos Fijos",
                ruta = new List<string>() { "ActivosFijos", "ctActFijos" }
            };
            breads.Add(objBreadsCumbs);
            objBreadsCumbs = new clBreadCumb()
            {
                nombre = titulo,
                ruta = new List<string>()
            };
            breads.Add(objBreadsCumbs);
            return breads;
        }
    }
}
