using ActivosFijos.Models.EntidadesBD;
using System.Data;

namespace ActivosFijos.Clases
{
    public static class clGlobal
    {
        public static bool siNo { get; set; }
        public static string objMensaje { get; set; } = String.Empty;
        public static string salt { get; set; } = "7#1F52DC1FEAC7-172635E71F75DC919@71B/F15F8E-4412251BD7346F2475D8133-112745/6CCB57D6ED36F8#7/BF34/65346702361F940FCF0B5B417+3F1D22716C36BFF3D69B85F+17E";
        public static MemoryStream vMemory { get; set; } = new MemoryStream();
        public static string vContentType { get; set; } = string.Empty;
        public static string vFileName { get; set; } = string.Empty;
        public static Guid vHandle { get; set; } = Guid.Empty;

        public static DataTable vDatos { get; set; } = new DataTable();
        public static string vEntidad { get; set; } = string.Empty;
        public static string vReporte { get; set; } = string.Empty;
    }

    public static class clMensaje
    {
        public const string Excepcion = "Ha ocurrido una excepción.";
        public const string ErrorListado = "Ha ocurrido un error al cargar el listado.";
        public const string ErrorCargarDatos = "Ha ocurrido un error al cargar los datos.";
        public const string ExisteRegistro = "Existe un registro de igual información.";
        public const string Satisfactorio = "La acción se ha realizado satisfactoriamente.";
        public const string Error = "Ha ocurrido un error inesperado.";
        public const string RegistroNoEncontrado = "El registro solicitado no se ha encontrado.";
        public const string ModeloInvalido = "El modelo es inválido.";
        public const string BorradoNoSatisfactorio = "No es posible eliminar el registro, existen relaciones que dependen de él.";
        public const string SuccessMsg = "success";
        public const string ErrorMsg = "error";
        public const string WarningMsg = "warning";
        public const string NoCoincideIdModuloClientId = "No está permitido acceder ó gestionar los recursos de otro módulo.";
    }
}
