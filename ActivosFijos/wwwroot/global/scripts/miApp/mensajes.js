MiApp.Mensajes = function () {
    return {
        mostrarNotificacion: function (tipo, texto) {
            toastr.options = {
                "closeButton": true,
                "debug": false,
                "positionClass": "toast-top-right",
                "onclick": null,
                "showDuration": "1000",
                "hideDuration": "1000",
                "timeOut": "5000",
                "extendedTimeOut": "1000",
                "showEasing": "swing",
                "hideEasing": "linear",
                "showMethod": "fadeIn",
                "hideMethod": "fadeOut",
                "progressBar": true
            };

            if (tipo == "success") {
                toastr.info(texto, "Información");
            }
            if (tipo == "error") {
                toastr.error(texto, "Error");
            }
            if (tipo == "warning") {
                toastr.warning(texto, "Aviso");
            }

        }
    }
}();