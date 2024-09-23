using ActivosFijos.Clases;
using ActivosFijos.Models.Auxiliar;
using ActivosFijos.Models.EntidadesBD;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Data;

namespace ActivosFijos.Repositorios
{
    public class repoServcioActas
    {
        private readonly repoActivosFijos _repoActivosFijos;
        private readonly repoResponsables _repoResponsables;
        private readonly repoUtilesHerramientas _repoUtilesHerramientas;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public repoServcioActas(repoActivosFijos repoActivosFijos, repoResponsables repoResponsables, repoUtilesHerramientas repoUtilesHerramientas, IWebHostEnvironment webHostEnvironment)
        {
            _repoActivosFijos = repoActivosFijos;
            _repoResponsables = repoResponsables;
            _repoUtilesHerramientas = repoUtilesHerramientas;
            _webHostEnvironment = webHostEnvironment;
        }
        public byte[] ExportarADocx(out bool result, out string msg, Guid responsable, Guid area, DateTime fecha, string responsableArea)
        {
            result = true;
            msg = "";
            MemoryStream ms = new MemoryStream();

            try
            {
                string webRootPath = _webHostEnvironment.WebRootPath;

                string path = "";
                path = Path.Combine(webRootPath, "plantillas\\Plantilla Acta Resp. Material.docx");

                byte[] templateBytes = System.IO.File.ReadAllBytes(path);
                new MemoryStream(templateBytes).CopyTo(ms);
                using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                {
                    Responsable resp = new Responsable();

                    if (responsable == Guid.Empty)
                    {
                        resp = _repoResponsables.ObtenerInfoDirector().Result;
                    }
                    else
                    {
                        resp = _repoResponsables.SeleccionarResponsablesPorID(responsable).Result;
                    }

                    string diasDelMes = DateTime.DaysInMonth(fecha.Year, fecha.Month).ToString();

                    SetBookmarkValue(doc, "RESPONSABLE", resp == null ? "<<S/R>>" : resp.Nombre + " " + resp.Apellidos);
                    SetBookmarkValue(doc, "CARGO", resp == null ? "<<S/C>>" : resp.CargoNavigation?.Nombre);
                    SetBookmarkValue(doc, "DIAS", diasDelMes ?? "<<S/D>");
                    SetBookmarkValue(doc, "MES", GetNameMonth(fecha.Month, fecha.Year).Split('/')[0] ?? "<<S/D>");
                    SetBookmarkValue(doc, "ANNO", fecha.Year.ToString() ?? "<<S/D>");

                    Responsable respCont = new Responsable();
                    respCont = _repoResponsables.ObtenerInfoEncargadoEconomia().Result;

                    SetBookmarkValue(doc, "FIRMADECONTABILIDAD", respCont == null ? "<<S/D>>" : respCont.Nombre + " " + respCont.Apellidos);
                    SetBookmarkValue(doc, "FIRMADERESPONSABLEDEAREA", resp == null ? "<<S/R>>" : resp.Nombre + " " + resp.Apellidos);

                    List<ActFijo> actFijos = new List<ActFijo>();
                    actFijos = _repoActivosFijos.ActivosFijos(fecha, area, responsable);

                    SetBookmarkValueWithTable(doc, "TABLA", actFijos, fecha);

                    var mainPart = doc.MainDocumentPart;

                    //mainPart.Document.Append(table_);

                    foreach (var p in mainPart.Document.Descendants<Paragraph>())
                    {
                        ApplyStyleToParagraph(doc, "BasicParagraphStyle", "Basic Paragraph Style", p);
                        p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "300", LineRule = LineSpacingRuleValues.Auto };
                    }

                    foreach (var t in mainPart.Document.Descendants<Table>())
                    {
                        foreach (var cell_p in t.Descendants<Paragraph>())
                        {
                            cell_p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "0", LineRule = LineSpacingRuleValues.Auto };
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                result = false;
                msg = ex.Message;
            }

            if (result)
            {
                return ms.ToArray();
            }
            else
            {
                return null;
            }
        }

        public byte[] ExportarUTHADocx(out bool result, out string msg, Guid responsable, Guid area, DateTime fecha, string responsableArea)
        {
            result = true;
            msg = "";
            MemoryStream ms = new MemoryStream();

            try
            {
                string webRootPath = _webHostEnvironment.WebRootPath;

                string path = "";
                path = Path.Combine(webRootPath, "plantillas\\Plantilla Acta Resp. Material UTH.docx");

                byte[] templateBytes = System.IO.File.ReadAllBytes(path);
                new MemoryStream(templateBytes).CopyTo(ms);
                using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                {
                    Responsable resp = new Responsable();

                    if (responsable == Guid.Empty)
                    {
                        resp = _repoResponsables.ObtenerInfoDirector().Result;
                    }
                    else
                    {
                        resp = _repoResponsables.SeleccionarResponsablesPorID(responsable).Result;
                    }

                    string diasDelMes = DateTime.DaysInMonth(fecha.Year, fecha.Month).ToString();

                    SetBookmarkValue(doc, "RESPONSABLE", resp == null ? "<<S/R>>" : resp.Nombre + " " + resp.Apellidos);
                    SetBookmarkValue(doc, "CARGO", resp == null ? "<<S/C>>" : resp.CargoNavigation?.Nombre);
                    SetBookmarkValue(doc, "DIAS", diasDelMes ?? "<<S/D>");
                    SetBookmarkValue(doc, "MES", GetNameMonth(fecha.Month, fecha.Year).Split('/')[0] ?? "<<S/D>");
                    SetBookmarkValue(doc, "ANNO", fecha.Year.ToString() ?? "<<S/D>");

                    Responsable respCont = new Responsable();
                    respCont = _repoResponsables.ObtenerInfoEncargadoEconomia().Result;

                    SetBookmarkValue(doc, "FIRMACONTABILIDAD", respCont == null ? "<<S/D>>" : respCont.Nombre + " " + respCont.Apellidos);
                    SetBookmarkValue(doc, "FIRMARESPONSABLEAREA", resp == null ? "<<S/R>>" : resp.Nombre + " " + resp.Apellidos);

                    List<UtilesHerramienta> utilesHerr = new List<UtilesHerramienta>();
                    utilesHerr = _repoUtilesHerramientas.UTHs(fecha, area, responsable);

                    SetBookmarkValueWithTable(doc, "TABLA", utilesHerr, fecha);

                    var mainPart = doc.MainDocumentPart;

                    //mainPart.Document.Append(table_);

                    foreach (var p in mainPart.Document.Descendants<Paragraph>())
                    {
                        ApplyStyleToParagraph(doc, "BasicParagraphStyle", "Basic Paragraph Style", p);
                        p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "300", LineRule = LineSpacingRuleValues.Auto };
                    }

                    foreach (var t in mainPart.Document.Descendants<Table>())
                    {
                        foreach (var cell_p in t.Descendants<Paragraph>())
                        {
                            cell_p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "0", LineRule = LineSpacingRuleValues.Auto };
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                result = false;
                msg = ex.Message;
            }

            if (result)
            {
                return ms.ToArray();
            }
            else
            {
                return null;
            }
        }

        public byte[] ExportarPorAreasDocx(out bool result, out string msg, Guid responsable, Guid area, DateTime fecha, string responsableArea)
        {
            result = true;
            msg = "";
            MemoryStream ms = new MemoryStream();

            try
            {
                List<ActFijo> actFijos = new List<ActFijo>();
                actFijos = _repoActivosFijos.ActivosFijos(fecha, area, responsable);

                if (actFijos.Count == 0)
                {
                    result = false;
                }
                else
                {
                    Responsable resp = new Responsable();
                    string respArea = "<<S/D>>";
                    if (responsable == null)
                    {
                        resp = null;
                    }
                    else if (responsable.Equals(Guid.Empty))
                    {
                        resp = _repoResponsables.SeleccionarResponsableEPDelArea(area).Result;
                        if (resp == null && actFijos.Count() > 0)
                            respArea = actFijos.Select(c => c.AreaResponsabilidadNavigation).Distinct().FirstOrDefault().Nombre;
                    }
                    else
                    {
                        resp = _repoResponsables.SeleccionarResponsablesPorID(responsable).Result;
                    }

                    CentroCosto centroCosto = new CentroCosto();
                    centroCosto = actFijos.Select(c => c.CentroCostoNavigation).Distinct().FirstOrDefault();

                    string webRootPath = _webHostEnvironment.WebRootPath;

                    string path = "";
                    path = Path.Combine(webRootPath, "plantillas\\Plantilla Acta Resp. Material Areas.docx");

                    byte[] templateBytes = System.IO.File.ReadAllBytes(path);
                    new MemoryStream(templateBytes).CopyTo(ms);
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                    {
                        string diasDelMes = DateTime.DaysInMonth(fecha.Year, fecha.Month).ToString();

                        SetBookmarkValue(doc, "RESPONSABLE", resp != null ? resp.Nombre + " " + resp.Apellidos : "<<S/D>>");
                        SetBookmarkValue(doc, "CARGO", (resp == null || resp.CargoNavigation == null) ? "<<S/D>>" : resp.CargoNavigation.Nombre);
                        SetBookmarkValue(doc, "AREA", resp != null ? resp.AreaResponsabilidadNavigation.Nombre : respArea);
                        SetBookmarkValue(doc, "CENTROCOSTO", centroCosto == null ? "<<S/D>>" : centroCosto.Numero.ToString());
                        SetBookmarkValue(doc, "DIAS", diasDelMes ?? "<<S/D>");
                        SetBookmarkValue(doc, "MES", GetNameMonth(fecha.Month, fecha.Year).Split('/')[0] ?? "<<S/D>");
                        SetBookmarkValue(doc, "ANNO", fecha.Year.ToString() ?? "<<S/D>");

                        resp = new Responsable();
                        resp = _repoResponsables.ObtenerInfoEncargadoEconomia().Result;

                        SetBookmarkValue(doc, "FIRMADECONTABILIDAD", resp == null ? "<<S/D>>" : resp.Nombre + " " + resp.Apellidos);

                        SetBookmarkValue(doc, "FIRMADERESPONSABLEDEAREA", responsableArea ?? "");

                        SetBookmarkValueWithTable(doc, "TABLA", actFijos, fecha, false);

                        var mainPart = doc.MainDocumentPart;

                        //mainPart.Document.Append(table_);

                        foreach (var p in mainPart.Document.Descendants<Paragraph>())
                        {
                            ApplyStyleToParagraph(doc, "BasicParagraphStyle", "Basic Paragraph Style", p);
                            p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "300", LineRule = LineSpacingRuleValues.Auto };
                        }

                        foreach (var t in mainPart.Document.Descendants<Table>())
                        {
                            foreach (var cell_p in t.Descendants<Paragraph>())
                            {
                                cell_p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "0", LineRule = LineSpacingRuleValues.Auto };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                msg = ex.Message;
            }

            if (result)
            {
                return ms.ToArray();
            }
            else
            {
                return null;
            }
        }

        public byte[] ExportarPorAreasUTHDocx(out bool result, out string msg, Guid responsable, Guid area, DateTime fecha, string responsableArea)
        {
            result = true;
            msg = "";
            MemoryStream ms = new MemoryStream();

            try
            {
                List<UtilesHerramienta> utilesHerr = new List<UtilesHerramienta>();
                utilesHerr = _repoUtilesHerramientas.UTHs(fecha, area, responsable);

                if (utilesHerr.Count == 0)
                {
                    result = false;
                }
                else
                {
                    Responsable resp = new Responsable();
                    string respArea = "<<S/D>>";
                    if (responsable == null)
                    {
                        resp = null;
                    }
                    else if (responsable.Equals(Guid.Empty))
                    {
                        resp = _repoResponsables.SeleccionarResponsableEPDelArea(area).Result;
                        if (resp == null && utilesHerr.Count() > 0)
                            respArea = utilesHerr.Select(c => c.AreaResponsabilidadNavigation).Distinct().FirstOrDefault().Nombre;
                    }
                    else
                    {
                        resp = _repoResponsables.SeleccionarResponsablesPorID(responsable).Result;
                    }

                    CentroCosto centroCosto = new CentroCosto();
                    centroCosto = utilesHerr.Select(c => c.CentroCostoNavigation).Distinct().FirstOrDefault();

                    string webRootPath = _webHostEnvironment.WebRootPath;

                    string path = "";
                    path = Path.Combine(webRootPath, "plantillas\\Plantilla Acta Resp. Material Areas UTH.docx");

                    byte[] templateBytes = System.IO.File.ReadAllBytes(path);
                    new MemoryStream(templateBytes).CopyTo(ms);
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                    {
                        string diasDelMes = DateTime.DaysInMonth(fecha.Year, fecha.Month).ToString();

                        SetBookmarkValue(doc, "RESPONSABLE", resp != null ? resp.Nombre + " " + resp.Apellidos : "<<S/D>>");
                        SetBookmarkValue(doc, "CARGO", (resp == null || resp.CargoNavigation == null) ? "<<S/D>>" : resp.CargoNavigation.Nombre);
                        SetBookmarkValue(doc, "AREA", resp != null ? resp.AreaResponsabilidadNavigation.Nombre : respArea);
						SetBookmarkValue(doc, "RESPONSABLEAREA", responsableArea != null ? responsableArea : "<<S/D>>");
						SetBookmarkValue(doc, "CENTROCOSTO", centroCosto == null ? "<<S/D>>" : centroCosto.Numero.ToString());
                        SetBookmarkValue(doc, "DIAS", diasDelMes ?? "<<S/D>");
                        SetBookmarkValue(doc, "MES", GetNameMonth(fecha.Month, fecha.Year).Split('/')[0] ?? "<<S/D>");
                        SetBookmarkValue(doc, "ANNO", fecha.Year.ToString() ?? "<<S/D>");

                        resp = new Responsable();
                        resp = _repoResponsables.ObtenerInfoEncargadoEconomia().Result;

                        SetBookmarkValue(doc, "FIRMACONTABILIDAD", resp == null ? "<<S/D>>" : resp.Nombre + " " + resp.Apellidos);

                        SetBookmarkValue(doc, "FIRMARESPONSABLEAREA", responsableArea ?? "");

                        SetBookmarkValueWithTable(doc, "TABLA", utilesHerr, fecha, false);

                        var mainPart = doc.MainDocumentPart;

                        //mainPart.Document.Append(table_);

                        foreach (var p in mainPart.Document.Descendants<Paragraph>())
                        {
                            ApplyStyleToParagraph(doc, "BasicParagraphStyle", "Basic Paragraph Style", p);
                            p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "300", LineRule = LineSpacingRuleValues.Auto };
                        }

                        foreach (var t in mainPart.Document.Descendants<Table>())
                        {
                            foreach (var cell_p in t.Descendants<Paragraph>())
                            {
                                cell_p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "0", LineRule = LineSpacingRuleValues.Auto };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                msg = ex.Message;
            }

            if (result)
            {
                return ms.ToArray();
            }
            else
            {
                return null;
            }
        }

        public byte[] ExportarPorAreasIndividualDocx(out bool result, out string msg, Guid responsable, Guid area, DateTime fecha, string responsableArea)
        {
            result = true;
            msg = "";
            MemoryStream ms = new MemoryStream();

            try
            {
                List<ActFijo> actFijos = new List<ActFijo>();
                actFijos = _repoActivosFijos.ActivosFijos(fecha, area, responsable);

                if (actFijos.Count == 0)
                {
                    result = false;
                }
                else
                {
                    Responsable resp = new Responsable();
                    resp = _repoResponsables.SeleccionarResponsablesPorID(responsable).Result;

                    CentroCosto centroCosto = new CentroCosto();
                    centroCosto = actFijos.Select(c => c.CentroCostoNavigation).Distinct().FirstOrDefault();

                    string webRootPath = _webHostEnvironment.WebRootPath;

                    string path = "";
                    path = Path.Combine(webRootPath, "plantillas\\Plantilla Acta Resp. Material Individual.docx");

                    byte[] templateBytes = System.IO.File.ReadAllBytes(path);
                    new MemoryStream(templateBytes).CopyTo(ms);
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                    {
                        string diasDelMes = DateTime.DaysInMonth(fecha.Year, fecha.Month).ToString();

                        SetBookmarkValue(doc, "RESPONSABLE", resp != null ? resp.Nombre + " " + resp.Apellidos : "<<S/D>>");
                        SetBookmarkValue(doc, "CARGO", (resp == null || resp.CargoNavigation == null) ? "<<S/D>>" : resp.CargoNavigation.Nombre);
                        SetBookmarkValue(doc, "AREA", resp != null ? resp.AreaResponsabilidadNavigation.Nombre : "<<S/D>>");
                        SetBookmarkValue(doc, "CENTROCOSTO", centroCosto == null ? "<<S/D>>" : centroCosto.Numero.ToString());
                        SetBookmarkValue(doc, "DIAS", diasDelMes ?? "<<S/D>");
                        SetBookmarkValue(doc, "MES", GetNameMonth(fecha.Month, fecha.Year).Split('/')[0] ?? "<<S/D>");
                        SetBookmarkValue(doc, "ANNO", fecha.Year.ToString() ?? "<<S/D>");
                        SetBookmarkValue(doc, "FIRMATRABAJADOR", resp == null ? "<<S/D>>" : resp.Nombre + " " + resp.Apellidos);
                        SetBookmarkValue(doc, "FIRMADERESPONSABLEDEAREA", responsableArea ?? "");
                        SetBookmarkValueWithTable(doc, "TABLA", actFijos, fecha, false);

                        var mainPart = doc.MainDocumentPart;

                        foreach (var p in mainPart.Document.Descendants<Paragraph>())
                        {
                            ApplyStyleToParagraph(doc, "BasicParagraphStyle", "Basic Paragraph Style", p);
                            p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "300", LineRule = LineSpacingRuleValues.Auto };
                        }

                        foreach (var t in mainPart.Document.Descendants<Table>())
                        {
                            foreach (var cell_p in t.Descendants<Paragraph>())
                            {
                                cell_p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "0", LineRule = LineSpacingRuleValues.Auto };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                msg = ex.Message;
            }

            if (result)
            {
                return ms.ToArray();
            }
            else
            {
                return null;
            }
        }

        public byte[] ExportarUTHPorAreasIndividualDocx(out bool result, out string msg, Guid responsable, Guid area, DateTime fecha, string responsableArea)
        {
            result = true;
            msg = "";
            MemoryStream ms = new MemoryStream();

            try
            {
                List<UtilesHerramienta> utilesHerr = new List<UtilesHerramienta>();
                utilesHerr = _repoUtilesHerramientas.UTHs(fecha, area, responsable);

                if (utilesHerr.Count == 0)
                {
                    result = false;
                }
                else
                {
                    Responsable resp = new Responsable();
                    resp = _repoResponsables.SeleccionarResponsablesPorID(responsable).Result;

                    CentroCosto centroCosto = new CentroCosto();
                    centroCosto = utilesHerr.Select(c => c.CentroCostoNavigation).Distinct().FirstOrDefault();

                    string webRootPath = _webHostEnvironment.WebRootPath;

                    string path = "";
                    path = Path.Combine(webRootPath, "plantillas\\Plantilla Acta Resp. Material Individual UTH.docx");

                    byte[] templateBytes = System.IO.File.ReadAllBytes(path);
                    new MemoryStream(templateBytes).CopyTo(ms);
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                    {
                        string diasDelMes = DateTime.DaysInMonth(fecha.Year, fecha.Month).ToString();

                        SetBookmarkValue(doc, "RESPONSABLE", resp != null ? resp.Nombre + " " + resp.Apellidos : "<<S/D>>");
                        SetBookmarkValue(doc, "CARGO", (resp == null || resp.CargoNavigation == null) ? "<<S/D>>" : resp.CargoNavigation.Nombre);
                        SetBookmarkValue(doc, "AREA", resp != null ? resp.AreaResponsabilidadNavigation.Nombre : "<<S/D>>");
                        SetBookmarkValue(doc, "CENTROCOSTO", centroCosto == null ? "<<S/D>>" : centroCosto.Numero.ToString());
						SetBookmarkValue(doc, "RESPONSABLEAREA", responsableArea ?? "");
						SetBookmarkValue(doc, "DIAS", diasDelMes ?? "<<S/D>");
                        SetBookmarkValue(doc, "MES", GetNameMonth(fecha.Month, fecha.Year).Split('/')[0] ?? "<<S/D>");
                        SetBookmarkValue(doc, "ANNO", fecha.Year.ToString() ?? "<<S/D>");
                        SetBookmarkValue(doc, "FIRMATRABAJADOR", resp == null ? "<<S/D>>" : resp.Nombre + " " + resp.Apellidos);
                        SetBookmarkValue(doc, "FIRMARESPONSABLEAREA", responsableArea ?? "");
                        SetBookmarkValueWithTable(doc, "TABLA", utilesHerr, fecha, false);

                        var mainPart = doc.MainDocumentPart;

                        foreach (var p in mainPart.Document.Descendants<Paragraph>())
                        {
                            ApplyStyleToParagraph(doc, "BasicParagraphStyle", "Basic Paragraph Style", p);
                            p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "300", LineRule = LineSpacingRuleValues.Auto };
                        }

                        foreach (var t in mainPart.Document.Descendants<Table>())
                        {
                            foreach (var cell_p in t.Descendants<Paragraph>())
                            {
                                cell_p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "0", LineRule = LineSpacingRuleValues.Auto };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                msg = ex.Message;
            }

            if (result)
            {
                return ms.ToArray();
            }
            else
            {
                return null;
            }
        }

        public MemoryStream ExportarPorAreasDocxMStream(out bool result, out string msg, Guid responsable, Guid area, DateTime fecha, string responsableArea)
        {
            result = true;
            msg = "";
            MemoryStream ms = new MemoryStream();

            try
            {
                List<ActFijo> actFijos = new List<ActFijo>();
                actFijos = _repoActivosFijos.ActivosFijos(fecha, area, responsable);

                Responsable resp = new Responsable();
                resp = _repoResponsables.SeleccionarResponsableEPDelArea(area).Result;

                List<Responsable> responsables = new List<Responsable>();
                responsables = _repoResponsables.ObtenerListadoResponsables(area).Result;

                CentroCosto centroCosto = new CentroCosto();
                centroCosto = actFijos.Select(c => c.CentroCostoNavigation).Distinct().FirstOrDefault();

                string webRootPath = _webHostEnvironment.WebRootPath;

                string path = "";
                path = Path.Combine(webRootPath, "plantillas\\Plantilla Acta Resp. Material Areas.docx");

                byte[] templateBytes = System.IO.File.ReadAllBytes(path);
                new MemoryStream(templateBytes).CopyTo(ms);
                using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                {
                    string diasDelMes = DateTime.DaysInMonth(fecha.Year, fecha.Month).ToString();

                    SetBookmarkValue(doc, "RESPONSABLE", responsableArea ?? "<<S/D>>");
                    //SetBookmarkValue(doc, "CARGO", resp == null ? "<<S/D>>" : resp.CargoNavigation.Nombre);
                    //SetBookmarkValue(doc, "CENTROCOSTO", centroCosto == null ? "<<S/D>>" : centroCosto.Numero.ToString());
                    //SetBookmarkValue(doc, "DIAS", diasDelMes ?? "<<S/D>");
                    //SetBookmarkValue(doc, "MES", GetNameMonth(fecha.Month, fecha.Year).Split('/')[0] ?? "<<S/D>");
                    //SetBookmarkValue(doc, "ANNO", fecha.Year.ToString() ?? "<<S/D>");
                    //SetBookmarkValue(doc, "RESPONSABLEDEECONOMIA", responsableArea ?? "");
                    //SetBookmarkValue(doc, "RESPONSABLEDELAREA", responsableArea ?? "");

                    //SetBookmarkValueWithTable(doc, "TABLA", actFijos, fecha, false);

                    var mainPart = doc.MainDocumentPart;

                    //mainPart.Document.Append(table_);

                    foreach (var p in mainPart.Document.Descendants<Paragraph>())
                    {
                        ApplyStyleToParagraph(doc, "BasicParagraphStyle", "Basic Paragraph Style", p);
                        p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "300", LineRule = LineSpacingRuleValues.Auto };
                    }

                    foreach (var t in mainPart.Document.Descendants<Table>())
                    {
                        foreach (var cell_p in t.Descendants<Paragraph>())
                        {
                            cell_p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { After = "0", LineRule = LineSpacingRuleValues.Auto };
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                result = false;
                msg = ex.Message;
            }

            if (result)
            {
                return ms;
            }
            else
            {
                return null;
            }
        }

        private static Table AddTitleRow(Table wordTable, DateTime fecha)
        {

            // Create a row.
            TableRow tRow = new TableRow();

            // Create a cell.
            TableCell tCell = new TableCell();

            TableCellProperties tCellProperties = new TableCellProperties();
            TableCellVerticalAlignment tcVA = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
            tCellProperties.Append(tcVA);

            System.Drawing.Color c = new System.Drawing.Color();
            c = System.Drawing.Color.FromArgb(255, 255, 255);

            // Set Cell Color
            Shading tCellColor = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = System.Drawing.ColorTranslator.ToHtml(c) };
            tCellProperties.Append(tCellColor);

            // Set Cell Span
            GridSpan tCellSpan = new GridSpan() { Val = 3 }; //table.Columns.Count };
            tCellProperties.Append(tCellSpan);

            // Append properties to the cell
            tCell.Append(tCellProperties);

            ParagraphProperties paragProperties = new ParagraphProperties();
            Justification justification1 = new Justification() { Val = JustificationValues.Center };
            SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };
            paragProperties.Append(justification1);
            paragProperties.Append(spacingBetweenLines1);

            var titleParagraph = new Paragraph();
            titleParagraph.Append(paragProperties);

            var run = new Run(new Text("Submayor de Activos Fijos cierre de " + GetNameMonth(fecha.Month, fecha.Year))); //tableStyle.Title));
            run = ApplyFontProperties(run, true);

            titleParagraph.Append(run);

            // Specify the table cell content.
            tCell.Append(titleParagraph);

            // Append the table cell to the table row.
            tRow.Append(tCell);

            // Append the table row to the table.
            wordTable.Append(tRow);

            return wordTable;
        }

        private static Table AddSubTitleRow(Table wordTable, string subTitulo)
        {

            // Create a row.
            TableRow tRow = new TableRow();

            // Create a cell.
            TableCell tCell = new TableCell();

            TableCellProperties tCellProperties = new TableCellProperties();
            TableCellVerticalAlignment tcVA = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
            tCellProperties.Append(tcVA);

            System.Drawing.Color c = new System.Drawing.Color();
            c = System.Drawing.Color.FromArgb(255, 255, 255);

            // Set Cell Color
            Shading tCellColor = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = System.Drawing.ColorTranslator.ToHtml(c) };
            tCellProperties.Append(tCellColor);

            // Set Cell Span
            GridSpan tCellSpan = new GridSpan() { Val = 3 }; //table.Columns.Count };
            tCellProperties.Append(tCellSpan);

            // Append properties to the cell
            tCell.Append(tCellProperties);

            ParagraphProperties paragProperties = new ParagraphProperties();
            Justification justification1 = new Justification() { Val = JustificationValues.Center };
            SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };
            paragProperties.Append(justification1);
            paragProperties.Append(spacingBetweenLines1);

            var titleParagraph = new Paragraph();
            titleParagraph.Append(paragProperties);

            var run = new Run(new Text(subTitulo)); //tableStyle.Title));

            run = ApplyFontProperties(run, true);

            titleParagraph.Append(run);

            // Specify the table cell content.
            tCell.Append(titleParagraph);

            // Append the table cell to the table row.
            tRow.Append(tCell);

            // Append the table row to the table.
            wordTable.Append(tRow);

            return wordTable;
        }

        private static Table AddHeaderRow(Table wordTable)
        {
            // Create a row.
            TableRow tRow = new TableRow();

            for (int i = 0; i < 3; i++)
            {
                // Create a cell.
                TableCell tCell = new TableCell();

                TableCellProperties tCellProperties = new TableCellProperties();
                TableCellVerticalAlignment tcVA = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
                tCellProperties.Append(tcVA);

                System.Drawing.Color c = new System.Drawing.Color();
                c = System.Drawing.Color.FromArgb(255, 255, 255);

                switch (i)
                {
                    case 0:
                        {
                            // Set Cell Color
                            Shading tCellColor = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = System.Drawing.ColorTranslator.ToHtml(c) };
                            tCellProperties.Append(tCellColor);

                            TableCellWidth tCellWidth = new TableCellWidth();
                            tCellWidth.Type = TableWidthUnitValues.Dxa;
                            tCellWidth.Width = "600";

                            tCellProperties.Append(tCellWidth);

                            // Append properties to the cell
                            tCell.Append(tCellProperties);

                            ParagraphProperties paragProperties = new ParagraphProperties();
                            Justification justification1 = new Justification() { Val = JustificationValues.Both };
                            SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };
                            paragProperties.Append(spacingBetweenLines1);
                            paragProperties.Append(justification1);

                            var parag = new Paragraph();
                            parag.Append(paragProperties);

                            var run = new Run(new Text("No. Inv."));
                            ApplyFontProperties(run, true);
                            parag.Append(run);

                            // Specify the table cell content.
                            tCell.Append(parag);

                            // Append the table cell to the table row.
                            tRow.Append(tCell);

                            break;
                        }
                    case 1:
                        {
                            // Set Cell Color
                            Shading tCellColor = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = System.Drawing.ColorTranslator.ToHtml(c) };
                            tCellProperties.Append(tCellColor);

                            TableCellWidth tCellWidth = new TableCellWidth();
                            tCellWidth.Type = TableWidthUnitValues.Dxa;
                            tCellWidth.Width = "2000";

                            tCellProperties.Append(tCellWidth);

                            // Append properties to the cell
                            tCell.Append(tCellProperties);

                            ParagraphProperties paragProperties = new ParagraphProperties();
                            Justification justification1 = new Justification() { Val = JustificationValues.Both };
                            SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };

                            paragProperties.Append(spacingBetweenLines1);
                            paragProperties.Append(justification1);

                            var parag = new Paragraph();
                            parag.Append(paragProperties);

                            var run = new Run(new Text("Descripción"));
                            ApplyFontProperties(run, true);
                            parag.Append(run);

                            // Specify the table cell content.
                            tCell.Append(parag);

                            // Append the table cell to the table row.
                            tRow.Append(tCell);

                            break;
                        }
                    case 2:
                        {
                            // Set Cell Color
                            Shading tCellColor = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = System.Drawing.ColorTranslator.ToHtml(c) };
                            tCellProperties.Append(tCellColor);

                            TableCellWidth tCellWidth = new TableCellWidth();
                            tCellWidth.Type = TableWidthUnitValues.Dxa;
                            tCellWidth.Width = "1400";

                            tCellProperties.Append(tCellWidth);

                            // Append properties to the cell
                            tCell.Append(tCellProperties);

                            ParagraphProperties paragProperties = new ParagraphProperties();
                            Justification justification1 = new Justification() { Val = JustificationValues.Both };
                            SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };
                            paragProperties.Append(spacingBetweenLines1);
                            paragProperties.Append(justification1);

                            var parag = new Paragraph();
                            parag.Append(paragProperties);

                            var run = new Run(new Text("Responsable"));
                            ApplyFontProperties(run, true);
                            parag.Append(run);

                            // Specify the table cell content.
                            tCell.Append(parag);

                            // Append the table cell to the table row.
                            tRow.Append(tCell);

                            break;
                        }
                    default:
                        break;
                }


            }

            // Append the table row to the table.
            wordTable.Append(tRow);

            return wordTable;
        }

        private static Table AddHeaderRowUTH(Table wordTable)
        {
            // Create a row.
            TableRow tRow = new TableRow();

            for (int i = 0; i < 3; i++)
            {
                // Create a cell.
                TableCell tCell = new TableCell();

                TableCellProperties tCellProperties = new TableCellProperties();
                TableCellVerticalAlignment tcVA = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
                tCellProperties.Append(tcVA);

                System.Drawing.Color c = new System.Drawing.Color();
                c = System.Drawing.Color.FromArgb(255, 255, 255);

                switch (i)
                {
                    case 0:
                        {
                            // Set Cell Color
                            Shading tCellColor = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = System.Drawing.ColorTranslator.ToHtml(c) };
                            tCellProperties.Append(tCellColor);

                            TableCellWidth tCellWidth = new TableCellWidth();
                            tCellWidth.Type = TableWidthUnitValues.Dxa;
                            tCellWidth.Width = "1200";

                            tCellProperties.Append(tCellWidth);

                            // Append properties to the cell
                            tCell.Append(tCellProperties);

                            ParagraphProperties paragProperties = new ParagraphProperties();
                            Justification justification1 = new Justification() { Val = JustificationValues.Both };
                            SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };
                            paragProperties.Append(spacingBetweenLines1);
                            paragProperties.Append(justification1);

                            var parag = new Paragraph();
                            parag.Append(paragProperties);

                            var run = new Run(new Text("No. Inv."));
                            ApplyFontProperties(run, true);
                            parag.Append(run);

                            // Specify the table cell content.
                            tCell.Append(parag);

                            // Append the table cell to the table row.
                            tRow.Append(tCell);

                            break;
                        }
                    case 1:
                        {
                            // Set Cell Color
                            Shading tCellColor = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = System.Drawing.ColorTranslator.ToHtml(c) };
                            tCellProperties.Append(tCellColor);

                            TableCellWidth tCellWidth = new TableCellWidth();
                            tCellWidth.Type = TableWidthUnitValues.Dxa;
                            tCellWidth.Width = "2300";

                            tCellProperties.Append(tCellWidth);

                            // Append properties to the cell
                            tCell.Append(tCellProperties);

                            ParagraphProperties paragProperties = new ParagraphProperties();
                            Justification justification1 = new Justification() { Val = JustificationValues.Both };
                            SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };

                            paragProperties.Append(spacingBetweenLines1);
                            paragProperties.Append(justification1);

                            var parag = new Paragraph();
                            parag.Append(paragProperties);

                            var run = new Run(new Text("Descripción"));
                            ApplyFontProperties(run, true);
                            parag.Append(run);

                            // Specify the table cell content.
                            tCell.Append(parag);

                            // Append the table cell to the table row.
                            tRow.Append(tCell);

                            break;
                        }
                    case 2:
                        {
                            // Set Cell Color
                            Shading tCellColor = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = System.Drawing.ColorTranslator.ToHtml(c) };
                            tCellProperties.Append(tCellColor);

                            TableCellWidth tCellWidth = new TableCellWidth();
                            tCellWidth.Type = TableWidthUnitValues.Dxa;
                            tCellWidth.Width = "1500";

                            tCellProperties.Append(tCellWidth);

                            // Append properties to the cell
                            tCell.Append(tCellProperties);

                            ParagraphProperties paragProperties = new ParagraphProperties();
                            Justification justification1 = new Justification() { Val = JustificationValues.Both };
                            SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };
                            paragProperties.Append(spacingBetweenLines1);
                            paragProperties.Append(justification1);

