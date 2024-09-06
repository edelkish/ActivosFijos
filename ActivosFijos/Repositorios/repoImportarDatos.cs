using ActivosFijos.Clases;
using ActivosFijos.Models.Auxiliar;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ActivosFijos.Repositorios
{
    public class repoImportarDatos
    {
        private aftContext _context;
        public repoImportarDatos(aftContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Datos a importar de AFT
        public clResponse<DataTable> GetDataTableFromSpreadsheet(Stream MyExcelStream, bool ReadOnly)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SpreadsheetDocument sDoc = SpreadsheetDocument.Open(MyExcelStream, ReadOnly))
                {
                    WorkbookPart workbookPart = sDoc.WorkbookPart;
                    IEnumerable<Sheet> sheets = sDoc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

                    SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                    SharedStringTable sst = sstpart.SharedStringTable;

                    string relationshipId = sheets.First().Id.Value;
                    WorksheetPart worksheetPart = (WorksheetPart)sDoc.WorkbookPart.GetPartById(relationshipId);
                    Worksheet workSheet = worksheetPart.Worksheet;
                    SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                    IEnumerable<Row> rows = sheetData.Descendants<Row>();

                    //foreach (Cell cell in rows.ElementAt(2))
                    //{
                    //    string getCellV = GetCellValue(sDoc, cell);

                    //    if (getCellV != null)
                    //        dt.Columns.Add(getCellV);
                    //}

                    dt.Columns.Add("No");
                    dt.Columns.Add("Descripcion");
                    dt.Columns.Add("NoInventario");
                    dt.Columns.Add("Grupo");
                    dt.Columns.Add("CentroCosto");
                    dt.Columns.Add("Responsable");
                    dt.Columns.Add("VAdquisicion", typeof(double));
                    dt.Columns.Add("VContable", typeof(double));
                    dt.Columns.Add("DAcumulada", typeof(double));
                    dt.Columns.Add("VResidual", typeof(double));
                    dt.Columns.Add("Estado");
                    dt.Columns.Add("Serie");
                    dt.Columns.Add("Matricula");
                    dt.Columns.Add("AreaResponsabilidad");

                    int siTitulo = 0;

                    foreach (Row item in rows.Take(2))
                    {
                        int j = 0;
                        for (int i = 0; i < item.Descendants<Cell>().Count(); i++)
                        {
                            string getCellV = GetCellValue(sDoc, item.Descendants<Cell>().ElementAt(i));
                            if (getCellV != null)
                            {
                                if (siTitulo > 0)
                                {
                                    if (getCellV.Contains("Entidad:"))
                                    {

                                    }
                                    else
                                    {
                                        clGlobal.vEntidad = GetCellValue(sDoc, item.Descendants<Cell>().ElementAt(i));
                                        siTitulo++;
                                        break;
                                    }
                                }
                                else
                                {
                                    clGlobal.vReporte = getCellV;
                                    siTitulo++;

                                    break;
                                }
                            }
                        }

                        if (siTitulo > 1)
                        {
                            break;
                        }
                    }

                    rows = rows.Skip(3);
                    string areaResp = string.Empty;

                    int testCOnt = 0;

                    foreach (Row row in rows) //this will also include your header row...
                    {
                        DataRow tempRow = dt.NewRow();
                        int j = 0;

                        for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                        {
                            string getCellV = GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                            if (getCellV != null)
                            {
                                if (getCellV == "N.º")
                                    break;

                                tempRow[j] = getCellV;
                                j++;
                            }
                        }

                        if (tempRow[0].ToString() == "-  Área de responsabilidad: ")
                        {
                            areaResp = tempRow[1].ToString();
                        }
                        else if ((tempRow[6] != null) && (tempRow[7] != null) && (tempRow[8] != null) && (tempRow[9] != null))
                        {
                            if ((long.TryParse(tempRow[0].ToString(), out _)) && (long.TryParse(tempRow[2].ToString(), out _)) &&
                                (double.TryParse(tempRow[6].ToString(), out _)) && (double.TryParse(tempRow[7].ToString(), out _)) &&
                                (double.TryParse(tempRow[8].ToString(), out _)) && (double.TryParse(tempRow[9].ToString(), out _)))
                            {
                                if (dt.Select($"NoInventario = '{tempRow[2].ToString()}'").Count() == 0)
                                {
                                    tempRow[j] = areaResp;
                                    dt.Rows.Add(tempRow);
                                }
                            }
                        }
                    }

                }

                //dt.Rows.RemoveAt(0);

                int cantidadFilasConErrores = dt.Select("Responsable = ''").Count();

                if (cantidadFilasConErrores > 0)
                    return new clResponse<DataTable>() { SiSatisfactorio = true, Mensaje = $"Los datos han sido importados satisfactoriamente; pero existen filas ({cantidadFilasConErrores}) con con posibles errores en sus valores", Resultado = dt };
                else
                    return new clResponse<DataTable>() { SiSatisfactorio = true, Mensaje = "Los datos han sido importados satisfactoriamente.", Resultado = dt };
            }
            catch (Exception ex)
            {
                return new clResponse<DataTable>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador.", Resultado = new DataTable() };
            }
        }

        public clResponse<mdDatosAdicionales> GetDatosAdicionalesFromDataTable(DataTable dt)
        {
            try
            {
                bool siFecha = false;
                mdDatosAdicionales datosAdicionales = new mdDatosAdicionales();

                List<mdDatosAdicionalesAreasResponsabilidad> areasResponsabilidades = new List<mdDatosAdicionalesAreasResponsabilidad>();
                mdDatosAdicionalesAreasResponsabilidad datosAdicAreasResponsabilidad = new mdDatosAdicionalesAreasResponsabilidad();

                int cantProductosArea = 0;
                double vAdquisicion = 0;
                double vContable = 0;
                double dAcumulada = 0;
                double vResidual = 0;
                bool siEncabezadoAreaPrimeraVez = false;

                datosAdicionales.vAdquisicion = dt.Select().Sum(c => double.Parse(c.ItemArray[6].ToString()));
                datosAdicionales.vContable = dt.Select().Sum(c => double.Parse(c.ItemArray[7].ToString()));
                datosAdicionales.dAcumulada = dt.Select().Sum(c => double.Parse(c.ItemArray[8].ToString()));
                datosAdicionales.vResidual = dt.Select().Sum(c => double.Parse(c.ItemArray[9].ToString()));
                datosAdicionales.Entidad = clGlobal.vEntidad;
                datosAdicionales.TotalProductos = dt.Rows.Count;
                datosAdicionales.TotalAreas = dt.Select().Distinct().Count();
                datosAdicionales.fGeneracion = DateTime.Today;

                return new clResponse<mdDatosAdicionales>() { SiSatisfactorio = true, Mensaje = "Los datos adicionales han sido cargados satisfactoriamente.", Resultado = datosAdicionales };
            }
            catch (Exception ex)
            {
                return new clResponse<mdDatosAdicionales>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador.", Resultado = new mdDatosAdicionales() };
            }
        }

        public List<SelectListItem> GetFromDataTableAreasDeResponsabilidad(DataTable dt, string? area)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            bool siSeleccionadoActivo = false;

            foreach (DataRow row in dt.Rows)
            {
                string areaResp = row["AreaResponsabilidad"].ToString();
                if (!string.IsNullOrEmpty(areaResp))
                {
                    if (!lista.Any(c => c.Value == row["AreaResponsabilidad"].ToString().Trim()))
                    {
                        bool siSeleccionar = row["AreaResponsabilidad"].ToString().Trim() == area.Trim();

                        if (siSeleccionar)
                        {
                            siSeleccionadoActivo = true;
                        }

                        SelectListItem itemSelect = new SelectListItem()
                        {
                            Value = row["AreaResponsabilidad"].ToString(),
                            Text = row["AreaResponsabilidad"].ToString(),
                            Selected = siSeleccionar
                        };

                        lista.Add(itemSelect);
                    }
                }
            }

            if (lista.Count > 0)
            {
                SelectListItem itemSelect = new SelectListItem()
                {
                    Value = "Todos",
                    Text = "Todos",
                    Selected = !siSeleccionadoActivo
                };

                lista.Add(itemSelect);
            }

            return lista;
        }

        public List<AreaDeResponsabilidad> GetFromDataTableAreasDeResponsabilidad(DataTable dt)
        {
            List<AreaDeResponsabilidad> lista = new List<AreaDeResponsabilidad>();
            lista = _context.AreaDeResponsabilidads.ToList();

            DataTable distinctDT = SelectDistinct(dt, "AreaResponsabilidad");

            foreach (DataRow row in distinctDT.Rows)
            {
                if (!string.IsNullOrEmpty(row["AreaResponsabilidad"].ToString()))
                {
                    string nombreArea = new string(row["AreaResponsabilidad"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray());
                    if (!lista.Any(c => new string(c.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(nombreArea)))
                    {
                        AreaDeResponsabilidad areaDeResponsabilidad = new AreaDeResponsabilidad()
                        {
                            IdArea = Guid.NewGuid(),
                            Nombre = row["AreaResponsabilidad"].ToString().Trim(),
                            Descripcion = row["AreaResponsabilidad"].ToString().Trim()
                        };

                        lista.Add(areaDeResponsabilidad);
                    }
                }
            }

            return lista;
        }

        public List<SelectListItem> GetFromDataTableCentrosDeCosto(DataTable dt, string? areaResponsabilidad)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            bool siAreaDisponibilidadEncontrada = false;
            string areaResp = string.Empty;

            //Si el área es null o vacía es que se necesitan todos los centros de costo,
            //de lo contrario serían los que pertenecen a determinada área de responsabilidad
            if (!string.IsNullOrEmpty(areaResponsabilidad))
            {
                DataRow[] prueba = dt.Select($"AreaResponsabilidad = '{areaResponsabilidad}'");
                foreach (DataRow row in prueba)
                {
                    if (!lista.Any(c => c.Text.Equals(row["CentroCosto"].ToString().Trim())))
                    {
                        siAreaDisponibilidadEncontrada = true;

                        SelectListItem itemSelect = new SelectListItem()
                        {
                            Text = row["CentroCosto"].ToString().Trim(),
                            Value = row["CentroCosto"].ToString().Trim(),
                            Selected = false
                        };

                        lista.Add(itemSelect);
                    }
                }
            }
            else
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (!lista.Any(c => c.Text == row["CentroCosto"].ToString().Trim()))
                    {
                        string centCost = row["CentroCosto"].ToString().Trim();
                        SelectListItem itemSelect = new SelectListItem()
                        {
                            Text = centCost,
                            Value = centCost,
                            Selected = false
                        };

                        lista.Add(itemSelect);
                    }

                }
            }

            lista = lista.OrderBy(c => c.Text).ToList();

            if (lista.Count > 0)
            {
                SelectListItem itemSelect = new SelectListItem()
                {
                    Value = "Todos",
                    Text = "Todos",
                    Selected = true
                };

                lista.Add(itemSelect);
            }

            return lista;
        }

        public List<CentroCosto> GetFromDataTableCentroDeCosto(DataTable dt)
        {
            List<CentroCosto> lista = new List<CentroCosto>();
            lista = _context.CentroCostos.ToList();

            DataTable distinctDT = SelectDistinct(dt, "CentroCosto");

            foreach (DataRow row in distinctDT.Rows)
            {
                if (!string.IsNullOrEmpty(row["CentroCosto"].ToString().Trim()))
                {
                    string textCentroCosto = new string(row["CentroCosto"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray());
                    if (!lista.Any(c => new string(c.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(textCentroCosto)))
                    {
                        CentroCosto centroCosto = new CentroCosto()
                        {
                            IdCentroCosto = Guid.NewGuid(),
                            Nombre = row["CentroCosto"].ToString().Trim(),
                            Numero = null
                        };

                        lista.Add(centroCosto);
                    }
                }
            }

            return lista;
        }

        public List<SelectListItem> GetFromDataTableResponsables(DataTable dt, string? areaResponsabilidad)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            bool siAreaDisponibilidadEncontrada = false;
            string areaResp = string.Empty;

            //Si el área es null o vacía es que se necesitan todos los centros de costo,
            //de lo contrario serían los que pertenecen a determinada área de responsabilidad
            if (!string.IsNullOrEmpty(areaResponsabilidad))
            {
                DataRow[] prueba = dt.Select($"AreaResponsabilidad = '{areaResponsabilidad}'");
                foreach (DataRow row in prueba)
                {
                    if (!lista.Any(c => c.Text.Equals(row["Responsable"].ToString().Trim())))
                    {
                        siAreaDisponibilidadEncontrada = true;

                        SelectListItem itemSelect = new SelectListItem()
                        {
                            Text = row["Responsable"].ToString().Trim() == String.Empty ? "<<S/R>>" : row["Responsable"].ToString().Trim(),
                            Value = row["Responsable"].ToString().Trim() == String.Empty ? "<<S/R>>" : row["Responsable"].ToString().Trim(),
                            Selected = false
                        };

                        lista.Add(itemSelect);
                    }
                }
            }
            else
            {
                DataTable distinctDT = SelectDistinct(dt, "Responsable");
                foreach (DataRow row in distinctDT.Rows)
                {
                    if (!lista.Any(c => c.Text == row["Responsable"].ToString().Trim()))
                    {
                        string resp = row["Responsable"].ToString().Trim();
                        SelectListItem itemSelect = new SelectListItem()
                        {
                            Text = resp == String.Empty ? "<<S/R>>" : resp,
                            Value = resp == String.Empty ? "<<S/R>>" : resp,
                            Selected = false
                        };

                        lista.Add(itemSelect);
                    }

                }
            }

            lista = lista.OrderBy(c => c.Text).ToList();

            if (lista.Count > 0)
            {
                SelectListItem itemSelect = new SelectListItem()
                {
                    Value = "Todos",
                    Text = "Todos",
                    Selected = true
                };

                lista.Add(itemSelect);
            }

            return lista;
        }

        public List<Responsable> GetFromDataTableResponsablesDB(DataTable dt, List<AreaDeResponsabilidad> areas)
        {
            List<Responsable> lista = new List<Responsable>();
            lista = _context.Responsables.ToList();

            DataTable distinctDT = SelectDistinct(dt, "Responsable");

            foreach (DataRow row in dt.Rows)
            {
                string areaResp = row["AreaResponsabilidad"].ToString().ToLower().Trim();
                string nombreArea = new string(areaResp.Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray());
                AreaDeResponsabilidad area = new AreaDeResponsabilidad();
                area = areas.SingleOrDefault(ar => new string(ar.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(nombreArea));

                if (!string.IsNullOrEmpty(row["Responsable"].ToString()))
                {
                    string nombreResp = new string(row["Responsable"].ToString().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray());
                    if (!lista.Any(c => new string((c.Nombre.ToLower().Trim() + " " + c.Apellidos.ToLower().Trim()).Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(nombreResp.ToLower())))
                    {
                        string[] nombre = nombreResp.Split(' ');
                        string apellidos = string.Empty;
                        for (int i = 1; i < nombre.Length; i++)
                        {
                            apellidos += " " + nombre[i];
                        }

                        Responsable responsable = new Responsable()
                        {
                            IdResponsable = Guid.NewGuid(),
                            Nombre = nombre[0].ToString(),
                            Apellidos = apellidos.Trim(),
                            AreaResponsabilidad = area.IdArea,
                            ResponsableDelArea = false,
                            Cargo = null
                        };

                        lista.Add(responsable);
                    }
                }
                else if (row["Responsable"].ToString() == String.Empty)
                {
                    if (!lista.Any(c => c.Nombre.ToLower().Trim() == "<<S/R>>".ToLower().Trim()))
                    {
                        Responsable responsable = new Responsable()
                        {
                            IdResponsable = Guid.NewGuid(),
                            Nombre = "<<S/R>>",
                            Apellidos = "<<Sin Responsable>>",
                            AreaResponsabilidad = area.IdArea,
                            ResponsableDelArea = false,
                            Cargo = null
                        };

                        lista.Add(responsable);
                    }
                }
            }

            return lista;
        }

        public clResponse<DataTable> GetDataTablePorResponsableYAreasDeResponsabilidad(DataTable dt, string? responsable, string? areaResponsabilidad)
        {
            try
            {
                DataTable dtResponsable = new DataTable();
                foreach (DataColumn itemCol in dt.Columns)
                {
                    dtResponsable.Columns.Add(itemCol.ColumnName);
                }

                DataRow[] prueba = null;

                //Si el área es null o vacía es que se necesitan todos los centros de costo,
                //de lo contrario serían los que pertenecen a determinada área de responsabilidad
                if (!string.IsNullOrEmpty(areaResponsabilidad))
                {
                    prueba = dt.Select($"AreaResponsabilidad = '{areaResponsabilidad}'");
                    if (!string.IsNullOrEmpty(responsable))
                    {
                        if (responsable == "<<S/R>>")
                            prueba = dt.Select($"AreaResponsabilidad = '{areaResponsabilidad}' AND Responsable = ''");
                        else
                        {
                            prueba = dt.Select($"AreaResponsabilidad = '{areaResponsabilidad}' AND Responsable = '{responsable}'");
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(responsable))
                    {
                        if (responsable == "<<S/R>>")
                            prueba = dt.Select($"Responsable = ''");
                        else
                        {
                            prueba = dt.Select($"Responsable = '{responsable}'");
                        }
                    }
                    else
                    {
                        prueba = dt.Select();
                    }
                }

                foreach (DataRow row in prueba)
                {
                    DataRow tempRow = dtResponsable.NewRow();
                    for (int i = 0; i < dtResponsable.Columns.Count; i++)
                    {
                        tempRow[i] = row[i];
                    }
                    dtResponsable.Rows.Add(tempRow);
                }

                return new clResponse<DataTable>() { SiSatisfactorio = true, Mensaje = "Los datos han sido cargados satisfactoriamente.", Resultado = dtResponsable };
            }
            catch (Exception ex)
            {
                return new clResponse<DataTable>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador.", Resultado = new DataTable() };
            }
        }

        public void GetFromDataTableInformacionAdicional(DataTable dt)
        {
            int areasResponsabilidad = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (row[0].ToString().Contains("-  Área de responsabilidad: "))
                {
                    areasResponsabilidad++;
                }
                else
                {

                }
            }
        }

        public List<mdActFijo> GetFromDataTableDatosImportados(DataTable dt, List<AreaDeResponsabilidad> areas, List<CentroCosto> centros, List<Responsable> responsables, DateTime fechaSubmayor)
        {
            List<ActFijo> actFijos = new List<ActFijo>();
            actFijos = _context.ActFijos.Where(act => act.FechaSubmayor.Year == fechaSubmayor.Year && act.FechaSubmayor.Month == fechaSubmayor.Month).ToList();

            List<mdActFijo> resultadoActFijos = new List<mdActFijo>();

            try
            {
                if (actFijos != null)
                {
                    if (actFijos.Count() > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            ActFijo act = new ActFijo();
                            act = _context.ActFijos.SingleOrDefault(act => act.NoInventario.Equals(long.Parse(row["NoInventario"].ToString().Trim())) && act.FechaSubmayor.Year == fechaSubmayor.Year && act.FechaSubmayor.Month == fechaSubmayor.Month);

                            if (act == null)
                            {
                                AreaDeResponsabilidad areaResp = areas.SingleOrDefault(e => new string(e.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["AreaResponsabilidad"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()), StringComparison.InvariantCultureIgnoreCase));
                                if (areaResp == null)
                                {
                                    areaResp = areas.Single(c => c.Nombre == "<<S/A>>");
                                }
                                                                
                                act = new ActFijo()
                                {
                                    AreaResponsabilidad = areaResp.IdArea,
                                    CentroCosto = centros.SingleOrDefault(ct => new string(ct.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["CentroCosto"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdCentroCosto,
                                    NoInventario = long.Parse(row["NoInventario"].ToString().Trim()),
                                    Descripcion = row["Descripcion"].ToString().Trim(),
                                    DepreciacionAcum = decimal.Parse(row["DAcumulada"].ToString().Trim()),
                                    ValorAdquisicion = decimal.Parse(row["VAdquisicion"].ToString().Trim()),
                                    ValorContable = decimal.Parse(row["VContable"].ToString().Trim()),
                                    ValorResidual = decimal.Parse(row["VResidual"].ToString().Trim()),
                                    FechaSubmayor = fechaSubmayor,
                                    Responsable = responsables.SingleOrDefault(res => new string((res.Nombre.ToLower().Trim() + " " + res.Apellidos.ToLower().Trim()).Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["Responsable"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdResponsable
                                };

                                mdActFijo mdActFijo = new mdActFijo();
                                mdActFijo.SiNuevo = true;
                                mdActFijo.ActivoFijo = act;

                                resultadoActFijos.Add(mdActFijo);
                            }
                            else
                            {
                                act.AreaResponsabilidad = areas.SingleOrDefault(ar => new string(ar.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["AreaResponsabilidad"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdArea;
                                act.CentroCosto = centros.SingleOrDefault(ct => new string(ct.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["CentroCosto"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdCentroCosto;
                                act.Descripcion = row["Descripcion"].ToString().Trim();
                                act.DepreciacionAcum = decimal.Parse(row["DAcumulada"].ToString().Trim());
                                act.ValorAdquisicion = decimal.Parse(row["VAdquisicion"].ToString().Trim());
                                act.ValorContable = decimal.Parse(row["VContable"].ToString().Trim());
                                act.ValorResidual = decimal.Parse(row["VResidual"].ToString().Trim());
                                act.Responsable = responsables.SingleOrDefault(res => new string((res.Nombre.ToLower().Trim() + " " + res.Apellidos.ToLower().Trim()).Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["Responsable"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdResponsable;

                                mdActFijo mdActFijo = new mdActFijo();
                                mdActFijo.SiNuevo = false;
                                mdActFijo.ActivoFijo = act;

                                resultadoActFijos.Add(mdActFijo);
                            }
                        }
                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            ActFijo act = new ActFijo();

                            AreaDeResponsabilidad areaResp = areas.SingleOrDefault(e => new string(e.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["AreaResponsabilidad"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray())));
                            if (areaResp == null)
                            {
                                areaResp = areas.Single(c => c.Nombre == "<<S/A>>");
                            }

                            if (row["Responsable"].ToString().Trim().Equals(String.Empty))
                            {
                                act = new ActFijo()
                                {
                                    AreaResponsabilidad = areaResp.IdArea,
                                    CentroCosto = centros.SingleOrDefault(ct => new string(ct.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["CentroCosto"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdCentroCosto,
                                    NoInventario = long.Parse(row["NoInventario"].ToString().Trim()),
                                    Descripcion = row["Descripcion"].ToString().Trim(),
                                    DepreciacionAcum = decimal.Parse(row["DAcumulada"].ToString().Trim()),
                                    ValorAdquisicion = decimal.Parse(row["VAdquisicion"].ToString().Trim()),
                                    ValorContable = decimal.Parse(row["VContable"].ToString().Trim()),
                                    ValorResidual = decimal.Parse(row["VResidual"].ToString().Trim()),
                                    FechaSubmayor = fechaSubmayor,
                                    Responsable = responsables.SingleOrDefault(res => res.Nombre.ToLower().Trim().Equals("<<S/R>>".ToLower().Trim())).IdResponsable
                                };
                            }
                            else
                            {
                                Responsable resp = new Responsable();
                                resp = responsables.SingleOrDefault(res => new string((res.Nombre.ToLower().Trim() + " " + res.Apellidos).ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["Responsable"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray())));

                                CentroCosto centC = new CentroCosto();
                                centC = centros.SingleOrDefault(ct => new string(ct.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["CentroCosto"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray())));

                                act = new ActFijo()
                                {
                                    AreaResponsabilidad = areaResp.IdArea,
                                    CentroCosto = centC.IdCentroCosto,
                                    NoInventario = long.Parse(row["NoInventario"].ToString().Trim()),
                                    Descripcion = row["Descripcion"].ToString().Trim(),
                                    DepreciacionAcum = decimal.Parse(row["DAcumulada"].ToString().Trim()),
                                    ValorAdquisicion = decimal.Parse(row["VAdquisicion"].ToString().Trim()),
                                    ValorContable = decimal.Parse(row["VContable"].ToString().Trim()),
                                    ValorResidual = decimal.Parse(row["VResidual"].ToString().Trim()),
                                    FechaSubmayor = fechaSubmayor,
                                    Responsable = resp.IdResponsable
                                };
                            }

                            mdActFijo mdActFijo = new mdActFijo();
                            mdActFijo.SiNuevo = true;
                            mdActFijo.ActivoFijo = act;

                            resultadoActFijos.Add(mdActFijo);
                        }
                    }
                }

                return resultadoActFijos;
            }
            catch (Exception ex)
            {
                return new List<mdActFijo>();
            }
        }

        public clResponse<bool> InsertIntoDataBaseFromDataTableDatosImportados(DateTime fecha, DataTable datos)
        {
            try
            {
                List<AreaDeResponsabilidad> areas = new List<AreaDeResponsabilidad>();
                List<CentroCosto> centros = new List<CentroCosto>();
                List<Responsable> responsables = new List<Responsable>();
                List<mdActFijo> actFijos = new List<mdActFijo>();
                areas = GetFromDataTableAreasDeResponsabilidad(clGlobal.vDatos);
                centros = GetFromDataTableCentroDeCosto(clGlobal.vDatos);
                responsables = GetFromDataTableResponsablesDB(clGlobal.vDatos, areas);

                var testResp = responsables.Where(c => c.Nombre == "Pedro").ToList();

                actFijos = GetFromDataTableDatosImportados(clGlobal.vDatos, areas, centros, responsables, fecha);

                if (areas.Count() > 0 && centros.Count() > 0 && responsables.Count() > 0 && actFijos.Count() > 0)
                {
                    _context.Database.BeginTransaction();

                    foreach (var area in areas)
                    {
                        AreaDeResponsabilidad areaResp = _context.AreaDeResponsabilidads.AsEnumerable().SingleOrDefault(e => new string(e.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(area.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray())));
                        if (areaResp == null)
                        {
                            _context.AreaDeResponsabilidads.Add(area);
                        }
                    }


                    foreach (var centro in centros)
                    {
                        if (!_context.CentroCostos.Any(ct => ct.Nombre.ToLower().Trim().Equals(centro.Nombre.ToLower().Trim())))
                        {
                            _context.CentroCostos.Add(centro);
                        }
                    }

                    foreach (var responsable in responsables)
                    {
                        if (responsable.Nombre.Equals("<<S/R>>"))
                        {
                            if (!_context.Responsables.Any(re => re.Nombre.ToLower().Trim().Equals(responsable.Nombre.ToLower().Trim())))
                            {
                                _context.Responsables.Add(responsable);
                            }
                        }

                        if (!_context.Responsables.Any(re => (re.Nombre.ToLower().Trim() + " " + re.Apellidos.ToLower().Trim()).Equals(responsable.Nombre.ToLower().Trim() + " " + responsable.Apellidos.ToLower().Trim())))
                        {
                            _context.Responsables.Add(responsable);
                        }
                    }

                    foreach (var actFijo in actFijos)
                    {
                        if (actFijo.SiNuevo)
                        {
                            _context.ActFijos.Add(actFijo.ActivoFijo);
                        }
                        else
                        {
                            _context.ActFijos.Update(actFijo.ActivoFijo);
                        }
                    }

                    _context.SaveChanges();
                    _context.Database.CommitTransaction();

                    return new clResponse<bool>() { Mensaje = "Los datos han sido guardados satisfactoriamente.", Resultado = true, SiSatisfactorio = true };
                }
                else
                {
                    return new clResponse<bool>() { Mensaje = "Ocurrió un error durante el proceso de guardado de los datos seleccionados.", Resultado = false, SiSatisfactorio = false };
                }
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                return new clResponse<bool>() { Mensaje = "Error: " + ex.Message, Resultado = false, SiSatisfactorio = false };
            }
        }
        private string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue != null ? cell.CellValue.InnerXml : null;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }

        static string RemoveDiacritics(string text)
        {
            return string.Concat(
                text.Normalize(NormalizationForm.FormD)
                .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) !=
                                              UnicodeCategory.NonSpacingMark)
              ).Normalize(NormalizationForm.FormC);
        }
        #endregion Datos a importar de AFT

        #region Datos a importar de UTH
        public clResponse<DataTable> GetDataTableFromSpreadsheetUTH(Stream MyExcelStream, bool ReadOnly)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SpreadsheetDocument sDoc = SpreadsheetDocument.Open(MyExcelStream, ReadOnly))
                {
                    WorkbookPart workbookPart = sDoc.WorkbookPart;
                    IEnumerable<Sheet> sheets = sDoc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

                    SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                    SharedStringTable sst = sstpart.SharedStringTable;

                    string relationshipId = sheets.First().Id.Value;
                    WorksheetPart worksheetPart = (WorksheetPart)sDoc.WorkbookPart.GetPartById(relationshipId);
                    Worksheet workSheet = worksheetPart.Worksheet;
                    SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                    IEnumerable<Row> rows = sheetData.Descendants<Row>();

                    dt.Columns.Add("No");
                    dt.Columns.Add("Codigo");
                    dt.Columns.Add("Descripcion");
                    dt.Columns.Add("Grupo");
                    dt.Columns.Add("CentroCosto");
                    dt.Columns.Add("Responsable");
                    dt.Columns.Add("VAdquisicion", typeof(double));
                    dt.Columns.Add("V", typeof(double));
                    dt.Columns.Add("Estado");
                    dt.Columns.Add("AreaResponsabilidad");

                    int siTitulo = 0;

                    foreach (Row item in rows.Take(2))
                    {
                        int j = 0;
                        for (int i = 0; i < item.Descendants<Cell>().Count(); i++)
                        {
                            string getCellV = GetCellValue(sDoc, item.Descendants<Cell>().ElementAt(i));
                            if (getCellV != null)
                            {
                                if (siTitulo > 0)
                                {
                                    if (getCellV.Contains("Entidad:"))
                                    {

                                    }
                                    else
                                    {
                                        clGlobal.vEntidad = GetCellValue(sDoc, item.Descendants<Cell>().ElementAt(i));
                                        siTitulo++;
                                        break;
                                    }
                                }
                                else
                                {
                                    clGlobal.vReporte = getCellV;
                                    siTitulo++;

                                    break;
                                }
                            }
                        }

                        if (siTitulo > 1)
                        {
                            break;
                        }
                    }

                    rows = rows.Skip(3);
                    string areaResp = string.Empty;

                    int testCOnt = 0;

                    foreach (Row row in rows) //this will also include your header row...
                    {
                        DataRow tempRow = dt.NewRow();
                        int j = 0;

                        for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                        {
                            string getCellV = GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                            if (getCellV != null)
                            {
                                if (getCellV == "N.º")
                                    break;

                                tempRow[j] = getCellV;
                                j++;
                            }
                        }

                        if (tempRow[0].ToString() == "-  Área de responsabilidad: ")
                        {
                            areaResp = tempRow[1].ToString();
                        }
                        else if ((tempRow[0] != null) && (tempRow[6] != null) && (tempRow[7] != null))
                        {
                            if ((long.TryParse(tempRow[0].ToString(), out _)) && (double.TryParse(tempRow[6].ToString(), out _)) &&
                                (double.TryParse(tempRow[7].ToString(), out _)))
                            {
                                //if (dt.Select($"Codigo = '{tempRow[1].ToString()}'").Count() == 0)
                                //{
                                tempRow[j] = areaResp;
                                dt.Rows.Add(tempRow);
                                //}
                            }
                        }
                    }

                }

                //dt.Rows.RemoveAt(0);

                int cantidadFilasConErrores = dt.Select("Responsable = ''").Count();

                if (cantidadFilasConErrores > 0)
                    return new clResponse<DataTable>() { SiSatisfactorio = true, Mensaje = $"Los datos han sido importados satisfactoriamente; pero existen filas ({cantidadFilasConErrores}) con con posibles errores en sus valores", Resultado = dt };
                else
                    return new clResponse<DataTable>() { SiSatisfactorio = true, Mensaje = "Los datos han sido importados satisfactoriamente.", Resultado = dt };
            }
            catch (Exception ex)
            {
                return new clResponse<DataTable>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador.", Resultado = new DataTable() };
            }
        }

        public clResponse<mdDatosAdicionalesUTH> GetDatosAdicionalesFromDataTableUTH(DataTable dt)
        {
            try
            {
                bool siFecha = false;
                mdDatosAdicionalesUTH datosAdicionales = new mdDatosAdicionalesUTH();

                List<mdDatosAdicionalesAreasResponsabilidad> areasResponsabilidades = new List<mdDatosAdicionalesAreasResponsabilidad>();
                mdDatosAdicionalesAreasResponsabilidad datosAdicAreasResponsabilidad = new mdDatosAdicionalesAreasResponsabilidad();

                int cantProductosArea = 0;
                double vAdquisicion = 0;
                double vContable = 0;
                bool siEncabezadoAreaPrimeraVez = false;

                datosAdicionales.vAdquisicion = dt.Select().Sum(c => double.Parse(c.ItemArray[6].ToString()));
                datosAdicionales.vContable = dt.Select().Sum(c => double.Parse(c.ItemArray[7].ToString()));
                datosAdicionales.Entidad = clGlobal.vEntidad;
                datosAdicionales.TotalProductos = dt.Rows.Count;
                datosAdicionales.TotalAreas = dt.Select().Distinct().Count();
                datosAdicionales.fGeneracion = DateTime.Today;

                return new clResponse<mdDatosAdicionalesUTH>() { SiSatisfactorio = true, Mensaje = "Los datos adicionales han sido cargados satisfactoriamente.", Resultado = datosAdicionales };
            }
            catch (Exception ex)
            {
                return new clResponse<mdDatosAdicionalesUTH>() { SiSatisfactorio = false, Mensaje = $"Ha ocurrido el siguiente error: {ex.Message}. Contacte con el Administrador.", Resultado = new mdDatosAdicionalesUTH() };
            }
        }

        public clResponse<bool> InsertIntoDataBaseFromDataTableDatosImportadosUTH(DateTime fecha, DataTable datos)
        {
            try
            {
                List<AreaDeResponsabilidad> areas = new List<AreaDeResponsabilidad>();
                List<CentroCosto> centros = new List<CentroCosto>();
                List<Responsable> responsables = new List<Responsable>();
                List<mdUtilesHerramientas> utilHerramientas = new List<mdUtilesHerramientas>();
                areas = GetFromDataTableAreasDeResponsabilidad(clGlobal.vDatos);
                centros = GetFromDataTableCentroDeCosto(clGlobal.vDatos);
                responsables = GetFromDataTableResponsablesDB(clGlobal.vDatos, areas);

                utilHerramientas = GetFromDataTableDatosImportadosUTH(clGlobal.vDatos, areas, centros, responsables, fecha);

                if (areas.Count() > 0 && centros.Count() > 0 && responsables.Count() > 0 && utilHerramientas.Count() > 0)
                {
                    _context.Database.BeginTransaction();

                    foreach (var area in areas)
                    {
                        AreaDeResponsabilidad areaResp = _context.AreaDeResponsabilidads.AsEnumerable().SingleOrDefault(e => new string(e.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(area.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray())));
                        if (areaResp == null)
                        {
                            _context.AreaDeResponsabilidads.Add(area);
                        }
                    }


                    foreach (var centro in centros)
                    {
                        if (!_context.CentroCostos.Any(ct => ct.Nombre.ToLower().Trim().Equals(centro.Nombre.ToLower().Trim())))
                        {
                            _context.CentroCostos.Add(centro);
                        }
                    }

                    foreach (var responsable in responsables)
                    {
                        if (responsable.Nombre.Equals("<<S/R>>"))
                        {
                            if (!_context.Responsables.Any(re => re.Nombre.ToLower().Trim().Equals(responsable.Nombre.ToLower().Trim())))
                            {
                                _context.Responsables.Add(responsable);
                            }
                        }

                        if (!_context.Responsables.Any(re => (re.Nombre.ToLower().Trim() + " " + re.Apellidos.ToLower().Trim()).Equals(responsable.Nombre.ToLower().Trim() + " " + responsable.Apellidos.ToLower().Trim())))
                        {
                            _context.Responsables.Add(responsable);
                        }
                    }

                    foreach (var actFijo in utilHerramientas)
                    {
                        if (actFijo.SiNuevo)
                        {
                            _context.UtilesHerramientas.Add(actFijo.UtilesHerramientas);
                        }
                        else
                        {
                            _context.UtilesHerramientas.Update(actFijo.UtilesHerramientas);
                        }
                    }

                    _context.SaveChanges();
                    _context.Database.CommitTransaction();

                    return new clResponse<bool>() { Mensaje = "Los datos han sido guardados satisfactoriamente.", Resultado = true, SiSatisfactorio = true };
                }
                else
                {
                    return new clResponse<bool>() { Mensaje = "Ocurrió un error durante el proceso de guardado de los datos seleccionados.", Resultado = false, SiSatisfactorio = false };
                }
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                return new clResponse<bool>() { Mensaje = "Error: " + ex.Message, Resultado = false, SiSatisfactorio = false };
            }
        }

        public List<mdUtilesHerramientas> GetFromDataTableDatosImportadosUTH(DataTable dt, List<AreaDeResponsabilidad> areas, List<CentroCosto> centros, List<Responsable> responsables, DateTime fechaSubmayor)
        {
            List<UtilesHerramienta> utilHerramienta = new List<UtilesHerramienta>();
            utilHerramienta = _context.UtilesHerramientas.Where(act => act.FechaListadoUth.Year == fechaSubmayor.Year && act.FechaListadoUth.Month == fechaSubmayor.Month).ToList();

            List<mdUtilesHerramientas> resultadoUtilHerramienta = new List<mdUtilesHerramientas>();

            try
            {
                if (utilHerramienta != null)
                {
                    if (utilHerramienta.Count() > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            UtilesHerramienta utilH = new UtilesHerramienta();
                            utilH = _context.UtilesHerramientas.Include(c => c.AreaResponsabilidadNavigation).SingleOrDefault(act => act.No.Equals(long.Parse(row["No"].ToString().Trim())) 
                                && new string(act.Codigo.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["Codigo"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()), StringComparison.InvariantCultureIgnoreCase)
                                && new string(act.AreaResponsabilidadNavigation.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["AreaResponsabilidad"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()), StringComparison.InvariantCultureIgnoreCase)
                                && act.FechaListadoUth.Year == fechaSubmayor.Year
                                && act.FechaListadoUth.Month == fechaSubmayor.Month);

                            if (utilH == null)
                            {
                                AreaDeResponsabilidad areaResp = areas.SingleOrDefault(e => new string(e.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["AreaResponsabilidad"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()), StringComparison.InvariantCultureIgnoreCase));
                                if (areaResp == null)
                                {
                                    areaResp = areas.Single(c => c.Nombre == "<<S/A>>");
                                }

                                utilH = new UtilesHerramienta()
                                {
                                    AreaResponsabilidad = areaResp.IdArea,
                                    CentroCosto = centros.SingleOrDefault(ct => new string(ct.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["CentroCosto"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdCentroCosto,
                                    No = long.Parse(row["No"].ToString().Trim()),
                                    IdUth = Guid.NewGuid(),
                                    Descripcion = row["Descripcion"].ToString().Trim(),
                                    Codigo = row["Codigo"].ToString().Trim(),
                                    ValorAdquisicion = decimal.Parse(row["VAdquisicion"].ToString().Trim()),
                                    ValorContable = decimal.Parse(row["V"].ToString().Trim()),
                                    FechaListadoUth = fechaSubmayor,
                                    Responsable = responsables.SingleOrDefault(res => new string((res.Nombre.ToLower().Trim() + " " + res.Apellidos.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray())).Equals(new string(row["Responsable"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdResponsable
                                };

                                mdUtilesHerramientas mdUTH = new mdUtilesHerramientas();
                                mdUTH.SiNuevo = true;
                                mdUTH.UtilesHerramientas = utilH;

                                resultadoUtilHerramienta.Add(mdUTH);
                            }
                            else
                            {
                                utilH.AreaResponsabilidad = areas.SingleOrDefault(ar => new string(ar.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["AreaResponsabilidad"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdArea;
                                utilH.CentroCosto = centros.SingleOrDefault(ct => new string(ct.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["CentroCosto"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdCentroCosto;
                                utilH.Descripcion = row["Descripcion"].ToString().Trim();
                                utilH.ValorAdquisicion = decimal.Parse(row["VAdquisicion"].ToString().Trim());
                                utilH.ValorContable = decimal.Parse(row["V"].ToString().Trim());
                                utilH.Responsable = responsables.SingleOrDefault(res => new string((res.Nombre.ToLower().Trim() + " " + res.Apellidos.ToLower().Trim()).Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["Responsable"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdResponsable;

                                mdUtilesHerramientas mdUTH = new mdUtilesHerramientas();
                                mdUTH.SiNuevo = false;
                                mdUTH.UtilesHerramientas = utilH;

                                resultadoUtilHerramienta.Add(mdUTH);
                            }
                        }
                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            UtilesHerramienta utilH = new UtilesHerramienta();

                            AreaDeResponsabilidad areaResp = areas.SingleOrDefault(e => new string(e.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["AreaResponsabilidad"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray())));
                            if (areaResp == null)
                            {
                                areaResp = areas.Single(c => c.Nombre == "<<S/A>>");
                            }

                            if (row["Responsable"].ToString().Trim().Equals(String.Empty))
                            {
                                utilH = new UtilesHerramienta()
                                {
                                    AreaResponsabilidad = areaResp.IdArea,
                                    CentroCosto = centros.SingleOrDefault(ct => new string(ct.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["CentroCosto"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()))).IdCentroCosto,
                                    No = long.Parse(row["No"].ToString().Trim()),
                                    IdUth = Guid.NewGuid(),
                                    Descripcion = row["Descripcion"].ToString().Trim(),
                                    Codigo = row["Codigo"].ToString().Trim(),
                                    ValorAdquisicion = decimal.Parse(row["VAdquisicion"].ToString().Trim()),
                                    ValorContable = decimal.Parse(row["V"].ToString().Trim()),
                                    FechaListadoUth = fechaSubmayor,
                                    Responsable = responsables.SingleOrDefault(res => res.Nombre.ToLower().Trim().Equals("<<S/R>>".ToLower().Trim())).IdResponsable
                                };
                            }
                            else
                            {
                                Responsable resp = new Responsable();
                                resp = responsables.SingleOrDefault(res => new string((res.Nombre.ToLower().Trim() + " " + res.Apellidos).ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["Responsable"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray())));

                                CentroCosto centC = new CentroCosto();
                                centC = centros.SingleOrDefault(ct => new string(ct.Nombre.ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray()).Equals(new string(row["CentroCosto"].ToString().ToLower().Trim().Normalize(NormalizationForm.FormD).Where(c => c < 128).ToArray())));

                                utilH = new UtilesHerramienta()
                                {
                                    AreaResponsabilidad = areaResp.IdArea,
                                    CentroCosto = centC.IdCentroCosto,
                                    No = long.Parse(row["No"].ToString().Trim()),
                                    IdUth = Guid.NewGuid(),
                                    Descripcion = row["Descripcion"].ToString().Trim(),
                                    Codigo = row["Codigo"].ToString().Trim(),
                                    ValorAdquisicion = decimal.Parse(row["VAdquisicion"].ToString().Trim()),
                                    ValorContable = decimal.Parse(row["V"].ToString().Trim()),
                                    FechaListadoUth = fechaSubmayor,
                                    Responsable = resp.IdResponsable
                                };
                            }

                            mdUtilesHerramientas mdUTH = new mdUtilesHerramientas();
                            mdUTH.SiNuevo = true;
                            mdUTH.UtilesHerramientas = utilH;

                            resultadoUtilHerramienta.Add(mdUTH);
                        }
                    }
                }

                return resultadoUtilHerramienta;
            }
            catch (Exception ex)
            {
                return new List<mdUtilesHerramientas>();
            }
        }
        #endregion Datos a importar de UTH

        #region DATASET HELPER  
        private bool ColumnEqual(object A, object B)
        {
            // Compares two values to see if they are equal. Also compares DBNULL.Value.             
            if (A == DBNull.Value && B == DBNull.Value) //  both are DBNull.Value  
                return true;
            if (A == DBNull.Value || B == DBNull.Value) //  only one is BNull.Value  
                return false;
            return (A.Equals(B)); // value type standard comparison  
        }
        public DataTable SelectDistinct(DataTable SourceTable, string FieldName)
        {
            // Create a Datatable – datatype same as FieldName  
            DataTable dt = new DataTable(SourceTable.TableName);
            dt.Columns.Add(FieldName, SourceTable.Columns[FieldName].DataType);
            // Loop each row & compare each value with one another  
            // Add it to datatable if the values are mismatch  
            object LastValue = null;
            foreach (DataRow dr in SourceTable.Select("", FieldName))
            {
                if (LastValue == null || !(ColumnEqual(LastValue, dr[FieldName])))
                {
                    LastValue = dr[FieldName];
                    dt.Rows.Add(new object[] { LastValue });
                }
            }
            return dt;
        }
        #endregion
    }
}
