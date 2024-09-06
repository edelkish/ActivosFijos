namespace ActivosFijos.Clases
{
    public class clResponse<T>
    {
        /// <summary>
        /// Si es verdadero, la conexión a la API se realizó de forma satisfactoria, sino existe un error que se devolvió en la propiedad <see cref="Message"/>
        /// </summary>
        public bool SiSatisfactorio { get; set; }
        /// <summary>
        /// Mensaje que devuelve la conexión a la API.
        /// </summary>
        public string Mensaje { get; set; }
        /// <summary>
        /// Objeto genérico que contiene el resultado de la llamada a un API.
        /// </summary>
        public T Resultado { get; set; }
    }
}
