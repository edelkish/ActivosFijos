MiApp.Generales = function () {
    var form = $(this);
    var callbackSubmitForm;
    this.gestionarMsg = function () {
        var mensaje = $("#spanMensaje").html();
        if (mensaje != "" && mensaje != null) {
            var arr_msg = mensaje.split('|');
            MiApp.Mensajes.mostrarNotificacion(arr_msg[0], arr_msg[1]);
        }
    };
    this.onSubmitForm = function () {
        form.on("submit", function (event) {
            if (callbackSubmitForm) {
                if (callbackSubmitForm() == false) {
                    event.preventDefault();
                    event.stopPropagation();
                    return;
                }
            }
            try {
                if (!$(event.target).hasClass("noFormLoading")) {
                    MiApp.LoadingPanel.mostrarNotificacion("formEnviar", "Enviando datos, por favor espere...");
                }
            } catch (e) { }
        });
    };
    return {
        init: function () {
            gestionarMsg()
            onSubmitForm();
        },
        //accion tiene que ser una función que retorne true ó false.
        gestionarCallbackSubmitForm: function (accion) {
            callbackSubmitForm = accion;
        }
    }
}();
MiApp.Generales.init();