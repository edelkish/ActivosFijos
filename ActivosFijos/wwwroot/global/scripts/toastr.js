if ((siSatisfactorio == "True") && (mensaje != "")) {
    MiApp.Mensajes.mostrarNotificacion("success", mensaje);
}
else if ((siSatisfactorio == "False") && (mensaje != "")) {
    MiApp.Mensajes.mostrarNotificacion("error", mensaje);
}