                            var parag = new Paragraph();
                            parag.Append(paragProperties);

                            var run = new Run(new Text("Responsable"));
                            ApplyFontProperties(run, true);
                            parag.Append(run);

                            // Specify the table cell content.
                            tCell.Append(parag);

                            // Append the table cell to the table row.
                            tRow.Append(tCell);

                            break;
                        }
                    default:
                        break;
                }


            }

            // Append the table row to the table.
            wordTable.Append(tRow);

            return wordTable;
        }

        private static Table AddRows(Table wordTable, List<ActFijo> datos)
        {
            foreach (ActFijo actFijo in datos)
            {

                // Create a row.
                TableRow tRow = new TableRow();

                // Create a cell.
                TableCell tCell = new TableCell();

                var tCellProperties = new TableCellProperties();
                TableCellVerticalAlignment tcVA = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
                tCellProperties.Append(tcVA);

                // Set Cell Color
                Shading tCellColor = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto" };
                tCellProperties.Append(tCellColor);
                
                //TableCellWidth tCellWidth = new TableCellWidth();
                //tCellWidth.Type = TableWidthUnitValues.Dxa;
                //tCellWidth.Width = "600";

                //tCellProperties.Append(tCellWidth);

                tCell.Append(tCellProperties);

                ParagraphProperties paragProperties = new ParagraphProperties();
                SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };
                paragProperties.Append(spacingBetweenLines1);

                Run run = new Run();

                Text newText = new Text(actFijo.NoInventario.ToString());
                run.AppendChild<Text>(newText);

                run = ApplyFontProperties(run);

                var parag = new Paragraph();
                parag.Append(paragProperties);
                parag.Append(run);

                tCell.Append(parag);
                tRow.Append(tCell);

                // Create a cell.
                TableCell tCellDescripcion = new TableCell();

                var tCellPropertiesDescripcion = new TableCellProperties();

                // Set Cell Color
                Shading tCellColorDescripcion = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto" };
                tCellPropertiesDescripcion.Append(tCellColorDescripcion);

                //TableCellWidth tCellWidthDescripcion = new TableCellWidth();
                //tCellWidthDescripcion.Type = TableWidthUnitValues.Dxa;
                //tCellWidthDescripcion.Width = "2400";

                //tCellPropertiesDescripcion.Append(tCellWidthDescripcion);
                tCellDescripcion.Append(tCellPropertiesDescripcion);

                ParagraphProperties paragPropertiesDescripcion = new ParagraphProperties();
                SpacingBetweenLines spacingBetweenLinesDescripcion = new SpacingBetweenLines() { After = "0" };
                paragPropertiesDescripcion.Append(spacingBetweenLinesDescripcion);

                Run runDescripcion = new Run();

                Text newTextDescripcion = new Text(actFijo.Descripcion);
                runDescripcion.AppendChild<Text>(newTextDescripcion);

                runDescripcion = ApplyFontProperties(runDescripcion);

                Paragraph paragDescripcion = new Paragraph();
                paragDescripcion.Append(paragPropertiesDescripcion);
                paragDescripcion.Append(runDescripcion);

                tCellDescripcion.Append(paragDescripcion);
                tRow.Append(tCellDescripcion);

                // Create a cell.
                TableCell tCellResponsable = new TableCell();

                var tCellPropertiesResponsable = new TableCellProperties();

                // Set Cell Color
                Shading tCellColorResponsable = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto" };
                tCellPropertiesResponsable.Append(tCellColorResponsable);

                //TableCellWidth tCellWidthResponsable = new TableCellWidth();
                //tCellWidthResponsable.Type = TableWidthUnitValues.Dxa;
                //tCellWidthResponsable.Width = "1000";

                //tCellPropertiesResponsable.Append(tCellWidthResponsable);
                tCellResponsable.Append(tCellPropertiesResponsable);

                ParagraphProperties paragPropertiesResponsable = new ParagraphProperties();
                SpacingBetweenLines spacingBetweenLinesResponsable = new SpacingBetweenLines() { After = "0" };
                paragPropertiesResponsable.Append(spacingBetweenLinesResponsable);

                Run runResponsable = new Run();

                Text newTextResponsable = new Text(actFijo.ResponsableNavigation.Nombre + " " + actFijo.ResponsableNavigation.Apellidos);
                runResponsable.AppendChild<Text>(newTextResponsable);

                runResponsable = ApplyFontProperties(runResponsable);

                Paragraph paragResponsable = new Paragraph();
                paragResponsable.Append(paragPropertiesResponsable);
                paragResponsable.Append(runResponsable);

                // Specify the table cell content.
                tCellResponsable.Append(paragResponsable);

                // Append the table cell to the table row.
                tRow.Append(tCellResponsable);

                // Append the table row to the table.
                wordTable.Append(tRow);
            }

            return wordTable;
        }

        private static Table AddRows(Table wordTable, List<UtilesHerramienta> datos)
        {
            foreach (UtilesHerramienta utiles in datos)
            {

                // Create a row.
                TableRow tRow = new TableRow();

                // Create a cell.
                TableCell tCell = new TableCell();

                var tCellProperties = new TableCellProperties();
                TableCellVerticalAlignment tcVA = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
                tCellProperties.Append(tcVA);

                // Set Cell Color
                Shading tCellColor = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto" };
                tCellProperties.Append(tCellColor);

                TableCellWidth tCellWidth = new TableCellWidth();
                tCellWidth.Type = TableWidthUnitValues.Dxa;
                tCellWidth.Width = "1000";

                tCellProperties.Append(tCellWidth);

                tCell.Append(tCellProperties);

                ParagraphProperties paragProperties = new ParagraphProperties();
                SpacingBetweenLines spacingBetweenLines1 = new SpacingBetweenLines() { After = "0" };
                paragProperties.Append(spacingBetweenLines1);

                Run run = new Run();

                Text newText = new Text(utiles.Codigo.ToString());
                run.AppendChild<Text>(newText);

                run = ApplyFontProperties(run);

                var parag = new Paragraph();
                parag.Append(paragProperties);
                parag.Append(run);

                tCell.Append(parag);
                tRow.Append(tCell);

                // Create a cell.
                TableCell tCellDescripcion = new TableCell();

                var tCellPropertiesDescripcion = new TableCellProperties();

                // Set Cell Color
                Shading tCellColorDescripcion = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto" };
                tCellPropertiesDescripcion.Append(tCellColorDescripcion);

                //TableCellWidth tCellWidthDescripcion = new TableCellWidth();
                //tCellWidthDescripcion.Type = TableWidthUnitValues.Dxa;
                //tCellWidthDescripcion.Width = "2400";

                //tCellPropertiesDescripcion.Append(tCellWidthDescripcion);
                tCellDescripcion.Append(tCellPropertiesDescripcion);

                ParagraphProperties paragPropertiesDescripcion = new ParagraphProperties();
                SpacingBetweenLines spacingBetweenLinesDescripcion = new SpacingBetweenLines() { After = "0" };
                paragPropertiesDescripcion.Append(spacingBetweenLinesDescripcion);

                Run runDescripcion = new Run();

                Text newTextDescripcion = new Text(utiles.Descripcion);
                runDescripcion.AppendChild<Text>(newTextDescripcion);

                runDescripcion = ApplyFontProperties(runDescripcion);

                Paragraph paragDescripcion = new Paragraph();
                paragDescripcion.Append(paragPropertiesDescripcion);
                paragDescripcion.Append(runDescripcion);

                tCellDescripcion.Append(paragDescripcion);
                tRow.Append(tCellDescripcion);

                // Create a cell.
                TableCell tCellResponsable = new TableCell();

                var tCellPropertiesResponsable = new TableCellProperties();

                // Set Cell Color
                Shading tCellColorResponsable = new Shading() { Val = ShadingPatternValues.Clear, Color = "auto" };
                tCellPropertiesResponsable.Append(tCellColorResponsable);

                TableCellWidth tCellWidthResponsable = new TableCellWidth();
                tCellWidthResponsable.Type = TableWidthUnitValues.Dxa;
                tCellWidthResponsable.Width = "900";

                tCellPropertiesResponsable.Append(tCellWidthResponsable);
                tCellResponsable.Append(tCellPropertiesResponsable);

                ParagraphProperties paragPropertiesResponsable = new ParagraphProperties();
                SpacingBetweenLines spacingBetweenLinesResponsable = new SpacingBetweenLines() { After = "0" };
                paragPropertiesResponsable.Append(spacingBetweenLinesResponsable);

                Run runResponsable = new Run();

                Text newTextResponsable = new Text(utiles.ResponsableNavigation.Nombre + " " + utiles.ResponsableNavigation.Apellidos);
                runResponsable.AppendChild<Text>(newTextResponsable);

                runResponsable = ApplyFontProperties(runResponsable);

                Paragraph paragResponsable = new Paragraph();
                paragResponsable.Append(paragPropertiesResponsable);
                paragResponsable.Append(runResponsable);

                // Specify the table cell content.
                tCellResponsable.Append(paragResponsable);

                // Append the table cell to the table row.
                tRow.Append(tCellResponsable);

                // Append the table row to the table.
                wordTable.Append(tRow);
            }

            return wordTable;
        }

        private static Run ApplyFontProperties(Run run, bool siBold = false)
        {

            int fontSize = 9;

            RunProperties runProp = new RunProperties();
            FontSize size = new FontSize();
            size.Val = new StringValue((fontSize * 2).ToString());
            runProp.Append(size);
            var runFont = new RunFonts { Ascii = @"Arial", HighAnsi = @"Arial", ComplexScript = @"Arial" };
            runProp.Append(runFont);

            if (siBold)
            {
                Bold bold = new Bold();
                runProp.Append(bold);
            }

            run.PrependChild<RunProperties>(runProp);
            return run;
        }

        private void SetBookmarkValue(WordprocessingDocument doc, string BookmarkName, string BookmarkValue, bool DeleteAfterSetvalue = true)
        {
            Body body = doc.MainDocumentPart.Document.GetFirstChild<Body>();
            var paras = body.Elements<Paragraph>();

            //Iterate through the paragraphs to find the bookmarks inside
            foreach (var para in paras)
            {
                var bookMarkStarts = para.Elements<BookmarkStart>();
                var bookMarkEnds = para.Elements<BookmarkEnd>();

                foreach (BookmarkStart bookMarkStart in bookMarkStarts)
                {
                    if (bookMarkStart.Name == BookmarkName)
                    {
                        //Get the id of the bookmark start to find the bookmark end
                        var id = bookMarkStart.Id.Value;
                        var bookmarkEnd = bookMarkEnds.Where(i => i.Id.Value == id).First();

                        var runProp = new RunProperties();

                        var runFont = new RunFonts { Ascii = @"Arial", HighAnsi = @"Arial", ComplexScript = @"Arial" };

                        // 48 half-point font size
                        var size = new FontSize { Val = new StringValue("18") };

                        runProp.Append(runFont);
                        runProp.Append(size);

                        Underline underline = new Underline() { Val = UnderlineValues.Single };
                        runProp.Append(underline);

                        var runElement = new Run(new Text(BookmarkValue));
                        runElement.PrependChild(runProp);
                        para.InsertAfter(runElement, bookmarkEnd);

                        if (DeleteAfterSetvalue)
                        {
                            //移除書籤
                            bookMarkStart.Remove();
                        }
                    }
                }
            }
        }

        private void SetBookmarkValueWithTable(WordprocessingDocument doc, string BookmarkName, List<ActFijo> datos, DateTime fecha, bool siActaGeneral = true, bool DeleteAfterSetValue = true)
        {
            var mainPart = doc.MainDocumentPart;
            var res = from bm in mainPart.Document.Body.Descendants<BookmarkStart>()
                      where bm.Name == BookmarkName
                      select bm;
            var bookmark = res.SingleOrDefault();
            if (bookmark != null)
            {
                var parent = bookmark.Parent;   // bookmark's parent element

                List<AreaDeResponsabilidad> areasResp = new List<AreaDeResponsabilidad>();
                areasResp = datos.Select(c => c.AreaResponsabilidadNavigation).Distinct().OrderBy(c => c.Nombre).ToList();

                bool siPrimeraTabla = true;

                foreach (AreaDeResponsabilidad item in areasResp)
                {
                    TableProperties tblProp = new TableProperties(
                        new TableBorders(
                            new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                            new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                            new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                            new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                            new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                            new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 }),
                        new TableWidth() { Width = "4000", Type = TableWidthUnitValues.Pct },
                        new TableJustification() { Val = TableRowAlignmentValues.Left });

                    // Now we create a new layout and make it "fixed".
                    TableLayout tl = new TableLayout() { Type = TableLayoutValues.Fixed };
                    tblProp.TableLayout = tl;

                    TableStyle tableStyle = new TableStyle { Val = "LightShadingAccent1" };
                    tblProp.TableStyle = tableStyle;

                    Table table = new Table();

                    table.AppendChild(tblProp);

                    if (siActaGeneral)
                    {
                        if (siPrimeraTabla)
                        {
                            table = AddTitleRow(table, fecha);
                            siPrimeraTabla = false;
                        }
                    }

                    table = AddSubTitleRow(table, item.Nombre.ToUpper());
                    table = AddHeaderRow(table);

                    table = AddRows(table, datos.Where(c => c.AreaResponsabilidad.Equals(item.IdArea)).ToList());

                    Paragraph tableParagraph = new Paragraph();
                    parent.InsertBeforeSelf(tableParagraph);
                    tableParagraph.InsertAfterSelf(table);
                }

                if (DeleteAfterSetValue)
                {
                    bookmark.Remove();
                }
            }
        }

        private void SetBookmarkValueWithTable(WordprocessingDocument doc, string BookmarkName, List<UtilesHerramienta> datos, DateTime fecha, bool siActaGeneral = true, bool DeleteAfterSetValue = true)
        {
            var mainPart = doc.MainDocumentPart;
            var res = from bm in mainPart.Document.Body.Descendants<BookmarkStart>()
                      where bm.Name == BookmarkName
                      select bm;
            var bookmark = res.SingleOrDefault();
            if (bookmark != null)
            {
                var parent = bookmark.Parent;   // bookmark's parent element

                List<AreaDeResponsabilidad> areasResp = new List<AreaDeResponsabilidad>();
                areasResp = datos.Select(c => c.AreaResponsabilidadNavigation).Distinct().OrderBy(c => c.Nombre).ToList();

                bool siPrimeraTabla = true;

                foreach (AreaDeResponsabilidad item in areasResp)
                {
                    TableProperties tblProp = new TableProperties(
                        new TableBorders(
                            new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                            new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                            new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                            new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                            new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 },
                            new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 1 }),
                        new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct },
                        new TableJustification() { Val = TableRowAlignmentValues.Left });

                    // Now we create a new layout and make it "fixed".
                    TableLayout tl = new TableLayout() { Type = TableLayoutValues.Fixed };
                    tblProp.TableLayout = tl;

                    TableStyle tableStyle = new TableStyle { Val = "LightShadingAccent1" };
                    tblProp.TableStyle = tableStyle;

                    Table table = new Table();

                    table.AppendChild(tblProp);

                    if (siActaGeneral)
                    {
                        if (siPrimeraTabla)
                        {
                            table = AddTitleRow(table, fecha);
                            siPrimeraTabla = false;
                        }
                    }

                    table = AddSubTitleRow(table, item.Nombre.ToUpper());
                    table = AddHeaderRowUTH(table);

                    table = AddRows(table, datos.Where(c => c.AreaResponsabilidad.Equals(item.IdArea)).ToList());

                    Paragraph tableParagraph = new Paragraph();
                    parent.InsertBeforeSelf(tableParagraph);
                    tableParagraph.InsertAfterSelf(table);
                }

                if (DeleteAfterSetValue)
                {
                    bookmark.Remove();
                }
            }
        }

        private static void ApplyStyleToParagraph(WordprocessingDocument doc, string styleid, string stylename, Paragraph p)
        {
            // If the paragraph has no ParagraphProperties object, create one.
            if (p.Elements<ParagraphProperties>().Count() == 0)
            {
                p.PrependChild<ParagraphProperties>(new ParagraphProperties());
            }

            // Get the paragraph properties element of the paragraph.
            ParagraphProperties pPr = p.Elements<ParagraphProperties>().First();

            // Get the Styles part for this document.
            StyleDefinitionsPart part =
                doc.MainDocumentPart.StyleDefinitionsPart;

            // If the Styles part does not exist, add it and then add the style.
            if (part == null)
            {
                part = AddStylesPartToPackage(doc);
                AddNewStyle(part, styleid, stylename);
            }
            else
            {
                // If the style is not in the document, add it.
                if (IsStyleIdInDocument(doc, styleid) != true)
                {
                    // No match on styleid, so let's try style name.
                    string styleidFromName = GetStyleIdFromStyleName(doc, stylename);
                    if (styleidFromName == null)
                    {
                        AddNewStyle(part, styleid, stylename);
                    }
                    else
                        styleid = styleidFromName;
                }
            }

            // Set the style of the paragraph.
            pPr.ParagraphStyleId = new ParagraphStyleId() { Val = styleid };
        }

        private static bool IsStyleIdInDocument(WordprocessingDocument doc, string styleid)
        {
            // Get access to the Styles element for this document.
            Styles s = doc.MainDocumentPart.StyleDefinitionsPart.Styles;

            // Check that there are styles and how many.
            int n = s.Elements<Style>().Count();
            if (n == 0)
                return false;

            // Look for a match on styleid.
            Style style = s.Elements<Style>()
                .Where(st => (st.StyleId == styleid) && (st.Type == StyleValues.Paragraph))
                .FirstOrDefault();
            if (style == null)
                return false;

            return true;
        }

        // Return styleid that matches the styleName, or null when there's no match.
        private static string GetStyleIdFromStyleName(WordprocessingDocument doc, string styleName)
        {
            StyleDefinitionsPart stylePart = doc.MainDocumentPart.StyleDefinitionsPart;
            string styleId = stylePart.Styles.Descendants<StyleName>()
                .Where(s => s.Val.Value.Equals(styleName) &&
                    (((Style)s.Parent).Type == StyleValues.Paragraph))
                .Select(n => ((Style)n.Parent).StyleId).FirstOrDefault();
            return styleId;
        }

        // Create a new style with the specified styleid and stylename and add it to the specified
        // style definitions part.
        private static void AddNewStyle(StyleDefinitionsPart styleDefinitionsPart, string styleid, string stylename)
        {
            // Get access to the root element of the styles part.
            Styles styles = styleDefinitionsPart.Styles;

            // Create a new paragraph style and specify some of the properties.
            Style style = new Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = styleid,
                CustomStyle = true
            };
            StyleName styleName1 = new StyleName() { Val = stylename };
            BasedOn basedOn1 = new BasedOn() { Val = "Normal" };
            NextParagraphStyle nextParagraphStyle1 = new NextParagraphStyle() { Val = "Normal" };
            style.Append(styleName1);
            style.Append(basedOn1);
            style.Append(nextParagraphStyle1);

            // Create the StyleRunProperties object and specify some of the run properties.
            StyleRunProperties styleRunProperties1 = new StyleRunProperties();
            Bold bold1 = new Bold();
            Color color1 = new Color() { ThemeColor = ThemeColorValues.Accent2 };
            RunFonts font1 = new RunFonts() { EastAsia = "DFKai-SB" /*正黑體: Microsoft JhengHei, 標楷體：DFKai-SB, 細明體：MingLiU, 新細明體：PMingLiU */};
            //Italic italic1 = new Italic();
            //Specify a 12 point size.
            FontSize fontSize1 = new FontSize() { Val = "24" };
            //styleRunProperties1.Append(bold1);
            //styleRunProperties1.Append(color1);
            styleRunProperties1.Append(font1);
            styleRunProperties1.Append(fontSize1);
            //styleRunProperties1.Append(italic1);


            // Add the run properties to the style.
            style.Append(styleRunProperties1);

            // Add the style to the styles part.
            styles.Append(style);
        }

        // Add a StylesDefinitionsPart to the document.  Returns a reference to it.
        private static StyleDefinitionsPart AddStylesPartToPackage(WordprocessingDocument doc)
        {
            StyleDefinitionsPart part;
            part = doc.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
            Styles root = new Styles();
            root.Save(part);
            return part;
        }

        public static string GetNameMonth(int month, int year)
        {
            string nomMesAnno = string.Empty;
            switch (month)
            {
                case 1:
                    {
                        nomMesAnno = "Enero/" + year.ToString();
                        break;
                    }
                case 2:
                    {
                        nomMesAnno = "Febrero/" + year.ToString();
                        break;
                    }
                case 3:
                    {
                        nomMesAnno = "Marzo/" + year.ToString();
                        break;
                    }
                case 4:
                    {
                        nomMesAnno = "Abril/" + year.ToString();
                        break;
                    }
                case 5:
                    {
                        nomMesAnno = "Mayo/" + year.ToString();
                        break;
                    }
                case 6:
                    {
                        nomMesAnno = "Junio/" + year.ToString();
                        break;
                    }
                case 7:
                    {
                        nomMesAnno = "Julio/" + year.ToString();
                        break;
                    }
                case 8:
                    {
                        nomMesAnno = "Agosto/" + year.ToString();
                        break;
                    }
                case 9:
                    {
                        nomMesAnno = "Septiembre/" + year.ToString();
                        break;
                    }
                case 10:
                    {
                        nomMesAnno = "Octubre/" + year.ToString();
                        break;
                    }
                case 11:
                    {
                        nomMesAnno = "Noviembre/" + year.ToString();
                        break;
                    }
                case 12:
                    {
                        nomMesAnno = "Diciembre/" + year.ToString();
                        break;
                    }
                default:
                    break;
            }

            return nomMesAnno;
        }
    }
}
